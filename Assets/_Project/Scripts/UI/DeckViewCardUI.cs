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

        // 🖼️ [추가] 카드 데이터에 있는 이미지를 UI의 Image 컴포넌트에 적용
        if (cardIcon != null && data.cardImage != null)
        {
            cardIcon.sprite = data.cardImage;
        }

        // 타입별 색상 지정 (예시)
    }
}
