using UnityEngine;

/// <summary>
/// 맵 노드 타입별/상태별 스프라이트 설정.
/// Battle/Rest → Normal, Boss → Elite 사용.
/// </summary>
[CreateAssetMenu(fileName = "MapNodeSpriteConfig", menuName = "StitchedSouls/Map Node Sprite Config")]
public class MapNodeSpriteConfig : ScriptableObject
{
    [System.Serializable]
    public class NodeSpriteSet
    {
        public Sprite active;       // Available (활성화)
        public Sprite inactive01;   // Completed (클리어 완료)
        public Sprite inactive02;   // Locked (비활성화)
    }

    [Header("Normal (Battle, Rest)")]
    public NodeSpriteSet normal;

    [Header("Elite (Boss)")]
    public NodeSpriteSet elite;

    [Header("Shop")]
    public NodeSpriteSet shop;

    /// <summary>
    /// NodeType과 NodeStatus에 맞는 스프라이트 반환.
    /// Battle/Rest→Normal, Boss→Elite, Shop→Shop
    /// </summary>
    public Sprite GetSprite(NodeType type, NodeStatus status)
    {
        NodeSpriteSet set = GetSpriteSet(type);
        if (set == null) return null;

        return status switch
        {
            NodeStatus.Available => set.active,
            NodeStatus.Completed => set.inactive01,
            NodeStatus.Locked => set.inactive02,
            _ => set.inactive02
        };
    }

    NodeSpriteSet GetSpriteSet(NodeType type)
    {
        return type switch
        {
            NodeType.Battle => normal,
            NodeType.Rest => normal,
            NodeType.Elite => elite,
            NodeType.Boss => elite,
            NodeType.Shop => shop,
            _ => normal
        };
    }
}
