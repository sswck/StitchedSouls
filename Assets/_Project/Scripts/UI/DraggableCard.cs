using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentToReturnTo = null;
    public CardData cardData;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    public bool IsDragging { get; private set; } = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnDisable()
    {
        // 오브젝트가 파괴되거나 비활성화될 때(예: 슬롯 등록 시 UI 갱신) 가이드라인 제거
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.StopPreviewRange();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //  Debug.Log("드래그 시작!");
        IsDragging = true;
        
        // [추가] 드래그 시작 시 호버 효과 즉시 초기화
        if (TryGetComponent<CardHoverEffect>(out var hoverEffect))
        {
            hoverEffect.ResetToDefaultImmediate();
        }
        
        parentToReturnTo = transform.parent;
        transform.SetParent(BattleUIManager.Instance.dragLayer);
        
        canvasGroup.blocksRaycasts = false; 
        canvasGroup.alpha = 0.6f;

        if (BattleManager.Instance != null && cardData != null)
        {
            BattleManager.Instance.PreviewCardRange(cardData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)BattleUIManager.Instance.dragLayer, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //  Debug.Log("드래그 종료!");
        IsDragging = false;
        
        canvasGroup.blocksRaycasts = true; // 다시 터치 가능하게 복구
        canvasGroup.alpha = 1.0f; // 투명도 복구

        // (나중에 여기에 '슬롯에 넣었나?' 확인하는 로직 추가 예정)
        // 지금은 무조건 원래 자리로 돌아오게 함
        transform.SetParent(parentToReturnTo);
        
        // 부모의 레이아웃(CardFanLayout)이 있다면 갱신하여 드래그 후 위치를 올바르게 잡습니다.
        if (parentToReturnTo != null && parentToReturnTo.TryGetComponent<CardFanLayout>(out var layout))
        {
            layout.UpdateLayout();
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.StopPreviewRange();
        }
    }
}
