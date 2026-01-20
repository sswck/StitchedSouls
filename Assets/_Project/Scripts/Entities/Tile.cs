using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public int x, y;

    private Color originalColor;
    private MeshRenderer meshRenderer;

    public void Init(int x, int y)
    {
        this.x = x;
        this.y = y;
        name = $"Tile {x},{y}";

        bool isOffset = (x + y) % 2 == 1;
        meshRenderer = GetComponent<MeshRenderer>();

        originalColor = isOffset ? new Color(0.8f, 0.8f, 0.8f) : Color.white;

        if(meshRenderer != null) 
            meshRenderer.material.color = originalColor;
    }

    public void SetHighlight(bool isOn, Color color)
    {
        if (meshRenderer == null) return;

        if (isOn)
        {
            meshRenderer.material.DOKill();
            meshRenderer.material.color = color;
        }
        else
        {
            meshRenderer.material.DOKill();
            meshRenderer.material.color = originalColor;
        }
    }
}
