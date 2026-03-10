using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;

    [Header("Setting UI")]
    public GameObject settingPanel;
    public Slider BGMVolumeSlider;
    public Button openSettingButton;
    public Button closeSettingButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        FindSettingUIInScene();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSettingUIInScene();
    }

    void FindSettingUIInScene()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // Setting 버튼
        var settingBtnTransform = canvas.transform.Find("SettingBtn");
        if (settingBtnTransform != null)
        {
            openSettingButton = settingBtnTransform.GetComponent<Button>();
            if (openSettingButton != null)
            {
                openSettingButton.onClick.RemoveListener(OpenSettingPanel);
                openSettingButton.onClick.AddListener(OpenSettingPanel);
            }
        }

        // Setting 패널
        var panelTransform = canvas.transform.Find("SettingPanel");
        if (panelTransform == null) return;

        settingPanel = panelTransform.gameObject;

        // Back 버튼
        var backBtnTransform = panelTransform.Find("Setting")?.Find("BackBtn");
        if (backBtnTransform != null)
        {
            closeSettingButton = backBtnTransform.GetComponent<Button>();
            if (closeSettingButton != null)
            {
                closeSettingButton.onClick.RemoveListener(CloseSettingPanel);
                closeSettingButton.onClick.AddListener(CloseSettingPanel);
            }
        }

        // BGM 슬라이더
        var sliderTransform = panelTransform
            .Find("Setting")?.Find("BGMTxt")
            ?.Find("BGMSlider");

        if (sliderTransform == null) return;

        BGMVolumeSlider = sliderTransform.GetComponent<Slider>();
        if (BGMVolumeSlider == null) return;

        if (SoundManager.Instance != null && SoundManager.Instance.bgmPlayer != null)
        {
            BGMVolumeSlider.value = SoundManager.Instance.bgmPlayer.volume;
        }

        BGMVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        BGMVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
    }

    void OpenSettingPanel()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
    }

    void CloseSettingPanel()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }

    void OnBGMVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(value);
        }
    }
}
