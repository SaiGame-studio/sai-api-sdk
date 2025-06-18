using System;

[Serializable]
public class AuthResponse
{
    public bool success;
    public string message;
    public UserData user;
    public string token;
    public long expires_at;    // Unix timestamp
    public int expires_in;     // Seconds until expiration
} 