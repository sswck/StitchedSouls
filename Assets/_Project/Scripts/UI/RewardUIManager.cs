using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RewardUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rewardPanel;
    public Button[] cardButtons;

    private List<CardData> currentOptions = new List<CardData>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    public void ShowReward()
    {
        rewardPanel.SetActive(true);
        GenerateChoices();
    }

    void GenerateChoices()
    {
        currentOptions.Clear();
        List<CardData> pool = GameManager.Instance.allAvailableCards;

        if (pool == null || pool.Count == 0)
        {
            Debug.LogError("GameManager에 보상용 카드 풀(allAvailableCards)이 비어있습니다!");
            return;
        }

        // 3장 랜덤 뽑기 (MVP 버전: 중복 허용)
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            CardData selectedCard = pool[randomIndex];
            currentOptions.Add(selectedCard);

            // 버튼의 텍스트를 카드 이름으로 변경
            TextMeshProUGUI btnText = cardButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = selectedCard.cardName;

            // 버튼 클릭 이벤트 연결 (기존 연결 지우고 새로 연결)
            int choiceIndex = i; // 클로저(Closure) 이슈 방지를 위해 지역 변수에 저장
            cardButtons[i].onClick.RemoveAllListeners();
            cardButtons[i].onClick.AddListener(() => OnCardSelected(choiceIndex));
        }
    }

    void OnCardSelected(int index)
    {
        CardData chosenCard = currentOptions[index];
        
        // 1. 마스터 덱에 카드 추가
        GameManager.Instance.masterDeck.Add(chosenCard);
        Debug.Log($"🎁 보상 획득: {chosenCard.cardName}! (현재 덱: {GameManager.Instance.masterDeck.Count}장)");

        // 2. 패널 닫기 및 다음 스테이지로 이동
        rewardPanel.SetActive(false);
        GameManager.Instance.CompleteStage();
    }
}
