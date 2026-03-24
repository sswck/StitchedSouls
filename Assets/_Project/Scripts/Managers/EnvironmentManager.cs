using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BackgroundSet
{
    public Sprite floorSprite;
    public Sprite gridSprite;
    public Sprite backSprite;
    public Sprite propSprite;
    public Sprite foreSprite;
}

[System.Serializable]
public struct NodeBackgroundMapping
{
    public NodeType nodeType;
    public BackgroundSet backgroundSet;
}

/// <summary>
/// GameManager의 현재 노드 타입에 따라 배경 스프라이트를 교체하고 
/// BackgroundFitter를 통해 화면에 맞게 재계산하는 매니저 클래스입니다.
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    [Header("Background Mappings")]
    [Tooltip("NodeType별 배경 세트를 매핑합니다. (예: Battle, Elite, Boss)")]
    public List<NodeBackgroundMapping> backgroundMappings = new List<NodeBackgroundMapping>();

    [Header("Target Sprite Renderers")]
    [Tooltip("인스펙터에서 5개의 대상 SpriteRenderer 컴포넌트를 미리 할당해주세요.")]
    public SpriteRenderer bgFloor;
    public SpriteRenderer bgGrid;
    public SpriteRenderer bgBack;
    public SpriteRenderer bgProp;
    public SpriteRenderer bgFore;

    private void Start()
    {
        // GameManager 인스턴스가 존재하면 현재 노드 타입에 맞는 배경 세트 적용
        if (GameManager.Instance != null)
        {
            ApplyBackground(GameManager.Instance.currentNodeType);
        }
        else
        {
            // GameManager가 없을 때 대비 (테스트 씬 등)
            Debug.LogWarning("[EnvironmentManager] GameManager 인스턴스를 찾을 수 없습니다. 배경을 변경하지 않습니다.");
        }
    }

    /// <summary>
    /// 지정된 노드 타입에 맞는 배경 세트를 찾아 적용합니다.
    /// </summary>
    public void ApplyBackground(NodeType type)
    {
        int index = backgroundMappings.FindIndex(m => m.nodeType == type);
        if (index >= 0)
        {
            BackgroundSet set = backgroundMappings[index].backgroundSet;
            UpdateSprite(bgFloor, set.floorSprite);
            UpdateSprite(bgGrid, set.gridSprite);
            UpdateSprite(bgBack, set.backSprite);
            UpdateSprite(bgProp, set.propSprite);
            UpdateSprite(bgFore, set.foreSprite);
        }
        else
        {
            Debug.LogWarning($"[EnvironmentManager] {type} 타입에 매핑된 배경 세트가 없습니다.");
        }
    }

    /// <summary>
    /// SpriteRenderer의 스프라이트를 교체하고, 교체 직후 BackgroundFitter.FitBackground()를 호출하여 스케일을 재계산합니다.
    /// </summary>
    private void UpdateSprite(SpriteRenderer sr, Sprite newSprite)
    {
        // 렌더러가 비워져 있거나 할당할 새 스프라이트가 없으면 안전하게 무시 (기존 스프라이트 유지)
        if (sr == null || newSprite == null) return;

        sr.sprite = newSprite;

        // 스프라이트가 변경되어 bounds가 달라졌으므로 스케일 재계산
        // 같은 오브젝트에 있는 BackgroundFitter 컴포넌트를 가져옵니다.
        BackgroundFitter fitter = sr.GetComponent<BackgroundFitter>();
        if (fitter != null)
        {
            fitter.FitBackground();
        }
    }
}
