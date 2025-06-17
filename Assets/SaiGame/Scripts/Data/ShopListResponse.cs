using System;
using System.Collections.Generic;

[Serializable]
public class ShopListResponse
{
    public string status;
    public string message;
    public string message_code;
    public List<ShopData> data;
} 