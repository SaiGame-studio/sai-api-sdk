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
        mainRect.sizeDelta = new Vector2(900, 1200); // Tăng kích thước lớn hơn nữa

        // Create Title - Font lớn hơn nữa
        GameObject title = CreateText("RegisterTitle", "CREATE ACCOUNT", mainPanel.transform, 64); // Tăng từ 36 lên 64
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 450); // Điều chỉnh vị trí
        titleRect.sizeDelta = new Vector2(400, 160); // Tăng kích thước

        // Create Email Input - Lớn hơn nữa
        GameObject emailInputGO = CreateInputField("EmailInput", "Email", mainPanel.transform);
        RectTransform emailRect = emailInputGO.GetComponent<RectTransform>();
        emailRect.anchoredPosition = new Vector2(0, 300); // Điều chỉnh vị trí
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
        passwordRect.anchoredPosition = new Vector2(0, 150); // Điều chỉnh vị trí
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

        // Create Confirm Password Input - Lớn hơn nữa
        GameObject confirmPasswordInputGO = CreateInputField("ConfirmPasswordInput", "Confirm Password", mainPanel.transform);
        RectTransform confirmPasswordRect = confirmPasswordInputGO.GetComponent<RectTransform>();
        confirmPasswordRect.anchoredPosition = new Vector2(0, 0); // Điều chỉnh vị trí
        confirmPasswordRect.sizeDelta = new Vector2(700, 100); // Tăng kích thước lớn hơn nữa
        confirmPasswordInput = confirmPasswordInputGO.GetComponent<TMP_InputField>();

        // Set confirm password input type và font size
        confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
        if (confirmPasswordInput.textComponent != null)
            confirmPasswordInput.textComponent.fontSize = 32; // Tăng font size lớn hơn nữa
        if (confirmPasswordInput.placeholder != null && confirmPasswordInput.placeholder is TextMeshProUGUI tmpConfirmPassPlaceholder)
        {
            tmpConfirmPassPlaceholder.fontSize = 32;
        }

        // Create Register Button - Lớn hơn nữa
        GameObject registerBtn = CreateButton("RegisterButton", "REGISTER", mainPanel.transform);
        RectTransform registerRect = registerBtn.GetComponent<RectTransform>();
        registerRect.anchoredPosition = new Vector2(0, -150); // Điều chỉnh vị trí
        registerRect.sizeDelta = new Vector2(400, 100); // Tăng kích thước lớn hơn nữa
        registerButton = registerBtn.GetComponent<Button>();

        // Set font size cho register button text
        TextMeshProUGUI registerText = registerBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (registerText != null)
            registerText.fontSize = 36; // Tăng font size lớn hơn nữa

        // Create Login Button - Lớn hơn nữa
        GameObject loginBtn = CreateButton("LoginButton", "ALREADY HAVE ACCOUNT", mainPanel.transform);
        RectTransform loginRect = loginBtn.GetComponent<RectTransform>();
        loginRect.anchoredPosition = new Vector2(0, -300); // Điều chỉnh vị trí
        loginRect.sizeDelta = new Vector2(400, 90); // Tăng kích thước lớn hơn nữa
        goToLoginButton = loginBtn.GetComponent<Button>();
        Button loginBtnComp = loginBtn.GetComponent<Button>();
        ColorBlock loginColors = loginBtnComp.colors;
        loginColors.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        loginBtnComp.colors = loginColors;

        // Set font size cho login button text
        TextMeshProUGUI loginText = loginBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (loginText != null)
            loginText.fontSize = 32; // Tăng font size lớn hơn nữa

        // Create Status Text - Lớn hơn nữa
        GameObject statusTextGO = CreateText("StatusText", "", mainPanel.transform, 28); // Tăng từ 18 lên 28
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
        GameObject loadingText = CreateText("LoadingText", "Processing...", loadingPanelGO.transform, 40); // Tăng từ 24 lên 40
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
                Debug.Log("[RegisterUISetup] APIManager created.");
            }
            else
            {
                Debug.Log("[RegisterUISetup] Found existing APIManager.");
            }
        }

        Debug.Log("Register UI created successfully!");
    }

    private void SetupUI()
    {
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterClick);

        if (goToLoginButton != null)
            goToLoginButton.onClick.AddListener(OnGoToLoginClick);

        if (statusText != null)
            statusText.text = "";

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
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
            Debug.Log("[RegisterUISetup] Calling APIManager.Register...");
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
            ShowStatus("Account created successfully!");
            Debug.Log($"[RegisterUISetup] Registration successful! User ID: {response?.user?.id}");

            // Auto login after successful registration
            ShowStatus("Logging in...");
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
            ShowStatus("Login successful! Redirecting...");
            
            Debug.Log($"[RegisterUISetup] Auto login successful! Token: {response.token.Substring(0, 20)}...");
            
            // Lưu email để ghi nhớ cho lần đăng nhập tiếp theo
            string email = emailInput?.text ?? "";
            if (!string.IsNullOrEmpty(email))
            {
                apiManager.SaveRememberedEmail(email);
                Debug.Log($"[RegisterUISetup] Saved remembered email: {email}");
            }
            
            // Gọi API tạo account cho user
            ShowStatus("Setting up user profile...");
            ShowLoading(true);
            apiManager.RegisterProfileForCurrentUser((profileResponse) =>
            {
                ShowLoading(false);
                if (profileResponse != null && profileResponse.status == "success")
                {
                    Debug.Log("[RegisterUISetup] User profile registered successfully!");
                    // Load main menu scene nếu tạo profile thành công
                    Invoke(nameof(LoadMainMenuScene), 1f);
                }
                else
                {
                    Debug.LogWarning($"[RegisterUISetup] Failed to register user profile: {profileResponse?.message ?? "Unknown error"}");
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

    // For testing purposes - can be called from inspector
    [ContextMenu("Go to Login")]
    public void TestGoToLogin()
    {
        OnGoToLoginClick();
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