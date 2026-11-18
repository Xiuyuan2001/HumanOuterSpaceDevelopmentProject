using System.Collections.Generic;

// 一个槽位堆叠同一种物品，并记录各份材料来自哪个背包格
internal sealed class WorkbenchStack
{
    private readonly List<InventoryEntry> portions = new List<InventoryEntry>();

    public IReadOnlyList<InventoryEntry> Portions => portions;
    public int Quantity { get; private set; }
    public AlchemyItem Item => portions[0].Item;
    public InventoryEntry DisplayEntry => portions[0].WithQuantity(Quantity);

    public bool CanAdd(AlchemyItem item, int quantity) => Item.id == item.id && int.MaxValue - Quantity >= quantity;

    public bool ContainsHome(ItemSlot home) => FindPortion(home) >= 0;

    public void Add(InventoryEntry entry, int quantity)
    {
        var i = FindPortion(entry.Home);
        if (i < 0)
            portions.Add(entry.WithQuantity(quantity));
        else
            portions[i].Quantity += quantity;

        Quantity += quantity;
    }

    public void Merge(WorkbenchStack source)
    {
        foreach (var portion in source.portions)
            Add(portion, portion.Quantity);
    }

    public void Consume(int quantity)
    {
        // 从最后放入的来源开始扣减，剩余材料仍可回原格
        while (quantity > 0)
        {
            var i = portions.Count - 1;
            var portion = portions[i];
            if (portion.Quantity <= quantity)
            {
                quantity -= portion.Quantity;
                RemovePortionAt(i);
            }
            else
            {
                portion.Quantity -= quantity;
                Quantity -= quantity;
                quantity = 0;
            }
        }
    }

    public void RemovePortionAt(int index)
    {
        Quantity -= portions[index].Quantity;
        portions.RemoveAt(index);
    }

    private int FindPortion(ItemSlot home)
    {
        for (var i = 0; i < portions.Count; i++)
            if (portions[i].Home.IsSameSlot(home))
                return i;
        return -1;
    }
}
