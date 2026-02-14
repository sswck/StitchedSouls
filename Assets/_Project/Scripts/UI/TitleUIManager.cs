using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SoundManager.Instance != null)
        SoundManager.Instance.PlayBGM(SoundManager.Instance.titleBGM);

        startButton.onClick.AddListener(OnStartClick);
        exitButton.onClick.AddListener(OnExitClick);
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
}
