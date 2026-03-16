using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckViewCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image cardIcon;
    public TextMeshProUGUI cardNameText;

    /// <summary>
    /// 카드 데이터를 UI 요소에 바인딩합니다.
    /// </summary>
    public void SetCard(CardData data)
    {
        if (data == null) return;

        if (cardNameText != null) cardNameText.text = data.cardName;


        // 타입별 색상 지정 (예시)
    }
}
