using UnityEngine.EventSystems;

public sealed class ItemSlotUI : ItemCardUI, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private ItemDragUI drag;
    private ItemSlot address;

    internal void Bind(ItemDragUI drag, ItemSlot address)
    {
        this.drag = drag;
        this.address = address;
    }

    public void OnBeginDrag(PointerEventData eventData) => drag.Begin(address, eventData);
    public void OnDrag(PointerEventData eventData) => drag.UpdatePosition(eventData);
    public void OnEndDrag(PointerEventData eventData) => drag.End(eventData);
    public void OnDrop(PointerEventData eventData) => drag.DropOnSlot(address, eventData);
}
