using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Test_juwan_BattleUI_defeat_clear : MonoBehaviour
{
    [Header("Result Panel")]
    public GameObject resultPanel;

    [Header("Result UI Elements")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI damageDealtText;
    public TextMeshProUGUI damageTakenText;
    public TextMeshProUGUI damageBlockedText;
    public TextMeshProUGUI goldText;
    public Button titleButton;

    void Awake()
    {
        if (titleButton != null)
            titleButton.onClick.AddListener(OnTitleButtonClick);
    }

    void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    /// <summary>
    /// 패배 또는 전체 클리어 시 결과 UI를 표시합니다.
    /// </summary>
    /// <param name="isDefeat">true면 패배, false면 All Clear</param>
    /// <param name="damageDealt">입힌 총 피해량</param>
    /// <param name="damageTaken">입은 총 피해량</param>
    /// <param name="damageBlocked">방어한 총 피해량</param>
    /// <param name="gold">현재 골드</param>
    public void ShowDefeatClearUI(bool isDefeat, int damageDealt, int damageTaken, int damageBlocked, int gold)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text = isDefeat ? "Defeat" : "All Clear";
            resultText.color = isDefeat ? Color.red : Color.yellow;
        }

        if (damageDealtText != null)
            damageDealtText.text = $"입힌 피해량: {damageDealt}";

        if (damageTakenText != null)
            damageTakenText.text = $"입은 피해량: {damageTaken}";

        if (damageBlockedText != null)
            damageBlockedText.text = $"방어한 수치: {damageBlocked}";

        if (goldText != null)
            goldText.text = $"골드: {gold}";
    }

    void OnTitleButtonClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("TitleScene");
        }
    }
}
