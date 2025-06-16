using System;
using System.Text;
using UnityEngine;

/// <summary>
/// Class xử lý mã hóa và giải mã token 2 chiều
/// Sử dụng thuật toán XOR với key tùy chỉnh
/// </summary>
public static class TokenEncryption
{
    // Key mã hóa - có thể thay đổi theo yêu cầu bảo mật
    private static readonly string encryptionKey = "SaiGameToken2024!@#";

    /// <summary>
    /// Mã hóa token
    /// </summary>
    /// <param name="plainText">Token gốc cần mã hóa</param>
    /// <returns>Token đã được mã hóa dưới dạng Base64</returns>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);

            // Mã hóa XOR
            byte[] encryptedBytes = new byte[plainBytes.Length];
            for (int i = 0; i < plainBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(plainBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            // Chuyển đổi sang Base64 để lưu trữ
            string encrypted = Convert.ToBase64String(encryptedBytes);

            return encrypted;
        }
        catch (Exception e)
        {
            Debug.LogError($"Encryption failed: {e.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Giải mã token
    /// </summary>
    /// <param name="encryptedText">Token đã được mã hóa dưới dạng Base64</param>
    /// <returns>Token gốc đã được giải mã</returns>
    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        try
        {
            // Chuyển đổi từ Base64
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);

            // Giải mã XOR
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            string decrypted = Encoding.UTF8.GetString(decryptedBytes);

            return decrypted;
        }
        catch (Exception e)
        {
            Debug.LogError($"Decryption failed: {e.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của token đã mã hóa
    /// </summary>
    /// <param name="encryptedToken">Token đã mã hóa</param>
    /// <returns>True nếu token hợp lệ</returns>
    public static bool IsValidEncryptedToken(string encryptedToken)
    {
        if (string.IsNullOrEmpty(encryptedToken))
            return false;

        try
        {
            // Thử giải mã để kiểm tra tính hợp lệ
            string decrypted = Decrypt(encryptedToken);
            return !string.IsNullOrEmpty(decrypted);
        }
        catch
        {
            return false;
        }
    }
} 