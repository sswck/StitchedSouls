using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "StitchedSouls/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea]
    public string itemDescription;
    public string itemEffectText;
    public Sprite itemIcon;

    [Header("Purchase Info")]
    public int itemPrice; // 아이템 가격

    // 필요하다면 아이템 타입, 실제 적용될 효과 수치 등의 필드를 추가할 수 있습니다.
    // public float effectValue;
}
