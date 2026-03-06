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
            // 뽑을 카드가 없다면
            if (drawPile.Count == 0)
            {
                // 버린 패도 없다면 더 이상 뽑을 수 없음
                if (discardPile.Count == 0)
                {
                    Debug.Log("⚠️ 덱과 버린 패가 모두 비어 더 이상 카드를 뽑을 수 없습니다!");
                    break;
                }
                
                // 무덤 섞어서 다시 덱으로
                ReshuffleDiscardIntoDraw();
            }

            // 맨 위 카드 1장 손패로 가져오기
            CardData drawnCard = drawPile[0];
            drawPile.RemoveAt(0);
            handDeck.Add(drawnCard);
        }

        // 손패 UI 업데이트 (BattleUIManager 호출)
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateHandUI(handDeck);
        }
        
        Debug.Log($"🎴 {amount}장 드로우 완료. (현재 패: {handDeck.Count}장, 남은 덱: {drawPile.Count}장)");
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
}
