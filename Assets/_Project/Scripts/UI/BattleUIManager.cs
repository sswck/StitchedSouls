using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Reflection;

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

    public TextMeshProUGUI damageDealText;
    public TextMeshProUGUI damageTakenText;
    public TextMeshProUGUI goldText;
    public Button restartButton;
    public Button titleButton;
    public List<GameObject> resultImage;

    [Header("Turn Order UI")]
    public TurnOrderUI turnOrderUI;

    [Header("Map Popup UI")]
    public GameObject mapPopup;

    [Header("Reward UI")]
    public RewardUIManager rewardUI;

    [Header("Ultimate UI")]
    public Image ultFillImage;
    public Button ultimateButton;
    public Sprite Ult_Active;
    public Sprite Ult_Inactive;
    public GameObject UltImage;

    [Header("Deck/Discard Count UI")]
    public TextMeshProUGUI deckCountText;
    public TextMeshProUGUI discardCountText;

    [Header("Movement Gauge UI")]
    public TextMeshProUGUI movementText;

    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnNextStageButton);

        if (titleButton != null)
            titleButton.onClick.AddListener(OnTitleButtonClick);

        // 시작할 땐 결과창 끄기
        if (resultPanel != null) resultPanel.SetActive(false);

        UpdatePPUI();
        UpdateDeckAndDiscardCountUI();
        UpdateMovementUI();
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
            BindCardSlot(newSlot, card);

            // 🖼️ [수정] 카드 이미지 설정 (프리팹 최상단에서 직접 Image 컴포넌트 가져오기)
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

                BindCardSlot(newSlot, card);

                RectTransform cardRect = newSlot.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    cardRect.anchoredPosition = Vector2.zero;
                    cardRect.localRotation = Quaternion.identity;
                    cardRect.localScale = Vector3.one;
                }

                DraggableCard draggable = newSlot.GetComponent<DraggableCard>();
                if (draggable != null)
                {
                    Destroy(draggable);
                }

                CardHoverEffect hoverEffect = newSlot.GetComponent<CardHoverEffect>();
                if (hoverEffect != null)
                {
                    Destroy(hoverEffect);
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

    private void BindCardSlot(GameObject slot, CardData card)
    {
        DeckViewCardUI cardUI = slot.GetComponent<DeckViewCardUI>();
        if (cardUI != null)
        {
            cardUI.SetCard(card);
            return;
        }

        TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.text = card.cardName;

        Image cardImg = slot.GetComponentInChildren<Image>();
        if (cardImg != null && card.cardImage != null)
        {
            cardImg.sprite = card.cardImage;
        }
    }

    /// <summary>
    /// PP바를 갱신합니다.
    /// </summary>
    public void UpdatePPUI()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.playerUnit != null)
        {
            BattleManager.Instance.playerUnit.UpdatePPBar();
        }
    }

    /// <summary>
    /// 궁극기 UI 게이지와 버튼 상태를 갱신합니다.
    /// </summary>
    public void UpdateUltUI()
    {
        if (GameManager.Instance == null) return;

        if (ultFillImage != null)
        {
            ultFillImage.fillAmount = (float)GameManager.Instance.currentUlt / GameManager.Instance.maxUlt;
        }

        if (ultimateButton != null)
        {
            bool isReady = GameManager.Instance.currentUlt >= GameManager.Instance.maxUlt;
            ultimateButton.interactable = isReady;

            // 여유가 되시면 여기서 버튼의 색상/이펙트를 켜고 끄는 로직을 넣어도 좋습니다!
            if (GameManager.Instance.currentUlt >= GameManager.Instance.maxUlt)
                UltImage.GetComponent<Image>().sprite = Ult_Active;
            else
                UltImage.GetComponent<Image>().sprite = Ult_Inactive;


        }
    }

    public void UpdateMovementUI()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.playerUnit != null)
        {
            if (movementText != null)
            {
                movementText.text = $"{BattleManager.Instance.playerUnit.currentMovePoints}/{BattleManager.Instance.playerUnit.maxMovePoints}";
            }
        }
    }

    public void ShowResultUI(bool isWin)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true); // 패널 켜기

        // [추가] 결과 패널이 체력바(Sorting Order 500+)보다 앞에 보이도록 설정
        Canvas canvas = resultPanel.GetComponent<Canvas>();
        if (canvas == null) canvas = resultPanel.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;

        // GraphicRaycaster가 없으면 버튼 클릭이 안 될 수 있으므로 체크 후 추가
        if (resultPanel.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            resultPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();


        if (isWin)
        {

            // 승리 시 효과음 재생 (나중에 SoundManager 연결)

            // TODO_juwan: 승리 시 집계된 데이터 표시
            if (GameManager.Instance.currentNodeType == NodeType.Boss)
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

    public void OnClickDiscardPileButton()
    {
        DeckViewUI deckViewUI = Object.FindFirstObjectByType<DeckViewUI>(FindObjectsInactive.Include);

        if (DeckManager.Instance != null && deckViewUI != null)
        {
            // 무덤 리스트를 명시적으로 넘겨서 팝업 열기
            deckViewUI.OpenDeckView(DeckManager.Instance.discardPile);
        }
        else if (deckViewUI == null)    // 디버그용
        {
            Debug.LogError("DeckViewUI 스크립트를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 현재 덱과 무덤의 카드 장수를 UI에 업데이트합니다.
    /// </summary>
    public void UpdateDeckAndDiscardCountUI()
    {
        if (DeckManager.Instance == null) return;

        if (deckCountText != null)
        {
            deckCountText.text = DeckManager.Instance.drawPile.Count.ToString();
        }

        if (discardCountText != null)
        {
            discardCountText.text = DeckManager.Instance.discardPile.Count.ToString();
        }
    }

    /// <summary>
    /// Map 팝업을 열고, 최신 상태로 지도를 생성합니다.
    /// </summary>
    public void OpenMapPopup()
    {
        if (mapPopup != null)
        {
            mapPopup.SetActive(true);

            // [추가] 맵 팝업이 체력바(Sorting Order 500+)보다 앞에 보이도록 설정
            Canvas canvas = mapPopup.GetComponent<Canvas>();
            if (canvas == null) canvas = mapPopup.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;

            // GraphicRaycaster가 없으면 버튼 클릭이 안 될 수 있으므로 체크 후 추가
            if (mapPopup.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                mapPopup.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // MapManager를 찾아서 맵 다시 그리기
            MapManager mapManager = mapPopup.GetComponentInChildren<MapManager>(true);
            if (mapManager != null)
            {
                mapManager.GenerateMap();
            }
        }
        else
        {
            Debug.LogError("Map Popup 오브젝트가 Inspector에 연결되지 않았습니다!");
        }
    }
}
