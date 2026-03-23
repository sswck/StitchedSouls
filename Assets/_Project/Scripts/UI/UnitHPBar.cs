using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UnitHPBar : MonoBehaviour
{
    [Header("HP UI")]
    public Image hpFill;

    [Header("PP UI")]
    public Image ppFill;
    public GameObject ppBarContainer;

    [Header("Shield UI")]
    public GameObject shieldIcon;
    public TextMeshProUGUI shieldText;
    private int currentDisplayShield = 0;

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

    public void UpdateShieldUI(int targetShield)
    {
        bool isActive = targetShield > 0;
        
        if (shieldIcon != null) shieldIcon.SetActive(isActive);
        if (shieldText != null) shieldText.gameObject.SetActive(isActive);

        if (!isActive)
        {
            currentDisplayShield = 0;
            if (shieldText != null) shieldText.text = "0";
            return;
        }

        if (shieldText != null)
        {
            // 1. 숫자가 부드럽게 카운팅되며 올라가는 효과 적용 (DOTween.To)
            DOTween.To(() => currentDisplayShield, x =>
            {
                currentDisplayShield = x;
                if (shieldText != null) shieldText.text = currentDisplayShield.ToString();
            }, targetShield, 0.4f).SetEase(Ease.OutCubic);

            // 2. 텍스트가 툭 튀어나오는 피드백 효과 (DOPunchScale)
            shieldText.transform.DOKill(true); // 중복 애니메이션 간섭 방지
            shieldText.transform.localScale = Vector3.one; 
            shieldText.transform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 5, 1f);
        }

        if (shieldIcon != null)
        {
            // 쉴드 아이콘도 같이 튀어나오는 효과 적용
            shieldIcon.transform.DOKill(true);
            shieldIcon.transform.localScale = Vector3.one;
            shieldIcon.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 1f);
        }
    }

    // (선택) 카메라를 항상 바라보게 하기 (빌보드)
    void LateUpdate()
    {
        // UI가 항상 카메라를 정면으로 보게 회전
        transform.forward = Camera.main.transform.forward;
    }
}
