using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public List<CardData> startingDeck;
    public int defaultMaxHP = 50;

    [Header("Current Run Data")]
    public int maxHP;
    public int currentHP;
    public List<CardData> masterDeck = new List<CardData>();

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
        
        // 맵 씬으로 이동 (지금은 MapScene이 없으니 바로 BattleScene으로)
        LoadScene("BattleScene"); 
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
