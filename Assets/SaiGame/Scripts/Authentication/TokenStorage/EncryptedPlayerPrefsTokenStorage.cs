using UnityEngine;

/// <summary>
/// Implementation của ITokenStorage sử dụng PlayerPrefs với mã hóa
/// Lưu trữ token được mã hóa trong PlayerPrefs của Unity
/// </summary>
public class EncryptedPlayerPrefsTokenStorage : ITokenStorage
{
    private const string TOKEN_KEY = "encrypted_auth_token";
    private const string TOKEN_TIMESTAMP_KEY = "token_timestamp";
    
    // Thời gian token hết hạn (7 ngày)
    private const double TOKEN_EXPIRY_DAYS = 7.0;
    
    /// <summary>
    /// Lưu token với mã hóa vào PlayerPrefs
    /// </summary>
    /// <param name="token">Token gốc cần lưu</param>
    public void SaveToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Cannot save empty token");
            return;
        }
        
        try
        {
            // Mã hóa token
            string encryptedToken = TokenEncryption.Encrypt(token);
            
            if (!string.IsNullOrEmpty(encryptedToken))
            {
                // Lưu token đã mã hóa
                PlayerPrefs.SetString(TOKEN_KEY, encryptedToken);
                
                // Lưu timestamp để quản lý thời gian hết hạn
                double currentTime = System.DateTime.Now.ToBinary();
                PlayerPrefs.SetString(TOKEN_TIMESTAMP_KEY, currentTime.ToString());
                
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError("Failed to encrypt token");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving token: {e.Message}");
        }
    }
    
    /// <summary>
    /// Lấy và giải mã token từ PlayerPrefs
    /// </summary>
    /// <returns>Token gốc đã được giải mã</returns>
    public string GetToken()
    {
        try
        {
            // Kiểm tra token có hết hạn không
            if (IsTokenExpired())
            {
                ClearToken();
                return string.Empty;
            }
            
            string encryptedToken = PlayerPrefs.GetString(TOKEN_KEY, string.Empty);
            
            if (string.IsNullOrEmpty(encryptedToken))
            {
                return string.Empty;
            }
            
            // Giải mã token
            string decryptedToken = TokenEncryption.Decrypt(encryptedToken);
            
            if (string.IsNullOrEmpty(decryptedToken))
            {
                Debug.LogWarning("Failed to decrypt token, clearing corrupted data");
                ClearToken();
                return string.Empty;
            }
            
            return decryptedToken;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting token: {e.Message}");
            ClearToken(); // Xóa dữ liệu lỗi
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Xóa token đã lưu
    /// </summary>
    public void ClearToken()
    {
        try
        {
            PlayerPrefs.DeleteKey(TOKEN_KEY);
            PlayerPrefs.DeleteKey(TOKEN_TIMESTAMP_KEY);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error clearing token: {e.Message}");
        }
    }
    
    /// <summary>
    /// Kiểm tra xem có token được lưu hay không
    /// </summary>
    /// <returns>True nếu có token hợp lệ</returns>
    public bool HasToken()
    {
        try
        {
            if (IsTokenExpired())
            {
                return false;
            }
            
            string encryptedToken = PlayerPrefs.GetString(TOKEN_KEY, string.Empty);
            return !string.IsNullOrEmpty(encryptedToken) && 
                   TokenEncryption.IsValidEncryptedToken(encryptedToken);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error checking token: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Kiểm tra token có hết hạn không
    /// </summary>
    /// <returns>True nếu token đã hết hạn</returns>
    private bool IsTokenExpired()
    {
        try
        {
            string timestampString = PlayerPrefs.GetString(TOKEN_TIMESTAMP_KEY, string.Empty);
            
            if (double.TryParse(timestampString, out double timestamp))
            {
                System.DateTime savedTime = System.DateTime.FromBinary((long)timestamp);
                System.TimeSpan timeDiff = System.DateTime.Now - savedTime;
                
                return timeDiff.TotalDays > TOKEN_EXPIRY_DAYS;
            }
            
            return true; // Parse lỗi = hết hạn
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error checking token expiry: {e.Message}");
            return true; // Lỗi = hết hạn
        }
    }
    
    /// <summary>
    /// Lấy thời gian còn lại của token (cho debug)
    /// </summary>
    /// <returns>Số ngày còn lại</returns>
    public double GetTokenRemainingDays()
    {
        try
        {
            string timestampString = PlayerPrefs.GetString(TOKEN_TIMESTAMP_KEY, string.Empty);
            
            if (string.IsNullOrEmpty(timestampString))
            {
                return 0;
            }
            
            if (double.TryParse(timestampString, out double timestamp))
            {
                System.DateTime savedTime = System.DateTime.FromBinary((long)timestamp);
                System.TimeSpan timeDiff = System.DateTime.Now - savedTime;
                
                return TOKEN_EXPIRY_DAYS - timeDiff.TotalDays;
            }
            
            return 0;
        }
        catch
        {
            return 0;
        }
    }
} 