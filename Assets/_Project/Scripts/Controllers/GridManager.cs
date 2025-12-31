using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Map Settings")]
    public int width = 5;
    public int height = 5;
    public float cellSize = 1.1f; // 타일 간격

    [Header("References")]
    public Tile tilePrefab; // 생성할 타일의 원본 프리팹
    public Transform cam;   // 메인 카메라 (위치 자동 정렬용)

    void Awake()
    {
        Instance = this;
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 3D 공간이므로 (x, 0, y) 좌표에 생성
                Vector3 spawnPos = new Vector3(x * cellSize, 0, y * cellSize);
                
                // 타일 생성 (Instantiate)
                Tile spawnedTile = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
                
                // 생성된 타일 초기화
                spawnedTile.Init(x, y);
                
                // 하이어라키 정리: GridManager의 자식으로 넣기
                spawnedTile.transform.SetParent(this.transform);
            }
        }

        // 카메라를 맵의 정중앙으로 이동시키기
        if (cam != null)
        {
            // 중앙 좌표 계산
            float centerX = (width * cellSize) / 2 - (cellSize / 2);
            float centerZ = (height * cellSize) / 2 - (cellSize / 2);
            
            cam.transform.position = new Vector3(centerX, 10, centerZ); // 높이(Y)는 10
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, 0.5f, y * cellSize);   // 높이(y)를 0.5f로 띄워 큐브가 바닥에 안 묻히게 함
    }
}
