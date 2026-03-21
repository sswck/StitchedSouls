using UnityEngine;
using UnityEngine.UI;

public class UnitIcon : MonoBehaviour
{
    public Image iconImage;

    public void SetIcon(Unit unit, bool isActive)
    {
        if (unit == null || iconImage == null) return;

        // 유닛의 sprite 필드를 사용하여 아이콘 설정
        Sprite targetSprite = isActive ? unit.activeIcon : unit.inactiveIcon;
        
        if (targetSprite != null)
        {
            iconImage.sprite = targetSprite;
        }
        else
        {
            // 스프라이트가 없을 경우 기본적으로 이미지가 보이지 않거나 색상으로 구분 가능
            // (사용자 요구사항에 따라 달라질 수 있으나, 일단 스프라이트 설정을 우선시함)
            iconImage.color = isActive ? Color.white : new Color(1, 1, 1, 0.5f);
        }
    }

}

