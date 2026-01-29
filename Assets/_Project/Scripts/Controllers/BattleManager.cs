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
    public List<CardData> handDeck;
    public List<CardData> actionSlots = new List<CardData>();

    public Unit playerUnit;

    [Header("Spawn Settings")]
    public Unit unitPrefab;

    [Header("Units")]
    public List<Unit> allUnits = new List<Unit>();

    void Start()
    {
        state = BattleState.Start;
        SpawnPlayer();

        StartCoroutine(SetupBattle());

        BattleUIManager.Instance.UpdateHandUI(handDeck);
        BattleUIManager.Instance.UpdateActionSlotUI(new List<CardData>());
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
        foreach (var unit in allUnits)
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
        Unit enemy = Instantiate(unitPrefab);
        enemy.name = "Sandbag Enemy";
        // 적은 빨간색으로 표시해서 구분 (MeshRenderer 사용)
        //enemy.GetComponent<MeshRenderer>().material.color = Color.red; // 큐브일때 적을 빨갛게 표시하는 코드
        
        enemy.Init(1, 4);
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

    public void OnUnitDead(Unit deadUnit)
    {
        // 1. 플레이어가 죽었을 때 -> 패배
        if (deadUnit == playerUnit)
        {
            GameOver();
            return;
        }

        // 2. 적이 죽었을 때 -> 남은 적이 있는지 확인
        // (지금은 적 리스트를 따로 관리하지 않고 allUnits에 섞여 있으므로 간단히 체크)
        bool anyEnemyAlive = false;
        foreach (var unit in allUnits)
        {
            // 플레이어가 아니고, 살아있는(Active) 유닛이 하나라도 있다면 적이 남은 것
            if (unit != playerUnit && unit.gameObject.activeInHierarchy && unit != deadUnit)
            {
                anyEnemyAlive = true;
                break;
            }
        }

        if (!anyEnemyAlive)
        {
            Victory();
        }
    }
    
    void Victory()
    {
        state = BattleState.Won;
        Debug.Log("🎉 승리했습니다! 모든 적을 처치했습니다. 🎉");
        
        // [추가] 승리 팝업 호출
        BattleUIManager.Instance.ShowResultUI(true);
    }

    void GameOver()
    {
        state = BattleState.Lost;
        Debug.Log("😭 패배했습니다... 플레이어가 사망했습니다. 😭");

        // [추가] 패배 팝업 호출
        BattleUIManager.Instance.ShowResultUI(false);
    }
}
