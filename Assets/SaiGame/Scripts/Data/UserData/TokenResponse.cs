using System;

[Serializable]
public class TokenResponse
{
    public string token;
    public long expires_at;    // Unix timestamp
    public int expires_in;     // Seconds until expiration
} 