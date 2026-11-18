using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SlotPanelUI : UIBase
{
    private readonly List<(ItemSlotUI View, ItemSlot Address)> slots = new List<(ItemSlotUI, ItemSlot)>();
    private InventoryState state;

    protected ItemDragUI Drag { get; private set; }

    internal void Build(
        AlchemyDemoConfig config, InventoryState state, ItemDragUI drag,
        Func<GameObject, Transform, GameObject> spawn)
    {
        this.state = state;
        Drag = drag;
        slots.Clear();
        BuildLayout(config, spawn);
        Refresh();
    }

    protected abstract void BuildLayout(AlchemyDemoConfig config, Func<GameObject, Transform, GameObject> spawn);

    internal void Refresh()
    {
        foreach (var (view, address) in slots)
            view.Display(state.GetItem(address));
    }

    private protected void BuildSlots(
        Transform root, Component prefab, int count, Func<GameObject, Transform, GameObject> spawn,
        Func<int, ItemSlot> addressAt, Action<Transform, int> labelSlot)
    {
        EnsureChildren(root, prefab, count, spawn);
        for (var i = 0; i < count; i++)
        {
            var child = root.GetChild(i);
            labelSlot(child, i);
            var view = child.GetComponentInChildren<ItemSlotUI>(true);
            var address = addressAt(i);
            view.Bind(Drag, address);
            slots.Add((view, address));
        }
    }

    protected static void EnsureChildren(
        Transform root, Component prefab, int count, Func<GameObject, Transform, GameObject> spawn)
    {
        while (root.childCount < count)
            spawn(prefab.gameObject, root);

        for (var i = 0; i < root.childCount; i++)
            root.GetChild(i).gameObject.SetActive(i < count);
    }

    protected virtual void OnDisable() => Drag?.Cancel();
}
