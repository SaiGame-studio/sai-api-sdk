namespace SaiGame.Enums
{
    /// <summary>
    /// Enum để chọn loại storage cho token
    /// </summary>
    public enum TokenStorageType
    {
        EncryptedPlayerPrefs,  // Mặc định - sử dụng PlayerPrefs với mã hóa
        EncryptedFile          // Sử dụng File system với mã hóa
    }
} 