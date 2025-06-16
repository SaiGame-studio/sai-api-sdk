using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RegisterUISetup : MonoBehaviour
{
    [Header("Auto Setup UI")]
    public bool autoSetup = true;
    
    void Start()
    {
        if (autoSetup)
        {
            CreateRegisterUI();
        }
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
    
    public void CreateRegisterUI()
    {
        // Check if Canvas already exists
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log("Canvas already exists. UI creation skipped.");
            return;
        }
        
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
        
        // Create Main Panel
        GameObject mainPanel = CreateUIElement("RegisterPanel", canvasGO.transform);
        Image mainImage = mainPanel.AddComponent<Image>();
        mainImage.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.sizeDelta = new Vector2(500, 700);
        
        // Create Title
        GameObject title = CreateText("RegisterTitle", "CREATE ACCOUNT", mainPanel.transform, 36);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 250);
        titleRect.sizeDelta = new Vector2(200, 100);
        
        // Create Email Input
        GameObject emailInput = CreateInputField("EmailInput", "Email", mainPanel.transform);
        RectTransform emailRect = emailInput.GetComponent<RectTransform>();
        emailRect.anchoredPosition = new Vector2(0, 150);
        emailRect.sizeDelta = new Vector2(400, 50);
        
        // Create Password Input
        GameObject passwordInput = CreateInputField("PasswordInput", "Password", mainPanel.transform);
        RectTransform passwordRect = passwordInput.GetComponent<RectTransform>();
        passwordRect.anchoredPosition = new Vector2(0, 80);
        passwordRect.sizeDelta = new Vector2(400, 50);
        
        // Set password input type
        TMP_InputField passwordField = passwordInput.GetComponent<TMP_InputField>();
        passwordField.contentType = TMP_InputField.ContentType.Password;
        
        // Create Confirm Password Input
        GameObject confirmPasswordInput = CreateInputField("ConfirmPasswordInput", "Confirm Password", mainPanel.transform);
        RectTransform confirmPasswordRect = confirmPasswordInput.GetComponent<RectTransform>();
        confirmPasswordRect.anchoredPosition = new Vector2(0, 10);
        confirmPasswordRect.sizeDelta = new Vector2(400, 50);
        
        // Set confirm password input type
        TMP_InputField confirmPasswordField = confirmPasswordInput.GetComponent<TMP_InputField>();
        confirmPasswordField.contentType = TMP_InputField.ContentType.Password;
        
        // Create Register Button
        GameObject registerBtn = CreateButton("RegisterButton", "REGISTER", mainPanel.transform);
        RectTransform registerRect = registerBtn.GetComponent<RectTransform>();
        registerRect.anchoredPosition = new Vector2(0, -70);
        registerRect.sizeDelta = new Vector2(200, 50);
        Button regBtnComp = registerBtn.GetComponent<Button>();
        ColorBlock regColors = regBtnComp.colors;
        regColors.normalColor = new Color(0.4f, 0.8f, 0.4f, 1f);
        regBtnComp.colors = regColors;
        
        // Create Login Button
        GameObject loginBtn = CreateButton("LoginButton", "ALREADY HAVE ACCOUNT", mainPanel.transform);
        RectTransform loginRect = loginBtn.GetComponent<RectTransform>();
        loginRect.anchoredPosition = new Vector2(0, -140);
        loginRect.sizeDelta = new Vector2(220, 40);
        Button loginBtnComp = loginBtn.GetComponent<Button>();
        ColorBlock loginColors = loginBtnComp.colors;
        loginColors.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        loginBtnComp.colors = loginColors;
        
        // Create Status Text
        GameObject statusText = CreateText("StatusText", "", mainPanel.transform, 18);
        RectTransform statusRect = statusText.GetComponent<RectTransform>();
        statusRect.anchoredPosition = new Vector2(0, -220);
        statusRect.sizeDelta = new Vector2(450, 60);
        TextMeshProUGUI statusTextComp = statusText.GetComponent<TextMeshProUGUI>();
        statusTextComp.color = Color.red;
        
        // Create Loading Panel
        GameObject loadingPanel = CreateUIElement("LoadingPanel", canvasGO.transform);
        Image loadingImage = loadingPanel.AddComponent<Image>();
        loadingImage.color = new Color(0, 0, 0, 0.7f);
        SetFullScreen(loadingPanel.GetComponent<RectTransform>());
        loadingPanel.SetActive(false);
        
        GameObject loadingText = CreateText("LoadingText", "Processing...", loadingPanel.transform, 24);
        loadingText.GetComponent<TextMeshProUGUI>().color = Color.white;
        
        // Setup RegisterManager
        GameObject managerGO = new GameObject("RegisterManager");
        managerGO.transform.SetParent(canvasGO.transform);
        RegisterManager registerManager = managerGO.AddComponent<RegisterManager>();
        
        // Assign references
        registerManager.emailInput = emailInput.GetComponent<TMP_InputField>();
        registerManager.passwordInput = passwordInput.GetComponent<TMP_InputField>();
        registerManager.confirmPasswordInput = confirmPasswordInput.GetComponent<TMP_InputField>();
        registerManager.registerButton = registerBtn.GetComponent<Button>();
        registerManager.goToLoginButton = loginBtn.GetComponent<Button>();
        registerManager.statusText = statusText.GetComponent<TextMeshProUGUI>();
        registerManager.loadingPanel = loadingPanel;
        
        // Setup APIManager as child of Manager (only if not exists)
        APIManager existingAPIManager = FindFirstObjectByType<APIManager>();
        if (existingAPIManager == null)
        {
            GameObject apiManagerGO = new GameObject("APIManager");
            APIManager apiManager = apiManagerGO.AddComponent<APIManager>();
            Debug.Log("APIManager created under Manager.");
        }
        
        Debug.Log("Register UI created successfully!");
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
        text.fontSize = 16;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        // Create Placeholder
        GameObject placeholderGO = CreateUIElement("Placeholder", go.transform);
        TextMeshProUGUI placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 16;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
        
        RectTransform placeholderRect = placeholderGO.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);
        
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
        
        GameObject textGO = CreateText("Text", text, go.transform, 18);
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
} 