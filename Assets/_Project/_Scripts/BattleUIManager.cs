using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("UI References")]
    public Transform handPanel;
    public Transform actionSlotPanel;
    public GameObject cardSlotPrefab;
    public GameObject emptySlotPrefab;

    [Header("Drag Layer")]
    public Transform dragLayer;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateHandUI(List<CardData> handDeck)
    {
        // 1. 기존 UI 싹 지우기 (초기화)
        foreach (Transform child in handPanel) Destroy(child.gameObject);

        // 2. 카드 개수만큼 새로 생성
        foreach (CardData card in handDeck)
        {
            GameObject newSlot = Instantiate(cardSlotPrefab, handPanel);

            // 텍스트 변경 (프리팹 구조에 따라 경로가 다를 수 있음. GetComponentInChildren 사용)
            TextMeshProUGUI text = newSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = card.cardName;
            
            DraggableCard draggable = newSlot.GetComponent<DraggableCard>();
            if (draggable != null)
            {
                draggable.cardData = card;
            }
        }
    }

    public void UpdateActionSlotUI(List<CardData> actionSlots)
    {
        // 1. 기존 슬롯 지우기
        foreach (Transform child in actionSlotPanel)
        {
            Destroy(child.gameObject);
        }

        int maxSlots = 3;   // 나중에 리팩토링할 것

        for (int i = 0; i < maxSlots; i++)
        {
            if (i < actionSlots.Count)
            {
                // [CASE A] 리스트에 카드가 있는 경우 -> 카드 슬롯 생성
                CardData card = actionSlots[i];
                GameObject newSlot = Instantiate(cardSlotPrefab, actionSlotPanel);
                
                // 텍스트 및 데이터 설정
                TextMeshProUGUI text = newSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = card.cardName;

                // (중요) 슬롯에 들어간 카드는 더 이상 드롭을 받지 않거나, 
                // 혹은 클릭해서 뺄 수 있어야 함. 일단은 드래그 기능만 넣어둠.
                // 이미 장착된 카드는 드래그 불가능하게 하려면 DraggableCard를 꺼도 됨.
            }
            else
            {
                // [CASE B] 리스트에 카드가 없는 경우 -> 빈 슬롯 생성
                // 빈 슬롯 프리팹에는 'ActionSlot' 스크립트가 있어서 드롭을 받을 수 있음
                Instantiate(emptySlotPrefab, actionSlotPanel);
            }
        }
    }
}
