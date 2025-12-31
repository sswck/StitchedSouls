using UnityEngine;
using UnityEngine.UI;

public class UnitHPBar : MonoBehaviour
{
    public Image fillImage;

    public void SetHP(int current, int max)
    {
        // 정수 나눗셈 방지를 위해 float로 캐스팅
        float ratio = (float)current / max;
        fillImage.fillAmount = ratio;
    }

    // (선택) 카메라를 항상 바라보게 하기 (빌보드)
    void LateUpdate()
    {
        // UI가 항상 카메라를 정면으로 보게 회전
        transform.forward = Camera.main.transform.forward;
    }
}
