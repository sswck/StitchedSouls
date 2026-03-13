using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    [Header("Result UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI damageDealText;
    public TextMeshProUGUI damageTakenText;
    public TextMeshProUGUI goldText;
    public Button restartButton;
    public Button titleButton;

    [Header("Reward UI")]
    public RewardUIManager rewardUI;

    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnNextStageButton);

        if (titleButton != null)
            titleButton.onClick.AddListener(OnTitleButtonClick);
            
        // 시작할 땐 결과창 끄기
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void Awake()
    {
        Instance = this;
    }

    public void UpdateHandUI(List<CardData> handDeck)
    {
        foreach (Transform child in handPanel) Destroy(child.gameObject);

        if (dragLayer != null)
        {
            foreach (Transform child in dragLayer) 
            {
                Destroy(child.gameObject);
            }
        }

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

        // 부채꼴 레이아웃 업데이트
        CardFanLayout fanLayout = handPanel.GetComponent<CardFanLayout>();
        if (fanLayout != null) fanLayout.UpdateLayout();
    }

    public void UpdateActionSlotUI(List<CardData> actionSlots)
    {
        foreach (Transform child in actionSlotPanel)
        {
            Destroy(child.gameObject);
        }

        int maxSlots = 3;   // 나중에 상수변수로 리팩토링할 것

        for (int i = 0; i < maxSlots; i++)
        {
            if (i < actionSlots.Count)
            {
                // [CASE A] 카드가 장착된 슬롯
                CardData card = actionSlots[i];
                GameObject newSlot = Instantiate(cardSlotPrefab, actionSlotPanel);
                
                TextMeshProUGUI text = newSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = card.cardName;

                DraggableCard draggable = newSlot.GetComponent<DraggableCard>();
                if (draggable != null)
                {
                    Destroy(draggable);
                }

                Button btn = newSlot.GetComponent<Button>();
                if (btn == null) btn = newSlot.AddComponent<Button>();

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => 
                {
                    if (BattleManager.Instance != null)
                    {
                        BattleManager.Instance.RemoveCardFromSlot(card);
                    }
                });
            }
            else
            {
                // [CASE B] 빈 슬롯
                Instantiate(emptySlotPrefab, actionSlotPanel);
            }
        }
    }

    public void ShowResultUI(bool isWin)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true); // 패널 켜기
        

        if (isWin)
        {
            resultText.text = "VICTORY!";
            resultText.color = Color.yellow;
            // 승리 시 효과음 재생 (나중에 SoundManager 연결)

            // TODO_juwan: 승리 시 집계된 데이터 표시
            if(GameManager.Instance.currentNodeType == NodeType.Elite)
            {
                damageDealText.gameObject.SetActive(true);
                damageTakenText.gameObject.SetActive(true);
              
                goldText.gameObject.SetActive(true);
                titleButton.gameObject.SetActive(true);
                restartButton.gameObject.SetActive(false);
                damageDealText.text = $"입힌 피해량: {BattleManager.Instance.totalDamageDeal}";
                damageTakenText.text = $"입은 피해량: {BattleManager.Instance.totalDamageTaken}";
                
                goldText.text = $"골드: {GameManager.Instance.gold}";
            }
            else
            {
                damageDealText.gameObject.SetActive(false);
                damageTakenText.gameObject.SetActive(false);
              
                goldText.gameObject.SetActive(false);
                titleButton.gameObject.SetActive(false);
            }
        }
        else
        {
            resultText.text = "GAME OVER";
            resultText.color = Color.red;

            //TODO_juwan: 게임 오버 시 집계된 데이터 표시
            damageDealText.gameObject.SetActive(true);
            damageTakenText.gameObject.SetActive(true);
            
            goldText.gameObject.SetActive(true);
            titleButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(false);
            damageDealText.text = $"입힌 피해량: {BattleManager.Instance.totalDamageDeal}";
        
            damageTakenText.text = $"입은 피해량: {BattleManager.Instance.totalDamageTaken}";

            
            goldText.text = $"골드: {GameManager.Instance.gold}";
        }
    }

    public void OnNextStageButton()
    {
        if (GameManager.Instance == null) return;

        if (BattleManager.Instance.state == BattleState.Won)
        {
            if (resultPanel != null) resultPanel.SetActive(false); // 결과창 끄기
            if (rewardUI != null) rewardUI.ShowReward();           // 보상창 켜기
        }
        else
        {
            // 패배했을 때는 타이틀로 돌아가거나 게임 오버 처리
            GameManager.Instance.LoadScene("TitleScene");
        }
    }

    public void OnTitleButtonClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("TitleScene");
        }
    }
}
