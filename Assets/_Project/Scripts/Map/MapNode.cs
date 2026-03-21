using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum NodeType { Battle, Elite, Rest, Shop, Boss }
public enum NodeStatus { Locked, Available, Completed }

public class MapNode : MonoBehaviour
{
    public int nodeIndex;
    public NodeType nodeType;
    public NodeStatus status;

    [Header("UI References")]
    public Button button;
    public Image iconImage;
    public TextMeshProUGUI label;
    public Image lineToNext;

    public void Init(int index, NodeType type, NodeStatus status, MapNodeSpriteConfig config)
    {
        this.nodeIndex = index;
        this.nodeType = type;
        this.status = status;

        // 텍스트 설정
        label.text = $"{type}";

        // 상태에 따른 UI 변경 - 스프라이트 기반
        if (config != null)
        {
            Sprite sprite = config.GetSprite(type, status);
            if (sprite != null)
            {
                iconImage.sprite = sprite;
            }
        }
        iconImage.color = Color.white; // 스프라이트 원본 색상 유지

        switch (status)
        {
            case NodeStatus.Locked:
                button.interactable = false;
                break;
            case NodeStatus.Available:
                button.interactable = true;
                break;
            case NodeStatus.Completed:
                button.interactable = false;
                label.text += " (V)";
                break;
        }

        // 버튼 클릭 시 실행할 함수 연결
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnNodeClick);
    }

    void OnNodeClick()
    {
        // 팝업으로 띄어졌을 때 실수로 눌러서 씬 이동하는 것 방지
        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            Debug.Log("현재 전투 중에는 이동할 수 없습니다");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentNodeType = this.nodeType;
        }

        // 노드 타입에 따라 다른 씬 로드
        switch (nodeType)
        {
            case NodeType.Battle:
            case NodeType.Elite:
            case NodeType.Boss:
                Debug.Log($"⚔️ {nodeType} 전투 진입!");
                GameManager.Instance.LoadScene("BattleScene");
                break;
            
            case NodeType.Shop:
                Debug.Log("💰 상점 입장!");
                GameManager.Instance.LoadScene("ShopScene");
                break;
            
            case NodeType.Rest:
                // 아직 RestScene이 없으면 임시 로그
                Debug.Log("휴식 씬으로 이동 (구현 필요)");
                // 일단은 그냥 통과 처리 (테스트용)
                GameManager.Instance.CompleteStage();
                break;
        }
    }

    public void SetLine(Vector2 targetPosition)
{
    if (lineToNext == null) return;

    RectTransform myRect = GetComponent<RectTransform>();
    RectTransform lineRect = lineToNext.GetComponent<RectTransform>();

    Vector2 startPos = myRect.anchoredPosition;
    Vector2 dir = targetPosition - startPos;
    float distance = dir.magnitude;

    // 선의 길이와 각도 조절
    lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);
    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    lineRect.rotation = Quaternion.Euler(0, 0, angle);
    
    // 선 활성화 (마지막 노드는 선이 없음)
    lineToNext.gameObject.SetActive(true);
}
}
