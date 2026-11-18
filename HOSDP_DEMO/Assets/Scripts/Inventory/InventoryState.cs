using System;
using System.Collections.Generic;

internal readonly struct ItemSlot
{
    private const int WorkbenchBag = -1;

    private ItemSlot(int bag, int index)
    {
        Bag = bag;
        Index = index;
    }

    public int Bag { get; }
    public int Index { get; }
    public bool IsWorkbench => Bag == WorkbenchBag;

    public static ItemSlot InBag(int bag, int index) => new ItemSlot(bag, index);
    public static ItemSlot OnWorkbench(int index) => new ItemSlot(WorkbenchBag, index);
    public bool IsSameSlot(ItemSlot other) => Bag == other.Bag && Index == other.Index;
}

internal sealed class InventoryEntry
{
    public InventoryEntry(AlchemyItem item, ItemSlot home, BagConfig bag, int quantity)
        : this(item, home, bag.ShowsFieldText, bag.AllowsWorkbench, quantity)
    {
    }

    private InventoryEntry(AlchemyItem item, ItemSlot home, bool showsFieldText, bool allowsWorkbench, int quantity)
    {
        Item = item;
        Home = home;
        ShowsFieldText = showsFieldText;
        AllowsWorkbench = allowsWorkbench;
        Quantity = quantity;
    }

    public AlchemyItem Item { get; }
    public ItemSlot Home { get; }
    public bool ShowsFieldText { get; }
    public bool AllowsWorkbench { get; }
    public int Quantity { get; set; }

    public InventoryEntry WithQuantity(int quantity) =>
        new InventoryEntry(Item, Home, ShowsFieldText, AllowsWorkbench, quantity);
}

// 本次运行的库存和工作台托管材料，配置资源只提供初始数据
public sealed class InventoryState
{
    private readonly BagConfig[] bagConfigs;
    private readonly Dictionary<string, AlchemyItem> catalog = new Dictionary<string, AlchemyItem>();
    private readonly InventoryEntry[][] bags;
    private readonly WorkbenchStack[] workbench;

    public InventoryState(AlchemyDemoConfig config)
    {
        bagConfigs = config.bags;
        foreach (var item in config.items)
            if (item != null && !string.IsNullOrWhiteSpace(item.id))
                catalog[item.id] = item;

        bags = new InventoryEntry[bagConfigs.Length][];
        for (var bagIndex = 0; bagIndex < bags.Length; bagIndex++)
        {
            var bag = bagConfigs[bagIndex];
            bags[bagIndex] = new InventoryEntry[bag.SlotCount];
            for (var i = 0; i < bag.startingItems.Count; i++)
            {
                var seed = bag.startingItems[i];
                if (seed == null)
                    continue;

                var item = string.IsNullOrEmpty(seed.itemId)
                    ? seed.ReadLegacyItem(bag.kind, bagIndex, i)
                    : FindItem(seed.itemId);
                if (item == null || item.kind != bag.kind)
                    continue;

                catalog[item.id] = item;
                bags[bagIndex][i] = new InventoryEntry(item, ItemSlot.InBag(bagIndex, i), bag, Math.Max(1, seed.quantity));
            }
        }

        workbench = new WorkbenchStack[config.GetWorkbenchSlots().Count];
    }

    public event Action Changed;

    // 库存查询、移除和使用仅作用于背包中的数量
    public long GetQuantity(string itemId)
    {
        long quantity = 0;
        foreach (var bag in bags)
            foreach (var stack in bag)
                if (stack != null && stack.Item.id == itemId)
                    quantity += stack.Quantity;

        return quantity;
    }

    public bool TryAdd(string itemId, int quantity = 1)
    {
        var item = FindItem(itemId);
        if (item == null || quantity < 1 || !TryFindAddSlot(item, quantity, out var target))
            return false;

        AddToBagSlot(target, item, quantity);
        Changed?.Invoke();
        return true;
    }

    public bool TryRemove(string itemId, int quantity = 1)
    {
        if (quantity < 1 || GetQuantity(itemId) < quantity)
            return false;

        foreach (var bag in bags)
        {
            for (var i = 0; i < bag.Length && quantity > 0; i++)
            {
                var stack = bag[i];
                if (stack == null || stack.Item.id != itemId)
                    continue;

                var removed = Math.Min(stack.Quantity, quantity);
                stack.Quantity -= removed;
                quantity -= removed;
                if (stack.Quantity == 0)
                    bag[i] = null;
            }
        }

        Changed?.Invoke();
        return true;
    }

    // 供道具使用入口在效果执行成功后扣减库存
    public bool TryUse(string itemId, int quantity = 1) => TryRemove(itemId, quantity);

    internal InventoryEntry GetItem(ItemSlot slot) =>
        slot.IsWorkbench ? workbench[slot.Index]?.DisplayEntry : bags[slot.Bag][slot.Index];

    internal InventoryEntry GetMovableItem(ItemSlot slot)
    {
        var entry = GetItem(slot);
        return entry?.AllowsWorkbench == true ? entry : null;
    }

    internal bool TryPlace(ItemSlot source, int target)
    {
        if (source.IsWorkbench && source.Index == target)
            return false;

        var entry = GetMovableItem(source);
        var targetStack = workbench[target];
        if (entry == null)
            return false;

        var quantity = source.IsWorkbench ? entry.Quantity : 1;
        if (targetStack != null && !targetStack.CanAdd(entry.Item, quantity))
            return false;

        if (source.IsWorkbench)
        {
            if (targetStack == null)
                workbench[target] = workbench[source.Index];
            else
                targetStack.Merge(workbench[source.Index]);
            workbench[source.Index] = null;
        }
        else
        {
            if (targetStack == null)
                workbench[target] = targetStack = new WorkbenchStack();
            targetStack.Add(entry, quantity);

            entry.Quantity -= quantity;
            if (entry.Quantity == 0)
                bags[source.Bag][source.Index] = null;
        }

        Changed?.Invoke();
        return true;
    }

    internal bool TryReturn(ItemSlot source, int targetBag)
    {
        if (!source.IsWorkbench)
            return false;

        var stack = workbench[source.Index];
        if (stack == null)
            return false;

        var canReturn = false;
        foreach (var portion in stack.Portions)
        {
            if (portion.Home.Bag != targetBag)
                continue;

            var existing = bags[targetBag][portion.Home.Index];
            if (existing != null && int.MaxValue - existing.Quantity < portion.Quantity)
                return false;
            canReturn = true;
        }

        if (!canReturn)
            return false;

        for (var i = stack.Portions.Count - 1; i >= 0; i--)
        {
            var portion = stack.Portions[i];
            if (portion.Home.Bag == targetBag)
            {
                AddToBagSlot(portion.Home, portion.Item, portion.Quantity);
                stack.RemovePortionAt(i);
            }
        }

        if (stack.Quantity == 0)
            workbench[source.Index] = null;
        Changed?.Invoke();
        return true;
    }

    internal bool TryCommitSmelt(int[] ingredients, string outputId, int quantity, out AlchemyItem output)
    {
        output = FindItem(outputId);
        // 先为产物找位置，再一次性扣材料并增加产物
        if (output == null || output.kind != BagKind.Spell || quantity < 1 ||
            !TryFindAddSlot(output, quantity, out var target))
            return false;

        for (var i = 0; i < ingredients.Length; i++)
        {
            var amount = ingredients[i];
            if (amount == 0)
                continue;

            var stack = workbench[i];
            stack.Consume(amount);
            if (stack.Quantity == 0)
                workbench[i] = null;
        }

        AddToBagSlot(target, output, quantity);
        Changed?.Invoke();
        return true;
    }

    private AlchemyItem FindItem(string itemId) =>
        itemId != null && catalog.TryGetValue(itemId, out var item) ? item : null;

    private bool TryFindAddSlot(AlchemyItem item, int quantity, out ItemSlot target)
    {
        ItemSlot? free = null;
        for (var bag = 0; bag < bags.Length; bag++)
        {
            if (bagConfigs[bag].kind != item.kind)
                continue;

            for (var i = 0; i < bags[bag].Length; i++)
            {
                var slot = ItemSlot.InBag(bag, i);
                var existing = bags[bag][i];
                if (existing != null && existing.Item.id == item.id && int.MaxValue - existing.Quantity >= quantity)
                {
                    target = slot;
                    return true;
                }

                if (free == null && existing == null && !IsReservedForAnotherItem(slot, item.id))
                    free = slot;
            }
        }

        target = free.GetValueOrDefault();
        return free.HasValue;
    }

    // 台上的字段保留来源位置，避免中途入包的其他物品占用原格
    private bool IsReservedForAnotherItem(ItemSlot slot, string itemId)
    {
        foreach (var stack in workbench)
            if (stack != null && stack.Item.id != itemId && stack.ContainsHome(slot))
                return true;
        return false;
    }

    private void AddToBagSlot(ItemSlot target, AlchemyItem item, int quantity)
    {
        var existing = bags[target.Bag][target.Index];
        if (existing == null)
            bags[target.Bag][target.Index] = new InventoryEntry(item, target, bagConfigs[target.Bag], quantity);
        else
            existing.Quantity += quantity;
    }
}
