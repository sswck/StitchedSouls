using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class TurnOrderUI : MonoBehaviour
{
    public static TurnOrderUI Instance;

    [Header("UI References")]
    public Transform container;
    public GameObject unitIconPrefab; // 유닛별 아이콘/이름을 표시할 프리팹

    [Header("Settings")]
    public float spacing = 120f;
    public float moveDuration = 0.5f;

    private Dictionary<Unit, UnitIcon> iconMap = new Dictionary<Unit, UnitIcon>();

    private void Awake()
    {
        Instance = this;
    }

    public void Refresh(List<Unit> queue, Unit currentUnit, List<Unit> actedUnits = null)
    {
        if (container == null || unitIconPrefab == null) return;

        List<Unit> targetOrder = new List<Unit>();
        
        // 1. 현재 턴 유닛 (가장 왼쪽)
        if (currentUnit != null && currentUnit.currentHP > 0)
            targetOrder.Add(currentUnit);

        // 2. 다음 턴 유닛들 (중앙)
        foreach (var unit in queue)
        {
            if (unit != null && unit.currentHP > 0 && !targetOrder.Contains(unit))
                targetOrder.Add(unit);
        }

        // 3. 이미 행동한 유닛들 (가장 오른쪽)
        if (actedUnits != null)
        {
            foreach (var unit in actedUnits)
            {
                if (unit != null && unit.currentHP > 0 && !targetOrder.Contains(unit))
                    targetOrder.Add(unit);
            }
        }

        // 죽은 유닛이나 리스트에 없는 아이콘 제거
        HashSet<Unit> activeUnits = new HashSet<Unit>(targetOrder);
        List<Unit> unitsToRemove = new List<Unit>();
        foreach (var unit in iconMap.Keys)
        {
            if (!activeUnits.Contains(unit) || unit == null || unit.currentHP <= 0)
                unitsToRemove.Add(unit);
        }

        foreach (var unit in unitsToRemove)
        {
            if (iconMap.TryGetValue(unit, out UnitIcon icon))
            {
                Destroy(icon.gameObject);
                iconMap.Remove(unit);
            }
        }

        // 배치 및 애니메이션
        for (int i = 0; i < targetOrder.Count; i++)
        {
            Unit unit = targetOrder[i];
            UnitIcon icon = GetOrCreateIcon(unit);
            
            bool isCurrent = (unit == currentUnit);
            icon.SetIcon(unit, isCurrent);

            // 목표 위치 계산 (좌측 정렬 기준)
            float targetX = i * spacing;
            icon.transform.SetAsLastSibling(); // 계층 구조 순서 정렬 (겹칠 경우 대비)
            
            // DOTween으로 부드럽게 이동
            RectTransform rect = icon.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOAnchorPos(new Vector2(targetX, 0), moveDuration).SetEase(Ease.OutCubic);
            }
        }
    }

    private UnitIcon GetOrCreateIcon(Unit unit)
    {
        if (iconMap.ContainsKey(unit))
            return iconMap[unit];

        GameObject go = Instantiate(unitIconPrefab, container);
        UnitIcon icon = go.GetComponent<UnitIcon>();
        
        if (icon == null)
        {
            icon = go.AddComponent<UnitIcon>();
            icon.iconImage = go.GetComponentInChildren<UnityEngine.UI.Image>();
        }

        iconMap[unit] = icon;
        
        // 생성 시 초기 위치 설정 (부자연스러운 순간이동 방지 위해 마지막 위치에서 생성 시작 가능)
        RectTransform rect = icon.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(container.childCount * spacing, 0);
        }

        return icon;
    }
}
