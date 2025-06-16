using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuUISetup : SaiBehaviour
{
    [Header("Auto Setup UI")]
    public bool autoSetup = true;
    
    [Header("UI References")]
    public Button shopButton;
    public Button myItemsButton;
    public Button myCharacterButton;
    public Button logoutButton;
    public TextMeshProUGUI welcomeText;
    
    [Header("Scene Names")]
    public string shopSceneName = SceneNames.SHOP;
    public string myItemsSceneName = SceneNames.MY_ITEMS;
    public string myCharacterSceneName = SceneNames.MY_CHARACTER;
    
    protected override void Start()
    {
        base.Start();
        if (autoSetup)
        {
            CreateLoginUI();
        }
        else
        {
            SetupUI();
            UpdateWelcomeText();
        }
    }
    
    void Reset()
    {
        // Gọi hàm tạo UI khi nhấn nút Reset trong Inspector
        CreateLoginUI();
    }
    
    [ContextMenu("Create Main Menu UI")]
    public void CreateLoginUIFromMenu()
    {
        CreateLoginUI();
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
        
        // Create Main Panel
        GameObject mainPanel = CreateUIElement("MainMenuPanel", canvasGO.transform);
        Image mainImage = mainPanel.AddComponent<Image>();
        mainImage.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.sizeDelta = new Vector2(600, 800);
        
        // Create Welcome Text
        GameObject welcomeTextGO = CreateText("WelcomeText", "Welcome!", mainPanel.transform, 36);
        RectTransform welcomeRect = welcomeTextGO.GetComponent<RectTransform>();
        welcomeRect.anchoredPosition = new Vector2(0, 300);
        welcomeRect.sizeDelta = new Vector2(500, 100);
        welcomeText = welcomeTextGO.GetComponent<TextMeshProUGUI>();
        
        // Create Shop Button
        GameObject shopBtn = CreateButton("ShopButton", "SHOP", mainPanel.transform);
        RectTransform shopRect = shopBtn.GetComponent<RectTransform>();
        shopRect.anchoredPosition = new Vector2(0, 150);
        shopRect.sizeDelta = new Vector2(300, 60);
        shopButton = shopBtn.GetComponent<Button>();
        
        // Create My Items Button
        GameObject myItemsBtn = CreateButton("MyItemsButton", "MY ITEMS", mainPanel.transform);
        RectTransform myItemsRect = myItemsBtn.GetComponent<RectTransform>();
        myItemsRect.anchoredPosition = new Vector2(0, 50);
        myItemsRect.sizeDelta = new Vector2(300, 60);
        myItemsButton = myItemsBtn.GetComponent<Button>();
        
        // Create My Character Button
        GameObject myCharacterBtn = CreateButton("MyCharacterButton", "MY CHARACTER", mainPanel.transform);
        RectTransform myCharacterRect = myCharacterBtn.GetComponent<RectTransform>();
        myCharacterRect.anchoredPosition = new Vector2(0, -50);
        myCharacterRect.sizeDelta = new Vector2(300, 60);
        myCharacterButton = myCharacterBtn.GetComponent<Button>();
        
        // Create Logout Button
        GameObject logoutBtn = CreateButton("LogoutButton", "LOGOUT", mainPanel.transform);
        RectTransform logoutRect = logoutBtn.GetComponent<RectTransform>();
        logoutRect.anchoredPosition = new Vector2(0, -200);
        logoutRect.sizeDelta = new Vector2(200, 50);
        logoutButton = logoutBtn.GetComponent<Button>();
        Button logoutBtnComp = logoutBtn.GetComponent<Button>();
        ColorBlock logoutColors = logoutBtnComp.colors;
        logoutColors.normalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        logoutBtnComp.colors = logoutColors;
        
        // Setup MainMenuUISetup
        MainMenuUISetup mainMenuSetup = canvasGO.AddComponent<MainMenuUISetup>();
        mainMenuSetup.autoSetup = false;
        mainMenuSetup.shopButton = shopButton;
        mainMenuSetup.myItemsButton = myItemsButton;
        mainMenuSetup.myCharacterButton = myCharacterButton;
        mainMenuSetup.logoutButton = logoutButton;
        mainMenuSetup.welcomeText = welcomeText;
        
        // Setup APIManager as child of Manager (only if not exists)
        APIManager existingAPIManager = FindFirstObjectByType<APIManager>();
        if (existingAPIManager == null)
        {
            GameObject apiManagerGO = new GameObject("APIManager");
            APIManager apiManager = apiManagerGO.AddComponent<APIManager>();
            Debug.Log("APIManager created under Manager.");
        }
        
        Debug.Log("Main Menu UI created successfully!");
    }
    
    private void SetupUI()
    {
        if (shopButton != null)
            shopButton.onClick.AddListener(OnShopClick);
            
        if (myItemsButton != null)
            myItemsButton.onClick.AddListener(OnMyItemsClick);
            
        if (myCharacterButton != null)
            myCharacterButton.onClick.AddListener(OnMyCharacterClick);
            
        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogoutClick);
    }
    
    private void UpdateWelcomeText()
    {
        //if (welcomeText != null)
        //{
        //    string userEmail = AuthenticationSystem.Instance.GetCurrentUserEmail();
        //    welcomeText.text = $"Welcome, {userEmail}!";
        //}
    }
    
    private void OnShopClick()
    {
        SceneManager.LoadScene(shopSceneName);
    }
    
    private void OnMyItemsClick()
    {
        SceneManager.LoadScene(myItemsSceneName);
    }
    
    private void OnMyCharacterClick()
    {
        SceneManager.LoadScene(myCharacterSceneName);
    }
    
    private void OnLogoutClick()
    {
        APIManager.Instance.LogoutWithAPI();
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