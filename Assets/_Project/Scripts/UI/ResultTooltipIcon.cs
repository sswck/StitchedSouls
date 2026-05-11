using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ResultTooltipStatType
{
    DamageDealt,
    DamageTaken,
    Gold,
    Sp
}

public class ResultTooltipIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ResultTooltipStatType statType;
    public TextMeshProUGUI valueText;
    public GameObject tooltipBox;
    public TextMeshProUGUI tooltipText;
    public Vector2 tooltipOffset = new Vector2(0f, 12f);

    [TextArea]
    public string descriptionOverride;

    private int currentValue;

    private void Awake()
    {
        CacheValueText();
    }

    public void Configure(GameObject sharedTooltipBox, TextMeshProUGUI sharedTooltipText)
    {
        CacheValueText();

        if (tooltipBox == null)
        {
            tooltipBox = sharedTooltipBox;
        }

        if (tooltipText == null)
        {
            tooltipText = sharedTooltipText;
        }
    }

    public void SetValue(int value)
    {
        currentValue = value;
        CacheValueText();

        if (valueText != null)
        {
            valueText.text = value.ToString();
        }

        RefreshTooltipContent();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    public void RefreshTooltipContent()
    {
        if (tooltipBox == null || tooltipText == null || !tooltipBox.activeSelf) return;

        tooltipText.text = BuildTooltipText();
    }

    private void ShowTooltip()
    {
        if (tooltipBox == null || tooltipText == null) return;

        tooltipBox.SetActive(true);
        tooltipText.text = BuildTooltipText();
        PositionTooltipAboveIcon();
    }

    private void HideTooltip()
    {
        if (tooltipBox != null)
        {
            tooltipBox.SetActive(false);
        }
    }

    private void PositionTooltipAboveIcon()
    {
        RectTransform iconRect = transform as RectTransform;
        RectTransform tooltipRect = tooltipBox.transform as RectTransform;
        RectTransform tooltipParentRect = tooltipRect != null ? tooltipRect.parent as RectTransform : null;

        if (iconRect == null || tooltipRect == null || tooltipParentRect == null) return;

        Canvas canvas = tooltipBox.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector3[] corners = new Vector3[4];
        iconRect.GetWorldCorners(corners);
        Vector3 iconTopCenterWorld = (corners[1] + corners[2]) * 0.5f;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, iconTopCenterWorld);
        screenPoint += tooltipOffset;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipParentRect, screenPoint, uiCamera, out Vector2 localPoint))
        {
            tooltipRect.pivot = new Vector2(0.5f, 0f);
            tooltipRect.anchoredPosition = localPoint;
        }
    }

    private string BuildTooltipText()
    {
        if (!string.IsNullOrWhiteSpace(descriptionOverride))
        {
            return $"{descriptionOverride}\n{currentValue}";
        }

        switch (statType)
        {
            case ResultTooltipStatType.DamageDealt:
                return $"입힌 피해량";
            case ResultTooltipStatType.DamageTaken:
                return $"입은 피해량";
            case ResultTooltipStatType.Gold:
                return $"전체 획득 골드";
            case ResultTooltipStatType.Sp:
                return $"전체 획득 SP";
            default:
                return string.Empty;
        }
    }

    private void CacheValueText()
    {
        if (valueText != null) return;

        valueText = GetComponentInChildren<TextMeshProUGUI>(true);
    }
}
