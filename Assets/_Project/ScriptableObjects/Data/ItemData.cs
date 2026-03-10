using UnityEngine;

public enum ItemEffectType
{
    HealHP,
    HealSP,
    DamageBuff,
    MaxMovePoints,
}

[CreateAssetMenu(fileName = "New Item", menuName = "Stitched Souls/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    public Sprite icon;
    [TextArea]
    public string description;
   
    public int price;

    [Header("Effect")]
    public ItemEffectType effectType;
     public string effect;
    public int value;
}
