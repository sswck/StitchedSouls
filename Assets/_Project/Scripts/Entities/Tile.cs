using Unity.Mathematics;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;

    public void Init(int x, int y)
    {
        this.x = x;
        this.y = y;
        name = $"Tile {x},{y}"; // 하이어라키에서 보여지는 이름

        // (선택 사항) 체크무늬 패턴으로 색상을 살짝 다르게 설정
        // x와 y를 더해서 홀수/짝수냐에 따라 색 변경
        bool isOffset = (x + y) % 2 == 1;
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // 흰색과 옅은 회색으로 교차
            renderer.material.color = isOffset ? Color.white : new Color(0.9f, 0.9f, 0.9f);
        }
    }
}
