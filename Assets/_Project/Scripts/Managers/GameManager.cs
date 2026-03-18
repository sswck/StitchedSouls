using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public List<CardData> startingDeck;
    public int defaultMaxHP = 50;
    public int defaultMaxSp = 10;

    [Header("Player Resources")]
    public int maxHP;
    public int currentHP;
    public int maxPP = 50;
    public int currentPP = 25;
    public int gold = 100;
    public int currentSp;
    public int maxSp;

    [Header("Player Status")]
    public int str;
    public int def;
    public int spd;
    public int movePoint;

    public List<CardData> masterDeck = new List<CardData>();

    [Header("Map Progress")]
    public int currentStageIndex = 0;
    public int lastClearedStageIndex = -1;

    [Header("Current Battle Info")]
    public NodeType currentNodeType;

    [Header("Reward System")]
    public List<CardData> allAvailableCards;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewRun()
    {
        maxHP = defaultMaxHP;
        currentHP = maxHP;

        masterDeck.Clear();
        if (startingDeck != null)
        {
            foreach (var card in startingDeck)
            {
                masterDeck.Add(card);
            }
        }

        Debug.Log($"🚀 새 게임 시작! 체력: {currentHP}, 카드: {masterDeck.Count}장");

        currentStageIndex = 0;
        lastClearedStageIndex = -1;

        LoadScene("MapScene");
    }

    public void CompleteStage()
    {
        lastClearedStageIndex = currentStageIndex;
        currentStageIndex++;
        LoadScene("MapScene");
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
