/// <summary>
/// Interface định nghĩa các phương thức lưu trữ token với tính đa hình
/// Cho phép dễ dàng thay đổi phương thức lưu token (PlayerPrefs, File, Database, etc.)
/// </summary>
public interface ITokenStorage
{
    /// <summary>
    /// Lưu token đã được mã hóa
    /// </summary>
    /// <param name="token">Token cần lưu</param>
    void SaveToken(string token);
    
    /// <summary>
    /// Lấy và giải mã token
    /// </summary>
    /// <returns>Token đã được giải mã, hoặc string rỗng nếu không có</returns>
    string GetToken();
    
    /// <summary>
    /// Xóa token đã lưu
    /// </summary>
    void ClearToken();
    
    /// <summary>
    /// Kiểm tra xem có token được lưu hay không
    /// </summary>
    /// <returns>True nếu có token, False nếu không</returns>
    bool HasToken();
} 