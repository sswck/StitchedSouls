using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;

public enum BattleState { Start, PlayerTurn, EnemyTurn, Won, Lost }

[System.Serializable]
public class SpawnOffsetConfig
{
    public int gridX;
    public int gridY;
    public Vector3 offset = Vector3.zero;
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Game State")]
    public BattleState state;

    [Header("Slot System")]
    public List<CardData> actionSlots = new List<CardData>();

    public Unit playerUnit;

    [Header("Spawn Settings")]
    public Unit unitPrefab;
    public Unit normalEnemyPrefab;
    public Unit eliteEnemyPrefab;
    [Tooltip("스폰 위치별 Tile Position Offset. (3,3), (3,0), (4,2) 등 각 타일마다 다른 오프셋을 Inspector에서 지정할 수 있습니다.")]
    public List<SpawnOffsetConfig> spawnPositionOffsets = new List<SpawnOffsetConfig>();

    [Header("Units")]
    public List<Unit> allUnits = new List<Unit>();

    // TODO_juwan: 배틀 통계 기능 추가
    [Header("Battle Statistics")]
    public int totalDamageDeal;
    public int totalDamageTaken;
    public int totalDamageBlocked;

    private bool isBattleEnded = false;

    void Start()
    {
        state = BattleState.Start;
        // ResetBattleStatistics();

        if (AnchorGridManager.Instance != null)
            AnchorGridManager.Instance.GenerateGrid();
        
        SpawnPlayer();
        LoadPlayerData();

        if (GameManager.Instance != null && DeckManager.Instance != null)
        {
             DeckManager.Instance.InitializeDeck(GameManager.Instance.masterDeck);
        }

        StartCoroutine(SetupBattle());

        BattleUIManager.Instance.UpdateActionSlotUI(new List<CardData>());

        if (SoundManager.Instance != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.currentNodeType == NodeType.Elite)
            {
                SoundManager.Instance.PlayBGM(SoundManager.Instance.eliteBattleBGM);
            }
            else if (GameManager.Instance.currentNodeType == NodeType.Boss)
            {
                // 보스곡이 있다면 여기에 (지금은 엘리트 곡 사용하거나 비워둠)
                SoundManager.Instance.PlayBGM(SoundManager.Instance.eliteBattleBGM);
            }
            else
            {
                SoundManager.Instance.PlayBGM(SoundManager.Instance.normalBattleBGM);
            }
        }
    }

    void LoadPlayerData()
    {
        // GameManager가 있고 데이터가 존재하면 가져오기
        if (GameManager.Instance != null && GameManager.Instance.masterDeck.Count > 0)
        {
            Debug.Log("📂 GameManager에서 데이터 로드 중...");

            if (playerUnit != null)
            {
                playerUnit.maxHP = GameManager.Instance.maxHP;
                playerUnit.currentHP = GameManager.Instance.currentHP;
                // spd: 전투 시 플레이어 턴 최대 이동 횟수 = 기본 2 + spd
                playerUnit.maxMovePoints = 2 + GameManager.Instance.spd;

                playerUnit.UpdateHPBar();
            }
        }
    }

    IEnumerator SetupBattle()
    {
        yield return new WaitForSeconds(0.5f);
        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        Debug.Log(">>> ⚔️ 플레이어 턴 시작! ⚔️ <<<");
        state = BattleState.PlayerTurn;
        
        if(playerUnit != null) playerUnit.OnTurnStart();

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DrawCards(DeckManager.Instance.drawCountPerTurn);
        }
    }

    public void EndPlayerTurn()
    {
        if (state == BattleState.Won || state == BattleState.Lost) return;

        Debug.Log("플레이어 턴 종료...");

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DiscardHand();
        }
        
        state = BattleState.EnemyTurn;
        StartCoroutine(EnemyTurnRoutine());
    }

    // AI 로직이 들어갈 곳
    IEnumerator EnemyTurnRoutine()
    {
        Debug.Log(">>> 😈 적 턴 시작! 😈 <<<");
        yield return new WaitForSeconds(1.0f);

        if (state == BattleState.Won || state == BattleState.Lost) yield break;

        // 1. 모든 적 유닛을 찾아서 행동시키기
        // (지금은 리스트에 플레이어도 섞여 있으니 구분해야 함. 
        //  하지만 간단하게 allUnits 중 playerUnit이 아닌 것만 적이라고 가정)
        // 복사본으로 순회 → AI_TakeAction 중 allUnits가 수정되어도 열거자 오류 방지
        foreach (var unit in new List<Unit>(allUnits))
        {
            // [추가] 행동 루프 도중에도 게임이 끝났다면 즉시 중단 (예: 반격으로 적 사망 등)
            if (state == BattleState.Won || state == BattleState.Lost) yield break;

            // 플레이어거나 죽은 유닛은 패스
            if (unit == playerUnit || unit.currentHP <= 0) continue;

            Debug.Log($"[{unit.name}] 행동 시작...");
            
            // AI 행동 실행 (타겟은 무조건 플레이어)
            unit.AI_TakeAction(playerUnit);

            // 행동 간 딜레이 (애니메이션 볼 시간 줌)
            yield return new WaitForSeconds(1.0f);
        }

        Debug.Log("적 턴 종료!");
        if (state == BattleState.Won || state == BattleState.Lost) yield break;
        StartPlayerTurn();
    }

    void SpawnPlayer()
    {
        playerUnit = Instantiate(unitPrefab);
        playerUnit.name = "Player Unit";
        ApplySpawnOffset(playerUnit, 0, 2);
        playerUnit.Init(0, 2);
        
        allUnits.Add(playerUnit);
        
        SpawnEnemy();
    }
    
    void SpawnEnemy()
    {
        // GameManager 정보가 없다면 기본 1마리만 중앙에 소환
        if (GameManager.Instance == null)
        {
            SpawnNormalEnemyAt(3, 3);
            return;
        }

        switch (GameManager.Instance.currentNodeType)
        {
            case NodeType.Battle:
                // 일반 전투: 노말 적 2마리 Tile_3_3, Tile_3_0
                SpawnNormalEnemyAt(3, 3);
                SpawnNormalEnemyAt(3, 0);
                break;

            case NodeType.Elite:
                // 엘리트 전투: 노말 2마리 (Tile_3_3, Tile_3_0) + 엘리트 1마리 (Tile_4_2)
                SpawnNormalEnemyAt(3, 3);
                SpawnNormalEnemyAt(3, 0);
                SpawnEliteEnemyAt(4, 2);
                Debug.Log("⚠️ 경고: 엘리트 몬스터 출현! (노말 x2 + 엘리트 x1)");
                break;

            case NodeType.Boss:
                // 보스 전투는 아직 구현 전이므로 임시로 노말 1마리만 소환
                // 이후 보스 프리팹이 생기면 여기서 교체
                Debug.Log("디버그 보스 스폰 지점 (임시로 노말 1마리 소환)");
                SpawnNormalEnemyAt(3, 3);
                break;

            default:
                // 그 외 타입은 안전하게 기본 1마리
                SpawnNormalEnemyAt(3, 3);
                break;
        }
    }

    void SpawnNormalEnemyAt(int x, int y)
    {
        if (normalEnemyPrefab == null)
        {
            Debug.LogWarning("normalEnemyPrefab 이 설정되지 않았습니다.");
            return;
        }

        Unit enemy = Instantiate(normalEnemyPrefab);
        enemy.name = "Normal Enemy";
        ApplySpawnOffset(enemy, x, y);
        enemy.Init(x, y);
        allUnits.Add(enemy);
    }

    void SpawnEliteEnemyAt(int x, int y)
    {
        if (eliteEnemyPrefab == null)
        {
            Debug.LogWarning("eliteEnemyPrefab 이 설정되지 않았습니다.");
            return;
        }

        Unit enemy = Instantiate(eliteEnemyPrefab);
        enemy.name = "Elite Enemy";
        ApplySpawnOffset(enemy, x, y);
        enemy.Init(x, y);
        allUnits.Add(enemy);
    }

    void ApplySpawnOffset(Unit unit, int x, int y)
    {
        if (spawnPositionOffsets == null) return;
        foreach (var config in spawnPositionOffsets)
        {
            if (config.gridX == x && config.gridY == y)
            {
                unit.tilePositionOffset = config.offset;
                break;
            }
        }
    }

    public Unit GetUnitAt(int x, int y)
    {
        foreach (var unit in allUnits)
        {
            if (unit.gridX == x && unit.gridY == y)
            {
                return unit;
            }
        }
        return null;
    }

    void Awake()
    {
        Instance = this;
    }

    public void AddCardToSlot(CardData card)
    {
        if (actionSlots.Count < 3) // 슬롯이 3개라고 가정 추후 3을 상수변수로 변경
        {
            actionSlots.Add(card);
            Debug.Log($"슬롯에 카드 등록됨: {card.cardName}");

            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.RemoveCardFromHand(card);
            }

            BattleUIManager.Instance.UpdateActionSlotUI(actionSlots);
        }
        else
        {
            Debug.Log("슬롯이 가득 찼습니다!");
        }
    }

    // -----------------------------------------------------------------------
    // 턴 종료 버튼(스페이스바)을 누르면 실행되는 함수
    // -----------------------------------------------------------------------
    public void ExecuteSlots()
    {
        if (actionSlots.Count == 0)
        {
            Debug.Log("--- 슬롯이 비어있는 상태로 턴 종료 ---");
            EndPlayerTurn();
            return;
        }

        state = BattleState.EnemyTurn;
        Debug.Log("--- 작전 실행 시작! ---");

        Sequence seq = DOTween.Sequence();

        foreach (var card in actionSlots)
        {
            playerUnit.PerformAction(card, seq);
            seq.AppendInterval(0.6f);
        }

        seq.OnComplete(() => {
            Debug.Log("--- 턴 종료 ---");

            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.DiscardUsedCards(actionSlots);
            }
            
            actionSlots.Clear();
            BattleUIManager.Instance.UpdateActionSlotUI(actionSlots);
            EndPlayerTurn();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (state != BattleState.PlayerTurn) return;
        
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ExecuteSlots();
        }

        // 데모 버전: 방향키를 누르면 즉시 이동 ?추후 어떻게 고도화할 건지...
        if (playerUnit != null && playerUnit.CanMove())
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) MovePlayer(0, 1);
            if (Keyboard.current.downArrowKey.wasPressedThisFrame) MovePlayer(0, -1);
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame) MovePlayer(-1, 0);
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) MovePlayer(1, 0);
        }

        // 아이템 사용 로직
        if (Keyboard.current.digit1Key.wasPressedThisFrame) TryUseItem(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) TryUseItem(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) TryUseItem(2);
    }

    void TryUseItem(int itemIndex)
    {
        if (InventoryManager.Instance == null || playerUnit == null) return;

        ItemData itemToUse = InventoryManager.Instance.GetItem(itemIndex);

        if (itemToUse == null)
        {
            Debug.Log($"아이템 슬롯 {itemIndex + 1}이 비어있습니다.");
            return;
        }

        // 아이템 사용! (인벤토리에서 제거 및 이벤트 발생)
        InventoryManager.Instance.UseItem(itemIndex);

        // 효과 적용
        ApplyItemEffect(itemToUse);
    }

    void ApplyItemEffect(ItemData item)
    {
        Debug.Log($"[아이템 효과] '{item.itemName}' 사용! 효과: {item.effectType}, 수치: {item.value}");

        switch (item.effectType)
        {
            case ItemEffectType.HealHP:
                playerUnit.Heal(item.value);
                Debug.Log($"플레이어의 HP를 {item.value} 만큼 회복. 현재 HP: {playerUnit.currentHP}/{playerUnit.maxHP}");
                break;
            case ItemEffectType.HealSP:
                // GameManager의 SP를 직접 조작해야 할 수 있습니다.
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.currentSp = Mathf.Min(GameManager.Instance.currentSp + item.value, GameManager.Instance.maxSp);
                    Debug.Log($"SP를 {item.value} 만큼 회복. 현재 SP: {GameManager.Instance.currentSp}/{GameManager.Instance.maxSp}");
                    // TODO: SP UI가 있다면 업데이트
                }
                break;
            case ItemEffectType.DamageBuff:
                playerUnit.ApplyDamageBuff(item.value, 3);
                Debug.Log($"플레이어의 데미지가 {item.value} 만큼 증가합니다. (3턴 지속)");
                break;
            case ItemEffectType.MaxMovePoints:
                playerUnit.ApplyMoveBuff(item.value, 2);
                Debug.Log($"플레이어의 이동 횟수가 {item.value} 만큼 증가합니다. (2턴 지속)");
                break;
        }

        // 체력바 UI 업데이트
        playerUnit.UpdateHPBar();
    }

    public void PreviewCardRange(CardData card)
    {
        if (playerUnit == null || card == null) return;

        // 플레이어가 왼쪽을 보고 있는지 확인 (Spine ScaleX 기준)
        bool isLeft = playerUnit.GetComponentInChildren<Spine.Unity.SkeletonAnimation>().Skeleton.ScaleX < 0;

        if (card.targetType == TargetType.Pattern)
        {
            AnchorGridManager.Instance.HighlightAttackRange(playerUnit.gridX, playerUnit.gridY, card.targetPattern, isLeft);
        }
        else if (card.targetType == TargetType.Self)
        {
            // (선택 사항) 버프 카드는 내 위치만 표시하거나 다른 색으로 표시
            AnchorGridManager.Instance.HighlightAttackRange(playerUnit.gridX, playerUnit.gridY, new List<Vector2Int>{ Vector2Int.zero }, isLeft);
        }
    }

    public void StopPreviewRange()
    {
        AnchorGridManager.Instance.ResetAllTiles();
    }

    void MovePlayer(int xDir, int yDir)
    {
        playerUnit.Move(xDir, yDir);
    }

    public void OnUnitDead(Unit unit)
    {
        allUnits.Remove(unit);

        if (unit == playerUnit)
        {
            OnGameOver();
        }
        else
        {
            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        if (isBattleEnded) return;

        int enemyCount = 0;
        foreach (var unit in allUnits)
        {
            // (임시) 플레이어가 아닌 유닛은 모두 적으로 간주
            if (unit != playerUnit) enemyCount++;
        }

        if (enemyCount <= 0)
        {
            OnVictory();
        }
    }
    
    private void OnVictory()
    {
        isBattleEnded = true;
        state = BattleState.Won;

        if (GameManager.Instance != null && playerUnit != null)
        {
            GameManager.Instance.currentHP = playerUnit.currentHP;
            Debug.Log($"💾 체력 저장 완료: {GameManager.Instance.currentHP}");
        }
        
        Debug.Log("🎉 VICTORY! 모든 적을 처치했습니다.");
        BattleUIManager.Instance.ShowResultUI(true);
        
        // sp 획득 로직
        //일반: +1, 엘리트: +2, 보스: +3
        if(GameManager.Instance.currentNodeType == NodeType.Elite)
            GameManager.Instance.currentSp += 2;
        else
            GameManager.Instance.currentSp += 1;

        GameManager.Instance.currentSp = Mathf.Min(GameManager.Instance.currentSp, GameManager.Instance.maxSp);
        // [수정] 결과창의 '확인/다음' 버튼이 누를 때 CompleteStage가 실행되도록 해야 함.
        // BattleUIManager의 OnRestartButton(또는 OnNextButton)을 수정해야 합니다.
    }

    private void OnGameOver()
    {
        isBattleEnded = true;
        state = BattleState.Lost;
        Debug.Log("💀 GAME OVER... 플레이어가 사망했습니다.");

        BattleUIManager.Instance.ShowResultUI(false);
    }

    // TODO_juwan: 배틀 통계 기능 추가
    public void RecordDamageDeal(int amount)
    {
        totalDamageDeal += amount;
    }

    public void RecordDamageTaken(int amount)
    {
        totalDamageTaken += amount;
    }

    public void RecordDamageBlocked(int amount)
    {
        totalDamageBlocked += amount;
    }

    private void ResetBattleStatistics()
    {
        totalDamageDeal = 0;
        totalDamageTaken = 0;
        totalDamageBlocked = 0;
    }
}
