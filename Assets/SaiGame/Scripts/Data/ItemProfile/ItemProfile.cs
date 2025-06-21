using System;

[Serializable]
public class CustomData
{
    public int hp_max;
    public int hp_current;
}

[Serializable]
public class ItemProfile
{
    public string id;
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
    public CustomData custom_data;
    // Note: 'custom_data' in the JSON can be an object or an empty array [].
    // JsonUtility might have trouble with this. If so, we may need to use a different JSON parser or a custom converter.
    // For now, let's assume it's consistently an object when not null.
} 