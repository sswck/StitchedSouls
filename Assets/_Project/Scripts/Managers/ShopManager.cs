using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public Button healButton;
    public Button exitButton;

    [Header("Shop Settings")]
    public int healCost = 50;
    public int healAmount = 20;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SoundManager.Instance != null)
        SoundManager.Instance.PlayBGM(SoundManager.Instance.shopBGM);

        UpdateUI();

        healButton.onClick.AddListener(OnBuyHeal);
        exitButton.onClick.AddListener(OnExit);
    }

    void UpdateUI()
    {
        // 소지금 표시
        if (GameManager.Instance != null)
        {
            goldText.text = $"소지금: {GameManager.Instance.gold} G";
            
            // 돈 없으면 버튼 비활성화
            healButton.interactable = (GameManager.Instance.gold >= healCost);
        }
    }

    public void OnBuyHeal()
    {
        if (GameManager.Instance == null) return;

        // 돈이 충분한지 확인
        if (GameManager.Instance.gold >= healCost)
        {
            // 돈 차감
            GameManager.Instance.gold -= healCost;

            // 체력 회복 (최대 체력 넘지 않게)
            GameManager.Instance.currentHP += healAmount;
            if (GameManager.Instance.currentHP > GameManager.Instance.maxHP)
            {
                GameManager.Instance.currentHP = GameManager.Instance.maxHP;
            }

            Debug.Log($"💖 체력 회복 완료! 현재 HP: {GameManager.Instance.currentHP}");

            // 구매 후 버튼 비활성화 (한 번만 구매 가능하게 하려면)
            healButton.interactable = false; 

            UpdateUI();
        }
    }

    public void OnExit()
    {
        // 상점을 나가면 스테이지 클리어 처리 -> 맵으로 복귀
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteStage();
        }
    }
}
