using System;
using System.Collections.Generic;
using UnityEngine;

// <summary> DEMO 阶段统一的 Data 配置，后续再细拆 </summary>

// 背包枚举
public enum BagKind
{
    Field,          // 字段背包
    Facility,       // 实验设施
    Spell           // 符咒背包
}

// 控制背包物品卡片显示的内容。
public enum BagItemDisplay
{
    // 根据背包类型显示物品名称或字段标签和符号 - 会根据 BagKind 去设定 bool
    ByBagKind,

    // 显示物品名称
    Name,

    // 显示字段标签和符号
    FieldText
}

// 控制某个背包的物品能否放入合成台 - 跟随背包 - TODO：涉及到其他背包、其他场景，后续补充
public enum BagWorkbenchAccess
{
    ByBagKind,

    // 禁止该背包的物品放入合成台
    Blocked,

    // 允许该背包的物品放入合成台的空格
    Allowed
}


// Item data
[Serializable]
public sealed class AlchemyItem
{
    [Tooltip("物品的唯一 ID，供初始库存、配方和库存操作引用")]
    public string id;

    public BagKind kind;                        // 物品所属背包类型

    [Tooltip("名称显示模式下使用的物品名称")]
    public string name;                         // 物品名称 - 量杯 坩埚

    [Tooltip("字段卡片上方显示的小字")]
    public string fieldLabel = "未知字段";       // 字段 使用
    
    [Tooltip("字段卡片上显示的符号，如 &&&&")]
    public string fieldText;                    // 字段 使用
}


// 一条初始库存记录，物品定义统一从目录读取
[Serializable]
public sealed class BagStartingItem
{
    public string itemId;
    [Min(1)] public int quantity = 1;       // 初始库存数量 1

    // 旧配置曾直接把物品定义存在初始库存里
    [SerializeField, HideInInspector] private string name;
    [SerializeField, HideInInspector] private string fieldLabel = "未知字段";
    [SerializeField, HideInInspector] private string fieldText;

    internal AlchemyItem ReadLegacyItem(BagKind kind, int bagIndex, int slotIndex)
    {
        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(fieldText))
            return null;

        return new AlchemyItem
        {
            id = $"legacy:{bagIndex}:{slotIndex}",
            kind = kind,
            name = name,
            fieldLabel = fieldLabel,
            fieldText = fieldText
        };
    }
}

// 工作台槽位配置，配方引用的槽位 ID 可能和显示的 label 不同
[Serializable]
public sealed class WorkbenchSlotConfig
{
    [Tooltip("配方引用的槽位 ID，改名称或拖动排序时可以保留这个值")]
    public string id;               // eg: :"effect"
    public string label;            // eg: "效果". 外部中文显示
}

// 背包配置
[Serializable]
public sealed class BagConfig
{
    public BagKind kind;        // 背包类型
    public string label;        // 背包标签

    [Tooltip("背包的最少格数。初始物品超出时会增加格数")]
    [Min(1)] public int capacity = 5;

    public List<BagStartingItem> startingItems = new List<BagStartingItem>();       // 背包初始物品和数量
    
    [SerializeField] private BagItemDisplay itemDisplay;                // 背包物品卡片显示模式

    [SerializeField] private BagWorkbenchAccess workbenchAccess;        // 背包物品是否允许放入合成台 - Field 背包允许，其他背包禁止

    internal int SlotCount => Math.Max(capacity, startingItems.Count);

    // 如果设定为 ByBagKind，则读取 BagKind 来决定显示模式 - Field 背包显示字段标签和符号，其他背包显示物品名称
    internal bool ShowsFieldText => itemDisplay == BagItemDisplay.ByBagKind
        ? kind == BagKind.Field
        : itemDisplay == BagItemDisplay.FieldText;

    // 如果是 Field 背包，bool 为 true - 允许放入工作台
    internal bool AllowsWorkbench => workbenchAccess == BagWorkbenchAccess.ByBagKind
        ? kind == BagKind.Field
        : workbenchAccess == BagWorkbenchAccess.Allowed;
}


// 在 Inspector 中维护的启动配置，用于初始化背包和合成台
[CreateAssetMenu(menuName = "Alchemy/Demo Config")]
public sealed class AlchemyDemoConfig : ScriptableObject
{
    public AlchemyItem[] items = Array.Empty<AlchemyItem>();            // 初始物品配置

    public BagConfig[] bags = Array.Empty<BagConfig>();                 // 背包List配置 - 后续增加背包在此修改

    public WorkbenchSlotConfig[] workbench = Array.Empty<WorkbenchSlotConfig>();    // 工作台槽位List - 后续增加槽位在此修改

    public SmeltingRecipe[] recipes = Array.Empty<SmeltingRecipe>();        // 配方List配置 - 在 Array 中添加 SO 配置

    [Tooltip("未匹配配方时加入符咒背包的产物 ID")]
    public string failedSpellId;

    [Tooltip("按背包列表顺序从 0 开始。-1 时优先打开字段背包")]
    [SerializeField, Min(-1)] private int defaultBagIndex = -1;

    internal int DefaultBagIndex
    {
        get
        {
            if (bags.Length == 0)
                return -1;

            if (defaultBagIndex >= 0 && defaultBagIndex < bags.Length)
                return defaultBagIndex;

            return Math.Max(0, Array.FindIndex(bags, bag => bag.kind == BagKind.Field));
        }
    }
}
