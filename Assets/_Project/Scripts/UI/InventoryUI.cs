using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private List<ItemSlotUI> itemSlots;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateAllSlots;
            InventoryManager.Instance.OnItemUsed += PlayUseAnimation;
            UpdateAllSlots();
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateAllSlots;
            InventoryManager.Instance.OnItemUsed -= PlayUseAnimation;
        }
    }

    public void UpdateAllSlots()
    {
        List<ItemData> inventory = InventoryManager.Instance.inventory;
        
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (i < inventory.Count)
            {
                itemSlots[i].UpdateSlot(inventory[i]);
            }
            else
            {
                itemSlots[i].UpdateSlot(null);
            }
        }
    }

    private void PlayUseAnimation(int index)
    {
        if (index >= 0 && index < itemSlots.Count)
        {
            itemSlots[index].PlayUseEffect();
        }
    }
}
