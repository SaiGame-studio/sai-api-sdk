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
    public Button myInventoryButton;
    public Button logoutButton;
    public TextMeshProUGUI welcomeText;
    
    [Header("Scene Names")]
    public string shopSceneName = SceneNames.SHOP;
    public string myItemsSceneName = SceneNames.MY_ITEMS;
    public string myInventorySceneName = SceneNames.MY_INVENTORY;
    
    protected override void Start()
    {
        base.Start();
        if (autoSetup)
        {
            CreateMainMenuUI();
        }
        else
        {
            SetupUI();
            UpdateWelcomeText();
        }
    }
    
    protected override void Reset()
    {
        base.Reset();
        CreateMainMenuUI();
    }
    
    [ContextMenu("Create Main Menu UI")]
    public void CreateMainMenuUIFromMenu()
    {
        CreateMainMenuUI();
    }
    
    [ContextMenu("Delete Main Menu UI")]
    public void DeleteMainMenuUI()
    {
        // Tìm và xóa Canvas
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            DestroyImmediate(existingCanvas.gameObject);
            Debug.Log("Main Menu UI Canvas deleted.");
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
        shopButton = null;
        myItemsButton = null;
        myInventoryButton = null;
        logoutButton = null;
        welcomeText = null;
    }
    
    public void CreateMainMenuUI()
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
        GameObject mainPanel = CreateUIElement("MainMenuPanel", canvasGO.transform);
        Image mainImage = mainPanel.AddComponent<Image>();
        mainImage.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.sizeDelta = new Vector2(900, 1100); // Tăng kích thước lớn hơn nữa
        
        // Create Welcome Text - Lớn hơn nữa
        GameObject welcomeTextGO = CreateText("WelcomeText", "Welcome!", mainPanel.transform, 64); // Tăng font size
        RectTransform welcomeRect = welcomeTextGO.GetComponent<RectTransform>();
        welcomeRect.anchoredPosition = new Vector2(0, 400); // Điều chỉnh vị trí
        welcomeRect.sizeDelta = new Vector2(600, 120); // Tăng kích thước
        welcomeText = welcomeTextGO.GetComponent<TextMeshProUGUI>();
        
        // Create Shop Button - Lớn hơn nữa
        GameObject shopBtn = CreateButton("ShopButton", "SHOP", mainPanel.transform);
        RectTransform shopRect = shopBtn.GetComponent<RectTransform>();
        shopRect.anchoredPosition = new Vector2(0, 200); // Điều chỉnh vị trí
        shopRect.sizeDelta = new Vector2(400, 100); // Tăng kích thước lớn hơn nữa
        shopButton = shopBtn.GetComponent<Button>();
        
        // Set font size cho shop button text
        TextMeshProUGUI shopText = shopBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (shopText != null)
            shopText.fontSize = 36; // Tăng font size
        
        // Create My Items Button - Lớn hơn nữa
        GameObject myItemsBtn = CreateButton("MyItemsButton", "MY ITEMS", mainPanel.transform);
        RectTransform myItemsRect = myItemsBtn.GetComponent<RectTransform>();
        myItemsRect.anchoredPosition = new Vector2(0, 50); // Điều chỉnh vị trí
        myItemsRect.sizeDelta = new Vector2(400, 100); // Tăng kích thước lớn hơn nữa
        myItemsButton = myItemsBtn.GetComponent<Button>();
        
        // Set font size cho my items button text
        TextMeshProUGUI myItemsText = myItemsBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (myItemsText != null)
            myItemsText.fontSize = 36; // Tăng font size
        
        // Create My Inventory Button - Lớn hơn nữa
        GameObject myInventoryBtn = CreateButton("MyInventoryButton", "MY INVENTORY", mainPanel.transform);
        RectTransform myInventoryRect = myInventoryBtn.GetComponent<RectTransform>();
        myInventoryRect.anchoredPosition = new Vector2(0, -100); // Điều chỉnh vị trí
        myInventoryRect.sizeDelta = new Vector2(400, 100); // Tăng kích thước lớn hơn nữa
        myInventoryButton = myInventoryBtn.GetComponent<Button>();
        
        // Set font size cho my inventory button text
        TextMeshProUGUI myInventoryText = myInventoryBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (myInventoryText != null)
            myInventoryText.fontSize = 36; // Tăng font size
        
        // Create Logout Button - Lớn hơn nữa
        GameObject logoutBtn = CreateButton("LogoutButton", "LOGOUT", mainPanel.transform);
        RectTransform logoutRect = logoutBtn.GetComponent<RectTransform>();
        logoutRect.anchoredPosition = new Vector2(0, -250); // Điều chỉnh vị trí
        logoutRect.sizeDelta = new Vector2(300, 80); // Tăng kích thước lớn hơn nữa
        logoutButton = logoutBtn.GetComponent<Button>();
        Button logoutBtnComp = logoutBtn.GetComponent<Button>();
        ColorBlock logoutColors = logoutBtnComp.colors;
        logoutColors.normalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        logoutBtnComp.colors = logoutColors;
        
        // Set font size cho logout button text
        TextMeshProUGUI logoutText = logoutBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (logoutText != null)
            logoutText.fontSize = 32; // Tăng font size
        
        // Setup MainMenuUISetup
        MainMenuUISetup mainMenuSetup = canvasGO.AddComponent<MainMenuUISetup>();
        mainMenuSetup.autoSetup = false;
        mainMenuSetup.shopButton = shopButton;
        mainMenuSetup.myItemsButton = myItemsButton;
        mainMenuSetup.myInventoryButton = myInventoryButton;
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
            
        if (myInventoryButton != null)
            myInventoryButton.onClick.AddListener(OnMyInventoryClick);
            
        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogoutClick);
    }
    
    public void UpdateWelcomeText()
    {
        if (welcomeText != null)
        {
            APIManager apiManager = FindFirstObjectByType<APIManager>();
            if (apiManager != null && apiManager.HasValidToken())
            {
                // Có thể lấy thông tin user từ API nếu cần
                welcomeText.text = "Welcome to the Game!";
            }
            else
            {
                welcomeText.text = "Welcome Guest!";
            }
        }
    }
    
    public void OnShopClick()
    {
        SceneManager.LoadScene(shopSceneName);
    }
    
    public void OnMyItemsClick()
    {
        SceneManager.LoadScene(myItemsSceneName);
    }
    
    public void OnMyInventoryClick()
    {
        SceneManager.LoadScene(myInventorySceneName);
    }
    
    public void OnLogoutClick()
    {
        APIManager apiManager = FindFirstObjectByType<APIManager>();
        if (apiManager != null)
        {
            apiManager.LogoutWithAPI();
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