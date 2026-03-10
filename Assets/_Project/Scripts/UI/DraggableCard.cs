using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentToReturnTo = null;
    public CardData cardData;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //  Debug.Log("드래그 시작!");
        
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
        
        canvasGroup.blocksRaycasts = true; // 다시 터치 가능하게 복구
        canvasGroup.alpha = 1.0f; // 투명도 복구

        // (나중에 여기에 '슬롯에 넣었나?' 확인하는 로직 추가 예정)
        // 지금은 무조건 원래 자리로 돌아오게 함
        transform.SetParent(parentToReturnTo);
        // 위치 초기화 (슬롯 중앙에 예쁘게 맞추기 위함)
        rectTransform.localPosition = Vector3.zero;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.StopPreviewRange();
        }
    }
}
