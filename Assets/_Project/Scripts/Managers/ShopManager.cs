using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopManager : MonoBehaviour
{
    [Header("Original UI")]
    public TextMeshProUGUI goldText;
    public Button healButton;
    public Button exitButton;

    [Header("Shop Settings")]
    public int healCost = 50;
    public int healAmount = 20;
    
    //[Header("Player Gold")]
    //[SerializeField] private int playerCurrentGold; // 임시: GameManager와 연동하기 전 테스트용 골드

    [Header("Item Description UI")]
    [SerializeField] private RectTransform itemDescriptionBox;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemTitle;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private TextMeshProUGUI itemEffect;

    [Header("Item Purchase UI")]
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Image purchaseButtonImage;
    [SerializeField] private Sprite activePurchaseSprite;
    [SerializeField] private Sprite inactivePurchaseSprite;

    private ItemData currentSelectedItem;
    private Vector2 descriptionBoxOnScreenPos;
    private Vector2 descriptionBoxOffScreenPos;
    private Tweener descriptionBoxTweener;

    void Awake()
    {
        descriptionBoxOnScreenPos = itemDescriptionBox.anchoredPosition;
        descriptionBoxOffScreenPos = new Vector2(2000f, descriptionBoxOnScreenPos.y);
        itemDescriptionBox.anchoredPosition = descriptionBoxOffScreenPos;
    }

    void Start()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(SoundManager.Instance.shopBGM);

        UpdateUI();

        healButton.onClick.AddListener(OnBuyHeal);
        exitButton.onClick.AddListener(OnExit);
        purchaseButton.onClick.AddListener(OnPurchaseButtonClick); // 구매 버튼 리스너 등록
    }

    void UpdateUI()
    {
        if (GameManager.Instance != null)
        {
            goldText.text = $"{GameManager.Instance.gold} G";
            healButton.interactable = (GameManager.Instance.gold >= healCost);
            // playerCurrentGold = GameManager.Instance.gold; // GameManager와 연동 시 이와 같이 사용
        }
        
        // 상세 정보창이 열려있고, 선택된 아이템이 있다면 골드 변동 시 구매 버튼 상태를 바로 업데이트
        if (itemDescriptionBox.anchoredPosition == descriptionBoxOnScreenPos && currentSelectedItem != null)
        {
            UpdatePurchaseButtonState(currentSelectedItem);
        }
    }

    public void OnItemButtonClick(ItemData data)
    {
        currentSelectedItem = data; // 현재 선택된 아이템 정보 저장

        if (descriptionBoxTweener != null && descriptionBoxTweener.IsActive())
        {
            descriptionBoxTweener.Kill();
        }

        itemImage.sprite = data.icon;
        itemTitle.text = data.itemName;
        itemDescription.text = data.description;
        itemEffect.text = GetItemEffectText(data);

        // 아이템 정보를 표시한 후, 구매 버튼 상태 업데이트
        UpdatePurchaseButtonState(data);

        descriptionBoxTweener = itemDescriptionBox.DOAnchorPos(descriptionBoxOnScreenPos, 0.5f)
            .SetEase(Ease.OutCubic);
    }

    private string GetItemEffectText(ItemData data)
    {
        switch (data.effectType)
        {
            case ItemEffectType.HealHP:
                return data.effect;
            case ItemEffectType.HealSP:
                return data.effect;
            case ItemEffectType.DamageBuff:
                return data.effect;
            case ItemEffectType.MaxMovePoints:
                return data.effect;
            case ItemEffectType.IncreaseGold:
                return data.effect;
            
            default:
                return "특별한 효과가 없는 아이템입니다.";
        }
    }
    
    
    /// <summary>
    /// 아이템 가격과 플레이어 골드를 비교하여 구매 버튼의 상태(Sprite, interactable)를 업데이트합니다.
    /// </summary>
    private void UpdatePurchaseButtonState(ItemData data)
    {
        bool hasEnoughGold = GameManager.Instance.gold >= data.price;
        bool hasSpace = data.isRelic ? 
                        GameManager.Instance.activeRelics.Count < GameManager.MAX_RELICS : 
                        InventoryManager.Instance.inventory.Count < InventoryManager.MAX_SLOTS;

        // 디버깅 로그 추가
        Debug.Log($"[ShopManager] Checking gold for item '{data.itemName}'. Player Gold: {GameManager.Instance.gold}, Item Price: {data.price}, HasEnoughGold: {hasEnoughGold}, HasSpace: {hasSpace}");

        if (hasEnoughGold && hasSpace)
        {
            // 골드 충분하고 공간 있음
            purchaseButtonImage.sprite = activePurchaseSprite;
            purchaseButton.interactable = true;
        }
        else
        {
            // 골드 부족하거나 공간 없음
            purchaseButtonImage.sprite = inactivePurchaseSprite;
            purchaseButton.interactable = false;
        }
    }

    /// <summary>
    /// 구매 버튼 클릭 시 호출될 함수
    /// </summary>
    private void OnPurchaseButtonClick()
    {
        if (currentSelectedItem == null || GameManager.Instance == null) return;

        // 골드가 충분한지 한번 더 확인
        if (GameManager.Instance.gold >= currentSelectedItem.price)
        {
            bool added = false;
            if (currentSelectedItem.isRelic)
            {
                added = GameManager.Instance.AddRelic(currentSelectedItem);
            }
            else
            {
                added = InventoryManager.Instance.AddItem(currentSelectedItem);
            }

            if (added)
            {
                // 인벤토리 달성/유물 달성 성공 시 골드 차감
                GameManager.Instance.gold -= currentSelectedItem.price;
                Debug.Log($"{currentSelectedItem.itemName} 을(를) 구매했습니다! 남은 골드: {GameManager.Instance.gold}");

                // --- UI 즉시 업데이트 ---
                UpdateUI();
            }
            else
            {
                Debug.Log(currentSelectedItem.isRelic ? "유물 슬롯이 가득 찼습니다." : "인벤토리가 가득 찼습니다.");
            }
        }
    }

    public void OnBuyHeal()
    {
        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.gold >= healCost)
        {
            GameManager.Instance.gold -= healCost;
            GameManager.Instance.currentHP = Mathf.Min(GameManager.Instance.currentHP + healAmount, GameManager.Instance.maxHP);
            Debug.Log($"💖 체력 회복 완료! 현재 HP: {GameManager.Instance.currentHP}");
            
            UpdateUI(); // 골드 변경이 있으므로 UI 전체 업데이트
        }
    }

    public void OnExit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteStage();
        }
    }
}
