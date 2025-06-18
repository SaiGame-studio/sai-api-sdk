using System;

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