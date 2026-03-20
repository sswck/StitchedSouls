using UnityEngine;

/// <summary>
/// 화면에 오직 한 장의 카드만 확대되도록 관리하는 매니저 클래스입니다.
/// </summary>
public class CardHoverManager : MonoBehaviour
{
    public static CardHoverManager Instance { get; private set; }

    private CardHoverEffect _currentHoveredCard;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 카드가 확대될 때 호출되어, 기존에 확대된 카드가 있다면 강제로 축소시킵니다.
    /// </summary>
    public void HandleCardHover(CardHoverEffect card)
    {
        if (_currentHoveredCard != null && _currentHoveredCard != card)
        {
            // 기존 카드를 강제로 원래 상태로 되돌림
            _currentHoveredCard.ExitHover();
        }

        _currentHoveredCard = card;
    }

    /// <summary>
    /// 카드가 축소될 때 호출되어 매니저의 추적 상태를 해제합니다.
    /// </summary>
    public void HandleCardExit(CardHoverEffect card)
    {
        if (_currentHoveredCard == card)
        {
            _currentHoveredCard = null;
        }
    }
}
