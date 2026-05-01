using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapEdgeData
{
    public int targetId;
    public List<Vector2> corners; // 명시적 꺾임(코너) 지점 리스트

    public MapEdgeData(int targetId, List<Vector2> corners = null)
    {
        this.targetId = targetId;
        this.corners = corners ?? new List<Vector2>();
    }
}

[System.Serializable]
public class MapNodeData
{
    public int id;
    public NodeType type;
    public Vector2 position;
    public NodeStatus status;
    public List<MapEdgeData> outEdges = new List<MapEdgeData>(); // 다음 노드로 향하는 엣지 목록

    public MapNodeData(int id, NodeType type, Vector2 position)
    {
        this.id = id;
        this.type = type;
        this.position = position;
        this.status = NodeStatus.Locked;
    }
    
    // 유틸: ID 기반 연결 여부 반환
    public bool HasConnectionTo(int targetId)
    {
        return outEdges.Exists(e => e.targetId == targetId);
    }
}

[System.Serializable]
public class MapData
{
    public List<MapNodeData> nodes = new List<MapNodeData>();

    public MapNodeData GetNode(int id)
    {
        return nodes.Find(n => n.id == id);
    }
}
