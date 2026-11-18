using System;
using UnityEngine;

// 一条材料要求对应一个工作台槽位
[Serializable]
public sealed class SmeltingIngredient
{
    public string slotId;
    public string itemId;
    [Min(1)] public int quantity = 1;
}

[CreateAssetMenu(menuName = "Alchemy/Smelting Recipe")]
public sealed class SmeltingRecipe : ScriptableObject
{
    public SmeltingIngredient[] ingredients = Array.Empty<SmeltingIngredient>();
    public string resultItemId;
    [Min(1)] public int resultQuantity = 1;
}
