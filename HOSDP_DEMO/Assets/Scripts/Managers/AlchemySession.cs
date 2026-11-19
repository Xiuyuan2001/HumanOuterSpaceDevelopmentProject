using UnityEngine;

public sealed class AlchemySession : MonoBehaviour
{
    [SerializeField] private AlchemyDemoConfig config;

    public InventoryState Inventory { get; private set; }
    internal AlchemyDemoConfig Config => config;
    internal SmeltingService Smelting { get; private set; }

    private void Awake()
    {
        Inventory = new InventoryState(config);
        Smelting = new SmeltingService(config, Inventory);
    }
}
