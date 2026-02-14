using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public void Init(int index, NodeType type, NodeStatus status)
    {
        this.nodeIndex = index;
        this.nodeType = type;
        this.status = status;

        // 텍스트 설정
        label.text = $"{type}";

        // 상태에 따른 UI 변경
        switch (status)
        {
            case NodeStatus.Locked:
                button.interactable = false;
                iconImage.color = Color.gray;
                break;
            case NodeStatus.Available:
                button.interactable = true;
                iconImage.color = Color.white;
                // 강조 효과 (애니메이션 등) 추가 가능
                break;
            case NodeStatus.Completed:
                button.interactable = false;
                iconImage.color = new Color(0.5f, 1f, 0.5f); // 초록색 (완료됨)
                label.text += " (V)";
                break;
        }

        // 버튼 클릭 시 실행할 함수 연결
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnNodeClick);
    }

    void OnNodeClick()
    {
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
}
