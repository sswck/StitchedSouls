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

            // 좌표 및 회전 적용
            child.anchoredPosition = new Vector2(x, y);
            child.localRotation = Quaternion.Euler(0, 0, -currentAngle);
            
            // Sibling Index에 따라 레이어 순서가 결정됨 (보통 오른쪽 카드가 위로 옴)
        }

        // ✨ 레이아웃 배치가 완료된 후, 각 카드가 돌아올 '기본 위치'를 현재 위치로 갱신합니다.
        for (int i = 0; i < childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<CardHoverEffect>(out var hover))
            {
                hover.RefreshDefaultState();
            }
        }
    }
}
