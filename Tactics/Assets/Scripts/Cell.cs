using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public bool IsDark { get; private set; }
    public Unit CurrentUnit { get; set; }

    private MeshRenderer _meshRenderer;
    private Color _baseColor;
    private Color _highlightColor = Color.yellow;
    private Color _availableMoveColor = Color.green;
    private Color _selectedColor = Color.cyan;
    private bool _isAvailableMove;
    private bool _isSelected;
    private BattleController _battleController;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _battleController = FindFirstObjectByType<BattleController>();
    }

    public void Init(int x, int y, bool isDark)
    {
        X = x;
        Y = y;
        IsDark = isDark;
        _baseColor = isDark ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.9f, 0.9f, 0.9f);
        ResetColor();
    }

    public void SetAvailableForMove(bool isAvailable)
    {
        _isAvailableMove = isAvailable;
        ResetColor();
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
        ResetColor();
    }

    public void ResetColor()
    {
        if (_isSelected)
            _meshRenderer.material.color = _selectedColor;
        else if (_isAvailableMove)
            _meshRenderer.material.color = _availableMoveColor;
        else
            _meshRenderer.material.color = _baseColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _meshRenderer.material.color = _highlightColor;
        if (_battleController != null) _battleController.SetHoveredCell(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetColor();
        if (_battleController != null) _battleController.SetHoveredCell(null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }
}
