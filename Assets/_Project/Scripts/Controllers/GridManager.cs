using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Map Settings")]
    public int width = 5;
    public int height = 5;
    public float cellSize = 1.1f;

    [Header("Camera Settings")]
    public Transform cam;
    public Vector3 cameraOffset = new Vector3(0, 7, -6); 
    public Vector3 cameraRotation = new Vector3(50, 0, 0);

    [Header("References")]
    public Tile tilePrefab;

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
                Vector3 spawnPos = new Vector3(x * cellSize, 0, y * cellSize);
                Tile spawnedTile = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
                spawnedTile.Init(x, y);
                spawnedTile.transform.SetParent(this.transform);
            }
        }

        if (cam != null)
        {
            float centerX = (width * cellSize) / 2 - (cellSize / 2);
            float centerZ = (height * cellSize) / 2 - (cellSize / 2);
            Vector3 centerPos = new Vector3(centerX, 0, centerZ);

            cam.transform.position = centerPos + cameraOffset;
            cam.transform.rotation = Quaternion.Euler(cameraRotation);
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, 0.5f, y * cellSize);   // 높이(y)를 0.5f로 띄워 큐브가 바닥에 안 묻히게 함
    }
}
