using System.IO;
using UnityEngine;

/// <summary>
/// Implementation thay thế của ITokenStorage sử dụng File System
/// Demo tính đa hình - có thể dễ dàng thay thế EncryptedPlayerPrefsTokenStorage
/// </summary>
public class EncryptedFileTokenStorage : ITokenStorage
{
    private readonly string tokenFilePath;
    private readonly string timestampFilePath;
    
    // Thời gian token hết hạn (7 ngày)
    private const double TOKEN_EXPIRY_DAYS = 7.0;
    
    public EncryptedFileTokenStorage()
    {
        // Lưu trong persistent data path của Unity
        string dataPath = Application.persistentDataPath;
        tokenFilePath = Path.Combine(dataPath, "encrypted_token.dat");
        timestampFilePath = Path.Combine(dataPath, "token_timestamp.dat");
    }
    
    /// <summary>
    /// Lưu token với mã hóa vào file
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
                // Lưu token đã mã hóa vào file
                File.WriteAllText(tokenFilePath, encryptedToken);
                
                // Lưu timestamp
                double currentTime = System.DateTime.Now.ToBinary();
                File.WriteAllText(timestampFilePath, currentTime.ToString());
                
                Debug.Log("Token saved and encrypted to file successfully");
            }
            else
            {
                Debug.LogError("Failed to encrypt token");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving token to file: {e.Message}");
        }
    }
    
    /// <summary>
    /// Lấy và giải mã token từ file
    /// </summary>
    /// <returns>Token gốc đã được giải mã</returns>
    public string GetToken()
    {
        try
        {
            // Kiểm tra token có hết hạn không
            if (IsTokenExpired())
            {
                Debug.Log("Token has expired");
                ClearToken();
                return string.Empty;
            }
            
            if (!File.Exists(tokenFilePath))
            {
                return string.Empty;
            }
            
            string encryptedToken = File.ReadAllText(tokenFilePath);
            
            if (string.IsNullOrEmpty(encryptedToken))
            {
                return string.Empty;
            }
            
            // Giải mã token
            string decryptedToken = TokenEncryption.Decrypt(encryptedToken);
            
            if (string.IsNullOrEmpty(decryptedToken))
            {
                Debug.LogWarning("Failed to decrypt token from file, clearing corrupted data");
                ClearToken();
                return string.Empty;
            }
            
            Debug.Log("Token retrieved and decrypted from file successfully");
            return decryptedToken;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting token from file: {e.Message}");
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
            if (File.Exists(tokenFilePath))
            {
                File.Delete(tokenFilePath);
            }
            
            if (File.Exists(timestampFilePath))
            {
                File.Delete(timestampFilePath);
            }
            
            Debug.Log("Token files cleared successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error clearing token files: {e.Message}");
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
            
            if (!File.Exists(tokenFilePath))
            {
                return false;
            }
            
            string encryptedToken = File.ReadAllText(tokenFilePath);
            return !string.IsNullOrEmpty(encryptedToken) && 
                   TokenEncryption.IsValidEncryptedToken(encryptedToken);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error checking token file: {e.Message}");
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
            if (!File.Exists(timestampFilePath))
            {
                return true; // Không có timestamp = hết hạn
            }
            
            string timestampString = File.ReadAllText(timestampFilePath);
            
            if (string.IsNullOrEmpty(timestampString))
            {
                return true;
            }
            
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
            Debug.LogError($"Error checking token expiry from file: {e.Message}");
            return true; // Lỗi = hết hạn
        }
    }
} 