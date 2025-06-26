using System.ComponentModel;

public enum ItemType
{
    [Description("char_profile")]
    CharProfile,
    [Description("equipment")]
    Equipment,
    [Description("quest_item")]
    QuestItem,
    [Description("inventory")]
    Inventory,
    [Description("currency")]
    Currency,
    [Description("misc")]
    Misc,
    [Description("loot_box")]
    LootBox,
    [Description("fix_loot_box")]
    FixLootBox
}

public static class ItemTypeExtensions
{
    public static string ToItemTypeString(this ItemType type)
    {
        var fi = type.GetType().GetField(type.ToString());
        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : type.ToString();
    }

    public static bool TryParseItemType(string value, out ItemType result)
    {
        // Chỉ parse theo tên enum, không dùng Description
        return System.Enum.TryParse<ItemType>(value, true, out result);
    }
}
