using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ShopUISetup : MonoBehaviour
{
    [Header("Auto Setup UI")]
    public bool autoSetup = true;

    [Header("APIManager Integration")]
    public APIManager apiManager;
    public ShopManager shopManager;

    [Header("UI References (Auto-assigned)")]
    [SerializeField] public Transform shopItemContainer;
    [SerializeField] public GameObject shopItemPrefab;
    [SerializeField] public Button refreshButton;
    [SerializeField] public Button backToMainMenuButton;
    [SerializeField] public Button buyItemButton;
    [SerializeField] public TextMeshProUGUI statusText;
    [SerializeField] public GameObject loadingPanel;
    [SerializeField] public ScrollRect shopScrollRect;
    [SerializeField] public Transform shopSelectionContainer;
    [SerializeField] public GameObject shopSelectionPrefab;
    [SerializeField] public GameObject shopSelectionPanel;
    [SerializeField] public GameObject shopItemsPanel;

    [Header("Scene Management")]
    public string mainMenuSceneName = SceneNames.MAIN_MENU;
    public string myItemSceneName = SceneNames.MY_ITEMS;

    [Header("Dummy Data (Editor Only)")]
    public bool showDummyData = true;
    public int dummyShopCount = 6;
    public int dummyItemCount = 12;

    // Private variables for tracking UI elements
    private List<GameObject> shopItems = new List<GameObject>();
    private List<GameObject> shopSelectionItems = new List<GameObject>();
    private ShopData selectedShop = null;

    void Awake()
    {
        // Khi play game thì ẩn dummy data đi bằng cách xóa các item con
        if (Application.isPlaying)
        {
            // Xóa các items trong shop selection
            if (shopSelectionContainer != null)
            {
                foreach (Transform child in shopSelectionContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            if (shopSelectionItems != null) shopSelectionItems.Clear();

            // Xóa các items trong shop
            if (shopItemContainer != null)
            {
                foreach (Transform child in shopItemContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            if (shopItems != null) shopItems.Clear();
        }
    }

    void Start()
    {
        // Tự động tìm và liên kết APIManager và ShopManager một lần duy nhất
        AutoLinkManagers();

        if (autoSetup)
        {
            CreateShopUI();
        }

        SetupUI();
        
        // Delay một frame để đảm bảo tất cả components đã được khởi tạo
        StartCoroutine(DelayedLoadShopData());
    }

    private void OnValidate()
    {
        // Tạo dummy data khi ở Editor mode và không play game
        if (showDummyData && !Application.isPlaying && autoSetup)
        {
            CreateShopUI();
        }
    }

    private void AutoLinkManagers()
    {
        // Tự động tìm và liên kết APIManager
        if (apiManager == null)
        {
            apiManager = FindFirstObjectByType<APIManager>();
            if (apiManager == null)
            {
                Debug.LogWarning("[ShopUISetup] ✗ APIManager not found in scene");
            }
        }

        // Tự động tìm và liên kết ShopManager
        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
            if (shopManager == null)
            {
                Debug.LogWarning("[ShopUISetup] ✗ ShopManager not found in scene");
            }
        }
    }

    private IEnumerator DelayedLoadShopData()
    {
        // Đợi một frame
        yield return null;
        
        // Kiểm tra lại APIManager và ShopManager
        AutoLinkManagers();
        
        // Đợi thêm một frame nữa để đảm bảo managers đã sẵn sàng
        yield return null;
        
        // Kiểm tra xem có token hợp lệ không
        if (apiManager != null && apiManager.HasValidToken())
        {
            LoadShopData();
        }
        else
        {
            Debug.LogWarning("[ShopUISetup] APIManager not found or no valid token. Loading dummy data...");
            ShowStatus("Loading dummy data...");
            
            // Load dummy data khi không có authentication (chỉ trong editor)
            if (Application.isEditor && showDummyData)
            {
                LoadDummyData();
            }
            else
            {
                ShowStatus("Waiting for authentication...");
            }
            
            // Nếu chưa có token, đợi authentication
            if (apiManager != null)
            {
                apiManager.OnAuthenticationSuccess += OnAuthenticationSuccess;
            }
        }
    }

    private void OnAuthenticationSuccess()
    {
        if (apiManager != null)
        {
            apiManager.OnAuthenticationSuccess -= OnAuthenticationSuccess;
        }
        
        // Clear dummy data and load real data
        ClearShopSelectionItems();
        ClearShopItems();
        LoadShopData();
    }

    void Reset()
    {
        // Gọi hàm tạo UI khi nhấn nút Reset trong Inspector
        CreateShopUI();
    }

    [ContextMenu("Create Shop UI")]
    public void CreateShopUIFromMenu()
    {
        CreateShopUI();
    }

    [ContextMenu("Delete Shop UI")]
    public void DeleteShopUI()
    {
        // Tìm và xóa Canvas
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            DestroyImmediate(existingCanvas.gameObject);
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
            }
        }

        // Xóa ShopSelectionPrefab nếu tồn tại
        if (shopSelectionPrefab != null)
        {
            DestroyImmediate(shopSelectionPrefab);
        }

        // Xóa ShopItemPrefab nếu tồn tại
        if (shopItemPrefab != null)
        {
            DestroyImmediate(shopItemPrefab);
        }

        // Reset references
        shopItemContainer = null;
        shopItemPrefab = null;
        refreshButton = null;
        backToMainMenuButton = null;
        buyItemButton = null;
        statusText = null;
        loadingPanel = null;
        shopScrollRect = null;
        shopSelectionContainer = null;
        shopSelectionPrefab = null;
        shopSelectionPanel = null;
        shopItemsPanel = null;
    }

    public void CreateShopUI()
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

        // Create Main Panel (removed Image component)
        GameObject mainPanel = CreateUIElement("ShopPanel", canvasGO.transform);
        // Removed Image component from ShopPanel
        SetFullScreen(mainPanel.GetComponent<RectTransform>());

        // Create Back to Main Menu button (position at top-left corner like MyItemUISetup)
        GameObject backButtonGO = CreateButton("BackButton", "Main Menu", mainPanel.transform);
        backToMainMenuButton = backButtonGO.GetComponent<Button>();
        TextMeshProUGUI backButtonText = backToMainMenuButton.GetComponentInChildren<TextMeshProUGUI>();
        if (backButtonText != null)
        {
            backButtonText.alignment = TextAlignmentOptions.Center;
            backButtonText.enableWordWrapping = true;
            backButtonText.fontSize = 24;
        }
        Image backImage = backToMainMenuButton.GetComponent<Image>();
        if (backImage != null)
        {
            backImage.color = new Color(0.2f, 0.6f, 1f, 1f); // Blue background
        }
        ColorBlock backCb = backToMainMenuButton.colors;
        backCb.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        backCb.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        backCb.pressedColor = new Color(0.15f, 0.5f, 0.9f, 1f);
        backToMainMenuButton.colors = backCb;

        RectTransform backRect = backToMainMenuButton.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 1);
        backRect.anchorMax = new Vector2(0, 1);
        backRect.pivot = new Vector2(0, 1);
        backRect.anchoredPosition = new Vector2(20, -20); // Position at top-left corner
        backRect.sizeDelta = new Vector2(200, 80);

        // Create My Item button and position it to the right of the Main Menu button
        GameObject myItemButtonGO = CreateButton("MyItemButton", "My Item", mainPanel.transform);
        Button myItemButton = myItemButtonGO.GetComponent<Button>();
        
        // Center the text in the my item button
        TextMeshProUGUI myItemButtonText = myItemButton.GetComponentInChildren<TextMeshProUGUI>();
        if (myItemButtonText != null)
        {
            myItemButtonText.alignment = TextAlignmentOptions.Center;
            myItemButtonText.fontSize = 24;
        }

        // Style the my item button
        Image myItemImage = myItemButton.GetComponent<Image>();
        if (myItemImage != null)
        {
            myItemImage.color = new Color(0.8f, 0.4f, 0.8f, 1f); // Purple background
        }
        ColorBlock myItemCb = myItemButton.colors;
        myItemCb.normalColor = new Color(0.8f, 0.4f, 0.8f, 1f);
        myItemCb.highlightedColor = new Color(0.9f, 0.5f, 0.9f, 1f);
        myItemCb.pressedColor = new Color(0.7f, 0.3f, 0.7f, 1f);
        myItemButton.colors = myItemCb;

        RectTransform myItemRect = myItemButton.GetComponent<RectTransform>();
        myItemRect.anchorMin = new Vector2(0, 1);
        myItemRect.anchorMax = new Vector2(0, 1);
        myItemRect.pivot = new Vector2(0, 1);
        // Position next to the main menu button: main menu pos x (20) + main menu width (200) + spacing (10)
        myItemRect.anchoredPosition = new Vector2(230, -20); 
        myItemRect.sizeDelta = new Vector2(150, 80);

        // Create Refresh button and position it to the right of the My Item button
        GameObject refreshButtonGO = CreateButton("RefreshButton", "Refresh", mainPanel.transform);
        refreshButton = refreshButtonGO.GetComponent<Button>();
        
        // Center the text in the refresh button
        TextMeshProUGUI refreshButtonText = refreshButton.GetComponentInChildren<TextMeshProUGUI>();
        if (refreshButtonText != null)
        {
            refreshButtonText.alignment = TextAlignmentOptions.Center;
            refreshButtonText.fontSize = 24;
        }

        // Style the refresh button to look nicer
        Image refreshImage = refreshButton.GetComponent<Image>();
        if (refreshImage != null)
        {
            refreshImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green background
        }
        ColorBlock cb = refreshButton.colors;
        cb.normalColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        cb.highlightedColor = new Color(0.3f, 0.9f, 0.3f, 1f); // Lighter green on highlight
        cb.pressedColor = new Color(0.15f, 0.7f, 0.15f, 1f); // Darker green on press
        refreshButton.colors = cb;

        RectTransform refreshRect = refreshButton.GetComponent<RectTransform>();
        refreshRect.anchorMin = new Vector2(0, 1);
        refreshRect.anchorMax = new Vector2(0, 1);
        refreshRect.pivot = new Vector2(0, 1);
        // Position next to the my item button: my item pos x (230) + my item width (150) + spacing (10)
        refreshRect.anchoredPosition = new Vector2(390, -20); 
        refreshRect.sizeDelta = new Vector2(120, 80);

        // Create Buy Item Button and position it to the right of the Refresh button
        GameObject buyItemButtonGO = CreateButton("BuyItemButton", "BUY ITEM", mainPanel.transform);
        buyItemButton = buyItemButtonGO.GetComponent<Button>();
        
        // Set color cho buy item button
        ColorBlock buyItemColors = buyItemButton.colors;
        buyItemColors.normalColor = new Color(0.8f, 0.4f, 0.2f, 1f); // Orange color
        buyItemColors.highlightedColor = new Color(0.9f, 0.5f, 0.3f, 1f);
        buyItemColors.pressedColor = new Color(0.7f, 0.3f, 0.1f, 1f);
        buyItemButton.colors = buyItemColors;

        // Set font size cho buy item button text
        TextMeshProUGUI buyItemText = buyItemButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buyItemText != null)
        {
            buyItemText.fontSize = 24;
            buyItemText.alignment = TextAlignmentOptions.Center;
        }

        RectTransform buyItemRect = buyItemButton.GetComponent<RectTransform>();
        buyItemRect.anchorMin = new Vector2(0, 1);
        buyItemRect.anchorMax = new Vector2(0, 1);
        buyItemRect.pivot = new Vector2(0, 1);
        // Position next to the refresh button: refresh pos x (390) + refresh width (120) + spacing (10)
        buyItemRect.anchoredPosition = new Vector2(520, -20); 
        buyItemRect.sizeDelta = new Vector2(200, 80);

        // Create Back to Shop Selection Button (initially hidden, position at center of top area)
        GameObject backToShopSelectionBtn = CreateButton("BackToShopSelectionButton", "BACK TO SHOPS", mainPanel.transform);
        RectTransform backToShopSelectionRect = backToShopSelectionBtn.GetComponent<RectTransform>();
        backToShopSelectionRect.anchorMin = new Vector2(0, 1);
        backToShopSelectionRect.anchorMax = new Vector2(0, 1);
        backToShopSelectionRect.pivot = new Vector2(0, 1);
        backToShopSelectionRect.anchoredPosition = new Vector2(580, -20); // Position to the right of Buy Item button
        backToShopSelectionRect.sizeDelta = new Vector2(200, 80);
        backToShopSelectionBtn.SetActive(false); // Initially hidden

        // Set color cho back to shop selection button
        Button backToShopSelectionBtnComp = backToShopSelectionBtn.GetComponent<Button>();
        ColorBlock backToShopSelectionColors = backToShopSelectionBtnComp.colors;
        backToShopSelectionColors.normalColor = new Color(0.6f, 0.4f, 0.8f, 1f);
        backToShopSelectionBtnComp.colors = backToShopSelectionColors;

        // Set font size cho back to shop selection button text
        TextMeshProUGUI backToShopSelectionText = backToShopSelectionBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (backToShopSelectionText != null)
        {
            backToShopSelectionText.fontSize = 24;
            backToShopSelectionText.alignment = TextAlignmentOptions.Center;
        }

        // Create Shop Selection Panel (position below the buttons at top-left)
        GameObject shopSelectionPanelGO = CreateUIElement("ShopSelectionPanel", mainPanel.transform);
        RectTransform shopSelectionRect = shopSelectionPanelGO.GetComponent<RectTransform>();
        shopSelectionRect.anchorMin = new Vector2(0, 1);
        shopSelectionRect.anchorMax = new Vector2(0, 1);
        shopSelectionRect.pivot = new Vector2(0, 1);
        // Position below the buttons: buttons are at y=-20, button height=80, spacing=50
        shopSelectionRect.anchoredPosition = new Vector2(20, -150); // -20 - 80 - 50 = -150
        shopSelectionRect.sizeDelta = new Vector2(430, 600); // Increased width for larger buttons (400 + 15*2)
        shopSelectionPanel = shopSelectionPanelGO;
        shopSelectionPanel.SetActive(true); // Always visible

        // Create Shop Selection Container with Grid Layout (use full panel space)
        GameObject shopSelectionContainerGO = CreateUIElement("ShopSelectionContainer", shopSelectionPanelGO.transform);
        RectTransform shopSelectionContainerRect = shopSelectionContainerGO.GetComponent<RectTransform>();
        shopSelectionContainerRect.anchoredPosition = new Vector2(0, 0);
        shopSelectionContainerRect.sizeDelta = new Vector2(430, 600); // Use full panel space
        shopSelectionContainer = shopSelectionContainerGO.transform;

        // Add ScrollRect to shop selection container
        ScrollRect shopSelectionScrollRect = shopSelectionContainerGO.AddComponent<ScrollRect>();

        // Create Content for Shop Selection ScrollRect
        GameObject shopSelectionContentGO = CreateUIElement("ShopSelectionContent", shopSelectionContainerGO.transform);
        RectTransform shopSelectionContentRect = shopSelectionContentGO.GetComponent<RectTransform>();
        shopSelectionContentRect.sizeDelta = new Vector2(430, 600);
        shopSelectionContentRect.anchorMin = new Vector2(0, 0);
        shopSelectionContentRect.anchorMax = new Vector2(0, 1);
        shopSelectionContentRect.pivot = new Vector2(0, 1);

        // Setup Shop Selection ScrollRect
        shopSelectionScrollRect.content = shopSelectionContentRect;
        shopSelectionScrollRect.horizontal = false;
        shopSelectionScrollRect.vertical = true;
        shopSelectionScrollRect.scrollSensitivity = 10f;

        // Add Grid Layout Group to shop selection content
        GridLayoutGroup shopSelectionGridGroup = shopSelectionContentGO.AddComponent<GridLayoutGroup>();
        shopSelectionGridGroup.cellSize = new Vector2(400, 150); // Updated cell size
        shopSelectionGridGroup.spacing = new Vector2(10, 10);
        shopSelectionGridGroup.padding = new RectOffset(10, 10, 10, 10);
        shopSelectionGridGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        shopSelectionGridGroup.constraintCount = 1;

        // Add Content Size Fitter to shop selection content
        ContentSizeFitter shopSelectionSizeFitter = shopSelectionContentGO.AddComponent<ContentSizeFitter>();
        shopSelectionSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Update shopSelectionContainer to point to content
        shopSelectionContainer = shopSelectionContentGO.transform;

        // Create Shop Items Panel (position to the right of shop selection panel)
        GameObject shopItemsPanelGO = CreateUIElement("ShopItemsPanel", mainPanel.transform);
        RectTransform shopItemsPanelRect = shopItemsPanelGO.GetComponent<RectTransform>();
        shopItemsPanelRect.anchorMin = new Vector2(0, 1);
        shopItemsPanelRect.anchorMax = new Vector2(0, 1);
        shopItemsPanelRect.pivot = new Vector2(0, 1);
        // Position below the buttons, aligned with Shop Selection Panel: buttons are at y=-20, button height=80, spacing=50
        // Shop Selection Panel is at y=-150, so align Shop Items Panel at the same y position
        shopItemsPanelRect.anchoredPosition = new Vector2(460, -150); // Same y as Shop Selection Panel, x=460 (430 + 30 margin)
        shopItemsPanelRect.sizeDelta = new Vector2(800, 600); // Fixed size, aligned with Shop Selection Panel height
        shopItemsPanel = shopItemsPanelGO;

        // Create Shop Items Container with Grid Layout
        GameObject containerGO = CreateUIElement("ShopItemsContainer", shopItemsPanelGO.transform);
        RectTransform containerRect = containerGO.GetComponent<RectTransform>();
        containerRect.anchoredPosition = new Vector2(0, 0);
        containerRect.sizeDelta = new Vector2(800, 600);
        shopItemContainer = containerGO.transform;

        // Add ScrollRect to container
        ScrollRect scrollRect = containerGO.AddComponent<ScrollRect>();
        shopScrollRect = scrollRect;

        // Create Content for ScrollRect
        GameObject contentGO = CreateUIElement("Content", containerGO.transform);
        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(800, 600);
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        // Reset left/right (offsetMin.x, offsetMax.x) về 0
        contentRect.offsetMin = new Vector2(0, contentRect.offsetMin.y);
        contentRect.offsetMax = new Vector2(0, contentRect.offsetMax.y);

        // Setup ScrollRect
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10f;

        // Add Grid Layout Group to content
        GridLayoutGroup layoutGroup = contentGO.AddComponent<GridLayoutGroup>();
        layoutGroup.cellSize = new Vector2(400, 150); // Updated to match new button size
        layoutGroup.spacing = new Vector2(15, 15);
        layoutGroup.padding = new RectOffset(15, 15, 15, 15);
        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = 3; // Reduced to 3 columns for 400x150 buttons

        // Add Content Size Fitter
        ContentSizeFitter sizeFitter = contentGO.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Update shopItemContainer to point to content
        shopItemContainer = contentGO.transform;

        // Create Shop Selection Prefab
        CreateShopSelectionPrefab();

        // Create Shop Item Prefab
        CreateShopItemPrefab();

        // Create Scene Title "Shops" at top-right corner
        GameObject sceneTitleGO = CreateText("SceneTitle", "Shops", mainPanel.transform, 48);
        RectTransform sceneTitleRect = sceneTitleGO.GetComponent<RectTransform>();
        sceneTitleRect.anchorMin = new Vector2(1, 1);
        sceneTitleRect.anchorMax = new Vector2(1, 1);
        sceneTitleRect.pivot = new Vector2(1, 1);
        sceneTitleRect.anchoredPosition = new Vector2(-20, -20); // Top-right corner with margin
        sceneTitleRect.sizeDelta = new Vector2(300, 80);
        TextMeshProUGUI sceneTitleText = sceneTitleGO.GetComponent<TextMeshProUGUI>();
        sceneTitleText.color = Color.white;
        sceneTitleText.alignment = TextAlignmentOptions.TopRight;
        sceneTitleText.fontStyle = FontStyles.Bold;

        // Create Status Text (position at bottom center)
        GameObject statusGO = CreateText("StatusText", "", mainPanel.transform, 24);
        RectTransform statusRect = statusGO.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0);
        statusRect.anchorMax = new Vector2(1, 0.1f); // Bottom 10% of screen
        statusRect.offsetMin = new Vector2(20, 10);
        statusRect.offsetMax = new Vector2(-20, -10);
        statusText = statusGO.GetComponent<TextMeshProUGUI>();
        statusText.color = Color.yellow;
        statusText.alignment = TextAlignmentOptions.Center;

        // Create Loading Panel (center of screen)
        GameObject loadingGO = CreateUIElement("LoadingPanel", mainPanel.transform);
        RectTransform loadingRect = loadingGO.GetComponent<RectTransform>();
        loadingRect.sizeDelta = new Vector2(400, 200);
        loadingRect.anchoredPosition = new Vector2(0, 0); // Center of screen
        loadingPanel = loadingGO;

        // Add background to loading panel
        Image loadingBg = loadingGO.AddComponent<Image>();
        loadingBg.color = new Color(0, 0, 0, 0.8f);

        // Create loading text
        GameObject loadingTextGO = CreateText("LoadingText", "Loading...", loadingGO.transform, 32);
        RectTransform loadingTextRect = loadingTextGO.GetComponent<RectTransform>();
        loadingTextRect.anchoredPosition = new Vector2(0, 0);
        loadingTextRect.sizeDelta = new Vector2(300, 80);

        // Hide loading panel initially
        loadingPanel.SetActive(false);

        // Tạo dummy data nếu ở Editor mode
        if (showDummyData && !Application.isPlaying)
        {
            LoadDummyData();
        }
    }

    private void CreateShopItemPrefab()
    {
        // Create shop item prefab as a 400x150 button
        GameObject itemPrefab = CreateUIElement("ShopItemPrefab", null);
        RectTransform itemRect = itemPrefab.GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(400, 150);

        // Add background image
        Image itemBg = itemPrefab.AddComponent<Image>();
        itemBg.color = new Color(0.3f, 0.3f, 0.4f, 0.8f);

        // Add Button component
        Button itemButton = itemPrefab.AddComponent<Button>();
        ColorBlock buttonColors = itemButton.colors;
        buttonColors.normalColor = new Color(0.3f, 0.3f, 0.4f, 0.8f);
        buttonColors.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 0.9f);
        buttonColors.pressedColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        itemButton.colors = buttonColors;

        // Add Vertical Layout Group for content
        VerticalLayoutGroup itemLayout = itemPrefab.AddComponent<VerticalLayoutGroup>();
        itemLayout.spacing = 5f;
        itemLayout.padding = new RectOffset(10, 10, 10, 10);
        itemLayout.childControlWidth = true;
        itemLayout.childControlHeight = false;
        itemLayout.childForceExpandWidth = false;
        itemLayout.childForceExpandHeight = false;

        // Create Item Name
        GameObject nameGO = CreateText("ItemName", "Item Name", itemPrefab.transform, 32);
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(380, 60); // Increased size for larger button
        TextMeshProUGUI nameText = nameGO.GetComponent<TextMeshProUGUI>();
        nameText.alignment = TextAlignmentOptions.Center;

        // Create Price Text
        GameObject priceGO = CreateText("PriceText", "$0", itemPrefab.transform, 36);
        RectTransform priceTextRect = priceGO.GetComponent<RectTransform>();
        priceTextRect.sizeDelta = new Vector2(380, 50); // Increased size for larger button
        TextMeshProUGUI priceText = priceGO.GetComponent<TextMeshProUGUI>();
        priceText.color = new Color(1f, 0.8f, 0.2f, 1f);
        priceText.alignment = TextAlignmentOptions.Center;

        // Store as prefab
        shopItemPrefab = itemPrefab;
        shopItemPrefab.SetActive(false); // Hide prefab
    }

    private void CreateShopSelectionPrefab()
    {
        // Create shop selection prefab as a 400x150 button
        GameObject selectionPrefab = CreateUIElement("ShopSelectionPrefab", null);
        RectTransform selectionRect = selectionPrefab.GetComponent<RectTransform>();
        selectionRect.sizeDelta = new Vector2(400, 150);

        // Add background image
        Image selectionBg = selectionPrefab.AddComponent<Image>();
        selectionBg.color = new Color(0.3f, 0.3f, 0.4f, 0.8f);

        // Add Button component
        Button selectionButton = selectionPrefab.AddComponent<Button>();
        ColorBlock buttonColors = selectionButton.colors;
        buttonColors.normalColor = new Color(0.3f, 0.3f, 0.4f, 0.8f);
        buttonColors.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 0.9f);
        buttonColors.pressedColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        selectionButton.colors = buttonColors;

        // Add Vertical Layout Group for padding and alignment
        VerticalLayoutGroup layoutGroup = selectionPrefab.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.spacing = 10;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = true;

        // Create Shop Name
        GameObject shopNameGO = CreateText("ShopName", "Shop", selectionPrefab.transform, 30); // Font size 30
        TextMeshProUGUI shopNameText = shopNameGO.GetComponent<TextMeshProUGUI>();
        shopNameText.alignment = TextAlignmentOptions.Center;

        // Create Items Count Text
        GameObject itemsCountGO = CreateText("ItemsCountText", "Items: 0", selectionPrefab.transform, 30); // Font size 30
        TextMeshProUGUI itemsCountText = itemsCountGO.GetComponent<TextMeshProUGUI>();
        itemsCountText.color = new Color(0.6f, 0.8f, 1f, 1f);
        itemsCountText.alignment = TextAlignmentOptions.Center;

        // Store as prefab
        shopSelectionPrefab = selectionPrefab;
        shopSelectionPrefab.SetActive(false); // Hide prefab
    }

    private void SetupUI()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshClick);

        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClick);

        if (buyItemButton != null)
            buyItemButton.onClick.AddListener(OnBuyItemClick);

        // Find and setup My Item button
        Button myItemBtn = GameObject.Find("MyItemButton")?.GetComponent<Button>();
        if (myItemBtn != null)
            myItemBtn.onClick.AddListener(OnMyItemClick);

        // Find and setup back to shop selection button
        Button backToShopSelectionBtn = GameObject.Find("BackToShopSelectionButton")?.GetComponent<Button>();
        if (backToShopSelectionBtn != null)
            backToShopSelectionBtn.onClick.AddListener(OnBackToShopSelectionClick);
    }

    public void OnBuyItemClick()
    {
        if (shopManager == null)
        {
            ShowStatus("ShopManager not found!");
            return;
        }

        if (string.IsNullOrEmpty(shopManager.itemProfileIdForEditor))
        {
            ShowStatus("Please select an item first!");
            return;
        }

        if (string.IsNullOrEmpty(shopManager.selectedShopIdForEditor))
        {
            ShowStatus("Please select a shop first!");
            return;
        }

        // Use default number of 1 for UI button (same as Inspector default)
        int number = 1;
        
        ShowStatus($"Buying item {shopManager.itemProfileIdForEditor} from shop {shopManager.selectedShopIdForEditor}...");
        shopManager.BuyItem(shopManager.selectedShopIdForEditor, shopManager.itemProfileIdForEditor, number);
    }

    private void LoadShopData()
    {
        if (shopManager == null)
        {
            Debug.LogError("[ShopUISetup] ShopManager is null!");
            ShowStatus("ShopManager not found!");
            return;
        }

        if (apiManager == null)
        {
            Debug.LogError("[ShopUISetup] APIManager is null!");
            ShowStatus("APIManager not found!");
            return;
        }

        if (!apiManager.HasValidToken())
        {
            Debug.LogWarning("[ShopUISetup] No valid token available!");
            ShowStatus("No valid authentication token!");
            return;
        }

        ShowLoading(true);
        ShowStatus("Loading shops...");
        
        // Subscribe to shop list changes
        shopManager.OnShopListChanged += OnShopListLoaded;
        shopManager.FetchShopList();
    }

    private void OnShopListLoaded(List<ShopData> shops)
    {
        ShowLoading(false);
        if (shops != null && shops.Count > 0)
        {
            PopulateShopSelection(shops);
            ShowStatus($"Loaded {shops.Count} shops");
        }
        else
        {
            ShowStatus("No shops available");
        }
    }

    private void PopulateShopSelection(List<ShopData> shops)
    {
        // Clear existing shop selection items
        ClearShopSelectionItems();

        if (shopSelectionContainer == null || shopSelectionPrefab == null) return;

        foreach (var shop in shops)
        {
            GameObject shopGO = Instantiate(shopSelectionPrefab, shopSelectionContainer);
            shopGO.SetActive(true);

            // Set shop data
            TextMeshProUGUI shopNameText = shopGO.transform.Find("ShopName")?.GetComponent<TextMeshProUGUI>();
            if (shopNameText != null)
                shopNameText.text = shop.name ?? "Shop";

            TextMeshProUGUI itemsCountText = shopGO.transform.Find("ItemsCountText")?.GetComponent<TextMeshProUGUI>();
            if (itemsCountText != null)
                itemsCountText.text = $"Items: {shop.items_in_shop_count}";

            // Setup button click event
            Button shopButton = shopGO.GetComponent<Button>();
            if (shopButton != null)
            {
                shopButton.onClick.RemoveAllListeners();
                shopButton.onClick.AddListener(() => OnShopSelected(shop));
            }

            shopSelectionItems.Add(shopGO);
        }
    }

    private void ClearShopSelectionItems()
    {
        foreach (var item in shopSelectionItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        shopSelectionItems.Clear();
    }

    private void OnShopSelected(ShopData shop)
    {
        selectedShop = shop;
        ShowLoading(true);
        ShowStatus($"Loading items from {shop.name}...");
        
        // Subscribe to shop items changes
        shopManager.OnShopItemsChanged += OnShopDataLoaded;
        shopManager.FetchShopItems(shop.id);
    }

    private void OnShopDataLoaded(List<ItemProfileData> shopItems)
    {
        ShowLoading(false);
        if (shopItems != null && shopItems.Count > 0)
        {
            // Both panels are always visible, just populate items
            PopulateShopItems(shopItems);
            ShowStatus($"Loaded {shopItems.Count} items from {selectedShop?.name}");
        }
        else
        {
            ShowStatus($"No items available in {selectedShop?.name}");
        }
    }

    private void PopulateShopItems(List<ItemProfileData> items)
    {
        ClearShopItems();

        if (items == null || items.Count == 0)
        {
            ShowStatus("No items found");
            return;
        }

        foreach (var item in items)
        {
            GameObject itemObj = Instantiate(shopItemPrefab, shopItemContainer);
            itemObj.SetActive(true);

            // Set item name
            TextMeshProUGUI nameText = itemObj.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            nameText.text = item.item_profile.name;

            // Set item price
            TextMeshProUGUI priceText = itemObj.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
            priceText.text = $"${item.price_current}";

            // Add click handler
            Button itemButton = itemObj.GetComponent<Button>();
            if (itemButton != null)
            {
                itemButton.onClick.AddListener(() => OnItemSelected(item));
            }

            shopItems.Add(itemObj);
        }
    }

    private void ClearShopItems()
    {
        foreach (var item in shopItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        shopItems.Clear();
    }

    private void OnItemSelected(ItemProfileData item)
    {
        if (shopManager != null)
        {
            // Update ItemProfileId in ShopManager for inspector use
            shopManager.UpdateItemProfileId(item.shop_id, item.item_profile_id);
            ShowStatus($"Selected: {item.item_profile?.name} (ID: {item.item_profile_id})");
        }
        else
        {
            ShowStatus("ShopManager not found!");
        }
    }

    public void OnRefreshClick()
    {
        if (shopManager != null)
        {
            ShowStatus("Refreshing shops...");
            shopManager.FetchShopList();
        }
        else
        {
            ShowStatus("ShopManager not found!");
        }
    }

    public void OnBackToMainMenuClick()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneController.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("[ShopUISetup] Main menu scene name not set");
        }
    }

    public void OnBackToShopSelectionClick()
    {
        // Both panels are always visible now, just clear selection and update status
        selectedShop = null;
        ShowStatus("Back to shop selection");
        
        // Update button visibility
        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        // Find buttons
        Button backToMainMenuBtn = GameObject.Find("BackButton")?.GetComponent<Button>();
        Button backToShopSelectionBtn = GameObject.Find("BackToShopSelectionButton")?.GetComponent<Button>();

        if (backToMainMenuBtn != null && backToShopSelectionBtn != null)
        {
            bool isInShopItems = selectedShop != null && shopItemsPanel != null && shopItemsPanel.activeSelf;
            
            backToMainMenuBtn.gameObject.SetActive(!isInShopItems);
            backToShopSelectionBtn.gameObject.SetActive(isInShopItems);
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

    [ContextMenu("Test Refresh")]
    public void TestRefresh()
    {
        OnRefreshClick();
    }

    [ContextMenu("Clear Status")]
    public void ClearStatus()
    {
        ShowStatus("");
    }

    [ContextMenu("Show Loading")]
    public void ShowLoadingTest()
    {
        ShowLoading(true);
    }

    [ContextMenu("Hide Loading")]
    public void HideLoadingTest()
    {
        ShowLoading(false);
    }

    [ContextMenu("Load Dummy Data")]
    public void TestLoadDummyData()
    {
        LoadDummyData();
    }

    [ContextMenu("Clear Dummy Data")]
    public void TestClearDummyData()
    {
        ClearShopSelectionItems();
        ClearShopItems();
        ShowStatus("Dummy data cleared");
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.AddComponent<RectTransform>();
        if (parent != null)
            go.transform.SetParent(parent, false);
        return go;
    }

    GameObject CreateText(string name, string text, Transform parent, int fontSize)
    {
        GameObject go = CreateUIElement(name, parent);
        TextMeshProUGUI tmpText = go.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.Center;
        return go;
    }

    GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject go = CreateUIElement(name, parent);
        
        // Add Image component for button background
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.5f, 1f);
        
        // Add Button component
        Button button = go.AddComponent<Button>();
        
        // Create text child
        GameObject textGO = CreateText(name + "Text", text, go.transform, 24);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        return go;
    }

    void SetFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private void ShowShopItemsPanel()
    {
        // Both panels are always visible now, just update button visibility
        UpdateButtonVisibility();
    }

    private void ShowShopSelectionPanel()
    {
        // Both panels are always visible now, just update button visibility
        UpdateButtonVisibility();
    }

    [ContextMenu("Retry Load Shop Data")]
    public void RetryLoadShopData()
    {
        StartCoroutine(DelayedLoadShopData());
    }

    [ContextMenu("Force Load Shop Data")]
    public void ForceLoadShopData()
    {
        LoadShopData();
    }

    private void OnDestroy()
    {
        // Cleanup event subscriptions
        if (shopManager != null)
        {
            shopManager.OnShopListChanged -= OnShopListLoaded;
            shopManager.OnShopItemsChanged -= OnShopDataLoaded;
        }
        
        if (apiManager != null)
        {
            apiManager.OnAuthenticationSuccess -= OnAuthenticationSuccess;
        }
    }

    public void OnMyItemClick()
    {
        if (!string.IsNullOrEmpty(myItemSceneName))
        {
            SceneController.LoadScene(myItemSceneName);
        }
        else
        {
            Debug.LogWarning("[ShopUISetup] My Item scene name not set");
        }
    }

    private void CreateDummyData()
    {
        // OLD CreateDummyData method removed - replaced with LoadDummyData
    }

    [ContextMenu("Create Dummy Data")]
    public void CreateDummyDataFromMenu()
    {
        LoadDummyData();
    }

    [ContextMenu("Clear Dummy Data")]
    public void ClearDummyData()
    {
        ClearShopSelectionItems();
        ClearShopItems();
        ShowStatus("Dummy data cleared");
    }

    // Public methods for Inspector buttons
    public void ShowDummyDataButton()
    {
        if (Application.isEditor)
        {
            LoadDummyData();
        }
        else
        {
            Debug.LogWarning("[ShopUISetup] Dummy data only available in Editor mode");
        }
    }

    public void DeleteDummyDataButton()
    {
        ClearShopSelectionItems();
        ClearShopItems();
        ShowStatus("Dummy data deleted");
    }

    public void ToggleDummyDataButton()
    {
        showDummyData = !showDummyData;
        if (showDummyData && Application.isEditor)
        {
            LoadDummyData();
        }
        else
        {
            ClearShopSelectionItems();
            ClearShopItems();
            ShowStatus($"Dummy data {(showDummyData ? "enabled" : "disabled")}");
        }
    }

    private void LoadDummyData()
    {
        // Chỉ load dummy data trong editor mode
        if (!Application.isEditor || !showDummyData) return;

        // Tạo dummy shop data
        List<ShopData> dummyShops = new List<ShopData>();
        string[] shopNames = {
            "Weapon Shop",
            "Armor Shop", 
            "Potion Shop",
            "Magic Shop",
            "Food Shop",
            "Tool Shop",
            "Jewelry Shop",
            "Book Shop",
            "Pet Shop",
            "Garden Shop"
        };
        
        string[] shopDescriptions = {
            "Best weapons in town",
            "Protection for heroes",
            "Healing and magic",
            "Spells and scrolls", 
            "Delicious meals",
            "Crafting materials",
            "Precious gems and rings",
            "Ancient knowledge",
            "Loyal companions",
            "Beautiful plants"
        };

        for (int i = 0; i < dummyShopCount && i < shopNames.Length; i++)
        {
            ShopData dummyShop = new ShopData
            {
                id = $"dummy_shop_{i}",
                name = shopNames[i],
                description = shopDescriptions[i]
            };
            dummyShops.Add(dummyShop);
        }

        // Tạo dummy item data
        List<ItemProfileData> dummyItems = new List<ItemProfileData>();
        string[] itemNames = {
            "Iron Sword",
            "Steel Armor",
            "Health Potion",
            "Magic Staff",
            "Golden Shield",
            "Mana Potion",
            "Dragon Scale",
            "Fire Scroll",
            "Leather Boots",
            "Crystal Ring",
            "Poison Dagger",
            "Holy Water",
            "Thunder Bow",
            "Ice Wand",
            "Phoenix Feather"
        };
        
        string[] itemDescriptions = {
            "A sharp iron sword",
            "Strong steel armor",
            "Restores 50 HP",
            "Powerful magic weapon",
            "Legendary shield",
            "Restores 50 MP",
            "Rare crafting material",
            "Cast fireball spell",
            "Lightweight boots",
            "Magical ring",
            "Venomous weapon",
            "Blessed healing item",
            "Lightning-fast bow",
            "Freezes enemies",
            "Rare magical item"
        };

        for (int i = 0; i < dummyItemCount && i < itemNames.Length; i++)
        {
            ItemProfileData dummyItem = new ItemProfileData
            {
                id = $"dummy_item_{i}",
                item_profile = new ItemProfile 
                { 
                    name = itemNames[i], 
                    description = itemDescriptions[i] 
                },
                price_current = Random.Range(50, 1000)
            };
            dummyItems.Add(dummyItem);
        }

        // Populate shop selection
        PopulateShopSelection(dummyShops);
        
        // Populate shop items (simulate selecting first shop)
        PopulateShopItems(dummyItems);
        
        ShowStatus($"Loaded {dummyShops.Count} dummy shops, {dummyItems.Count} dummy items (Editor Mode)");
    }
} 