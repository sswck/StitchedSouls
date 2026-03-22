using UnityEngine;
using System.Collections.Generic;

public class RelicPanelUI : MonoBehaviour
{
    [SerializeField] private List<ItemSlotUI> relicSlots;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRelicChanged += UpdateAllSlots;
            UpdateAllSlots();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRelicChanged -= UpdateAllSlots;
        }
    }

    public void UpdateAllSlots()
    {
        if (GameManager.Instance == null) return;
        
        List<ItemData> relics = GameManager.Instance.activeRelics;
        
        for (int i = 0; i < relicSlots.Count; i++)
        {
            if (i < relics.Count)
            {
                relicSlots[i].UpdateSlot(relics[i]);
            }
            else
            {
                relicSlots[i].UpdateSlot(null);
            }
        }
    }
}
