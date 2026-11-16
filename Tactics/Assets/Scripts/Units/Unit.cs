using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public PlayerColor ColorType { get; private set; }
    public UnitType Type { get; private set; } = UnitType.Man;
    public Cell CurrentCell { get; set; }

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Init(PlayerColor color, Cell startCell)
    {
        ColorType = color;
        CurrentCell = startCell;
        CurrentCell.CurrentUnit = this;

        _meshRenderer.material.color = (color == PlayerColor.White) ? Color.white : Color.black;
    }

    public void PromoteToKing()
    {
        Type = UnitType.King;       
        transform.localScale = new Vector3(0.75f, 0.25f, 0.75f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentCell != null) CurrentCell.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CurrentCell != null) CurrentCell.OnPointerExit(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CurrentCell != null) CurrentCell.OnPointerClick(eventData);
    }
}