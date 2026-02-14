using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    public Transform contentParent;
    public MapNode nodePrefab;
    
    // [중요] 데모에서 사용할 임시 맵 패턴 정의
    private List<NodeType> fixedMapPath = new List<NodeType>()
    {
        NodeType.Battle,
        NodeType.Battle,
        NodeType.Shop,
        NodeType.Elite
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SoundManager.Instance != null)
        SoundManager.Instance.PlayBGM(SoundManager.Instance.mapBGM);

        if (GameManager.Instance.currentStageIndex >= fixedMapPath.Count)
        {
            Debug.Log("🎉 축하합니다! 모든 스테이지를 클리어했습니다! 타이틀로 돌아갑니다...");
            GameManager.Instance.LoadScene("TitleScene");
            return;
        }

        GenerateMap();
    }

    void GenerateMap()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        int currentStage = GameManager.Instance.currentStageIndex;
        
        for (int i = 0; i < fixedMapPath.Count; i++)
        {
            NodeType type = fixedMapPath[i];
            NodeStatus status = NodeStatus.Locked;

            if (i < currentStage) status = NodeStatus.Completed;
            else if (i == currentStage) status = NodeStatus.Available;
            else status = NodeStatus.Locked;

            MapNode newNode = Instantiate(nodePrefab, contentParent);
            newNode.Init(i, type, status);
        }
    }
}
