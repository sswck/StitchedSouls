using UnityEngine;
using UnityEngine.EventSystems;

public class ActionSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // 드래그 중인 카드 스크립트 가져오기
            DraggableCard d = eventData.pointerDrag.GetComponent<DraggableCard>();

            if (d != null && d.cardData != null)
            {
                //Debug.Log($"슬롯에 카드 드롭됨: {d.cardData.cardName}");

                BattleManager.Instance.AddCardToSlot(d.cardData);

                // ※ 참고: 이전 단계의 'Visual Snap' 코드(d.parentToReturnTo = this.transform)는 
                // BattleManager가 UI를 새로 그려버리면 의미가 없어지므로 없어도 됩니다.
                // 하지만 부드러운 연출을 위해 남겨둬도 상관은 없습니다.
                //d.parentToReturnTo = this.transform;
            }
        }
    }
}
