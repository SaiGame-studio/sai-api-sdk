using System;

[Serializable]
public class ShopItemProfile
{
    public string id;
    public string shop_id;
    public string item_profile_id;
    public float price_current;
    public float price_old;
    public int stock_quantity;
    public long created_at;
    public long updated_at;
    public ItemProfile item_profile;
}

[Serializable]
public class ShopItemProfileListResponse
{
    public string status;
    public string message;
    public string message_code;
    public ShopItemProfile[] data;
} 