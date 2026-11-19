using System.Collections.Generic;

internal enum SmeltingResult
{
    Empty,
    Stored,
    OutputUnavailable
}

// 根据配置匹配配方，再把扣料和产物入包交给库存一次完成
internal sealed class SmeltingService
{
    private readonly AlchemyDemoConfig config;      // 获得当前的配方和工作台槽位配置
    private readonly InventoryState inventory;
    private readonly Dictionary<string, int> slotIndices = new Dictionary<string, int>();
    private readonly int slotCount;

    public SmeltingService(AlchemyDemoConfig config, InventoryState inventory)
    {
        this.config = config;
        this.inventory = inventory;
        var slots = config.workbench;
        slotCount = slots.Length;

        for (var i = 0; i < slotCount; i++)
        {
            var id = slots[i].id;
            if (!string.IsNullOrEmpty(id))
                slotIndices[id] = i;
        }
    }

    public SmeltingResult Smelt(out AlchemyItem output)
    {
        output = null;
        var contents = new InventoryEntry[slotCount];
        var placedQuantities = new int[slotCount];
        var occupied = 0;
        for (var i = 0; i < slotCount; i++)
        {
            var entry = inventory.GetItem(ItemSlot.OnWorkbench(i));
            contents[i] = entry;
            if (entry != null)
            {
                placedQuantities[i] = entry.Quantity;
                occupied++;
            }
        }

        if (occupied == 0)
            return SmeltingResult.Empty;

        foreach (var recipe in config.recipes)
        {
            if (recipe != null && TryMatch(recipe, contents, occupied, out var costs))
                return StoreResult(costs, recipe.resultItemId, recipe.resultQuantity, out output);
        }

        // 未匹配配方时扣掉整个工作台的材料
        return StoreResult(placedQuantities, config.failedSpellId, 1, out output);
    }

    private bool TryMatch(SmeltingRecipe recipe, InventoryEntry[] contents, int occupied, out int[] costs)
    {
        costs = null;
        // 配方需要覆盖所有已摆放的槽位，同槽超出的数量可以保留
        if (recipe.ingredients.Length != occupied)
            return false;

        var quantities = new int[slotCount];
        foreach (var ingredient in recipe.ingredients)
        {
            if (ingredient == null || ingredient.quantity < 1 || ingredient.slotId == null ||
                !slotIndices.TryGetValue(ingredient.slotId, out var index) || quantities[index] > 0)
                return false;

            var entry = contents[index];
            if (entry == null || entry.Item.id != ingredient.itemId || entry.Quantity < ingredient.quantity)
                return false;

            quantities[index] = ingredient.quantity;
        }

        costs = quantities;
        return true;
    }

    private SmeltingResult StoreResult(int[] costs, string itemId, int quantity, out AlchemyItem output) =>
        inventory.TryCommitSmelt(costs, itemId, quantity, out output)
            ? SmeltingResult.Stored
            : SmeltingResult.OutputUnavailable;
}
