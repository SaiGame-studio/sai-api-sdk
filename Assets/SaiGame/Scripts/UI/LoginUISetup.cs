using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginUISetup : MonoBehaviour
{
    [Header("Auto Setup UI")]
    public bool autoSetup = true;

    [Header("APIManager Integration")]
    public APIManager apiManager;

    [Header("UI References (Auto-assigned)")]
    [SerializeField] public TMP_InputField emailInput;
    [SerializeField] public TMP_InputField passwordInput;
    [SerializeField] public Button loginButton;
    [SerializeField] public Button goToRegisterButton;
    [SerializeField] public TextMeshProUGUI apiStatusLabel;
    [SerializeField] public TextMeshProUGUI statusText;
    [SerializeField] public GameObject loadingPanel;

    [Header("Scene Management")]
    public string mainMenuSceneName = "2_MainMenu";
    public string registerSceneName = "0_Register";

    // Flag để đảm bảo chỉ tìm APIManager một lần duy nhất
    private bool hasTriedToFindAPIManager = false;

    void Start()
    {
        // Tự động tìm và liên kết APIManager nếu chưa có
        TryFindAndLinkAPIManager();

        if (autoSetup)
        {
            CreateLoginUI();
        }

        SetupUI();
        LoadRememberedEmail();
        CheckAutoLogin();
    }

    void Reset()
    {
        // Gọi hàm tạo UI khi nhấn nút Reset trong Inspector
        CreateLoginUI();
    }

    [ContextMenu("Create Login UI")]
    public void CreateLoginUIFromMenu()
    {
        CreateLoginUI();
    }

    [ContextMenu("Delete Login UI")]
    public void DeleteLoginUI()
    {
        // Tìm và xóa Canvas
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            DestroyImmediate(existingCanvas.gameObject);
            Debug.Log("Login UI Canvas deleted.");
        }

        // Tìm và xóa EventSystem nếu không có UI nào khác sử dụng
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();
        if (existingEventSystem != null)
        {
            // Kiểm tra xem có Canvas nào khác không
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (allCanvases.Length == 0)
            {
                DestroyImmediate(existingEventSystem.gameObject);
                Debug.Log("EventSystem deleted.");
            }
        }

        // Reset references
        emailInput = null;
        passwordInput = null;
        loginButton = null;
        goToRegisterButton = null;
        apiStatusLabel = null;
        statusText = null;
        loadingPanel = null;
    }

    public void CreateLoginUI()
    {
        // Check if Canvas already exists
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null) return;

        // Check and create EventSystem if needed
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();
        if (existingEventSystem == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            Debug.Log("EventSystem created.");
        }

        // Create Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();

        // Create Background Panel
        GameObject bgPanel = CreateUIElement("Background", canvasGO.transform);
        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        SetFullScreen(bgPanel.GetComponent<RectTransform>());

        // Create Main Panel - Lớn hơn nữa
        GameObject mainPanel = CreateUIElement("LoginPanel", canvasGO.transform);
        Image mainImage = mainPanel.AddComponent<Image>();
        mainImage.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.sizeDelta = new Vector2(900, 1100); // Tăng kích thước lớn hơn nữa

        // Create Title - Font lớn hơn nữa
        GameObject title = CreateText("LoginTitle", "LOGIN", mainPanel.transform, 64); // Tăng từ 48 lên 64
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 400); // Điều chỉnh vị trí
        titleRect.sizeDelta = new Vector2(400, 160); // Tăng kích thước

        // Create Email Input - Lớn hơn nữa
        GameObject emailInputGO = CreateInputField("EmailInput", "Email", mainPanel.transform);
        RectTransform emailRect = emailInputGO.GetComponent<RectTransform>();
        emailRect.anchoredPosition = new Vector2(0, 250); // Điều chỉnh vị trí
        emailRect.sizeDelta = new Vector2(700, 100); // Tăng kích thước lớn hơn nữa
        emailInput = emailInputGO.GetComponent<TMP_InputField>();

        // Set font size cho email input
        if (emailInput.textComponent != null)
            emailInput.textComponent.fontSize = 32; // Tăng font size lớn hơn nữa
        if (emailInput.placeholder != null && emailInput.placeholder is TextMeshProUGUI tmpPlaceholder)
        {
            tmpPlaceholder.fontSize = 32;
        }

        // Create Password Input - Lớn hơn nữa
        GameObject passwordInputGO = CreateInputField("PasswordInput", "Password", mainPanel.transform);
        RectTransform passwordRect = passwordInputGO.GetComponent<RectTransform>();
        passwordRect.anchoredPosition = new Vector2(0, 100); // Điều chỉnh vị trí
        passwordRect.sizeDelta = new Vector2(700, 100); // Tăng kích thước lớn hơn nữa
        passwordInput = passwordInputGO.GetComponent<TMP_InputField>();

        // Set password input type và font size
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        if (passwordInput.textComponent != null)
            passwordInput.textComponent.fontSize = 32; // Tăng font size lớn hơn nữa
        if (passwordInput.placeholder != null && passwordInput.placeholder is TextMeshProUGUI tmpPassPlaceholder)
        {
            tmpPassPlaceholder.fontSize = 32;
        }

        // Create Login Button - Lớn hơn nữa
        GameObject loginBtn = CreateButton("LoginButton", "LOGIN", mainPanel.transform);
        RectTransform loginRect = loginBtn.GetComponent<RectTransform>();
        loginRect.anchoredPosition = new Vector2(0, -100); // Điều chỉnh vị trí
        loginRect.sizeDelta = new Vector2(400, 100); // Tăng kích thước lớn hơn nữa
        loginButton = loginBtn.GetComponent<Button>();

        // Set font size cho login button text
        TextMeshProUGUI loginText = loginBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (loginText != null)
            loginText.fontSize = 36; // Tăng font size lớn hơn nữa

        // Create Register Button - Lớn hơn nữa
        GameObject registerBtn = CreateButton("RegisterButton", "REGISTER", mainPanel.transform);
        RectTransform registerRect = registerBtn.GetComponent<RectTransform>();
        registerRect.anchoredPosition = new Vector2(0, -250); // Điều chỉnh vị trí
        registerRect.sizeDelta = new Vector2(400, 90); // Tăng kích thước lớn hơn nữa
        goToRegisterButton = registerBtn.GetComponent<Button>();
        Button regBtnComp = registerBtn.GetComponent<Button>();
        ColorBlock regColors = regBtnComp.colors;
        regColors.normalColor = new Color(0.4f, 0.8f, 0.4f, 1f);
        regBtnComp.colors = regColors;

        // Set font size cho register button text
        TextMeshProUGUI registerText = registerBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (registerText != null)
            registerText.fontSize = 32; // Tăng font size lớn hơn nữa

        // Create API Status Label - Lớn hơn nữa
        GameObject apiStatusLabelGO = CreateText("APIStatusLabel", "API Status:", mainPanel.transform, 28); // Tăng từ 22 lên 28
        RectTransform apiStatusLabelRect = apiStatusLabelGO.GetComponent<RectTransform>();
        apiStatusLabelRect.anchoredPosition = new Vector2(0, -350); // Điều chỉnh vị trí
        apiStatusLabelRect.sizeDelta = new Vector2(800, 120); // Tăng kích thước lớn hơn nữa
        apiStatusLabel = apiStatusLabelGO.GetComponent<TextMeshProUGUI>();
        apiStatusLabel.color = Color.white;

        // Create Status Text - Lớn hơn nữa
        GameObject statusTextGO = CreateText("StatusText", "", mainPanel.transform, 28); // Tăng từ 22 lên 28
        RectTransform statusRect = statusTextGO.GetComponent<RectTransform>();
        statusRect.anchoredPosition = new Vector2(0, -400); // Điều chỉnh vị trí
        statusRect.sizeDelta = new Vector2(800, 120); // Tăng kích thước lớn hơn nữa
        statusText = statusTextGO.GetComponent<TextMeshProUGUI>();
        statusText.color = Color.red;

        // Create Loading Panel
        GameObject loadingPanelGO = CreateUIElement("LoadingPanel", canvasGO.transform);
        Image loadingImage = loadingPanelGO.AddComponent<Image>();
        loadingImage.color = new Color(0, 0, 0, 0.7f);
        SetFullScreen(loadingPanelGO.GetComponent<RectTransform>());
        loadingPanelGO.SetActive(false);
        loadingPanel = loadingPanelGO;

        // Create loading text với font lớn hơn nữa
        GameObject loadingText = CreateText("LoadingText", "Processing...", loadingPanelGO.transform, 40); // Tăng từ 32 lên 40
        loadingText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Setup APIManager reference
        if (apiManager == null)
        {
            // Tìm APIManager hiện có trước
            apiManager = FindFirstObjectByType<APIManager>();
            if (apiManager == null)
            {
                // Chỉ tạo mới nếu không tìm thấy
                GameObject apiManagerGO = new GameObject("APIManager");
                apiManager = apiManagerGO.AddComponent<APIManager>();
                Debug.Log("[LoginUISetup] APIManager created.");
            }
            else
            {
                Debug.Log("[LoginUISetup] Found existing APIManager.");
            }
        }

        Debug.Log("Login UI created successfully!");
    }

    private void SetupUI()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginClick);

        if (goToRegisterButton != null)
            goToRegisterButton.onClick.AddListener(OnGoToRegisterClick);

        if (apiStatusLabel != null)
            apiStatusLabel.text = "API Status:";

        if (statusText != null)
            statusText.text = "";

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private void LoadRememberedEmail()
    {
        if (apiManager != null)
        {
            string rememberedEmail = apiManager.GetRememberedEmail();
            if (!string.IsNullOrEmpty(rememberedEmail) && emailInput != null)
            {
                emailInput.text = rememberedEmail;
            }
        }
    }

    private void CheckAutoLogin()
    {
        if (apiManager != null)
        {
            string savedToken = apiManager.GetAuthToken();
            if (!string.IsNullOrEmpty(savedToken))
            {
                ShowStatus("Checking saved session...");
                ShowLoading(true);

                apiManager.VerifyToken(OnAutoLoginComplete);
            }
        }
    }

    private void OnAutoLoginComplete(TokenInfoResponse tokenInfo)
    {
        ShowLoading(false);

        // Điều kiện login thành công: API trả về thông tin token và token hợp lệ
        if (tokenInfo != null && tokenInfo.IsValid())
        {
            // Token hợp lệ và chưa hết hạn
            ShowStatus("Login successful!");

            LoadGameScene();
        }
        else
        {
            // Token không hợp lệ hoặc đã hết hạn
            if (apiManager != null)
            {
                apiManager.ClearAuthToken();
            }
            if (tokenInfo != null && !tokenInfo.IsValid())
            {
                ShowStatus("Session expired. Please login again.");
            }
            else
            {
                ShowStatus("Please login");
            }
        }
    }

    public void OnLoginClick()
    {
        string email = emailInput?.text ?? "";
        string password = passwordInput?.text ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter email and password!");
            return;
        }

        if (!IsValidEmail(email))
        {
            ShowStatus("Invalid email format!");
            return;
        }

        ShowLoading(true);
        ShowStatus("Logging in...");

        if (apiManager != null)
        {
            apiManager.LoginWithToken(email, password, OnLoginComplete);
        }
        else
        {
            ShowStatus("APIManager not found!");
            ShowLoading(false);
        }
    }

    private void OnLoginComplete(TokenResponse response)
    {
        ShowLoading(false);
        
        if (response != null && !string.IsNullOrEmpty(response.token))
        {
            // Debug thông tin token sau khi login
            DebugTokenInfo();
            
            // APIManager đã tự động lưu token với expire info trong LoginWithToken
            // Không cần gọi SetAuthToken thêm lần nữa
            
            // Lưu email để ghi nhớ cho lần đăng nhập tiếp theo
            string email = emailInput?.text ?? "";
            if (!string.IsNullOrEmpty(email))
            {
                apiManager.SaveRememberedEmail(email);
                Debug.Log($"[LoginUISetup] Saved remembered email: {email}");
            }
            
            ShowLoading(true);
            apiManager.RegisterProfileForCurrentUser((profileResponse) =>
            {
                ShowLoading(false);
                if (profileResponse != null && profileResponse.status == "success")
                {
                    Invoke(nameof(LoadGameScene), 1f);
                }
                else
                {
                    Debug.LogWarning($"[LoginUISetup] Failed to register user profile: {profileResponse?.message ?? "Unknown error"}");
                    ShowStatus("Failed to register user profile!");
                }
            });
        }
        else
        {
            Debug.LogWarning("[LoginUISetup] Login failed! Response is null or token is empty.");
            ShowStatus("Login failed! Please check your credentials.");
        }
    }

    // Debug method để kiểm tra thông tin token
    private void DebugTokenInfo()
    {
        if (apiManager != null)
        {
            // Sử dụng reflection để truy cập các field private của APIManager
            var tokenExpiresAtField = typeof(APIManager).GetField("tokenExpiresAt", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tokenExpiresInField = typeof(APIManager).GetField("tokenExpiresIn", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentTokenField = typeof(APIManager).GetField("currentToken", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (tokenExpiresAtField != null && tokenExpiresInField != null && currentTokenField != null)
            {
                long expiresAt = (long)tokenExpiresAtField.GetValue(apiManager);
                int expiresIn = (int)tokenExpiresInField.GetValue(apiManager);
                string currentToken = (string)currentTokenField.GetValue(apiManager);
                
                Debug.Log($"[LoginUISetup] Debug Token Info:");
                Debug.Log($"[LoginUISetup] - Current Token: {(string.IsNullOrEmpty(currentToken) ? "null" : currentToken.Substring(0, 20) + "...")}");
                Debug.Log($"[LoginUISetup] - Token Expires At: {expiresAt}");
                Debug.Log($"[LoginUISetup] - Token Expires In: {expiresIn}");
            }
            
            // Force update token display info
            apiManager.UpdateTokenDisplayInfo();
        }
    }

    public void OnGoToRegisterClick()
    {
        if (!string.IsNullOrEmpty(registerSceneName))
        {
            SceneManager.LoadScene(registerSceneName);
        }
    }

    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            ShowStatus("Game scene not configured!");
        }
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(show);
    }

    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }

    // For testing purposes - can be called from inspector
    [ContextMenu("Test Login")]
    public void TestLogin()
    {
        if (emailInput != null && passwordInput != null)
        {
            OnLoginClick();
        }
        else
        {
            Debug.LogWarning("UI elements not set up. Please create UI first.");
        }
    }

    // For testing purposes - can be called from inspector
    [ContextMenu("Test Login Direct")]
    public void TestLoginDirect()
    {
        if (apiManager != null)
        {
            Debug.Log("[LoginUISetup] Testing direct login with APIManager...");
            apiManager.LoginWithToken("test@example.com", "password", (response) =>
            {
                Debug.Log($"[LoginUISetup] Direct login result: {(response != null ? "Success" : "Failed")}");
                if (response != null)
                {
                    Debug.Log($"[LoginUISetup] Direct login - Token: {response.token.Substring(0, 20)}..., Expires At: {response.expires_at}");
                    DebugTokenInfo();
                }
            });
        }
        else
        {
            Debug.LogWarning("APIManager not found!");
        }
    }

    // For testing purposes - can be called from inspector
    [ContextMenu("Clear Status")]
    public void ClearStatus()
    {
        ShowStatus("");
    }

    // For testing purposes - can be called from inspector
    [ContextMenu("Show Loading")]
    public void ShowLoadingTest()
    {
        ShowLoading(true);
    }

    // For testing purposes - can be called from inspector
    [ContextMenu("Hide Loading")]
    public void HideLoadingTest()
    {
        ShowLoading(false);
    }

    // For testing purposes - can be called from inspector
    [ContextMenu("Compare Token Info")]
    public void CompareTokenInfo()
    {
        if (apiManager != null)
        {
            Debug.Log("[LoginUISetup] === Token Info Comparison ===");
            
            // Lấy thông tin từ APIManager
            string currentToken = apiManager.GetAuthToken();
            bool hasValidToken = apiManager.HasValidToken();
            
            Debug.Log($"[LoginUISetup] APIManager.GetAuthToken(): {(string.IsNullOrEmpty(currentToken) ? "null" : currentToken.Substring(0, 20) + "...")}");
            Debug.Log($"[LoginUISetup] APIManager.HasValidToken(): {hasValidToken}");
            
            // Debug internal token info
            DebugTokenInfo();
            
            Debug.Log("[LoginUISetup] === End Comparison ===");
        }
        else
        {
            Debug.LogWarning("APIManager not found!");
        }
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        return go;
    }

    GameObject CreateText(string name, string text, Transform parent, int fontSize)
    {
        GameObject go = CreateUIElement(name, parent);

        TextMeshProUGUI textComp = go.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;

        return go;
    }

    GameObject CreateInputField(string name, string placeholder, Transform parent)
    {
        GameObject go = CreateUIElement(name, parent);

        Image image = go.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 1f); // White background

        TMP_InputField inputField = go.AddComponent<TMP_InputField>();

        // Create Text component for input text
        GameObject textGO = CreateUIElement("Text", go.transform);
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.fontSize = 20; // Tăng font size mặc định
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 0); // Tăng padding
        textRect.offsetMax = new Vector2(-15, 0); // Tăng padding

        // Create Placeholder
        GameObject placeholderGO = CreateUIElement("Placeholder", go.transform);
        TextMeshProUGUI placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 20; // Tăng font size mặc định
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform placeholderRect = placeholderGO.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(15, 0); // Tăng padding
        placeholderRect.offsetMax = new Vector2(-15, 0); // Tăng padding

        inputField.textComponent = text;
        inputField.placeholder = placeholderText;

        return go;
    }

    GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject go = CreateUIElement(name, parent);

        Image image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.5f, 0.8f, 1f); // Blue background

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;

        GameObject textGO = CreateText("Text", text, go.transform, 24); // Tăng font size mặc định
        TextMeshProUGUI textComp = textGO.GetComponent<TextMeshProUGUI>();
        textComp.color = Color.white;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return go;
    }

    void SetFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Tự động tìm và liên kết APIManager nếu chưa có liên kết
    /// Chỉ thực hiện một lần duy nhất khi start game
    /// </summary>
    private void TryFindAndLinkAPIManager()
    {
        // Chỉ thực hiện nếu chưa có APIManager và chưa từng thử tìm
        if (apiManager == null && !hasTriedToFindAPIManager)
        {
            hasTriedToFindAPIManager = true;
            
            // Tìm APIManager trong scene
            APIManager foundAPIManager = FindFirstObjectByType<APIManager>();
            
            if (foundAPIManager != null)
            {
                apiManager = foundAPIManager;
            }
            else
            {
                Debug.LogError("[LoginUISetup] Failed to find APIManager in scene! Please ensure APIManager is present in the scene.");
            }
        }
    }
}
