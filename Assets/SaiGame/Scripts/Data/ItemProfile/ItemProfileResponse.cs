using System;

[Serializable]
public class ItemProfileResponse
{
    public string status;
    public string message;
    public string message_code;
    public ItemProfileData[] data;
} 