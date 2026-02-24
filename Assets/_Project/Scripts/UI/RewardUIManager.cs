using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RewardUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rewardPanel;
    public Button[] cardButtons;
    public Button skipButton;

    private List<CardData> currentOptions = new List<CardData>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkip);
        }
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

        List<CardData> tempPool = new List<CardData>(pool);
        int countToDraw = Mathf.Min(3, tempPool.Count);

        for (int i = 0; i < 3; i++)
        {
            if (i < countToDraw)
            {
                // 랜덤으로 한 장 뽑기
                int randomIndex = Random.Range(0, tempPool.Count);
                CardData selectedCard = tempPool[randomIndex];
                currentOptions.Add(selectedCard);
                
                // [핵심] 뽑은 카드는 임시 리스트에서 제거하여 중복 방지
                tempPool.RemoveAt(randomIndex); 

                // 버튼 활성화 및 텍스트 설정
                cardButtons[i].gameObject.SetActive(true);
                TextMeshProUGUI btnText = cardButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = selectedCard.cardName;

                int choiceIndex = i; 
                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => OnCardSelected(choiceIndex));
            }
            else
            {
                // 뽑을 카드가 부족하면 남은 버튼은 숨김 처리
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnCardSelected(int index)
    {
        CardData chosenCard = currentOptions[index];
        GameManager.Instance.masterDeck.Add(chosenCard);
        Debug.Log($"🎁 보상 획득: {chosenCard.cardName}! (현재 덱: {GameManager.Instance.masterDeck.Count}장)");

        CloseRewardAndProceed();
    }

    public void OnSkip()
    {
        Debug.Log("⏩ 보상을 건너뛰었습니다.");
        CloseRewardAndProceed();
    }

    private void CloseRewardAndProceed()
    {
        rewardPanel.SetActive(false);
        GameManager.Instance.CompleteStage();
    }
}
