using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Quản lý việc chuyển scene một cách an toàn và hiệu quả
/// Sử dụng SceneNames để đảm bảo tính nhất quán
/// </summary>
public static class SceneController
{
    /// <summary>
    /// Event được gọi trước khi bắt đầu load scene mới
    /// </summary>
    public static System.Action<string> OnSceneLoadStarted;
    
    /// <summary>
    /// Event được gọi khi scene load hoàn thành
    /// </summary>
    public static System.Action<string> OnSceneLoadCompleted;
    
    /// <summary>
    /// Load scene với validation và logging
    /// </summary>
    /// <param name="sceneName">Tên scene (sử dụng SceneNames constants)</param>
    /// <param name="mode">Chế độ load scene</param>
    public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneController: Scene name is null or empty!");
            return;
        }
        
        if (!SceneNames.IsValidSceneName(sceneName))
        {
            Debug.LogWarning($"SceneController: Scene name '{sceneName}' is not defined in SceneNames. Loading anyway...");
        }
        
        OnSceneLoadStarted?.Invoke(sceneName);
        
        try
        {
            SceneManager.LoadScene(sceneName, mode);
            OnSceneLoadCompleted?.Invoke(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SceneController: Failed to load scene '{sceneName}': {e.Message}");
        }
    }
    
    /// <summary>
    /// Load scene bất đồng bộ với callback
    /// </summary>
    /// <param name="sceneName">Tên scene (sử dụng SceneNames constants)</param>
    /// <param name="onComplete">Callback khi load hoàn thành</param>
    /// <param name="mode">Chế độ load scene</param>
    public static void LoadSceneAsync(string sceneName, System.Action<bool> onComplete = null, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneController: Scene name is null or empty!");
            onComplete?.Invoke(false);
            return;
        }
        
        if (!SceneNames.IsValidSceneName(sceneName))
        {
            Debug.LogWarning($"SceneController: Scene name '{sceneName}' is not defined in SceneNames. Loading anyway...");
        }
        
        Debug.Log($"SceneController: Loading scene '{sceneName}' asynchronously");
        OnSceneLoadStarted?.Invoke(sceneName);
        
        // Cần MonoBehaviour để chạy Coroutine, có thể sử dụng với singleton hoặc tìm MonoBehaviour available
        var coroutineRunner = Object.FindFirstObjectByType<MonoBehaviour>();
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onComplete, mode));
        }
        else
        {
            Debug.LogWarning("SceneController: No MonoBehaviour found to run coroutine. Using synchronous loading...");
            LoadScene(sceneName, mode);
            onComplete?.Invoke(true);
        }
    }
    
    private static IEnumerator LoadSceneAsyncCoroutine(string sceneName, System.Action<bool> onComplete, LoadSceneMode mode)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);
        
        if (asyncLoad == null)
        {
            Debug.LogError($"SceneController: Failed to start async loading for scene '{sceneName}'");
            onComplete?.Invoke(false);
            yield break;
        }
        
        // Không cho phép activate scene ngay lập tức
        asyncLoad.allowSceneActivation = false;
        
        // Chờ load đến 90%
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // Có thể thêm loading UI hoặc delay ở đây
        yield return new WaitForSeconds(0.1f);
        
        // Activate scene
        asyncLoad.allowSceneActivation = true;
        
        // Chờ load hoàn thành
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        Debug.Log($"SceneController: Scene '{sceneName}' loaded successfully");
        OnSceneLoadCompleted?.Invoke(sceneName);
        onComplete?.Invoke(true);
    }
    
    /// <summary>
    /// Lấy tên scene hiện tại
    /// </summary>
    /// <returns>Tên scene hiện tại</returns>
    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// Kiểm tra xem scene hiện tại có phải là scene được chỉ định không
    /// </summary>
    /// <param name="sceneName">Tên scene cần kiểm tra</param>
    /// <returns>True nếu đang ở scene được chỉ định</returns>
    public static bool IsCurrentScene(string sceneName)
    {
        return GetCurrentSceneName() == sceneName;
    }
    
    /// <summary>
    /// Reload scene hiện tại
    /// </summary>
    public static void ReloadCurrentScene()
    {
        string currentScene = GetCurrentSceneName();
        LoadScene(currentScene);
    }
    
    /// <summary>
    /// Quick methods để chuyển đến các scene thường dùng
    /// </summary>
    public static class QuickNavigation
    {
        public static void GoToLogin() => LoadScene(SceneNames.LOGIN);
        public static void GoToRegister() => LoadScene(SceneNames.REGISTER);
        public static void GoToMainMenu() => LoadScene(SceneNames.MAIN_MENU);
        
        // Async versions
        public static void GoToLoginAsync(System.Action<bool> onComplete = null) => LoadSceneAsync(SceneNames.LOGIN, onComplete);
        public static void GoToRegisterAsync(System.Action<bool> onComplete = null) => LoadSceneAsync(SceneNames.REGISTER, onComplete);
        public static void GoToMainMenuAsync(System.Action<bool> onComplete = null) => LoadSceneAsync(SceneNames.MAIN_MENU, onComplete);
    }
} 