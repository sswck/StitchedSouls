using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [Header("Deck States")]
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> handDeck = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    [Header("Settings")]
    public int drawCountPerTurn = 4;
    public int maxHandSize = 8;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 전투 시작 시 초기 덱 세팅 (MasterDeck -> DrawPile 복사 후 셔플)
    /// </summary>
    public void InitializeDeck(List<CardData> masterDeck)
    {
        drawPile.Clear();
        handDeck.Clear();
        discardPile.Clear();

        if (masterDeck == null || masterDeck.Count == 0)
        {
            Debug.LogWarning("마스터 덱이 비어있어 기본 카드로 초기화합니다.");
            // 임시 테스트용 카드가 필요하다면 이 곳에서 추가
        }
        else
        {
            // 리스트 깊은 복사 (원본 훼손 방지)
            drawPile = new List<CardData>(masterDeck);
        }

        ShuffleDeck(drawPile);
        Debug.Log($"🃏 덱 초기화 완료. 덱 장수: {drawPile.Count}");
    }

    /// <summary>
    /// 카드 지정된 수만큼 드로우 (Hand로 가져오기)
    /// </summary>
    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    Debug.Log("⚠️ 덱과 버린 패가 모두 비어 더 이상 카드를 뽑을 수 없습니다!");
                    break;
                }
                ReshuffleDiscardIntoDraw();
            }

            // 맨 위 카드 1장 손패로 가져오기
            CardData drawnCard = drawPile[0];
            drawPile.RemoveAt(0);

            // [추가] 손패가 꽉 찼는지 체크
            if (handDeck.Count >= maxHandSize)
            {
                // 패가 꽉 찼다면 버린 패(무덤)로 직행
                Debug.Log($"✋ 손패가 꽉 찼습니다! [{drawnCard.cardName}] 카드가 무덤으로 버려집니다.");
                discardPile.Add(drawnCard);
            }
            else
            {
                // 패에 여유가 있다면 손으로 가져옴
                handDeck.Add(drawnCard);
            }
        }

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateHandUI(handDeck);
        }
        
        Debug.Log($"🎴 드로우 완료. (현재 패: {handDeck.Count}장 / 덱: {drawPile.Count}장 / 무덤: {discardPile.Count}장)");
    }

    /// <summary>
    /// 턴 종료 시 손패의 모든 카드를 버린 패로 이동
    /// </summary>
    public void DiscardHand()
    {
        discardPile.AddRange(handDeck);
        handDeck.Clear();
        
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateHandUI(handDeck);
        }
        Debug.Log($"🗑️ 손패를 모두 버렸습니다. (무덤: {discardPile.Count}장)");
    }

    /// <summary>
    /// 버린 패를 섞어서 다시 뽑기 뭉치로 이동
    /// </summary>
    private void ReshuffleDiscardIntoDraw()
    {
        Debug.Log("🔄 덱을 다 썼습니다! 무덤을 섞어 새로운 덱을 만듭니다.");
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck(drawPile);
    }
    
    /// <summary>
    /// 리스트 섞기 (Fisher-Yates 알고리즘)
    /// </summary>
    private void ShuffleDeck(List<CardData> deckToShuffle)
    {
        for (int i = deckToShuffle.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = deckToShuffle[i];
            deckToShuffle[i] = deckToShuffle[randomIndex];
            deckToShuffle[randomIndex] = temp;
        }
    }

    /// <summary>
    /// 슬롯에 카드를 등록할 때 손패에서 제거하고 UI를 갱신
    /// </summary>
    public void RemoveCardFromHand(CardData card)
    {
        if (handDeck.Contains(card))
        {
            handDeck.Remove(card);
            
            // 손패 UI 즉시 갱신
            if (BattleUIManager.Instance != null)
            {
                BattleUIManager.Instance.UpdateHandUI(handDeck);
            }
        }
    }

    /// <summary>
    /// 액션 슬롯에서 사용한 카드들을 무덤으로 보냄
    /// </summary>
    public void DiscardUsedCards(List<CardData> usedCards)
    {
        discardPile.AddRange(usedCards);
        Debug.Log($"🪦 사용한 카드 {usedCards.Count}장을 무덤으로 보냈습니다.");
    }

    /// <summary>
    /// 액션 슬롯에서 등록된 카드를 손패로 되돌림
    /// </summary>
    public void ReturnCardToHand(CardData card)
    {
        if (handDeck.Count >= maxHandSize)
        {
            Debug.Log($"✋ 손패가 꽉 차서 장착 취소된 [{card.cardName}] 카드가 무덤으로 버려집니다.");
            discardPile.Add(card);
        }
        else
        {
            handDeck.Add(card);
            Debug.Log($"🔙 카드 장착 취소: {card.cardName}이(가) 손패로 돌아왔습니다.");
        }
        
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateHandUI(handDeck);
        }
    }
}
