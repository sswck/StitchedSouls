using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentToReturnTo = null;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 시작!");
        
        parentToReturnTo = transform.parent; // 원래 부모(Panel_Hand) 기억하기
        
        transform.SetParent(BattleUIManager.Instance.dragLayer);
        
        // 드래그 중에는 마우스가 이 카드를 통과해서 뒤에 있는 슬롯을 감지해야 함
        canvasGroup.blocksRaycasts = false; 
        canvasGroup.alpha = 0.6f; // 약간 투명하게 연출
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치로 이동
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 종료!");
        
        canvasGroup.blocksRaycasts = true; // 다시 터치 가능하게 복구
        canvasGroup.alpha = 1.0f; // 투명도 복구

        // (나중에 여기에 '슬롯에 넣었나?' 확인하는 로직 추가 예정)
        // 지금은 무조건 원래 자리로 돌아오게 함
        transform.SetParent(parentToReturnTo);

        // 위치 초기화 (슬롯 중앙에 예쁘게 맞추기 위함)
        rectTransform.localPosition = Vector3.zero;
    }
}
