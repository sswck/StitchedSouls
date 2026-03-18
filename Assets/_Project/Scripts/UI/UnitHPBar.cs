using UnityEngine;
using UnityEngine.UI;

public class UnitHPBar : MonoBehaviour  // 클래스명 변경?
{
    [Header("HP UI")]
    public Image hpFill;

    [Header("PP UI")]
    public Image ppFill;
    public GameObject ppBarContainer;

    public void SetHP(int currentHP, int maxHP)
    {
        // 정수 나눗셈 방지를 위해 float로 캐스팅
        float ratio = (float)currentHP / maxHP;
        hpFill.fillAmount = ratio;
    }

    /// <summary>
    /// PP바의 게이지를 업데이트합니다.
    /// </summary>
    public void SetPP(int currentPP, int maxPP)
    {
        if (ppFill != null)
        {
            ppFill.fillAmount = (float)currentPP / maxPP;
        }
    }

    /// <summary>
    /// 적군 유닛일 경우 PP바를 화면에서 숨깁니다.
    /// </summary>
    public void ShowPPBar(bool show)
    {
        if (ppBarContainer != null)
        {
            ppBarContainer.SetActive(show);
        }
        else if (ppFill != null)
        {
            ppFill.gameObject.SetActive(show);
        }
    }

    // (선택) 카메라를 항상 바라보게 하기 (빌보드)
    void LateUpdate()
    {
        // UI가 항상 카메라를 정면으로 보게 회전
        transform.forward = Camera.main.transform.forward;
    }
}
