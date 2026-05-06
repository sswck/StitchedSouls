using UnityEngine;

public class CardFanLayout : MonoBehaviour
{
    [Header("Fan Settings")]
    public float radius = 1000f;      // 부채꼴의 반지름 (클수록 완만해짐)
    public float maxAngle = 20f;      // 전체 부채꼴 각도 (한쪽 방향 최대치)
    public float heightOffset = -50f; // 높이 보정값
    public float spacingMultiplier = 1.0f; // 카드 간격 배율

    [ContextMenu("Update Layout")]
    public void UpdateLayout()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        // 카드가 한 장일 때는 중앙 배치, 여러 장일 때 각도 분할
        float anglePerCard = childCount > 1 ? (maxAngle * 2f) / (childCount - 1) : 0;
        
        // 카드가 너무 적을 때 너무 넓게 벌어지는 것 방지
        anglePerCard = Mathf.Min(anglePerCard, 10f * spacingMultiplier);
        
        float totalAngle = anglePerCard * (childCount - 1);
        float startAngle = -totalAngle / 2f;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;

            float currentAngle = startAngle + (i * anglePerCard);
            float rad = currentAngle * Mathf.Deg2Rad;

            // 원형 좌표 계산 (x = sin, y = cos)
            float x = Mathf.Sin(rad) * radius;
            float y = (Mathf.Cos(rad) * radius - radius) + heightOffset;

            // ✨ [개선] 좌표 및 회전을 직접 설정하는 대신, 호버 효과 컴포넌트에 '기본 상태'로 전달합니다.
            // 이렇게 하면 카드가 호버 중이더라도 배치가 변경되었을 때 돌아갈 위치를 정확히 알 수 있습니다.
            if (child.TryGetComponent<CardHoverEffect>(out var hover))
            {
                hover.SetDefaultState(new Vector2(x, y), Quaternion.Euler(0, 0, -currentAngle), Vector3.one, i);
            }
            else
            {
                child.anchoredPosition = new Vector2(x, y);
                child.localRotation = Quaternion.Euler(0, 0, -currentAngle);
                child.localScale = Vector3.one;
                child.SetSiblingIndex(i);
            }
        }
    }
}
