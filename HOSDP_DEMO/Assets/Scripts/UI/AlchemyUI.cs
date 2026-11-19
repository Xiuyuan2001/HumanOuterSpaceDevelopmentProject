using System;
using UnityEngine;

[RequireComponent(typeof(UIManager))]
public sealed class AlchemyUI : MonoBehaviour
{
    [SerializeField] private AlchemySession session;
    [SerializeField] private InventoryUI inventory;
    [SerializeField] private WorkbenchUI workbench;
    [SerializeField] private ItemDragUI drag;

    public InventoryState Inventory { get; private set; }

    private void Start()
    {
        Inventory = session.Inventory;
        drag.Initialize(Inventory);
        BuildViews(session.Config, Inventory, drag, (prefab, parent) => Instantiate(prefab, parent));
        workbench.InitializeSmelting(session.Smelting);
        Inventory.Changed += RefreshViews;

        var ui = GetComponent<UIManager>();
        ui.Show(inventory);
        ui.Show(workbench);
    }

    private void BuildViews(
        AlchemyDemoConfig config, InventoryState inventoryState, ItemDragUI interaction,
        Func<GameObject, Transform, GameObject> spawn)
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
    public AlchemyDemoConfig PreviewConfig => session.Config;

    public void BuildPreview(InventoryState previewInventory, Func<GameObject, Transform, GameObject> spawn) =>
        BuildViews(PreviewConfig, previewInventory, null, spawn);
#endif
}
