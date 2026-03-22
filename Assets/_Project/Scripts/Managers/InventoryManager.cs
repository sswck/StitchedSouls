using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    #region Singleton
    public static InventoryManager Instance { get; private set; }

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
    #endregion

    public List<ItemData> inventory = new List<ItemData>();
    public const int MAX_SLOTS = 3;

    public event Action OnInventoryChanged;
    public event Action<int> OnItemUsed;

    public bool AddItem(ItemData item)
    {
        if (item.isRelic) return false;

        if (inventory.Count >= MAX_SLOTS)
        {
            Debug.Log("Inventory is full.");
            return false;
        }

        inventory.Add(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public ItemData UseItem(int index)
    {
        if (index < 0 || index >= inventory.Count)
        {
            Debug.LogError($"Invalid item index to use: {index}");
            return null;
        }

        ItemData itemToUse = inventory[index];
        
        // Trigger animation/feedback event before removing
        OnItemUsed?.Invoke(index);

        // The item is consumed
        RemoveItem(index);
        
        Debug.Log($"Used item: {itemToUse.itemName}");

        return itemToUse;
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= inventory.Count)
        {
            Debug.LogError($"Invalid item index: {index}");
            return;
        }

        inventory.RemoveAt(index);
        OnInventoryChanged?.Invoke();
    }
    
    public ItemData GetItem(int index)
    {
        if (index < 0 || index >= inventory.Count)
        {
            return null;
        }
        return inventory[index];
    }
}
