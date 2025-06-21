using System;
using System.Collections.Generic;

[Serializable]
public class InventoryItemCustomData
{
    public int hp_max;
    public int hp_current;
}

[Serializable]
public class InventoryItem
{
    public string name;
    public string description;
    public InventoryItemCustomData custom_data;
    public string type;
    public int create_on_registry;
    public int amount_on_registry;
    public int level_max;
    public int stackable;
    public int stack_limit;
    public int amount;
    public string game_id;
    public string user_profile_id;
    public string item_profile_id;
    public long created_at;
    public long updated_at;
    public string inventory_item_id;
    public string id;
}

[Serializable]
public class InventoryItemsResponse
{
    public string status;
    public string message;
    public string message_code;
    public List<InventoryItem> data;
} 