using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICanvasRaycastFilter
{
    [Header("Hover Settings")]
    [SerializeField] private float scaleFactor = 1.25f;
    [SerializeField] private float yOffset = 120f;
    [SerializeField] private float duration = 0.2f;

    [Header("References")]
    [SerializeField] private RectTransform visualRoot;

    private bool _isHovered = false;

    private Vector2 _defaultAnchoredPosition;
    private Quaternion _defaultLocalRotation;
    private Vector3 _defaultLocalScale;
    private int _defaultSiblingIndex;

    private Vector2 _visualDefaultAnchoredPosition;
    private Quaternion _visualDefaultLocalRotation;
    private Vector3 _visualDefaultLocalScale;

    private RectTransform _rectTransform;
    private DraggableCard _draggableCard;
    private Canvas _canvas;
    private bool _usesSelfAsVisualRoot;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _draggableCard = GetComponent<DraggableCard>();

        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();
        }

        if (visualRoot == null) visualRoot = _rectTransform;
        _usesSelfAsVisualRoot = visualRoot == _rectTransform;

        CacheVisualDefaultState();
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
        _defaultLocalScale = _rectTransform.localScale;
        _defaultSiblingIndex = transform.GetSiblingIndex();

        CacheVisualDefaultState();
    }

    public void SetDefaultState(Vector2 pos, Quaternion rot, Vector3 scale, int index)
    {
        _defaultAnchoredPosition = pos;
        _defaultLocalRotation = rot;
        _defaultLocalScale = scale;
        _defaultSiblingIndex = index;

        if (!_isHovered)
        {
            _rectTransform.anchoredPosition = pos;
            _rectTransform.localRotation = rot;
            _rectTransform.localScale = scale;
            transform.SetSiblingIndex(index);
            if (_usesSelfAsVisualRoot)
            {
                CacheVisualDefaultState();
            }
            else
            {
                ResetVisualRootImmediate();
            }
        }
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (_draggableCard != null && _draggableCard.IsDragging) return false;

        Vector2 localPointInParent;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, screenPoint, eventCamera, out localPointInParent))
            return false;

        Matrix4x4 idleMatrix = Matrix4x4.TRS(_defaultAnchoredPosition, _defaultLocalRotation, _defaultLocalScale);
        Vector2 idleLocalPoint = idleMatrix.inverse.MultiplyPoint3x4(localPointInParent);

        Rect idleRect = _rectTransform.rect;
        float idlePadding = _isHovered ? 30f : 0f;
        idleRect.xMin -= idlePadding;
        idleRect.xMax += idlePadding;
        idleRect.yMin -= idlePadding;
        idleRect.yMax += idlePadding;

        return idleRect.Contains(idleLocalPoint);
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

        visualRoot.DOKill();

        _canvas.overrideSorting = true;
        _canvas.sortingOrder = 1000;

        visualRoot.DOScale(_visualDefaultLocalScale * scaleFactor, duration).SetEase(Ease.OutCubic);
        visualRoot.DOAnchorPos(_visualDefaultAnchoredPosition + new Vector2(0f, yOffset), duration).SetEase(Ease.OutCubic);
        visualRoot.DOLocalRotate(Vector3.zero, duration).SetEase(Ease.OutCubic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isHovered) return;

        if (_draggableCard != null && _draggableCard.IsDragging)
        {
            ResetToDefaultImmediate();
            return;
        }

        ExitHover();
    }

    public void ExitHover()
    {
        if (!_isHovered) return;
        _isHovered = false;

        if (CardHoverManager.Instance != null)
        {
            CardHoverManager.Instance.HandleCardExit(this);
        }

        visualRoot.DOKill();

        visualRoot.DOScale(_visualDefaultLocalScale, duration).SetEase(Ease.OutQuad);
        visualRoot.DOAnchorPos(_visualDefaultAnchoredPosition, duration).SetEase(Ease.OutQuad);
        visualRoot.DOLocalRotateQuaternion(_visualDefaultLocalRotation, duration).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                if (!_isHovered)
                {
                    _canvas.overrideSorting = false;
                    _canvas.sortingOrder = 0;
                    transform.SetSiblingIndex(_defaultSiblingIndex);
                }
            });
    }

    public void ResetToDefaultImmediate()
    {
        _isHovered = false;
        if (CardHoverManager.Instance != null) CardHoverManager.Instance.HandleCardExit(this);

        _rectTransform.DOKill();
        visualRoot.DOKill();

        _rectTransform.anchoredPosition = _defaultAnchoredPosition;
        _rectTransform.localRotation = _defaultLocalRotation;
        _rectTransform.localScale = _defaultLocalScale;
        transform.SetSiblingIndex(_defaultSiblingIndex);

        ResetVisualRootImmediate();

        _canvas.overrideSorting = false;
        _canvas.sortingOrder = 0;
    }

    private void CacheVisualDefaultState()
    {
        _visualDefaultAnchoredPosition = visualRoot.anchoredPosition;
        _visualDefaultLocalRotation = visualRoot.localRotation;
        _visualDefaultLocalScale = visualRoot.localScale;
    }

    private void ResetVisualRootImmediate()
    {
        visualRoot.DOKill();
        visualRoot.anchoredPosition = _visualDefaultAnchoredPosition;
        visualRoot.localRotation = _visualDefaultLocalRotation;
        visualRoot.localScale = _visualDefaultLocalScale;
    }
}
