using UnityEngine;

/// <summary>
/// Ví dụ về cách sử dụng hệ thống quản lý scene mới
/// File này chỉ để tham khảo, có thể xóa sau khi hiểu cách sử dụng
/// </summary>
public class SceneManagementExample : MonoBehaviour
{
    void Start()
    {
        // Đăng ký events để theo dõi việc chuyển scene
        SceneController.OnSceneLoadStarted += OnSceneLoadStarted;
        SceneController.OnSceneLoadCompleted += OnSceneLoadCompleted;
    }
    
    void OnDestroy()
    {
        // Hủy đăng ký events
        SceneController.OnSceneLoadStarted -= OnSceneLoadStarted;
        SceneController.OnSceneLoadCompleted -= OnSceneLoadCompleted;
    }
    
    private void OnSceneLoadStarted(string sceneName)
    {
        Debug.Log($"Bắt đầu load scene: {sceneName}");
        // Có thể hiển thị loading UI ở đây
    }
    
    private void OnSceneLoadCompleted(string sceneName)
    {
        Debug.Log($"Hoàn thành load scene: {sceneName}");
        // Có thể ẩn loading UI ở đây
    }
    
    // CÁCH SỬ DỤNG CŨ (KHÔNG NÊN):
    [ContextMenu("Old Way - Not Recommended")]
    public void ExampleOldWay()
    {
        // ❌ Cách cũ - sử dụng string trực tiếp
        // UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
        // UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        
        // ❌ Dễ bị lỗi typo
        // UnityEngine.SceneManagement.SceneManager.LoadScene("Lgoin"); // Lỗi chính tả!
    }
    
    // CÁCH SỬ DỤNG MỚI (KHUYẾN KHÍCH):
    [ContextMenu("New Way - Recommended")]
    public void ExampleNewWay()
    {
        // ✅ Cách mới - sử dụng SceneNames constants
        // Autocomplete sẽ gợi ý và tránh lỗi typo
        SceneController.LoadScene(SceneNames.LOGIN);
        
        // ✅ Hoặc sử dụng Quick Navigation
        SceneController.QuickNavigation.GoToLogin();
        SceneController.QuickNavigation.GoToMainMenu();
        
        // ✅ Hoặc Quick Navigation async
        SceneController.QuickNavigation.GoToMainMenuAsync((success) => {
            Debug.Log($"Game scene load result: {success}");
        });
    }
    
    [ContextMenu("Scene Utilities")]
    public void ExampleUtilities()
    {
        // Kiểm tra scene hiện tại
        string currentScene = SceneController.GetCurrentSceneName();
        Debug.Log($"Scene hiện tại: {currentScene}");
        
        // Kiểm tra xem có đang ở scene login không
        bool isInLogin = SceneController.IsCurrentScene(SceneNames.LOGIN);
        Debug.Log($"Đang ở scene login: {isInLogin}");
        
        // Reload scene hiện tại
        SceneController.ReloadCurrentScene();
        
        // Kiểm tra tên scene có hợp lệ không
        bool isValid = SceneNames.IsValidSceneName("Login");
        Debug.Log($"'Login' là scene name hợp lệ: {isValid}");
        
        // Lấy tất cả scene names
        string[] allScenes = SceneNames.GetAllSceneNames();
        Debug.Log($"Tất cả scene names: {string.Join(", ", allScenes)}");
    }
    
    // Ví dụ sử dụng trong các Manager classes
    public void ExampleInManagers()
    {
        // Thay vì:
        // public string loginScene = "Login";
        // SceneManager.LoadScene(loginScene);
        
        // Bây giờ có thể sử dụng:
        // public string loginScene = SceneNames.LOGIN;
        // SceneController.LoadScene(loginScene);
        
        // Hoặc trực tiếp:
        // SceneController.QuickNavigation.GoToLogin();
    }
}

/* 
LỢI ÍCH CỦA HỆ THỐNG MỚI:

1. TRÁNH LỖI TYPO:
   - SceneNames.LOGIN thay vì "Login"
   - Autocomplete của IDE sẽ gợi ý
   - Compiler sẽ báo lỗi nếu sai tên

2. DỄ DÀNG REFACTOR:
   - Muốn đổi "Login" thành "LoginScene"?
   - Chỉ cần sửa 1 chỗ trong SceneNames.cs
   - Tất cả code khác tự động cập nhật

3. KIỂM SOÁT TẬP TRUNG:
   - Tất cả scene names ở một nơi
   - Dễ dàng xem có bao nhiêu scene
   - Validation tự động

4. AN TOÀN HỌN:
   - SceneController có error handling
   - Logging để debug
   - Events để theo dõi

5. HIỆU NĂNG TỐT HƠN:
   - Hỗ trợ async loading
   - Loading progress
   - Memory management tốt hơn

CÁCH SỬ DỤNG NHANH:
- Thay SceneManager.LoadScene("SceneName") 
  bằng SceneController.QuickNavigation.GoToSceneName()
- Hoặc SceneController.LoadScene(SceneNames.SCENE_NAME)
*/ 