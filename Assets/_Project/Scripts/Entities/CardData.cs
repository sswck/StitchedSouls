using UnityEngine;

public enum CardType
{
    Attack,
    Defense,
    Skill
}

[CreateAssetMenu(fileName = "New Card", menuName = "StitchedSouls/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Logic")]
    public CardType cardType;
    public int ppCost;
    public int range;

    public int value;

    public int pushPower = 0;
}
