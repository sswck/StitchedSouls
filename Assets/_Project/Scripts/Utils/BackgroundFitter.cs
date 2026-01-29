using UnityEngine;

[ExecuteInEditMode]
public class BackgroundFitter : MonoBehaviour
{
    public bool keepAspectRatio = true;
    
    // [설정] 카메라로부터 얼마나 떨어뜨릴지 (이 값이 곧 레이어 깊이)
    // 01_Floor는 멀리(예: 15), 05_Fore는 가까이(예: 5) 설정하세요.
    [Range(1f, 100f)]
    public float distance = 10.0f; 

    // [추가] 오프셋 (혹시 미세 조정이 필요하면 사용)
    public Vector2 offset = Vector2.zero;

    void Update()
    {
#if UNITY_EDITOR
        FitBackground();
#endif
    }

    [ContextMenu("Fit Now")] // 인스펙터 우클릭 메뉴에서 수동 실행 가능
    public void FitBackground()
    {
        Camera cam = Camera.main;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (cam == null || sr == null || sr.sprite == null) return;

        // 1. 위치 & 회전 동기화 (가장 중요!)
        // 카메라 위치에서 시선 방향(Forward)으로 distance만큼 떨어진 곳에 배치
        transform.position = cam.transform.position + (cam.transform.forward * distance);
        
        // 카메라는 (50, 0, 0)인데 배경이 (0, 0, 0)이면 비스듬하게 보임.
        // 배경도 카메라랑 똑같이 회전시켜서 정면을 보게 함.
        transform.rotation = cam.transform.rotation;

        // 오프셋 적용 (필요 시)
        transform.Translate(offset);

        // 2. 화면 크기 계산 (이 거리에 있는 절두체 높이)
        // 공식: 높이 = 2 * 거리 * tan(FOV / 2)
        float worldScreenHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float worldScreenWidth = worldScreenHeight * cam.aspect;

        // 3. 스프라이트 크기
        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth = sr.sprite.bounds.size.x;

        // 4. 스케일 적용
        float scaleY = worldScreenHeight / spriteHeight;
        float scaleX = worldScreenWidth / spriteWidth;

        if (keepAspectRatio)
        {
            float maxScale = Mathf.Max(scaleX, scaleY);
            transform.localScale = new Vector3(maxScale, maxScale, 1f);
        }
        else
        {
            transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }
}
