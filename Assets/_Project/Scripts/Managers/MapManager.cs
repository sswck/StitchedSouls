using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    public Transform contentParent;
    public MapNode nodePrefab;
    public MapNodeSpriteConfig spriteConfig;
    public GameObject linePrefab; // 점선 1개 단위 프리팹
    public float dotSpacing = 40f; // 점선 조각 사이의 간격

    [Header("Popup Settings")]
    [Tooltip("BattleScene 등에서 팝업으로 띄울 경우 체크하세요.")]
    public bool isPopup = false;
    public float lineRotationOffset = -90f; // 점선 스프라이트 원본이 세로(|) 방향일 경우 보정용 각도

    void Start()
    {
        // 맵 좌우 스크롤 고정 (위아래만 가능하게)
        ScrollRect scrollRect = contentParent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
        }

        // [추가] 비활성화된 DeckViewUI를 찾아 즉시 카드 수 반영
        DeckViewUI deckViewUI = Object.FindFirstObjectByType<DeckViewUI>(FindObjectsInactive.Include);
        if (deckViewUI != null)
        {
            deckViewUI.RefreshCount();
        }

        // [보완] 현재 씬이 "MapScene"이거나 팝업 모드일 때 맵 생성
        bool isMapScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MapScene";

        if (isMapScene || isPopup)
        {
            // BGM과 스테이지 보스 체크는 실제 맵씬일 때만 작동
            if (isMapScene)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayBGM(SoundManager.Instance.mapBGM);

                if (GameManager.Instance.currentNodeId >= 3) // 4번 노드(Boss)까지 클리어 시
                {
                    Debug.Log("🎉 보스 격파! 타이틀로 이동.");
                    GameManager.Instance.LoadScene("TitleScene");
                    return;
                }
            }

            GenerateMap();
        }
    }

    public void GenerateMap()
    {
        // 기존 노드 및 선 객체 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        MapData mapData = GameManager.Instance.currentMapData;
        if (mapData == null)
        {
            Debug.LogError("MapData가 존재하지 않습니다!");
            return;
        }

        // 점선들을 그룹화할 컨테이너 생성
        GameObject dotContainerObj = new GameObject("DotContainer");
        dotContainerObj.layer = contentParent.gameObject.layer; // UI 레이어 등 부모와 일치시킴

        RectTransform dotContainerRect = dotContainerObj.AddComponent<RectTransform>();
        dotContainerRect.SetParent(contentParent, false);
        dotContainerRect.SetAsFirstSibling();
        // 부모(contentParent)와 완벽히 동일한 공간이 되도록 꽉 채움(Stretch)
        dotContainerRect.anchorMin = Vector2.zero;
        dotContainerRect.anchorMax = Vector2.one;
        dotContainerRect.offsetMin = Vector2.zero;
        dotContainerRect.offsetMax = Vector2.zero;

        Transform dotContainer = dotContainerObj.transform;

        Dictionary<int, MapNode> spawnedNodes = new Dictionary<int, MapNode>();

        // 1. 점(노드) 생성 및 배치
        foreach (var nodeData in mapData.nodes)
        {
            MapNode newNode = Instantiate(nodePrefab, contentParent);

            NodeStatus status = GetNodeStatus(nodeData);
            newNode.Init(nodeData.id, nodeData.type, status, spriteConfig);

            RectTransform rect = newNode.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = nodeData.position;
            }

            spawnedNodes.Add(nodeData.id, newNode);
        }

        // 2. 꺾인(직각) 코너 점들을 거치는 선 연결
        if (linePrefab == null)
        {
            Debug.LogError("MapManager: linePrefab이 할당되지 않았습니다! Inspector에서 프리팹을 연결해주세요.");
        }

        int globalDotSequence = 0; // 점선 DOTween 연출용 순서 전역 인덱스
        foreach (var kvp in spawnedNodes)
        {
            MapNodeData sourceData = mapData.GetNode(kvp.Key);
            MapNode sourceNode = kvp.Value;

            foreach (MapEdgeData edge in sourceData.outEdges)
            {
                if (spawnedNodes.TryGetValue(edge.targetId, out MapNode targetNode))
                {
                    DrawEdgeCoords(sourceNode.GetComponent<RectTransform>().anchoredPosition, targetNode.GetComponent<RectTransform>().anchoredPosition, edge, dotContainer, ref globalDotSequence);
                }
            }

            // 노드들이 점선들보다 화면에 앞으로 보이도록 계층 정렬
            sourceNode.transform.SetAsLastSibling();
        }
    }

    private void DrawEdgeCoords(Vector2 startPos, Vector2 endPos, MapEdgeData edge, Transform dotContainer, ref int globalDotSequence)
    {
        if (linePrefab == null) return;

        List<Vector2> pathPoints = new List<Vector2>();
        pathPoints.Add(startPos);
        if (edge.corners != null) pathPoints.AddRange(edge.corners);
        pathPoints.Add(endPos);

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            DrawDotLine(pathPoints[i], pathPoints[i + 1], dotContainer, ref globalDotSequence);
        }
    }

    private void DrawDotLine(Vector2 start, Vector2 end, Transform dotContainer, ref int globalDotSequence)
    {
        float dist = Vector2.Distance(start, end);
        int dotCount = Mathf.FloorToInt(dist / dotSpacing);

        // 선분의 방향 각도 계산 (가로/세로 방향에 맞게 점선 이미지 회전)
        // 원본 프리팹 이미지가 세로 방향일 경우를 위해 lineRotationOffset(-90 등)을 더해줍니다.
        Vector2 dir = (end - start).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + lineRotationOffset;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 시작점과 도착점(코너) 정중앙에 겹치지 않게 하기 위해 i < dotCount 로 설정합니다.
        // 이렇게 하면 꺾이는 모서리 부분이 살짝 비워져 훨씬 자연스러운 이음새가 만들어집니다.
        for (int i = 1; i < dotCount; i++)
        {
            float t = (float)i / dotCount;
            Vector2 pos = Vector2.Lerp(start, end, t);

            GameObject dot = Instantiate(linePrefab, dotContainer);
            RectTransform rect = dot.GetComponent<RectTransform>();
            if (rect != null)
            {
                // 프리팹 본래의 앵커와 피봇 세팅을 유지한 채 좌표만 부여 (엉뚱한 오프셋 방지)
                rect.anchoredPosition = pos;
                // 세로 구간에서는 세로로, 가로 구간에서는 가로로 회전 반영
                rect.localRotation = rotation;
            }

            // DOTween 투명도 페이드 연출 (팝업이나 BattleScene일 경우 즉시 표시)
            CanvasGroup cg = dot.GetComponent<CanvasGroup>();
            if (cg == null) cg = dot.AddComponent<CanvasGroup>();

            bool isMapScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MapScene";

            if (!isMapScene || isPopup)
            {
                cg.alpha = 1f;
            }
            else
            {
                cg.alpha = 0f;
                float delay = globalDotSequence * 0.03f; // 점 하나당 등장 딜레이
                cg.DOFade(1f, 0.3f).SetDelay(delay).SetUpdate(true); // 시간 정지 상태에서도 작동하도록
            }

            globalDotSequence++;
        }
    }

    NodeStatus GetNodeStatus(MapNodeData node)
    {
        int curId = GameManager.Instance.currentNodeId;

        if (node.status == NodeStatus.Completed) return NodeStatus.Completed;

        // 시작 전 (0번 노드만 활성화)
        if (curId == -1)
        {
            return (node.id == 0) ? NodeStatus.Available : NodeStatus.Locked;
        }

        // 현재 밟고 있는 노드의 다음 노드라면 활성화
        var curData = GameManager.Instance.currentMapData.GetNode(curId);
        if (curData != null && curData.HasConnectionTo(node.id))
        {
            return NodeStatus.Available;
        }

        return NodeStatus.Locked;
    }
}
