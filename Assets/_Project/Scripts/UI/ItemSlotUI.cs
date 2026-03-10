using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject emptySlotVisual;
    [SerializeField] private GameObject filledSlotVisual;

    public void UpdateSlot(ItemData item)
    {
        if (item != null)
        {
            itemIcon.sprite = item.icon;
            emptySlotVisual.SetActive(false);
            filledSlotVisual.SetActive(true);
        }
        else
        {
            emptySlotVisual.SetActive(true);
            filledSlotVisual.SetActive(false);
        }
    }

    public void PlayUseEffect()
    {
        // DOTween 연출: 살짝 흔들리고 사라지기
        transform.DOShakePosition(0.3f, 10f, 20);
        filledSlotVisual.transform.DOScale(0f, 0.2f).OnComplete(() => {
            filledSlotVisual.transform.localScale = Vector3.one;
        });
    }
}
