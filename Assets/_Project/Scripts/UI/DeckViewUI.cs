using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DeckViewUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;      // 팝업 전체 패널 (기본 비활성화)
    public Transform cardContainer;    // Scroll View의 Content (Grid Layout Group)


    public GameObject cardPrefab; // 덱에 표시할 카드 프리팹 (DeckViewCardUI 부착됨)

    private List<GameObject> activeCardUIs = new List<GameObject>();

    private void Start()
    {
        // [수정] 이 부분을 과감하게 주석 처리하거나 삭제하세요!
        // 시작 시 팝업 닫기는 코드(Start)로 하지 말고, 유니티 에디터 인스펙터 창에서 
        // 팝업 패널(Popup Panel) 오브젝트 자체의 체크박스를 해제해서 기본적으로 꺼두는 것이 정석입니다.
        
        // if (popupPanel != null)
        //     popupPanel.SetActive(false);
    }

    /// <summary>
    /// 유니티 에디터 버튼(On Click)에서 기본 호출용 (매개변수 없음)
    /// </summary>
    public void OpenDeckView()
    {
        // 매개변수 없이 호출되면 아래 함수에 null을 넘겨서 기본 로직을 타게 합니다.
        OpenDeckView(null); 
    }

    /// <summary>
    /// 버튼 클릭 이벤트 등에서 호출하여 덱 확인 팝업을 엽니다.
    /// </summary>
    public void OpenDeckView(List<CardData> customDeck)  // [수정] 매개변수(customDeck)를 받을 수 있도록 오버로딩 추가
    {
        if (popupPanel == null) return;

        // 팝업 활성화
        popupPanel.SetActive(true);

        // 기존 생성된 카드 UI 제거
        ClearExistingCards();

        // 현재 덱 가져오기 (전투 중인지 맵 상인지에 따라 다르게 설정 가능)
        List<CardData> deckToShow = customDeck != null ? customDeck : GetCurrentPlayerDeck();   // [수정] 매개변수로 덱을 넘겨받았다면 그걸 쓰고, 아니면 기존 로직(GetCurrentPlayerDeck) 실행

        // 카드 타입(Attack -> Defense -> Skill) 순으로 정렬
        var sortedDeck = deckToShow
            .OrderBy(c => c.cardType)
            .ThenBy(c => c.cardName)
            .ToList();

        // 정렬된 카드들을 프리팹으로 생성하여 UI에 추가
        foreach (var cardData in sortedDeck)
        {
            GameObject go = Instantiate(cardPrefab, cardContainer);
            DeckViewCardUI cardUI = go.GetComponent<DeckViewCardUI>();
            if (cardUI != null)
            {
                cardUI.SetCard(cardData);
            }
            activeCardUIs.Add(go);
        }
    }

    /// <summary>
    /// 팝업 닫기 버튼 클릭 시 호출합니다.
    /// </summary>
    public void CloseDeckView()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        ClearExistingCards();
    }

    private void ClearExistingCards()
    {
        foreach (var card in activeCardUIs)
        {
            if (card != null) Destroy(card);
        }
        activeCardUIs.Clear();
    }

    /// <summary>
    /// 상황에 맞는 현재 플레이어 덱을 반환합니다.
    /// </summary>
    private List<CardData> GetCurrentPlayerDeck()
    {
        // 1. 전투 중인 경우 (뽑기 더미(Draw Pile)에 남은 카드만 표시 - 손패/무덤 제외)
        if (DeckManager.Instance != null && BattleManager.Instance != null)
        {
            return new List<CardData>(DeckManager.Instance.drawPile);
        }
        // 2. 맵 이동 중이거나 기본 상태 (GameManager의 마스터 덱 전체 표시)
        else if (GameManager.Instance != null)
        {
            return GameManager.Instance.masterDeck;
        }

        return new List<CardData>();
    }
}
