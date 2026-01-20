using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public int x, y;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public void Init(int x, int y)
    {
        this.x = x;
        this.y = y;
        name = $"Tile {x},{y}";

        spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = Color.white;

        if (spriteRenderer != null) 
            spriteRenderer.color = originalColor;
    }

    public void SetHighlight(bool isOn, Color color)
    {
        if (spriteRenderer == null) return;

        if (isOn)
        {
            spriteRenderer.DOKill();
            spriteRenderer.color = color;
        }
        else
        {
            spriteRenderer.DOKill();
            spriteRenderer.color = originalColor;
        }
    }
}
