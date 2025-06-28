using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public static class ItemTypeFilter
{
    public static List<InventoryItem> FilterByType(List<InventoryItem> items, ItemType? type)
    {
        if (items == null) return new List<InventoryItem>();
        if (type == null) return items;
        string typeString = GetTypeString(type.Value);
        return items.Where(item => string.Equals(item.type, typeString, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public static string GetTypeString(ItemType type)
    {
        switch (type)
        {
            case ItemType.CharProfile: return "char_profile";
            case ItemType.Equipment: return "equipment";
            case ItemType.QuestItem: return "quest_item";
            case ItemType.Inventory: return "inventory";
            case ItemType.Currency: return "currency";
            case ItemType.Misc: return "misc";
            case ItemType.LootBox: return "loot_box";
            default: return string.Empty;
        }
    }
}
