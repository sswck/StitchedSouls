using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [System.Serializable]
    public struct NodeData
    {
        public NodeType type;
        public Vector2 position; // 인스펙터에서 설정할 UI 좌표
    }

    [Header("Map Settings")]
    public Transform contentParent;
    public MapNode nodePrefab;
    public MapNodeSpriteConfig spriteConfig;
    
    [Header("Path Design")]
    // [중요] 이제 인스펙터에서 NodeType과 Position을 함께 설정하세요.
    public List<NodeData> fixedMapPath = new List<NodeData>();

    void Start()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(SoundManager.Instance.mapBGM);

        if (GameManager.Instance.currentStageIndex >= fixedMapPath.Count)
        {
            Debug.Log("🎉 모든 스테이지 클리어! 타이틀로 이동.");
            GameManager.Instance.LoadScene("TitleScene");
            return;
        }

        GenerateMap();
    }

    void GenerateMap()
    {
        // 기존 노드 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        int currentStage = GameManager.Instance.currentStageIndex;
        List<MapNode> spawnedNodes = new List<MapNode>();
        
        // 1. 노드 생성 및 배치
        for (int i = 0; i < fixedMapPath.Count; i++)
        {
            NodeData data = fixedMapPath[i];
            NodeStatus status = GetNodeStatus(i, currentStage);

            MapNode newNode = Instantiate(nodePrefab, contentParent);
            
            // 좌표 설정 (RectTransform 사용)
            RectTransform rect = newNode.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = data.position;
            }

            newNode.Init(i, data.type, status, spriteConfig);
            spawnedNodes.Add(newNode);
        }

        // 2. 노드 간 선 연결 (이미지의 점선 효과)
        for (int i = 0; i < spawnedNodes.Count - 1; i++)
        {
            // 다음 노드의 위치를 전달하여 선을 그리게 함
            Vector2 nextPos = fixedMapPath[i + 1].position;
            spawnedNodes[i].SetLine(nextPos);
        }
    }

    NodeStatus GetNodeStatus(int index, int currentStage)
    {
        if (index < currentStage) return NodeStatus.Completed;
        if (index == currentStage) return NodeStatus.Available;
        return NodeStatus.Locked;
    }
}
