using System;
using System.Collections.Generic;

[Serializable]
public class ItemProfileResponse
{
    public string status;
    public string message;
    public string message_code;
    public ItemProfileData[] data;
}

[Serializable]
public class ItemProfileData
{
    public string shop_id;
    public string game_id;
    public string item_profile_id;
    public long updated_at;
    public long created_at;
    public float price_current;
    public float price_old;
    public string id;
    public ItemProfile item_profile;
}

[Serializable]
public class ItemProfile
{
    public string name;
    public string code_name;
    public string status;
    public string description;
    public int create_on_registry;
    public int amount_on_registry;
    public string type;
    public int level_start;
    public int level_max;
    public int stackable;
    public int stack_limit;
    public string game_id;
    public long updated_at;
    public long created_at;
}

[Serializable]
public class ItemProfileListResponse
{
    public string status;
    public string message;
    public string message_code;
    public List<ItemProfileSimple> data;
}

[Serializable]
public class ItemProfileSimple
{
    public string name;
    public string code_name;
    public string status;
    public string description;
    public int create_on_registry;
    public int amount_on_registry;
    public string type;
    public int level_start;
    public int level_max;
    public int stackable;
    public int stack_limit;
    public string game_id;
    public long updated_at;
    public long created_at;
    public string inventory_profile_id;
    public string id;
}
