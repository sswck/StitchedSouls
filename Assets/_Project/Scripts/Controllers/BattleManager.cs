using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;

public enum BattleState { Start, PlayerTurn, EnemyTurn, Won, Lost }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Game State")]
    public BattleState state;

    [Header("Slot System")]
    public List<CardData> deck = new List<CardData>();
    public List<CardData> handDeck;
    public List<CardData> actionSlots = new List<CardData>();

    public Unit playerUnit;

    [Header("Spawn Settings")]
    public Unit unitPrefab;
    public Unit eliteEnemyPrefab;

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

        StartCoroutine(SetupBattle());

        // (지금은 드로우 기능이 없으므로, 가져온 덱을 전부 손패로 보여줌)
        handDeck = new List<CardData>(deck);

        BattleUIManager.Instance.UpdateHandUI(handDeck);
        BattleUIManager.Instance.UpdateActionSlotUI(new List<CardData>());

        // BGM 재생 로직 추가
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

            // 1. 덱 복사 (MasterDeck -> BattleDeck)
            deck = new List<CardData>(GameManager.Instance.masterDeck);

            // 2. 체력 동기화 (SpawnPlayer에서 초기화된 체력을 현재 체력으로 덮어씀)
            if (playerUnit != null)
            {
                playerUnit.maxHP = GameManager.Instance.maxHP;
                playerUnit.currentHP = GameManager.Instance.currentHP;
                
                playerUnit.UpdateHPBar();
            }
        }
        else
        {
            // 데이터가 없으면(테스트 실행) Inspector에 넣어둔 handDeck을 그대로 사용
            Debug.Log("⚠️ 저장된 데이터가 없어 Inspector의 HandDeck을 사용합니다.");
            deck = new List<CardData>(handDeck);
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
        
        // 플레이어 유닛들의 상태 리셋 (이동력 회복 등)
        if(playerUnit != null) playerUnit.OnTurnStart();
    }

    public void EndPlayerTurn()
    {
        if (state == BattleState.Won || state == BattleState.Lost)
        {
            return;
        }

        Debug.Log("플레이어 턴 종료...");
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
        playerUnit.Init(1, 1);

        allUnits.Add(playerUnit);

        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        Unit enemyToSpawn = unitPrefab; // 기본값

        if (GameManager.Instance != null)
        {
            switch (GameManager.Instance.currentNodeType)
            {
                case NodeType.Elite:
                    if (eliteEnemyPrefab != null) enemyToSpawn = eliteEnemyPrefab;
                    Debug.Log("⚠️ 경고: 엘리트 몬스터 출현!");
                    break;
                case NodeType.Boss:
                    // if (bossEnemyPrefab != null) enemyToSpawn = bossEnemyPrefab;
                    Debug.Log("디버그 보스 스폰 지점");
                    break;
                // 일반 Battle, Boss은 기본값 유지
            }
        }

        Unit enemy = Instantiate(enemyToSpawn);
        enemy.name = (GameManager.Instance.currentNodeType == NodeType.Elite) ? "Elite Enemy" : "Normal Enemy";

        // (선택 사항) 엘리트는 좀 더 뒤쪽에 배치하고 싶다면?
        // int spawnX = (GameManager.Instance.currentNodeType == NodeType.Elite) ? 2 : 1;
        enemy.Init(1, 3);
        
        allUnits.Add(enemy);
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

    // UI 버튼이나 키보드 입력으로 호출할 함수: 슬롯에 카드 등록
    public void AddCardToSlot(CardData card)
    {
        if (actionSlots.Count < 3) // 슬롯이 3개라고 가정 추후 3을 상수변수로 변경
        {
            actionSlots.Add(card);
            Debug.Log($"슬롯에 카드 등록됨: {card.cardName}");

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
