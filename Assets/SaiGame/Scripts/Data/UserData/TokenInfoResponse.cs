using System;

[Serializable]
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