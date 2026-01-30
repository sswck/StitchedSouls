using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startButton.onClick.AddListener(OnStartClick);
        exitButton.onClick.AddListener(OnExitClick);
    }

    void OnStartClick()
    {
        Debug.Log("게임 시작!");
        // 지금은 바로 전투씬으로 가지만, 나중에 'MapScene'으로 연결하면 됩니다.
        GameManager.Instance.LoadScene("BattleScene"); 
    }

    void OnExitClick()
    {
        Debug.Log("게임 종료");
        GameManager.Instance.QuitGame();
    }
}
