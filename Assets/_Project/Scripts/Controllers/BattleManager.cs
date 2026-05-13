using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using System.Reflection;
using UnityEngine.UI;

public enum BattleState { Start, PlayerTurn, EnemyTurn, Won, Lost }
public enum EnemyType { Normal, Elite, Boss, Player }

[System.Serializable]
public class SpawnOffsetConfig
{
    public int gridX;
    public int gridY;
    public Vector3 offset = Vector3.zero;
    public EnemyType enemyType = EnemyType.Normal;
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
    public List<Unit> normalEnemyPrefabs = new List<Unit>();
    public List<Unit> eliteEnemyPrefabs = new List<Unit>();
    public List<Unit> bossEnemyPrefabs = new List<Unit>();
    [Tooltip("스폰 위치별 Tile Position Offset. (3,3), (3,0), (4,2) 등 각 타일마다 다른 오프셋을 Inspector에서 지정할 수 있습니다.")]
    public List<SpawnOffsetConfig> spawnPositionOffsets = new List<SpawnOffsetConfig>();

    [Header("Turn System")]
    public List<Unit> turnQueue = new List<Unit>();
    public List<Unit> actedUnits = new List<Unit>(); // 이미 턴을 마친 유닛들
    public Unit currentTurnUnit;

    [Header("Units")]
    public List<Unit> allUnits = new List<Unit>();

    [Header("Item VFX Prefabs")]
    public GameObject healItemVFXPrefab;

    [Header("Battle Statistics")]
    public int totalDamageDeal;
    public int totalDamageTaken;
    public int totalDamageBlocked;
    public int totalGoldEarned;
    public int totalSpEarned;

    private bool isBattleEnded = false;

    public int goldReward = 0;

    [Header("execite_Image")]
    public Sprite exectionImage_Active;
    public Sprite exectionImage_Inactive;
    public GameObject execiteButton;






    void Start()
    {
        state = BattleState.Start;
        ResetBattleStatistics();

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
                SoundManager.Instance.PlayBGM(SoundManager.Instance.bossBattleBGM);
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
                playerUnit.attackSpeed = playerUnit.attackSpeed + GameManager.Instance.spd; // 공격속도 연동
                // spd: 전투 시 플레이어 턴 최대 이동 횟수 = 기본 2 + spd (기존 로직 유지 가능 혹은 변경 가능)
                playerUnit.maxMovePoints = 2 + GameManager.Instance.movePoint;

                playerUnit.UpdateHPBar();
                if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdateMovementUI();
            }
        }
    }

    IEnumerator SetupBattle()
    {
        yield return new WaitForSeconds(0.5f);
        StartNextTurn();
    }

    public void GenerateTurnQueue()
    {
        turnQueue.Clear();
        actedUnits.Clear();
        // 공격속도 내림차순 정렬
        List<Unit> sortedUnits = new List<Unit>(allUnits);

        Debug.Log("--- [턴 큐 생성 시작] ---");
        foreach (var u in sortedUnits)
        {
            Debug.Log($"유닛: {u.name}, 공격속도: {u.attackSpeed}");
        }

        sortedUnits.Sort((a, b) => b.attackSpeed.CompareTo(a.attackSpeed));

        foreach (var unit in sortedUnits)
        {
            if (unit.currentHP > 0)
                turnQueue.Add(unit);
        }

        string orderStr = "최종 턴 순서: ";
        foreach (var u in turnQueue) orderStr += $"{u.name}({u.attackSpeed}) -> ";
        Debug.Log(orderStr);
        Debug.Log($"--- 새로운 라운드 시작! 유닛 수: {turnQueue.Count} ---");

        if (TurnOrderUI.Instance != null)
        {
            TurnOrderUI.Instance.Refresh(turnQueue, currentTurnUnit, actedUnits);
        }
    }

    public void StartNextTurn()
    {
        if (state == BattleState.Won || state == BattleState.Lost) return;

        // 큐가 비어있으면 새로운 라운드 생성
        if (turnQueue.Count == 0)
        {
            GenerateTurnQueue();
        }

        if (turnQueue.Count == 0)
        {
            Debug.LogWarning("행동 가능한 유닛이 없습니다.");
            return;
        }

        currentTurnUnit = turnQueue[0];
        turnQueue.RemoveAt(0);

        if (currentTurnUnit == null || currentTurnUnit.currentHP <= 0)
        {
            StartNextTurn();
            return;
        }

        Debug.Log($">>> [{currentTurnUnit.name}]의 턴! (SPD: {currentTurnUnit.attackSpeed}) <<<");

        if (TurnOrderUI.Instance != null)
        {
            TurnOrderUI.Instance.Refresh(turnQueue, currentTurnUnit, actedUnits);
        }

        if (currentTurnUnit == playerUnit)
        {
            StartPlayerTurn();
        }
        else
        {
            state = BattleState.EnemyTurn;
            StartCoroutine(EnemyTurnRoutine(currentTurnUnit));
        }
    }

    void StartPlayerTurn()
    {
        Debug.Log(">>> ⚔️ 플레이어 턴 시작! ⚔️ <<<");
        state = BattleState.PlayerTurn;

        if (playerUnit != null) playerUnit.OnTurnStart();

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DrawCards(DeckManager.Instance.drawCountPerTurn);
        }

        if (execiteButton != null)
        {
            execiteButton.GetComponent<Image>().sprite = exectionImage_Active;

        }
    }

    public void EndPlayerTurn()
    {
        if (state == BattleState.Won || state == BattleState.Lost) return;

        if (currentTurnUnit != null && !actedUnits.Contains(currentTurnUnit))
        {
            actedUnits.Add(currentTurnUnit);
        }

        Debug.Log("플레이어 턴 종료...");

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DiscardHand();
        }

        if (execiteButton != null)
        {
            execiteButton.GetComponent<Image>().sprite = exectionImage_Inactive;

        }

        ChargeUlt(10);

        StartNextTurn();
    }

    // AI 로직이 들어갈 곳
    IEnumerator EnemyTurnRoutine(Unit unit)
    {
        yield return new WaitForSeconds(0.5f);

        if (state == BattleState.Won || state == BattleState.Lost) yield break;

        if (unit == null || unit.currentHP <= 0)
        {
            StartNextTurn();
            yield break;
        }

        Debug.Log($"[{unit.name}] 행동 시작...");

        // AI 행동 실행 (타겟은 무조건 플레이어)
        unit.AI_TakeAction(playerUnit);

        // 행동 간 딜레이 (애니메이션 볼 시간 줌)
        yield return new WaitForSeconds(1.0f);

        if (state == BattleState.Won || state == BattleState.Lost) yield break;

        if (unit != null && !actedUnits.Contains(unit))
        {
            actedUnits.Add(unit);
        }

        StartNextTurn();
    }

    void SpawnPlayer()
    {
        playerUnit = Instantiate(unitPrefab);
        playerUnit.name = "Player Unit";
        ApplySpawnOffset(playerUnit, 0, 2, EnemyType.Player);
        playerUnit.Init(0, 2);

        allUnits.Add(playerUnit);

        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        if (GameManager.Instance == null)
        {
            // 기본값으로 일반 전투 스폰 진행
            SpawnNormalEnemyAt(3, 0);
            SpawnNormalEnemyAt(3, 3);
            return;
        }

        switch (GameManager.Instance.currentNodeType)
        {
            case NodeType.Battle:
                // 일반 전투: (3,0), (3,3)에 일반 몬스터 소환
                SpawnNormalEnemyAt(3, 0);
                SpawnNormalEnemyAt(3, 3);
                break;

            case NodeType.Elite:
                // 엘리트 전투: (3,0), (3,3)에 일반 몬스터, (4,2)에 엘리트 몬스터 소환
                SpawnNormalEnemyAt(3, 0);
                SpawnNormalEnemyAt(3, 3);
                SpawnEliteEnemyAt(4, 2);
                Debug.Log("⚠️ 경고: 엘리트 몬스터 출현! (노말 x2 + 엘리트 x1)");
                break;

            case NodeType.Boss:
                // 보스 전투: (3,0), (3,3)에 엘리트 몬스터, (4,2)에 보스 몬스터 소환
                SpawnEliteEnemyAt(3, 0);
                SpawnEliteEnemyAt(3, 3);
                SpawnBossEnemyAt(4, 2);
                Debug.Log("⚠️ 경고: 보스 몬스터 출현! (엘리트 x2 + 보스 x1)");
                break;

            default:
                SpawnNormalEnemyAt(3, 0);
                SpawnNormalEnemyAt(3, 3);
                break;
        }
    }

    void SpawnNormalEnemyAt(int x, int y)
    {
        if (normalEnemyPrefabs == null || normalEnemyPrefabs.Count == 0)
        {
            Debug.LogWarning("normalEnemyPrefabs 리스트가 비어있습니다.");
            return;
        }

        Unit prefab = normalEnemyPrefabs[UnityEngine.Random.Range(0, normalEnemyPrefabs.Count)];
        Unit enemy = Instantiate(prefab);
        enemy.name = "Normal Enemy";

        ApplySpawnOffset(enemy, x, y, EnemyType.Normal);
        enemy.Init(x, y);

        allUnits.Add(enemy);
    }

    void SpawnEliteEnemyAt(int x, int y)
    {
        if (eliteEnemyPrefabs == null || eliteEnemyPrefabs.Count == 0)
        {
            Debug.LogWarning("eliteEnemyPrefabs 리스트가 비어있습니다.");
            return;
        }

        Unit prefab = eliteEnemyPrefabs[UnityEngine.Random.Range(0, eliteEnemyPrefabs.Count)];
        Unit enemy = Instantiate(prefab);
        enemy.name = "Elite Enemy";

        ApplySpawnOffset(enemy, x, y, EnemyType.Elite);
        enemy.Init(x, y);

        allUnits.Add(enemy);
    }

    void SpawnBossEnemyAt(int x, int y)
    {
        if (bossEnemyPrefabs == null || bossEnemyPrefabs.Count == 0)
        {
            Debug.LogWarning("bossEnemyPrefabs 리스트가 비어있습니다.");
            return;
        }

        Unit prefab = bossEnemyPrefabs[UnityEngine.Random.Range(0, bossEnemyPrefabs.Count)];
        Unit enemy = Instantiate(prefab);
        enemy.name = "Boss Enemy";

        ApplySpawnOffset(enemy, x, y, EnemyType.Boss);
        enemy.Init(x, y);

        allUnits.Add(enemy);
    }

    void ApplySpawnOffset(Unit unit, int x, int y, EnemyType tier)
    {
        // 1. 프리팹 자체에 설정된 기본 오프셋을 '베이스'로 유지합니다.
        // 2. 만약 특정 좌표(x, y)와 해당 몬스터 등급(tier)에 대한 설정이 있다면 더해줍니다.
        if (spawnPositionOffsets == null) return;

        foreach (var config in spawnPositionOffsets)
        {
            if (config.gridX == x && config.gridY == y && config.enemyType == tier)
            {
                unit.tilePositionOffset += config.offset;
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
            if (GameManager.Instance != null && GameManager.Instance.currentPP < card.ppCost)
            {
                Debug.LogWarning($"PP가 부족합니다! (필요: {card.ppCost}, 현재: {GameManager.Instance.currentPP})");
                // TODO: 텍스트가 빨간색으로 흔들리는 등의 UI 피드백을 주면 좋습니다.
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentPP -= card.ppCost;
            }
            actionSlots.Add(card);
            Debug.Log($"슬롯에 카드 등록됨: {card.cardName} (남은 PP: {GameManager.Instance.currentPP})");

            if (SoundManager.Instance != null && SoundManager.Instance.cardAttachSFX != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.cardAttachSFX);
            }

            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.RemoveCardFromHand(card);
            }

            BattleUIManager.Instance.UpdateActionSlotUI(actionSlots);
            BattleUIManager.Instance.UpdatePPUI();
        }
        else
        {
            Debug.Log("슬롯이 가득 찼습니다!");
        }
    }

    public void RemoveCardFromSlot(CardData card)
    {
        if (actionSlots.Contains(card))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentPP = Mathf.Min(GameManager.Instance.currentPP + card.ppCost, GameManager.Instance.maxPP);
            }
            actionSlots.Remove(card);

            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.ReturnCardToHand(card);
            }

            BattleUIManager.Instance.UpdateActionSlotUI(actionSlots);
            BattleUIManager.Instance.UpdatePPUI();
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
            seq.AppendCallback(() => ChargeUlt(card.ultCharge));
            seq.AppendInterval(0.6f);
        }

        seq.OnComplete(() =>
        {
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

        if (SoundManager.Instance != null && SoundManager.Instance.potionUseSFX != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.potionUseSFX);
        }

        switch (item.effectType)
        {
            case ItemEffectType.HealHP:
                playerUnit.Heal(item.value);
                Debug.Log($"플레이어의 HP를 {item.value} 만큼 회복. 현재 HP: {playerUnit.currentHP}/{playerUnit.maxHP}");

                if (healItemVFXPrefab != null && playerUnit != null)
                {
                    playerUnit.SpawnVFX(healItemVFXPrefab, playerUnit.transform.position, playerUnit.transform, 1.2f);
                }
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
            case ItemEffectType.IncreaseStr:
                playerUnit.damageMultiplier += 0.1f;
                if (playerUnit.increaseStrVFXPrefab != null && playerUnit != null)
                {
                    playerUnit.SpawnVFX(playerUnit.increaseStrVFXPrefab, playerUnit.transform.position, playerUnit.transform, 1.2f);
                }
                Debug.Log($"플레이어의 공격 카드 데미지가 10% 증가합니다. (전투 종료 시까지 지속)");
                break;
            case ItemEffectType.MaxMovePoints:
                playerUnit.ApplyMoveBuff(item.value, 2);
                Debug.Log($"플레이어의 이동 횟수가 {item.value} 만큼 증가합니다. (2턴 지속)");
                break;
        }

        // 체력바 UI 업데이트
        playerUnit.UpdateHPBar();
    }

    public void ChargeUlt(int amount)
    {
        if (GameManager.Instance == null || amount <= 0) return;

        GameManager.Instance.currentUlt += amount;
        if (GameManager.Instance.currentUlt > GameManager.Instance.maxUlt)
            GameManager.Instance.currentUlt = GameManager.Instance.maxUlt;

        Debug.Log($"⚡ SP 충전! (+{amount}) 현재 게이지: {GameManager.Instance.currentUlt}%");

        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.UpdateUltUI();
    }

    public void OnClickUltimateButton()
    {
        if (state != BattleState.PlayerTurn)
        {
            Debug.Log("지금은 궁극기를 사용할 수 없는 상태입니다!");
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.currentUlt < GameManager.Instance.maxUlt)
        {
            Debug.LogWarning("궁극기 게이지가 부족합니다!");
            return;
        }

        if (playerUnit == null || playerUnit.ultimateSkillCard == null)
        {
            Debug.LogError($"🚨 {playerUnit.unitName}에게 장착된 궁극기 데이터가 없습니다!");
            return;
        }

        GameManager.Instance.currentUlt = 0;
        if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdateUltUI();

        Debug.Log($"🔥 궁극기 발동!! [{playerUnit.unitName}]의 <{playerUnit.ultimateSkillCard.cardName}>!!");

        Sequence ultSeq = DOTween.Sequence();
        playerUnit.PerformAction(playerUnit.ultimateSkillCard, ultSeq, true);
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
            AnchorGridManager.Instance.HighlightAttackRange(playerUnit.gridX, playerUnit.gridY, new List<Vector2Int> { Vector2Int.zero }, isLeft);
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
        if (turnQueue.Contains(unit))
            turnQueue.Remove(unit);
        if (actedUnits.Contains(unit))
            actedUnits.Remove(unit);

        UpdateUnitTransparencies();

        if (TurnOrderUI.Instance != null)
        {
            TurnOrderUI.Instance.Refresh(turnQueue, currentTurnUnit, actedUnits);
        }

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

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundManager.Instance.victorySFX);

        if (GameManager.Instance != null && playerUnit != null)
        {
            GameManager.Instance.currentHP = playerUnit.currentHP;
            Debug.Log($"💾 체력 저장 완료: {GameManager.Instance.currentHP}");
        }

        Debug.Log("🎉 VICTORY! 모든 적을 처치했습니다.");

        if (GameManager.Instance != null)
        {
            int spReward;
            if (GameManager.Instance.currentNodeType == NodeType.Elite)
                spReward = 2;
            else if (GameManager.Instance.currentNodeType == NodeType.Boss)
                spReward = 3;
            else
                spReward = 1;

            int spBeforeReward = GameManager.Instance.currentSp;
            GameManager.Instance.currentSp = Mathf.Min(GameManager.Instance.currentSp + spReward, GameManager.Instance.maxSp);
            RecordSpEarned(GameManager.Instance.currentSp - spBeforeReward);

            goldReward = 0;
            if (GameManager.Instance.currentNodeType == NodeType.Elite)
                goldReward = 50; // 엘리트 전투 보상
            else if (GameManager.Instance.currentNodeType == NodeType.Boss)
                goldReward = 100; // 보스 전투 보상
            else
                goldReward = 20; // 일반 전투 보상

            GameManager.Instance.gold += goldReward;
            RecordGoldEarned(goldReward);
        }

        BattleUIManager.Instance.ShowResultUI(true);
        BattleUIManager.Instance.resultImage[0].SetActive(true);
        // [수정] 결과창의 '확인/다음' 버튼이 누를 때 CompleteStage가 실행되도록 해야 함.
        // BattleUIManager의 OnRestartButton(또는 OnNextButton)을 수정해야 합니다.
    }

    private void OnGameOver()
    {
        isBattleEnded = true;
        state = BattleState.Lost;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundManager.Instance.defeatSFX);

        Debug.Log("💀 GAME OVER... 플레이어가 사망했습니다.");

        BattleUIManager.Instance.ShowResultUI(false);
        BattleUIManager.Instance.resultImage[1].SetActive(true);

    }

    // TODO_juwan: 배틀 통계 기능 추가
    public void RecordDamageDeal(int amount)
    {
        totalDamageDeal += amount;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RecordRunDamageDealt(amount);
        }
    }

    public void RecordDamageTaken(int amount)
    {
        totalDamageTaken += amount;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RecordRunDamageTaken(amount);
        }
    }

    public void RecordDamageBlocked(int amount)
    {
        if (amount <= 0) return;
        totalDamageBlocked += amount;
    }

    public void RecordGoldEarned(int amount)
    {
        if (amount <= 0) return;

        totalGoldEarned += amount;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RecordRunGoldEarned(amount);
        }
    }

    public void RecordSpEarned(int amount)
    {
        if (amount <= 0) return;

        totalSpEarned += amount;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RecordRunSpEarned(amount);
        }
    }

    private void ResetBattleStatistics()
    {
        totalDamageDeal = 0;
        totalDamageTaken = 0;
        totalDamageBlocked = 0;
        totalGoldEarned = 0;
        totalSpEarned = 0;
    }

    public void UpdateUnitTransparencies()
    {
        foreach (var unit in allUnits)
        {
            if (unit == null || !unit.gameObject.activeInHierarchy || unit.currentHP <= 0) continue;

            bool isObscuring = false;
            foreach (var other in allUnits)
            {
                if (other == unit || other == null || !other.gameObject.activeInHierarchy || other.currentHP <= 0) continue;

                // 같은 열(X축)에 있으면서 바로 뒤(Y가 1만큼 큼)에 존재한다면, 실제로 시각적으로 겹친다고 판별
                if (other.gridX == unit.gridX && other.gridY > unit.gridY && (other.gridY - unit.gridY) <= 1)
                {
                    isObscuring = true;
                    break;
                }
            }

            unit.SetTransparency(isObscuring ? 0.4f : 1f);
        }
    }
}
