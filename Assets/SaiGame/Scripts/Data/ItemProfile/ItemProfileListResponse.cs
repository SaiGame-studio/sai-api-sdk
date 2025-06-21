using System;
using System.Collections.Generic;

[Serializable]
public class ItemProfileListResponse
{
    public string status;
    public string message;
    public string message_code;
    public List<ItemProfile> data;
} 