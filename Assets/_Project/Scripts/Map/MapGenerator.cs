using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public static MapData GenerateMap()
    {
        MapData map = new MapData();

        // 좌표를 전달해주신 원본 mockup 이미지와 거의 일치하게 세팅
        Vector2 pos0 = new Vector2(-320, -240);   // Battle (좌하단)
        MapNodeData node0 = new MapNodeData(0, NodeType.Battle, pos0);

        Vector2 pos1 = new Vector2(220, -80);    // Elite (우측)
        MapNodeData node1 = new MapNodeData(1, NodeType.Elite, pos1);

        Vector2 pos2 = new Vector2(10, 40);       // Shop (중앙 부근)
        MapNodeData node2 = new MapNodeData(2, NodeType.Shop, pos2);

        Vector2 pos3 = new Vector2(-400, 270);    // Boss (좌상단, Battle과 동일한 X 선상)
        MapNodeData node3 = new MapNodeData(3, NodeType.Boss, pos3);

        // --- 엣지 라우팅 구조 (정확히 원본 이미지의 선 경로를 모방) ---

        // 1. Battle -> Elite 경로 (우측으로 가다가 위로)
        Vector2 corner0_1 = new Vector2(pos1.x, pos0.y);
        node0.outEdges.Add(new MapEdgeData(1, new List<Vector2> { corner0_1 }));

        // 2. Elite -> Shop 경로 (위로 가다가 좌측으로)
        Vector2 corner1_2 = new Vector2(pos1.x, pos2.y);
        node1.outEdges.Add(new MapEdgeData(2, new List<Vector2> { corner1_2 }));

        // 3. Shop -> Boss 경로 (좌측으로 가다가 위로)
        Vector2 corner2_3 = new Vector2(pos3.x, pos2.y); // pos3.x는 pos0.x와 동일하므로 수직 일직선이 생김
        node2.outEdges.Add(new MapEdgeData(3, new List<Vector2> { corner2_3 }));

        // 데이터 등록
        map.nodes.Add(node0);
        map.nodes.Add(node1);
        map.nodes.Add(node2);
        map.nodes.Add(node3);

        return map;
    }
}
