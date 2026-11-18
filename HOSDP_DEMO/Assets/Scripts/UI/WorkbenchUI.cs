using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WorkbenchUI : SlotPanelUI
{
    [SerializeField] private RectTransform rowsRoot;
    [SerializeField] private RectTransform rowPrefab;
    [SerializeField] private Button smeltButton;
    [SerializeField] private TMP_Text smeltStatus;

    private SmeltingService smelting;

    protected override void BuildLayout(AlchemyDemoConfig config, Func<GameObject, Transform, GameObject> spawn)
    {
        var slots = config.GetWorkbenchSlots();
        BuildSlots(rowsRoot, rowPrefab, slots.Count, spawn, ItemSlot.OnWorkbench, (row, index) =>
        {
            var label = slots[index].label;
            row.name = $"{index + 1:D2} {label}";
            row.GetComponent<TMP_Text>().text = label;
        });
    }

    internal void InitializeSmelting(SmeltingService service)
    {
        smelting = service;
        smeltButton.onClick.AddListener(Smelt);
    }

    private void Smelt()
    {
        Drag.Cancel();
        var result = smelting.Smelt(out var output);
        smeltStatus.text = result switch
        {
            SmeltingResult.Stored => $"已入符咒背包  {output.name}",
            SmeltingResult.Empty => "请先放入材料",
            _ => "符咒无法入包，材料已保留"
        };
    }

    private void OnDestroy() => smeltButton.onClick.RemoveListener(Smelt);
}
