using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

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
        SpawnPlayer();
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
        enemy.GetComponent<MeshRenderer>().material.color = Color.red; 
        
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
        if (actionSlots.Count < 3) // 슬롯이 3개라고 가정
        {
            actionSlots.Add(card);
            Debug.Log($"슬롯에 카드 등록됨: {card.cardName}");
        }
        else
        {
            Debug.Log("슬롯이 가득 찼습니다!");
        }
    }

    // 턴 종료 버튼을 누르면 실행되는 함수: 시퀀스 실행
    public void ExecuteSlots()
    {
        if (actionSlots.Count == 0) return;

        Debug.Log("--- 작전 실행 시작! ---");

        // 시퀀스(Sequence)는 DoTween의 기능으로, 애니메이션을 순서대로 실행해줍니다.
        Sequence seq = DOTween.Sequence();

        foreach (var card in actionSlots)
        {
            // 카드를 하나씩 꺼내서 실행 예약
            // AppendCallback: 순서대로 함수를 호출하게 함
            seq.AppendCallback(() => 
            {
                Debug.Log($"[{card.cardName}] 실행!");

                // 카드 이름으로 행동 구분 (나중에는 Enum 타입으로 바꿀 예정)
                if (card.cardName == "공격" || card.cardName == "강타")
                {
                    playerUnit.Attack(card.pushPower);
                    playerUnit.transform.DOShakePosition(0.3f, 0.2f); // 공격하는 척 흔들기
                }
            });

            // 행동 사이 딜레이 (0.6초 대기) - 이동 애니메이션 끝날 때까지 기다림
            seq.AppendInterval(0.6f);
        }

        // 모든 행동이 끝나면 슬롯 비우기
        seq.OnComplete(() => {
            Debug.Log("--- 턴 종료 ---");
            actionSlots.Clear();

            playerUnit.currentMovePoints = playerUnit.maxMovePoints;
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ExecuteSlots(); // 스페이스바로 실행 테스트
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (handDeck.Count > 0) // 카드가 있는지 확인
            {
                AddCardToSlot(handDeck[0]);
            }
            else
            {
                Debug.Log("핸드에 1번 카드가 없습니다! (Inspector에서 Hand Deck을 채워주세요)");
            }
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            if (handDeck.Count > 1)
            {
                AddCardToSlot(handDeck[1]);
            }
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            if (handDeck.Count > 2)
            {
                AddCardToSlot(handDeck[2]);
            }
        }

        // 데모 버전: 방향키를 누르면 즉시 이동
        if (playerUnit != null && playerUnit.CanMove())
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) MovePlayer(0, 1);
            if (Keyboard.current.downArrowKey.wasPressedThisFrame) MovePlayer(0, -1);
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame) MovePlayer(-1, 0);
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) MovePlayer(1, 0);
        }
    }

    void MovePlayer(int xDir, int yDir)
    {
        playerUnit.Move(xDir, yDir);
    }
}
