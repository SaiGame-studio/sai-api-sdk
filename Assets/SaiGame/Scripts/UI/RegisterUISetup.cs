using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class RegisterUISetup : MonoBehaviour
{
    [Header("Auto Setup UI")]
    public bool autoSetup = true;

    [Header("APIManager Integration")]
    public APIManager apiManager;

    [Header("UI References (Auto-assigned)")]
    [SerializeField] public TMP_InputField emailInput;
    [SerializeField] public TMP_InputField passwordInput;
    [SerializeField] public TMP_InputField confirmPasswordInput;
    [SerializeField] public Button registerButton;
    [SerializeField] public Button goToLoginButton;
    [SerializeField] public TextMeshProUGUI apiStatusLabel;
    [SerializeField] public TextMeshProUGUI statusText;
    [SerializeField] public GameObject loadingPanel;

    [Header("Scene Management")]
    public string loginSceneName = "1_Login";
    public string mainMenuSceneName = "2_MainMenu";

    // Flag để đảm bảo chỉ tìm APIManager một lần duy nhất
    private bool hasTriedToFindAPIManager = false;

    void Start()
    {
        // Tự động tìm và liên kết APIManager nếu chưa có
        TryFindAndLinkAPIManager();

        if (autoSetup)
        {
            CreateRegisterUI();
        }

        SetupUI();
    }

    void Reset()
    {
        // Gọi hàm tạo UI khi nhấn nút Reset trong Inspector
        CreateRegisterUI();
    }

    [ContextMenu("Create Register UI")]
    public void CreateRegisterUIFromMenu()
    {
        CreateRegisterUI();
    }

    [ContextMenu("Delete Register UI")]
    public void DeleteRegisterUI()
    {
        // Tìm và xóa Canvas
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            DestroyImmediate(existingCanvas.gameObject);
            Debug.Log("Register UI Canvas deleted.");
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
        confirmPasswordInput = null;
        registerButton = null;
        goToLoginButton = null;
        apiStatusLabel = null;
        statusText = null;
        loadingPanel = null;
    }

    public void CreateRegisterUI()
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
        GameObject mainPanel = CreateUIElement("RegisterPanel", canvasGO.transform);
        Image mainImage = mainPanel.AddComponent<Image>();
        mainImage.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.sizeDelta = new Vector2(900, 1300); // Tăng kích thước để chứa thêm confirm password

        // Create Title - Font lớn hơn nữa
        GameObject title = CreateText("RegisterTitle", "CREATE ACCOUNT", mainPanel.transform, 64);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 500);
        titleRect.sizeDelta = new Vector2(400, 160);

        // Create Email Input - Lớn hơn nữa
        GameObject emailInputGO = CreateInputField("EmailInput", "Email", mainPanel.transform);
        RectTransform emailRect = emailInputGO.GetComponent<RectTransform>();
        emailRect.anchoredPosition = new Vector2(0, 350);
        emailRect.sizeDelta = new Vector2(700, 100);
        emailInput = emailInputGO.GetComponent<TMP_InputField>();

        // Set font size cho email input
        if (emailInput.textComponent != null)
            emailInput.textComponent.fontSize = 32;
        if (emailInput.placeholder != null && emailInput.placeholder is TextMeshProUGUI tmpPlaceholder)
        {
            tmpPlaceholder.fontSize = 32;
        }

        // Create Password Input - Lớn hơn nữa
        GameObject passwordInputGO = CreateInputField("PasswordInput", "Password", mainPanel.transform);
        RectTransform passwordRect = passwordInputGO.GetComponent<RectTransform>();
        passwordRect.anchoredPosition = new Vector2(0, 200);
        passwordRect.sizeDelta = new Vector2(700, 100);
        passwordInput = passwordInputGO.GetComponent<TMP_InputField>();

        // Set password input type và font size
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        if (passwordInput.textComponent != null)
            passwordInput.textComponent.fontSize = 32;
        if (passwordInput.placeholder != null && passwordInput.placeholder is TextMeshProUGUI tmpPassPlaceholder)
        {
            tmpPassPlaceholder.fontSize = 32;
        }

        // Create Confirm Password Input - Lớn hơn nữa
        GameObject confirmPasswordInputGO = CreateInputField("ConfirmPasswordInput", "Confirm Password", mainPanel.transform);
        RectTransform confirmPasswordRect = confirmPasswordInputGO.GetComponent<RectTransform>();
        confirmPasswordRect.anchoredPosition = new Vector2(0, 50);
        confirmPasswordRect.sizeDelta = new Vector2(700, 100);
        confirmPasswordInput = confirmPasswordInputGO.GetComponent<TMP_InputField>();

        // Set confirm password input type và font size
        confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
        if (confirmPasswordInput.textComponent != null)
            confirmPasswordInput.textComponent.fontSize = 32;
        if (confirmPasswordInput.placeholder != null && confirmPasswordInput.placeholder is TextMeshProUGUI tmpConfirmPassPlaceholder)
        {
            tmpConfirmPassPlaceholder.fontSize = 32;
        }

        // Create Register Button - Lớn hơn nữa
        GameObject registerBtn = CreateButton("RegisterButton", "REGISTER", mainPanel.transform);
        RectTransform registerRect = registerBtn.GetComponent<RectTransform>();
        registerRect.anchoredPosition = new Vector2(0, -100);
        registerRect.sizeDelta = new Vector2(400, 100);
        registerButton = registerBtn.GetComponent<Button>();

        // Set font size cho register button text
        TextMeshProUGUI registerText = registerBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (registerText != null)
            registerText.fontSize = 36;

        // Create Login Button - Lớn hơn nữa
        GameObject loginBtn = CreateButton("LoginButton", "ALREADY HAVE ACCOUNT", mainPanel.transform);
        RectTransform loginRect = loginBtn.GetComponent<RectTransform>();
        loginRect.anchoredPosition = new Vector2(0, -250);
        loginRect.sizeDelta = new Vector2(400, 90);
        goToLoginButton = loginBtn.GetComponent<Button>();
        Button loginBtnComp = loginBtn.GetComponent<Button>();
        ColorBlock loginColors = loginBtnComp.colors;
        loginColors.normalColor = new Color(0.4f, 0.8f, 0.4f, 1f);
        loginBtnComp.colors = loginColors;

        // Set font size cho login button text
        TextMeshProUGUI loginText = loginBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (loginText != null)
            loginText.fontSize = 32;

        // Create API Status Label - Lớn hơn nữa
        GameObject apiStatusLabelGO = CreateText("APIStatusLabel", "API Status:", mainPanel.transform, 28);
        RectTransform apiStatusLabelRect = apiStatusLabelGO.GetComponent<RectTransform>();
        apiStatusLabelRect.anchoredPosition = new Vector2(0, -350);
        apiStatusLabelRect.sizeDelta = new Vector2(400, 60);
        apiStatusLabel = apiStatusLabelGO.GetComponent<TextMeshProUGUI>();
        apiStatusLabel.color = Color.yellow;

        // Create Status Text - Lớn hơn nữa
        GameObject statusTextGO = CreateText("StatusText", "", mainPanel.transform, 24);
        RectTransform statusTextRect = statusTextGO.GetComponent<RectTransform>();
        statusTextRect.anchoredPosition = new Vector2(0, -420);
        statusTextRect.sizeDelta = new Vector2(800, 80);
        statusText = statusTextGO.GetComponent<TextMeshProUGUI>();
        statusText.color = Color.white;
        statusText.alignment = TextAlignmentOptions.Center;

        // Create Loading Panel
        GameObject loadingPanelGO = CreateUIElement("LoadingPanel", canvasGO.transform);
        Image loadingBgImage = loadingPanelGO.AddComponent<Image>();
        loadingBgImage.color = new Color(0, 0, 0, 0.8f);
        SetFullScreen(loadingPanelGO.GetComponent<RectTransform>());

        // Create Loading Text
        GameObject loadingTextGO = CreateText("LoadingText", "Loading...", loadingPanelGO.transform, 48);
        RectTransform loadingTextRect = loadingTextGO.GetComponent<RectTransform>();
        loadingTextRect.anchoredPosition = new Vector2(0, 0);
        loadingTextRect.sizeDelta = new Vector2(400, 100);
        TextMeshProUGUI loadingText = loadingTextGO.GetComponent<TextMeshProUGUI>();
        loadingText.color = Color.white;

        loadingPanel = loadingPanelGO;
        loadingPanel.SetActive(false);

        Debug.Log("Register UI created successfully.");
    }

    private void SetupUI()
    {
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterClick);

        if (goToLoginButton != null)
            goToLoginButton.onClick.AddListener(OnGoToLoginClick);

        if (statusText != null)
            statusText.text = "";

        if (apiStatusLabel != null)
            apiStatusLabel.text = "API Status: Ready";

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Update API status
        UpdateAPIStatus();
    }

    private void UpdateAPIStatus()
    {
        if (apiStatusLabel != null)
        {
            if (apiManager != null)
            {
                if (apiManager.HasValidToken())
                {
                    apiStatusLabel.text = "API Status: Connected";
                    apiStatusLabel.color = Color.green;
                }
                else
                {
                    apiStatusLabel.text = "API Status: Ready";
                    apiStatusLabel.color = Color.yellow;
                }
            }
            else
            {
                apiStatusLabel.text = "API Status: Not Found";
                apiStatusLabel.color = Color.red;
            }
        }
    }

    public void OnRegisterClick()
    {
        string email = emailInput?.text ?? "";
        string password = passwordInput?.text ?? "";
        string confirmPassword = confirmPasswordInput?.text ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowStatus("Please fill in all fields!");
            return;
        }

        if (!IsValidEmail(email))
        {
            ShowStatus("Invalid email format!");
            return;
        }

        if (password != confirmPassword)
        {
            ShowStatus("Passwords do not match!");
            return;
        }

        if (password.Length < 6)
        {
            ShowStatus("Password must be at least 6 characters!");
            return;
        }

        ShowLoading(true);
        ShowStatus("Creating account...");

        if (apiManager != null)
        {
            apiManager.Register(email, password, confirmPassword, OnRegisterComplete);
        }
        else
        {
            ShowStatus("APIManager not found!");
            ShowLoading(false);
        }
    }

    private void OnRegisterComplete(AuthResponse response)
    {
        ShowLoading(false);

        // Kiểm tra trạng thái thực tế của APIManager sau khi đăng ký
        bool isRegisterSuccess = false;
        string errorMessage = response?.message ?? "Registration failed!";
        if (apiManager != null && apiManager.HasValidToken())
        {
            isRegisterSuccess = true;
        }

        if (isRegisterSuccess)
        {
            // Auto login after successful registration
            ShowLoading(true);

            if (apiManager != null)
            {
                apiManager.LoginWithToken(emailInput?.text ?? "", passwordInput?.text ?? "", OnAutoLoginComplete);
            }
        }
        else
        {
            Debug.LogWarning($"[RegisterUISetup] Registration failed: {errorMessage}");
            ShowStatus($"Registration failed: {errorMessage}");
        }
    }

    private void OnAutoLoginComplete(TokenResponse response)
    {
        ShowLoading(false);
        
        if (response != null && !string.IsNullOrEmpty(response.token))
        {
            // Lưu email để ghi nhớ cho lần đăng nhập tiếp theo
            string email = emailInput?.text ?? "";
            if (!string.IsNullOrEmpty(email))
            {
                apiManager.SaveRememberedEmail(email);
            }
            
            // Gọi API tạo account cho user
            ShowLoading(true);
            apiManager.RegisterProfileForCurrentUser((profileResponse) =>
            {
                ShowLoading(false);
                if (profileResponse != null && profileResponse.status == "success")
                {
                    Invoke(nameof(LoadMainMenuScene), 1f);
                }
                else
                {
                    ShowStatus("Failed to setup user profile!");
                }
            });
        }
        else
        {
            Debug.LogWarning("[RegisterUISetup] Auto login failed!");
            ShowStatus("Registration successful but auto login failed. Please login manually.");
        }
    }

    public void OnGoToLoginClick()
    {
        if (!string.IsNullOrEmpty(loginSceneName))
        {
            SceneManager.LoadScene(loginSceneName);
        }
        else
        {
            ShowStatus("Login scene not configured!");
        }
    }

    private void LoadMainMenuScene()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            ShowStatus("Main menu scene not configured!");
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

    [ContextMenu("Go to Login")]
    public void TestGoToLogin()
    {
        OnGoToLoginClick();
    }

    [ContextMenu("Update API Status")]
    public void TestUpdateAPIStatus()
    {
        UpdateAPIStatus();
    }

    [ContextMenu("Test Register")]
    public void TestRegister()
    {
        if (emailInput != null && passwordInput != null && confirmPasswordInput != null)
        {
            emailInput.text = "test@example.com";
            passwordInput.text = "password123";
            confirmPasswordInput.text = "password123";
            OnRegisterClick();
        }
        else
        {
            Debug.LogWarning("[RegisterUISetup] UI elements not found for test!");
        }
    }

    [ContextMenu("Test Register Direct")]
    public void TestRegisterDirect()
    {
        if (apiManager != null)
        {
            ShowLoading(true);
            ShowStatus("Testing registration...");
            apiManager.Register("test@example.com", "password123", "password123", (response) =>
            {
                ShowLoading(false);
                if (response != null)
                {
                    ShowStatus($"Test registration result: {response.message}");
                }
                else
                {
                    ShowStatus("Test registration failed!");
                }
            });
        }
        else
        {
            ShowStatus("APIManager not found for test!");
        }
    }

    [ContextMenu("Debug Token Info")]
    public void DebugTokenInfo()
    {
        if (apiManager != null)
        {
            string token = apiManager.GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                Debug.Log($"[RegisterUISetup] Current token: {token.Substring(0, Mathf.Min(20, token.Length))}...");
                Debug.Log($"[RegisterUISetup] Token length: {token.Length}");
                Debug.Log($"[RegisterUISetup] Has valid token: {apiManager.HasValidToken()}");
            }
            else
            {
                Debug.Log("[RegisterUISetup] No token found.");
            }
        }
        else
        {
            Debug.LogWarning("[RegisterUISetup] APIManager not found for debug!");
        }
    }

    [ContextMenu("Compare Token Info")]
    public void CompareTokenInfo()
    {
        if (apiManager != null)
        {
            string token = apiManager.GetAuthToken();
            bool hasValidToken = apiManager.HasValidToken();
            
            Debug.Log($"[RegisterUISetup] Token exists: {!string.IsNullOrEmpty(token)}");
            Debug.Log($"[RegisterUISetup] Has valid token: {hasValidToken}");
            
            if (!string.IsNullOrEmpty(token))
            {
                Debug.Log($"[RegisterUISetup] Token preview: {token.Substring(0, Mathf.Min(20, token.Length))}...");
            }
        }
        else
        {
            Debug.LogWarning("[RegisterUISetup] APIManager not found for comparison!");
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
            
            Debug.Log("[RegisterUISetup] APIManager not linked. Attempting to find APIManager in scene...");
            
            // Tìm APIManager trong scene
            APIManager foundAPIManager = FindFirstObjectByType<APIManager>();
            
            if (foundAPIManager != null)
            {
                apiManager = foundAPIManager;
                Debug.Log("[RegisterUISetup] Successfully found and linked APIManager!");
            }
            else
            {
                Debug.LogError("[RegisterUISetup] Failed to find APIManager in scene! Please ensure APIManager is present in the scene.");
            }
        }
    }
} 