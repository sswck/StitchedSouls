using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 마우스 오버 시 카드를 확대하고 위로 띄우는 효과를 담당하는 컴포넌트입니다.
/// </summary>
public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICanvasRaycastFilter
{
    [Header("Hover Settings")]
    [SerializeField] private float scaleFactor = 1.3f; // 확대 배율
    [SerializeField] private float yOffset = 100f;    // 위로 이동할 오프셋
    [SerializeField] private float duration = 0.15f;  // 애니메이션 소요 시간

    private bool _isHovered = false; // 현재 호버 중인지 여부

    // 기본 상태 저장용 변수
    private Vector2 _defaultAnchoredPosition;
    private Quaternion _defaultLocalRotation;
    private Vector3 _defaultLocalScale;
    private int _defaultSiblingIndex;

    private RectTransform _rectTransform;
    private DraggableCard _draggableCard;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _draggableCard = GetComponent<DraggableCard>();
    }

    private void Start()
    {
        RefreshDefaultState();
    }

    public void RefreshDefaultState()
    {
        if (_isHovered) return;

        _defaultAnchoredPosition = _rectTransform.anchoredPosition;
        _defaultLocalRotation = _rectTransform.localRotation;
        _defaultLocalScale = transform.localScale;
        _defaultSiblingIndex = transform.GetSiblingIndex();
    }

    /// <summary>
    /// 🌟 [핵심] 마우스 판정 영역을 커스텀합니다. 
    /// 카드가 위로 올라가더라도 마우스가 원래 위치에 있다면 '호버 중'으로 간주합니다.
    /// </summary>
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // 드래그 중이거나 호버 중이 아닐 때는 기본 판정을 따릅니다.
        if (!_isHovered || (_draggableCard != null && _draggableCard.IsDragging)) return true;

        // 1. 현재(이동/확대된) 상태에서 마우스가 시각적 영역 위에 있는지 체크
        if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, screenPoint, eventCamera))
            return true;

        // 2. ✨ [개선] 원래(이동 전) 위치 영역 체크
        // 애니메이션 중인 현재 상태가 아니라, 저장된 기본 위치(_defaultAnchoredPosition)를 기준으로 판정합니다.
        // 부모(Hand 등) 좌표계에서 마우스 위치를 가져온 뒤, 마트릭스 연산을 통해 '이동 전의 로컬 좌표'로 역변환합니다.
        Vector2 localPointInParent;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, screenPoint, eventCamera, out localPointInParent))
        {
            // 기본 상태의 변환 행렬 (위치, 회전, 스케일)
            Matrix4x4 idleMatrix = Matrix4x4.TRS(_defaultAnchoredPosition, _defaultLocalRotation, _defaultLocalScale);
            
            // 부모 좌표 -> 원래 로컬(Idle) 좌표로 변환
            Vector2 idleLocalPoint = idleMatrix.inverse.MultiplyPoint3x4(localPointInParent);
            
            // 원래 Rect 영역에 있는지 체크 
            // (하단에 padding을 주어 마우스가 아주 살짝 벗어나도 안정적으로 유지되게 함)
            Rect expandedRect = _rectTransform.rect;
            float padding = 20f;
            expandedRect.yMin -= padding; 

            return expandedRect.Contains(idleLocalPoint);
        }

        return false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isHovered) return;
        if (_draggableCard != null && _draggableCard.IsDragging) return;

        if (CardHoverManager.Instance != null)
        {
            CardHoverManager.Instance.HandleCardHover(this);
        }

        _isHovered = true;

        _rectTransform.DOKill();

        _defaultSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        // 애니메이션 실행
        _rectTransform.DOScale(_defaultLocalScale * scaleFactor, duration).SetEase(Ease.OutCubic);
        _rectTransform.DOAnchorPos(new Vector2(_defaultAnchoredPosition.x, _defaultAnchoredPosition.y + yOffset), duration).SetEase(Ease.OutCubic);
        _rectTransform.DOLocalRotate(Vector3.zero, duration).SetEase(Ease.OutCubic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isHovered) return;

        // 드래그가 시작된 경우 즉시 위치 초기화
        if (_draggableCard != null && _draggableCard.IsDragging)
        {
            ResetToDefaultImmediate();
            return;
        }

        ExitHover();
    }

    /// <summary>
    /// 호버 상태를 종료하고 부드럽게 원래 위치로 되돌립니다.
    /// </summary>
    public void ExitHover()
    {
        if (!_isHovered) return;
        _isHovered = false;

        if (CardHoverManager.Instance != null)
        {
            CardHoverManager.Instance.HandleCardExit(this);
        }

        _rectTransform.DOKill();

        _rectTransform.DOScale(_defaultLocalScale, duration).SetEase(Ease.OutQuad);
        _rectTransform.DOAnchorPos(_defaultAnchoredPosition, duration).SetEase(Ease.OutQuad);
        _rectTransform.DOLocalRotateQuaternion(_defaultLocalRotation, duration).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 다른 카드가 호버 중이지 않을 때만 순서 복구 (레이아웃 겹침 방지)
                if (!_isHovered) transform.SetSiblingIndex(_defaultSiblingIndex);
            });
    }

    /// <summary>
    /// 드래그 시작 시 애니메이션 없이 즉시 기본 상태로 복구합니다.
    /// </summary>
    public void ResetToDefaultImmediate()
    {
        _isHovered = false;
        if (CardHoverManager.Instance != null) CardHoverManager.Instance.HandleCardExit(this);

        _rectTransform.DOKill();
        _rectTransform.localScale = _defaultLocalScale;
        _rectTransform.anchoredPosition = _defaultAnchoredPosition;
        _rectTransform.localRotation = _defaultLocalRotation;
        transform.SetSiblingIndex(_defaultSiblingIndex);
    }
}
