using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleUIManager : MonoBehaviour
{
    [Header("Main Buttons")]
    public Button startButton;
    public Button exitButton;
    public Button StatusButton;

    [Header("Status Panel")]
    public GameObject statusPanel;
    public Button closeStatusButton;
    public TextMeshProUGUI spText;
    public TextMeshProUGUI strText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI spdText;
    public Button upgradeStrButton;
    public Button upgradeDefButton;
    public Button upgradeSpdButton;
    [Tooltip("SP 부족 시 안내 메시지 (선택)")]
    public TextMeshProUGUI statusMessageText;

    void Start()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(SoundManager.Instance.titleBGM);

        startButton.onClick.AddListener(OnStartClick);
        exitButton.onClick.AddListener(OnExitClick);
        StatusButton.onClick.AddListener(OnStatusClick);

        if (statusPanel != null)
        {
            statusPanel.SetActive(false);
            if (closeStatusButton != null)
                closeStatusButton.onClick.AddListener(OnCloseStatusClick);
            if (upgradeStrButton != null)
                upgradeStrButton.onClick.AddListener(() => OnUpgradeStat("str"));
            if (upgradeDefButton != null)
                upgradeDefButton.onClick.AddListener(() => OnUpgradeStat("def"));
            if (upgradeSpdButton != null)
                upgradeSpdButton.onClick.AddListener(() => OnUpgradeStat("spd"));
        }
    }

    void OnStartClick()
    {
        Debug.Log("게임 시작!");
        GameManager.Instance.StartNewRun();
    }

    void OnExitClick()
    {
        Debug.Log("게임 종료");
        GameManager.Instance.QuitGame();
    }

    void OnStatusClick()
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(true);
            RefreshStatusPanel();
        }
    }

    void OnCloseStatusClick()
    {
        if (statusPanel != null)
            statusPanel.SetActive(false);
    }

    void RefreshStatusPanel()
    {
        if (GameManager.Instance == null) return;

        int sp = GameManager.Instance.currentSp;
        int maxSp = GameManager.Instance.maxSp;
        bool canUpgrade = sp >= 1;

        if (spText != null)
            spText.text = $"{sp} / {maxSp}";
        if (strText != null)
            strText.text = GameManager.Instance.str.ToString();
        if (defText != null)
            defText.text = GameManager.Instance.def.ToString();
        if (spdText != null)
            spdText.text = GameManager.Instance.spd.ToString();

        if (upgradeStrButton != null)
            upgradeStrButton.interactable = canUpgrade;
        if (upgradeDefButton != null)
            upgradeDefButton.interactable = canUpgrade;
        if (upgradeSpdButton != null)
            upgradeSpdButton.interactable = canUpgrade;

        if (statusMessageText != null)
            statusMessageText.text = canUpgrade ? "" : "SP가 부족합니다.";
    }

    void OnUpgradeStat(string statKey)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.currentSp < 1) return;

        GameManager.Instance.currentSp -= 1;
        switch (statKey)
        {
            case "str":
                GameManager.Instance.str += 1;
                break;
            case "def":
                GameManager.Instance.def += 1;
                break;
            case "spd":
                GameManager.Instance.spd += 1;
                break;
        }
        RefreshStatusPanel();
    }
}
