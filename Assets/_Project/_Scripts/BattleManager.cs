using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Slot System")]
    public List<CardData> handDeck;
    public List<CardData> actionSlots = new List<CardData>();

    public Unit playerUnit;

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
        Debug.Log("--- 작전 실행 시작! ---");
        
        // 코루틴 등을 사용해 순차적으로 실행해야 하지만, 지금은 로그로 확인
        for (int i = 0; i < actionSlots.Count; i++)
        {
            CardData card = actionSlots[i];
            Debug.Log($"[{i + 1}번 행동] {card.cardName} 발동! (PP 소모: {card.ppCost})");
            
            // 여기서 실제 유닛의 행동 함수를 호출 (나중에 구현)
            // 예: if(card.type == Move) playerUnit.MoveTo(...);
        }

        actionSlots.Clear(); // 실행 후 슬롯 비우기
        Debug.Log("--- 턴 종료 ---");
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
    }
}
