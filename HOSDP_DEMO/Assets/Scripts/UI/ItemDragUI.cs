using UnityEngine;
using UnityEngine.EventSystems;

// 拖拽物品卡片的 UI 组件
public sealed class ItemDragUI : MonoBehaviour
{
    [SerializeField] private ItemCardUI dragCardPrefab;

    private InventoryState state;
    private ItemCardUI dragCard;
    private RectTransform dragCardRect;
    private ItemSlot source;
    private int? pointer;

    internal void Initialize(InventoryState state)
    {
        this.state = state;
        dragCard = Instantiate(dragCardPrefab, transform);
        dragCard.name = "Dragged Item";
        dragCardRect = (RectTransform)dragCard.transform;
        var canvasGroup = dragCard.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.92f;
        dragCard.gameObject.SetActive(false);
    }

    internal void Begin(ItemSlot slot, PointerEventData eventData)
    {
        if (pointer.HasValue || eventData.button != PointerEventData.InputButton.Left)
            return;

        var entry = state.GetMovableItem(slot);
        if (entry == null)
            return;

        source = slot;
        pointer = eventData.pointerId;
        dragCard.Display(entry, slot.IsWorkbench ? entry.Quantity : 1);
        dragCard.gameObject.SetActive(true);
        UpdatePosition(eventData);
    }

    internal void UpdatePosition(PointerEventData eventData)
    {
        if (!Owns(eventData))
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, eventData.position, eventData.pressEventCamera, out var point);
        dragCardRect.anchoredPosition = point;
    }

    internal void DropOnSlot(ItemSlot target, PointerEventData eventData)
    {
        if (target.IsWorkbench)
        {
            if (Owns(eventData) && state.TryPlace(source, target.Index))
                Cancel();
        }
        else
        {
            DropOnBag(target.Bag, eventData);
        }
    }

    internal void DropOnBag(int bag, PointerEventData eventData)
    {
        if (Owns(eventData) && state.TryReturn(source, bag))
            Cancel();
    }

    internal void End(PointerEventData eventData)
    {
        if (Owns(eventData))
            Cancel();
    }

    internal void Cancel()
    {
        pointer = null;
        if (dragCard != null)
            dragCard.gameObject.SetActive(false);
    }

    private bool Owns(PointerEventData eventData) => pointer == eventData.pointerId;

    private void OnDisable() => Cancel();
}
