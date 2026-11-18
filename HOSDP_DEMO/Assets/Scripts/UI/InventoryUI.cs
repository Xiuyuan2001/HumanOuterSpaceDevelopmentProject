using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class InventoryUI : SlotPanelUI, IDropHandler
{
    [SerializeField] private RectTransform tabsRoot;
    [SerializeField] private Button tabPrefab;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private RectTransform pagePrefab;
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private Color selectedTab;
    [SerializeField] private Color idleTab;

    private int bagCount;
    private int selectedBag;

    protected override void BuildLayout(AlchemyDemoConfig config, Func<GameObject, Transform, GameObject> spawn)
    {
        bagCount = config.bags.Length;
        EnsureChildren(tabsRoot, tabPrefab, bagCount, spawn);
        EnsureChildren(scroll.viewport, pagePrefab, bagCount, spawn);

        for (var i = 0; i < bagCount; i++)
        {
            var bagIndex = i;
            var bag = config.bags[i];
            var tab = tabsRoot.GetChild(i).GetComponent<Button>();
            tab.name = bag.label;
            tab.GetComponentInChildren<TMP_Text>(true).text = bag.label;

            var page = scroll.viewport.GetChild(i);
            page.name = bag.label;
            BuildSlots(page, slotPrefab, bag.SlotCount, spawn,
                slot => ItemSlot.InBag(bagIndex, slot),
                (child, slot) => child.name = $"Slot {slot + 1:D2}");

            if (Drag != null)
                tab.onClick.AddListener(() => SelectBag(bagIndex));
        }

        SelectBag(config.DefaultBagIndex);
    }

    private void SelectBag(int index)
    {
        Drag?.Cancel();
        selectedBag = index;
        for (var i = 0; i < bagCount; i++)
        {
            var selected = i == index;
            tabsRoot.GetChild(i).GetComponent<Button>().image.color = selected ? selectedTab : idleTab;
            scroll.viewport.GetChild(i).gameObject.SetActive(selected);
        }

        scroll.content = index >= 0 ? (RectTransform)scroll.viewport.GetChild(index) : null;
        scroll.StopMovement();
        if (scroll.content != null)
            scroll.verticalNormalizedPosition = 1;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (selectedBag >= 0)
            Drag.DropOnBag(selectedBag, eventData);
    }
}
