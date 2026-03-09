using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ItemButton : MonoBehaviour
{
    [Header("Data")]
    public ItemData itemData; // 이 버튼이 대표하는 아이템 데이터를 에디터에서 할당합니다.

    private Button button;
    private ShopManager shopManager;

    void Start()
    {
        button = GetComponent<Button>();
        
        // ShopManager는 씬에 하나만 존재한다고 가정합니다.
        shopManager = Object.FindAnyObjectByType<ShopManager>();

        if (shopManager != null && itemData != null)
        {
            // 버튼 클릭 시 ShopManager의 OnItemButtonClick 함수를 itemData와 함께 호출하도록 리스너를 추가합니다.
            button.onClick.AddListener(() => shopManager.OnItemButtonClick(itemData));
        }
        else
        {
            if(shopManager == null)
                Debug.LogError("ShopManager를 씬에서 찾을 수 없습니다.");
            if(itemData == null)
                Debug.LogError($"게임 오브젝트 '{gameObject.name}'의 ItemButton에 ItemData가 할당되지 않았습니다.");
        }
    }
}
