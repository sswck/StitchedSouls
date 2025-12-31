using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "StitchedSouls/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    [TextArea] public string description;
    public Sprite icon; // 카드 아이콘 (나중에 아티스트가 주면 넣기)

    [Header("Logic")]
    public int ppCost;
    public int range;
    // 나중에 여기에 '효과 타입(공격/방어/스킬 등)' Enum을 추가할 예정

    public int pushPower = 0;
}
