[System.Serializable]
public class UserData
{
    public string email;
    public string password;
    public string password_confirmation;
    public string token;
    public int id;
    public string name;
    
    public UserData()
    {
        
    }
    
    public UserData(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
    
    public UserData(string email, string password, string passwordConfirmation)
    {
        this.email = email;
        this.password = password;
        this.password_confirmation = passwordConfirmation;
    }
}

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string password;
    public string password_confirmation;
}

[System.Serializable]
public class AuthResponse
{
    public bool success;
    public string message;
    public UserData user;
    public string token;
    public long expires_at;    // Unix timestamp
    public int expires_in;     // Seconds until expiration
}

[System.Serializable]
public class TokenResponse
{
    public string token;
    public long expires_at;    // Unix timestamp
    public int expires_in;     // Seconds until expiration
}

[System.Serializable]
public class TokenInfoResponse
{
    public string token_name;
    public string[] abilities;
    public long created_at;
    public long expires_at;
    public long last_used_at;
    public bool is_expired;
    
    /// <summary>
    /// Kiểm tra token có hợp lệ không
    /// </summary>
    /// <returns>True nếu token hợp lệ</returns>
    public bool IsValid()
    {
        // Kiểm tra is_expired flag
        if (is_expired)
            return false;
            
        // Kiểm tra thời gian hết hạn (Unix timestamp)
        if (expires_at > 0)
        {
            var currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (currentTime >= expires_at)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Lấy thời gian còn lại của token (giây)
    /// </summary>
    /// <returns>Số giây còn lại, hoặc -1 nếu đã hết hạn</returns>
    public long GetRemainingSeconds()
    {
        if (is_expired)
            return -1;
            
        if (expires_at > 0)
        {
            var currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return expires_at - currentTime;
        }
        
        return long.MaxValue; // Token không có thời gian hết hạn
    }
}

[System.Serializable]
public class LogoutResponse
{
    public string message;
}

[System.Serializable]
public class UserProfileResponse
{
    public string status;
    public string message;
    public string message_code;
    public UserProfileData data;
}

[System.Serializable]
public class UserProfileData
{
    public UserProfile user;
}

[System.Serializable]
public class UserProfile
{
    public int id;
    public string name;
    public string email;
    public string email_verified_at;
    public long created_at;
    public long updated_at;
} 