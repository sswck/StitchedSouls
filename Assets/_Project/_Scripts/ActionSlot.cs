using UnityEngine;
using UnityEngine.EventSystems;

public class ActionSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // 1. 드래그 중인 물체가 있는지 확인
        if (eventData.pointerDrag != null)
        {
            Debug.Log("슬롯에 무언가 드롭됨!");

            // 드래그 중인 카드 스크립트 가져오기
            DraggableCard d = eventData.pointerDrag.GetComponent<DraggableCard>();
            if (d != null)
            {
                // [핵심] "너 이제 핸드로 돌아가지 말고, 내(슬롯) 자식으로 들어와!"
                d.parentToReturnTo = this.transform;
            }
        }
    }
}
