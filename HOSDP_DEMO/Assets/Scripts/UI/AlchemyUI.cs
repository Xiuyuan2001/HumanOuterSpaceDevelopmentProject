using System;
using UnityEngine;

[RequireComponent(typeof(UIManager))]
public sealed class AlchemyUI : MonoBehaviour
{
    [SerializeField] private AlchemyDemoConfig config;
    [SerializeField] private InventoryUI inventory;
    [SerializeField] private WorkbenchUI workbench;
    [SerializeField] private ItemDragUI drag;

    // 运行时玩法可以通过 Inventory 增加、移除和使用物品
    public InventoryState Inventory { get; private set; }

    private void Awake()
    {
        Inventory = new InventoryState(config);
        drag.Initialize(Inventory);
        BuildViews(Inventory, drag, (prefab, parent) => Instantiate(prefab, parent));
        workbench.InitializeSmelting(new SmeltingService(config, Inventory));
        Inventory.Changed += RefreshViews;

        var ui = GetComponent<UIManager>();
        ui.Show(inventory);
        ui.Show(workbench);
    }

    private void BuildViews(
        InventoryState inventoryState, ItemDragUI interaction, Func<GameObject, Transform, GameObject> spawn)
    {
        inventory.Build(config, inventoryState, interaction, spawn);
        workbench.Build(config, inventoryState, interaction, spawn);
    }

    private void RefreshViews()
    {
        inventory.Refresh();
        workbench.Refresh();
    }

    private void OnDestroy()
    {
        if (Inventory != null)
            Inventory.Changed -= RefreshViews;
    }

#if UNITY_EDITOR
    public void BuildPreview(Func<GameObject, Transform, GameObject> spawn)
    {
        BuildViews(new InventoryState(config), null, spawn);
    }
#endif
}
