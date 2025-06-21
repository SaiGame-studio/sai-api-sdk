using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Text;
using SaiGame.Enums;

public class APIManager : SaiSingleton<APIManager>
{
    [Header("API Configuration")]
    public string baseURL = "https://local-api.saigame.studio/api"; // Thay đổi URL phù hợp với server của bạn
    [SerializeField] protected bool showDebugLog = true;

    [Header("Token Storage Configuration")]
    [SerializeField] private TokenStorageType storageType = TokenStorageType.EncryptedPlayerPrefs;

    [Header("Remembered Login")]
    [SerializeField] private string rememberedEmail = "";
    [SerializeField] private bool rememberEmailEnabled = true;

    [Header("Token Information (Debug)")]
    [SerializeField] protected string currentTokenDisplay = "";
    [SerializeField] protected bool hasValidTokenDisplay = false;
    [SerializeField] protected string tokenStorageTypeDisplay = "";
    [SerializeField] protected int tokenLengthDisplay = 0;
    [Space(5)]
    [SerializeField] protected bool isTokenExpired = false;
    [SerializeField] protected string timeUntilExpire = "N/A";

    // Events
    public event System.Action OnAuthenticationSuccess;

    // Hệ thống lưu trữ token với tính đa hình
    [Header("Token")]
    private ITokenStorage tokenStorage;
    [SerializeField] protected string currentToken = "";
    [SerializeField] protected long tokenExpiresAt = 0;  // Unix timestamp
    [SerializeField] protected int tokenExpiresIn = 0;   // Seconds

    [Header("Game Info")]
    [SerializeField] protected string gameId = "68482e25731d20624900f952"; // UUID, chỉnh trong Inspector
    public string GameId => gameId;

    protected override void Awake()
    {
        base.Awake();

        // Đảm bảo APIManager không bị destroy khi chuyển scene
        DontDestroyOnLoad(gameObject);

        InitializeTokenStorage();
        LoadSavedToken();
        UpdateTokenDisplayInfo();
    }

    private void LoadSavedToken()
    {
        if (tokenStorage == null)
        {
            InitializeTokenStorage();
        }

        string savedToken = tokenStorage.GetToken();
        if (!string.IsNullOrEmpty(savedToken))
        {
            currentToken = savedToken;
            tokenExpiresAt = GetTokenExpireTime(savedToken);
        }
    }

    // Removed OnValidate to prevent automatic token checking spam

    // Removed automatic token checking to prevent spam logs

    /// <summary>
    /// Cập nhật thông tin hiển thị token trong Inspector
    /// </summary>
    public void UpdateTokenDisplayInfo()
    {
        if (tokenStorage == null && Application.isPlaying)
        {
            InitializeTokenStorage();
        }

        // Load remembered email if empty
        if (rememberEmailEnabled && string.IsNullOrEmpty(rememberedEmail) && Application.isPlaying)
        {
            rememberedEmail = PlayerPrefs.GetString("RememberedEmail", "");
        }

        // Tránh vòng lặp vô hạn bằng cách không gọi GetAuthToken() từ đây
        string displayToken = currentToken;
        if (string.IsNullOrEmpty(displayToken) && Application.isPlaying && tokenStorage != null)
        {
            displayToken = tokenStorage.GetToken(); // Gọi trực tiếp tokenStorage thay vì GetAuthToken()

            // Nếu có token từ storage, cố gắng lấy expire time từ JWT
            if (!string.IsNullOrEmpty(displayToken) && tokenExpiresAt == 0)
            {
                tokenExpiresAt = GetTokenExpireTime(displayToken);
            }
        }

        currentTokenDisplay = string.IsNullOrEmpty(displayToken) ? "No token" : displayToken;
        hasValidTokenDisplay = !string.IsNullOrEmpty(displayToken); // Chỉ kiểm tra token có tồn tại, không kiểm tra expiration
        tokenStorageTypeDisplay = storageType.ToString();
        tokenLengthDisplay = string.IsNullOrEmpty(displayToken) ? 0 : displayToken.Length;


        // Tính toán thời gian còn lại và trạng thái expired
        if (tokenExpiresAt > 0)
        {
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long timeLeft = tokenExpiresAt - currentTimestamp;

            isTokenExpired = timeLeft <= 0;

            if (timeLeft > 0)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(timeLeft);
                if (timeSpan.TotalDays >= 1)
                {
                    timeUntilExpire = $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    timeUntilExpire = $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
                }
                else
                {
                    timeUntilExpire = $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
                }
            }
            else
            {
                timeUntilExpire = "EXPIRED";
            }
        }
        else
        {
            isTokenExpired = false;
            timeUntilExpire = "N/A";
        }

        // Ẩn một phần token để bảo mật (chỉ hiển thị 10 ký tự đầu và cuối)
        if (!string.IsNullOrEmpty(currentTokenDisplay) && currentTokenDisplay != "No token" && currentTokenDisplay.Length > 20)
        {
            currentTokenDisplay = currentTokenDisplay.Substring(0, 10) + "..." +
                                currentTokenDisplay.Substring(currentTokenDisplay.Length - 10);
        }
    }

    /// <summary>
    /// Khởi tạo hệ thống lưu trữ token dựa trên cấu hình
    /// </summary>
    private void InitializeTokenStorage()
    {
        switch (storageType)
        {
            case TokenStorageType.EncryptedPlayerPrefs:
                tokenStorage = new EncryptedPlayerPrefsTokenStorage();
                break;

            case TokenStorageType.EncryptedFile:
                tokenStorage = new EncryptedFileTokenStorage();
                break;

            default:
                tokenStorage = new EncryptedPlayerPrefsTokenStorage();
                Debug.LogWarning("Unknown storage type, defaulting to Encrypted PlayerPrefs");
                break;
        }
    }

    /// <summary>
    /// Lưu token với mã hóa (tính đa hình)
    /// </summary>
    /// <param name="token">Token cần lưu</param>
    public void SetAuthToken(string token)
    {
        if (tokenStorage == null)
        {
            InitializeTokenStorage();
        }

        currentToken = token;
        tokenStorage.SaveToken(token);

        // Parse JWT để lấy expire time từ token
        tokenExpiresAt = GetTokenExpireTime(token);

        UpdateTokenDisplayInfo();
    }

    /// <summary>
    /// Lưu token cùng với thông tin expire từ API response
    /// </summary>
    /// <param name="token">Token cần lưu</param>
    /// <param name="expiresAt">Unix timestamp expire time</param>
    /// <param name="expiresIn">Seconds until expiration</param>
    public void SetAuthTokenWithExpire(string token, long expiresAt, int expiresIn)
    {
        if (tokenStorage == null)
        {
            InitializeTokenStorage();
        }

        currentToken = token;
        tokenExpiresAt = expiresAt;
        tokenExpiresIn = expiresIn;

        tokenStorage.SaveToken(token);
        UpdateTokenDisplayInfo();
    }

    /// <summary>
    /// Lấy token đã được giải mã (tính đa hình)
    /// </summary>
    /// <returns>Token đã giải mã</returns>
    public string GetAuthToken()
    {
        if (tokenStorage == null)
        {
            InitializeTokenStorage();
        }

        if (string.IsNullOrEmpty(currentToken))
        {
            currentToken = tokenStorage.GetToken();

            // Nếu load token từ storage và chưa có expire info, parse từ JWT
            if (!string.IsNullOrEmpty(currentToken) && tokenExpiresAt == 0)
            {
                tokenExpiresAt = GetTokenExpireTime(currentToken);
                UpdateTokenDisplayInfo(); // Cập nhật display sau khi có expire info
            }
        }

        return currentToken;
    }

    /// <summary>
    /// Xóa token (tính đa hình)
    /// </summary>
    public void ClearAuthToken()
    {
        if (tokenStorage == null)
        {
            InitializeTokenStorage();
        }

        currentToken = "";
        tokenExpiresAt = 0;
        tokenExpiresIn = 0;
        tokenStorage.ClearToken();
        UpdateTokenDisplayInfo();
    }

    /// <summary>
    /// Kiểm tra xem có token hợp lệ không
    /// </summary>
    /// <returns>True nếu có token hợp lệ</returns>
    public bool HasValidToken()
    {
        if (tokenStorage == null)
        {
            InitializeTokenStorage();
        }

        return tokenStorage.HasToken();
    }

    /// <summary>
    /// Thay đổi phương thức lưu trữ token (Runtime)
    /// Cho phép developer dễ dàng chuyển đổi giữa các phương thức lưu trữ
    /// </summary>
    /// <param name="newStorageType">Loại storage mới</param>
    public void ChangeTokenStorageType(TokenStorageType newStorageType)
    {
        if (storageType == newStorageType)
        {
            Debug.LogWarning($"Already using {newStorageType}");
            return;
        }

        // Lưu token hiện tại
        string existingToken = GetAuthToken();

        // Xóa token cũ
        ClearAuthToken();

        // Thay đổi storage type
        storageType = newStorageType;
        InitializeTokenStorage();

        // Lưu lại token với storage mới
        if (!string.IsNullOrEmpty(existingToken))
        {
            SetAuthToken(existingToken);
            Debug.Log($"Token migrated to {newStorageType}");
        }

        UpdateTokenDisplayInfo();
    }

    // Login API Call
    public void Login(string email, string password, Action<AuthResponse> onComplete)
    {
        LoginRequest loginData = new LoginRequest
        {
            email = email,
            password = password
        };

        StartCoroutine(PostRequest("/login", loginData, (AuthResponse response) =>
        {
            if (response != null && !string.IsNullOrEmpty(response.token))
            {
                // Lưu token với thông tin expire
                SetAuthTokenWithExpire(response.token, response.expires_at, response.expires_in);
                if (showDebugLog) Debug.Log("Login successful, triggering OnAuthenticationSuccess event");
                OnAuthenticationSuccess?.Invoke();
            }
            onComplete?.Invoke(response);
        }));
    }

    // Login API Call với Token Response format
    public void LoginWithToken(string email, string password, Action<TokenResponse> onComplete)
    {
        LoginRequest loginData = new LoginRequest
        {
            email = email,
            password = password
        };

        StartCoroutine(PostRequest("/login", loginData, (TokenResponse response) =>
        {
            if (response != null && !string.IsNullOrEmpty(response.token))
            {
                // Lưu token với thông tin expire
                SetAuthTokenWithExpire(response.token, response.expires_at, response.expires_in);
                if (showDebugLog) Debug.Log("Login successful, triggering OnAuthenticationSuccess event");
                OnAuthenticationSuccess?.Invoke();
            }
            onComplete?.Invoke(response);
        }));
    }

    // Register API Call  
    public void Register(string email, string password, string passwordConfirmation, Action<AuthResponse> onComplete)
    {
        RegisterRequest registerData = new RegisterRequest
        {
            email = email,
            password = password,
            password_confirmation = passwordConfirmation
        };

        StartCoroutine(PostRequest("/register", registerData, (AuthResponse response) =>
        {
            if (response != null && !string.IsNullOrEmpty(response.token))
            {
                // Lưu token với thông tin expire
                SetAuthTokenWithExpire(response.token, response.expires_at, response.expires_in);
                if (showDebugLog) Debug.Log("Register successful, triggering OnAuthenticationSuccess event");
                OnAuthenticationSuccess?.Invoke();
            }
            onComplete?.Invoke(response);
        }));
    }

    // Register API Call với Token Response format
    public void RegisterWithToken(string email, string password, string passwordConfirmation, Action<TokenResponse> onComplete)
    {
        RegisterRequest registerData = new RegisterRequest
        {
            email = email,
            password = password,
            password_confirmation = passwordConfirmation
        };

        StartCoroutine(PostRequest("/register", registerData, (TokenResponse response) =>
        {
            if (response != null && !string.IsNullOrEmpty(response.token))
            {
                // Lưu token với thông tin expire
                SetAuthTokenWithExpire(response.token, response.expires_at, response.expires_in);
                if (showDebugLog) Debug.Log("Register successful, triggering OnAuthenticationSuccess event");
                OnAuthenticationSuccess?.Invoke();
            }
            onComplete?.Invoke(response);
        }));
    }

    // Get User Profile
    public void GetUserProfile(Action<UserData> onComplete)
    {
        StartCoroutine(GetRequest("/user", onComplete));
    }

    // Verify Token - Check if current token is valid and get a new one if needed
    public void VerifyToken(Action<TokenInfoResponse> onComplete)
    {
        StartCoroutine(GetRequest<TokenInfoResponse>("/auth/token-info", onComplete));
    }

    // Generic POST request
    public IEnumerator PostRequest<T>(string endpoint, object data, Action<T> onComplete)
    {
        string url = baseURL + endpoint;
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            if (this.showDebugLog) Debug.Log("<b>POST:</b>" + endpoint);

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            // Add auth token if available
            if (!string.IsNullOrEmpty(GetAuthToken()))
            {
                request.SetRequestHeader("Authorization", "Bearer " + GetAuthToken());
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;

                    T response = JsonUtility.FromJson<T>(responseText);
                    onComplete?.Invoke(response);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("JSON Parse Error: " + e.Message);
                    onComplete?.Invoke(default(T));
                }
            }
            else
            {
                Debug.LogWarning("API Error: " + request.error);
                Debug.LogWarning("Response: " + request.downloadHandler.text);
                onComplete?.Invoke(default(T));
            }
        }
    }

    // Generic GET request
    public IEnumerator GetRequest<T>(string endpoint, Action<T> onComplete)
    {
        string url = baseURL + endpoint;
        if (showDebugLog) Debug.Log($"[APIManager] GET: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            string token = GetAuthToken();

            // Add auth token if available
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;

                    T response = JsonUtility.FromJson<T>(responseText);
                    onComplete?.Invoke(response);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("JSON Parse Error: " + e.Message);
                    onComplete?.Invoke(default(T));
                }
            }
            else
            {
                Debug.LogWarning("API Error: " + request.error);
                onComplete?.Invoke(default(T));
            }
        }
    }

    /// <summary>
    /// Parse JWT token để lấy thông tin expire time
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Unix timestamp của expire time, 0 nếu không parse được</returns>
    private long GetTokenExpireTime(string token)
    {
        if (string.IsNullOrEmpty(token))
            return 0;

        try
        {
            // JWT có format: header.payload.signature
            string[] parts = token.Split('.');
            if (parts.Length != 3)
                return 0;

            // Decode payload (base64url)
            string payload = parts[1];

            // Thêm padding nếu cần
            int padding = 4 - (payload.Length % 4);
            if (padding != 4)
            {
                payload += new string('=', padding);
            }

            // Chuyển base64url sang base64 thông thường
            payload = payload.Replace('-', '+').Replace('_', '/');

            // Decode base64
            byte[] payloadBytes = Convert.FromBase64String(payload);
            string payloadJson = Encoding.UTF8.GetString(payloadBytes);

            // Parse JSON đơn giản để lấy exp
            // Tìm "exp":
            string expKey = "\"exp\":";
            int expIndex = payloadJson.IndexOf(expKey);
            if (expIndex == -1)
                return 0;

            int startIndex = expIndex + expKey.Length;
            int endIndex = payloadJson.IndexOfAny(new char[] { ',', '}' }, startIndex);
            if (endIndex == -1)
                endIndex = payloadJson.Length;

            string expValue = payloadJson.Substring(startIndex, endIndex - startIndex).Trim();

            if (long.TryParse(expValue, out long expTimestamp))
            {
                return expTimestamp;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse JWT token: {e.Message}");
        }

        return 0;
    }


    /// <summary>
    /// Lưu email để ghi nhớ cho lần đăng nhập tiếp theo
    /// </summary>
    /// <param name="email">Email cần ghi nhớ</param>
    public void SaveRememberedEmail(string email)
    {
        if (rememberEmailEnabled && !string.IsNullOrEmpty(email))
        {
            rememberedEmail = email;
            PlayerPrefs.SetString("RememberedEmail", email);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Lấy email đã ghi nhớ
    /// </summary>
    /// <returns>Email đã ghi nhớ hoặc chuỗi rỗng</returns>
    public string GetRememberedEmail()
    {
        if (rememberEmailEnabled)
        {
            if (string.IsNullOrEmpty(rememberedEmail))
            {
                rememberedEmail = PlayerPrefs.GetString("RememberedEmail", "");
            }
            return rememberedEmail;
        }
        return "";
    }

    /// <summary>
    /// Xóa email đã ghi nhớ
    /// </summary>
    public void ClearRememberedEmail()
    {
        rememberedEmail = "";
        PlayerPrefs.DeleteKey("RememberedEmail");
        PlayerPrefs.Save();
        if (this.showDebugLog) Debug.Log("Remembered email cleared");
    }

    /// <summary>
    /// Logout hoàn chỉnh: Gọi API logout, xóa token và chuyển về scene login
    /// </summary>
    public void LogoutWithAPI()
    {
        // Gọi API logout trước khi xóa token
        StartCoroutine(LogoutCoroutine());
    }

    /// <summary>
    /// Coroutine thực hiện logout process
    /// </summary>
    private IEnumerator LogoutCoroutine()
    {
        // Nếu có token, gọi API logout
        if (HasValidToken())
        {
            string url = baseURL + "/auth/logout";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();

                // Set headers
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

                // Add auth token
                string token = GetAuthToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + token);
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string responseText = request.downloadHandler.text;

                        // Parse response để lấy message
                        var response = JsonUtility.FromJson<LogoutResponse>(responseText);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Failed to parse logout response: " + e.Message);
                        Debug.LogWarning("Logout API call successful (response parse failed)");
                    }
                }
                else
                {
                    Debug.LogWarning("Logout API call failed: " + request.error);
                    Debug.LogWarning("Response: " + request.downloadHandler.text);
                    // Vẫn tiếp tục logout locally dù API fail
                }
            }
        }

        // Xóa token local
        ClearAuthToken();

        // Chuyển về scene login
        NavigateToLoginScene();
    }

    /// <summary>
    /// Chuyển về scene login
    /// </summary>
    private void NavigateToLoginScene()
    {
        try
        {
            SceneManager.LoadScene(SceneNames.LOGIN);

        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to navigate to login scene: " + e.Message);
            // Fallback cuối cùng
            try
            {
                SceneManager.LoadScene(SceneNames.LOGIN);
            }
            catch (System.Exception fallbackError)
            {
                Debug.LogError("Fallback scene loading also failed: " + fallbackError.Message);
            }
        }
    }

    /// <summary>
    /// Bật/tắt tính năng ghi nhớ email
    /// </summary>
    /// <param name="enabled">True để bật, false để tắt</param>
    public void SetRememberEmailEnabled(bool enabled)
    {
        rememberEmailEnabled = enabled;
        if (!enabled)
        {
            ClearRememberedEmail();
        }
    }

    /// <summary>
    /// Gọi API tạo account cho user hiện tại (sau khi login/register thành công)
    /// </summary>
    public void RegisterProfileForCurrentUser(Action<UserProfileResponse> onComplete)
    {
        string endpoint = $"/games/{gameId}/register/profiles";
        StartCoroutine(PostRequest<UserProfileResponse>(endpoint, null, onComplete));
    }

    /// <summary>
    /// Trigger authentication success event from outside the class
    /// </summary>
    public void TriggerAuthenticationSuccess()
    {
        if (showDebugLog) Debug.Log("Triggering OnAuthenticationSuccess event");
        OnAuthenticationSuccess?.Invoke();
    }
}