using UnityEngine;
using System.Collections.Generic;

public class AnchorGridManager : MonoBehaviour
{
    public static AnchorGridManager Instance;

    [Header("Map Settings")]
    public int width = 5;
    public int height = 4;

    [Header("Anchors (Manual Setup)")]
    public Transform topLeft;
    public Transform topRight;
    public Transform bottomLeft;
    public Transform bottomRight;

    [Header("References")]
    public Tile tilePrefab;
    private Tile[,] tiles;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateGrid();
    }

    public Vector3 GetPoint(float xRatio, float yRatio)
    {
        // 1. 위쪽 변과 아래쪽 변의 X지점 보간
        Vector3 topPos = Vector3.Lerp(topLeft.position, topRight.position, xRatio);
        Vector3 bottomPos = Vector3.Lerp(bottomLeft.position, bottomRight.position, xRatio);

        // 2. Y지점 보간
        return Vector3.Lerp(bottomPos, topPos, yRatio);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        // 타일의 인덱스(0, 1, 2...)에 0.5를 더해 정중앙 비율을 계산합니다.
        float xRatio = (x + 0.5f) / width;
        float yRatio = (y + 0.5f) / height;

        return GetPoint(xRatio, yRatio);
    }

    public void GenerateGrid()
    {
        if (tiles != null)
        {
            foreach (var t in tiles) if(t != null) Destroy(t.gameObject);
        }

        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 1. 네 꼭짓점 비율 계산
                // 예: x=0이면 0.0 ~ 0.166, x=1이면 0.166 ~ 0.333 ...
                float xMin = (float)x / width;
                float xMax = (float)(x + 1) / width;
                float yMin = (float)y / height;
                float yMax = (float)(y + 1) / height;

                // 2. 실제 월드 좌표 4개 구하기 (Anchor 기준)
                Vector3 p_bl = GetPoint(xMin, yMin); // 좌하
                Vector3 p_tl = GetPoint(xMin, yMax); // 좌상
                Vector3 p_tr = GetPoint(xMax, yMax); // 우상
                Vector3 p_br = GetPoint(xMax, yMin); // 우하
                
                // 3. 타일의 중심 위치 (생성 위치)
                Vector3 centerPos = (p_bl + p_tl + p_tr + p_br) / 4f;

                // 4. 타일 생성 및 모양 잡기
                Tile spawnedTile = Instantiate(tilePrefab, centerPos, Quaternion.identity);
                spawnedTile.transform.SetParent(this.transform);
                spawnedTile.name = $"Tile_{x}_{y}";
                
                // [중요] 꼭짓점 정보 전달 -> 타일이 알아서 모양 변형
                spawnedTile.Init(x, y, p_bl, p_tl, p_tr, p_br);

                tiles[x, y] = spawnedTile;
            }
        }
    }

    // --------------------------------------------------------
    // 공격 범위 하이라이트 기능
    // --------------------------------------------------------
    public void ResetAllTiles()
    {
        if (tiles == null) return;
        foreach (var tile in tiles)
        {
            if (tile != null) tile.SetHighlight(false, Color.white);
        }
    }

    public void HighlightAttackRange(int centerX, int centerY, List<Vector2Int> pattern, bool lookLeft)
    {
        ResetAllTiles();

        if (pattern == null) return;

        int direction = lookLeft ? -1 : 1;

        foreach (Vector2Int offset in pattern)
        {
            int targetX = centerX + (offset.x * direction);
            int targetY = centerY + offset.y;

            if (IsValidCoord(targetX, targetY))
            {
                tiles[targetX, targetY].SetHighlight(true, new Color(1f, 0.3f, 0.3f)); // 연한 빨강
            }
        }
    }

    private bool IsValidCoord(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
