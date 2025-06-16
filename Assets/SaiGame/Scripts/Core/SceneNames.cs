/// <summary>
/// Quản lý tập trung tất cả tên scene trong game
/// Sử dụng const string để dễ dàng bảo trì và tránh lỗi typo
/// </summary>
public static class SceneNames
{
    /// <summary>
    /// Scene đăng ký
    /// </summary>
    public const string REGISTER = "0_Register";

    /// <summary>
    /// Scene đăng nhập
    /// </summary>
    public const string LOGIN = "1_Login";
    
    /// <summary>
    /// Scene game chính
    /// </summary>
    public const string GAME = "Game";
    
    /// <summary>
    /// Scene menu chính
    /// </summary>
    public const string MAIN_MENU = "2_MainMenu";
    
    /// <summary>
    /// Scene shop
    /// </summary>
    public const string SHOP = "3_Shop";
    
    /// <summary>
    /// Scene my items
    /// </summary>
    public const string MY_ITEMS = "4_MyItems";
    
    /// <summary>
    /// Scene my character
    /// </summary>
    public const string MY_CHARACTER = "5_MyCharacter";
    
    // Thêm các scene khác khi cần
    // public const string PROFILE = "Profile";
    // public const string LEADERBOARD = "Leaderboard";
    
    /// <summary>
    /// Kiểm tra xem tên scene có hợp lệ không
    /// </summary>
    /// <param name="sceneName">Tên scene cần kiểm tra</param>
    /// <returns>True nếu scene name hợp lệ</returns>
    public static bool IsValidSceneName(string sceneName)
    {
        return !string.IsNullOrEmpty(sceneName) &&
               (sceneName == LOGIN ||
                sceneName == REGISTER ||
                sceneName == MAIN_MENU ||
                sceneName == SHOP ||
                sceneName == MY_ITEMS ||
                sceneName == MY_CHARACTER);
    }
    
    /// <summary>
    /// Lấy danh sách tất cả scene names
    /// </summary>
    /// <returns>Mảng chứa tất cả scene names</returns>
    public static string[] GetAllSceneNames()
    {
        return new string[]
        {
            LOGIN,
            REGISTER,
            MAIN_MENU,
            SHOP,
            MY_ITEMS,
            MY_CHARACTER
        };
    }
} 