using UnityEngine;
using System.Collections.Generic;

public class RelicPanelUI : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject popupPanel;
    public GameObject openButton;
    public Transform slotContainer;
    public GameObject relicSlotPrefab;

    private List<ItemSlotUI> spawnedSlots = new List<ItemSlotUI>();

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRelicChanged += UpdateAllSlots;
            UpdateAllSlots();
        }

        ClosePopup();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRelicChanged -= UpdateAllSlots;
        }
    }

    public void OpenPopup()
    {
        if (popupPanel != null)
        {
            openButton.SetActive(false);
            popupPanel.SetActive(true);
            UpdateAllSlots();
        }
    }

    public void ClosePopup()
    {
        if (popupPanel != null) {
            popupPanel.SetActive(false);
            openButton.SetActive(true);
        }
    }

    public void UpdateAllSlots()
    {
        if (GameManager.Instance == null || slotContainer == null) return;

        foreach (var slot in spawnedSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        spawnedSlots.Clear();
        
        List<ItemData> relics = GameManager.Instance.activeRelics;
        foreach (var relic in relics)
        {
            GameObject go = Instantiate(relicSlotPrefab, slotContainer);
            ItemSlotUI slotUI = go.GetComponent<ItemSlotUI>();
            if (slotUI != null)
            {
                slotUI.UpdateSlot(relic);
                spawnedSlots.Add(slotUI);
            }
        }
    }
}
