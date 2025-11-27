using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "StitchedSouls/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName; // 카드 이름 (예: 방패 밀치기)
    [TextArea] public string description;   // 설명
    public Sprite icon; // 카드 아이콘 (나중에 아티스트가 주면 넣기)

    [Header("Logic")]
    public int ppCost; // 소모 PP
    public int range; // 사거리
    // 나중에 여기에 '효과 타입(공격/방어/스킬 등)' Enum을 추가할 예정
}
