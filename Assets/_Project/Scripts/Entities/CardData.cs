using UnityEngine;
using System.Collections.Generic;

public enum CardType
{
    Attack,
    Defense,
    Skill
}

public enum TargetType
{
    Pattern,    // 그리드 패턴 (일반적인 행동 범위)
    Self,       // 나 자신 (방어도 등)
    AllEnemies, // 적 전체 (광역기, 실명 등)
    AllAllies   // 아군 전체 (힐, PP 회복)
}

public enum AttackVFXType
{
    Default,
    NormalAttack,
    AllAttack,
    Ultimate
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
    public TargetType targetType;

    [Header("Cost")]
    public int ppCost = 0;

    [Header("Recovery Effects")]
    public int healHP = 0;
    public int healPP = 0;

    [Header("Delayed Effects (Next Turn)")]
    public int nextTurnHealHP = 0;
    public int nextTurnHealPP = 0;

    [Header("Defense Effects")]
    public int block = 0;
    public bool isMissingHPBlock = false;   // true일 경우 잃은 체력만큼 방어도

    [Header("Damage")]
    public int value;

    [Header("Ultimate System")]
    public int ultCharge = 0;

    [Header("Pattern Attack Settings")]
    public List<Vector2Int> targetPattern;

    [Header("Visuals")]
    public Sprite cardImage;
    public AttackVFXType attackVFXType = AttackVFXType.Default;
}
