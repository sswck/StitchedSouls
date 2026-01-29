using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Tile : MonoBehaviour
{
    public int x, y;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;

    public void Init(int x, int y, Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br)
    {
        this.x = x;
        this.y = y;

        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.name = $"TileMesh_{x}_{y}";

        Vector3 center = transform.position;
        Vector3[] vertices = new Vector3[]
        {
            bl - center, // 0: 좌하
            tl - center, // 1: 좌상
            tr - center, // 2: 우상
            br - center  // 3: 우하
        };

        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // 빛 반사 계산

        meshFilter.mesh = mesh;

        // 'Unlit/Color' 쉐이더 등을 가진 머티리얼을 사용하면 좋습니다.
        if (meshRenderer.material == null)
            meshRenderer.material = new Material(Shader.Find("Sprites/Default")); // 임시 쉐이더

        SetHighlight(false, Color.white);
    }

    public void SetHighlight(bool isOn, Color color)
    {
        if (meshRenderer == null) return;

        meshRenderer.material.DOKill();

        Color targetColor = isOn ? color : new Color(1, 1, 1, 0); // 꺼지면 완전 투명

        meshRenderer.material.color = targetColor;
    }
}
