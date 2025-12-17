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
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. 카드 개수만큼 새로 생성
        foreach (CardData card in handDeck)
        {
            GameObject newSlot = Instantiate(cardSlotPrefab, handPanel);
            // 텍스트 변경 (프리팹 구조에 따라 경로가 다를 수 있음. GetComponentInChildren 사용)
            TextMeshProUGUI text = newSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = card.cardName;
            
            // (나중에 아이콘 변경 로직 등 추가)
        }
    }

    public void UpdateActionSlotUI(List<CardData> actionSlots)
    {
        // 1. 기존 슬롯 지우기
        foreach (Transform child in actionSlotPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. 등록된 카드만큼 생성
        foreach (CardData card in actionSlots)
        {
            GameObject newSlot = Instantiate(cardSlotPrefab, actionSlotPanel);
            TextMeshProUGUI text = newSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = card.cardName;
            
            // 액션 슬롯은 색상을 좀 다르게 해서 구분해볼까요? (선택)
            newSlot.GetComponent<Image>().color = Color.yellow; 
        }
    }

    public void SetupEmptySlots()
    {
        // 기존 슬롯 다 지우고
        foreach (Transform child in actionSlotPanel) Destroy(child.gameObject);

        // 빈 슬롯 3개 생성
        for (int i = 0; i < 3; i++)
        {
            Instantiate(emptySlotPrefab, actionSlotPanel);
        }
    }
}
