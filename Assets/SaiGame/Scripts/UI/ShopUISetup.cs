using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class ShopUISetup : SaiBehaviour
{
    [Header("Auto Setup UI")]
    public bool autoSetup = true;

    [Header("UI References")]
    public Button logoutButton;
    public Button backToMainMenuButton;
    public ScrollRect shopListScrollView;
    public RectTransform shopListContent;
    public ScrollRect itemListScrollView;
    public RectTransform itemListContent;

    [Header("Scene Names")]
    public string mainMenuSceneName = SceneNames.MAIN_MENU;

    protected override void Start()
    {
        base.Start();
        if (autoSetup)
        {
            CreateShopUI();
        }
        else
        {
            SetupUI();
        }
    }

    void Reset()
    {
        CreateShopUI();
    }

    [ContextMenu("Create Shop UI")]
    public void CreateShopUIFromMenu()
    {
        CreateShopUI();
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
            Debug.Log("EventSystem created.");
        }

        // Create Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Background Panel
        GameObject bgPanel = CreateUIElement("Background", canvasGO.transform);
        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        SetFullScreen(bgPanel.GetComponent<RectTransform>());

        // Top Left Panel for Logout & Back
        GameObject topLeftPanel = CreateUIElement("TopLeftPanel", canvasGO.transform);
        RectTransform topLeftRect = topLeftPanel.GetComponent<RectTransform>();
        topLeftRect.anchorMin = new Vector2(0, 1);
        topLeftRect.anchorMax = new Vector2(0, 1);
        topLeftRect.pivot = new Vector2(0, 1);
        topLeftRect.anchoredPosition = new Vector2(20, -20);
        topLeftRect.sizeDelta = new Vector2(400, 60);

        // Back to MainMenu Button
        GameObject backBtn = CreateButton("BackToMainMenuButton", "MAIN MENU", topLeftPanel.transform);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchoredPosition = new Vector2(100, -30);
        backRect.sizeDelta = new Vector2(180, 50);
        backToMainMenuButton = backBtn.GetComponent<Button>();

        // Logout Button
        GameObject logoutBtn = CreateButton("LogoutButton", "LOGOUT", topLeftPanel.transform);
        RectTransform logoutRect = logoutBtn.GetComponent<RectTransform>();
        logoutRect.anchoredPosition = new Vector2(300, -30);
        logoutRect.sizeDelta = new Vector2(100, 50);
        logoutButton = logoutBtn.GetComponent<Button>();
        Button logoutBtnComp = logoutBtn.GetComponent<Button>();
        ColorBlock logoutColors = logoutBtnComp.colors;
        logoutColors.normalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        logoutBtnComp.colors = logoutColors;

        // Main Content Panel
        GameObject mainPanel = CreateUIElement("ShopMainPanel", canvasGO.transform);
        Image mainImage = mainPanel.AddComponent<Image>();
        mainImage.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0.5f, 0.5f);
        mainRect.anchorMax = new Vector2(0.5f, 0.5f);
        mainRect.pivot = new Vector2(0.5f, 0.5f);
        mainRect.anchoredPosition = Vector2.zero;
        mainRect.sizeDelta = new Vector2(1200, 900);

        // Shop List (ScrollView)
        GameObject shopScrollGO = CreateScrollView("ShopListScrollView", mainPanel.transform, out RectTransform shopContentRect);
        RectTransform shopScrollRect = shopScrollGO.GetComponent<RectTransform>();
        shopScrollRect.anchorMin = new Vector2(0.5f, 1f);
        shopScrollRect.anchorMax = new Vector2(0.5f, 1f);
        shopScrollRect.pivot = new Vector2(0.5f, 1f);
        shopScrollRect.anchoredPosition = new Vector2(0, -60);
        shopScrollRect.sizeDelta = new Vector2(1000, 200);
        shopListScrollView = shopScrollGO.GetComponent<ScrollRect>();
        shopListContent = shopContentRect;

        // Item List (ScrollView)
        GameObject itemScrollGO = CreateScrollView("ItemListScrollView", mainPanel.transform, out RectTransform itemContentRect);
        RectTransform itemScrollRect = itemScrollGO.GetComponent<RectTransform>();
        itemScrollRect.anchorMin = new Vector2(0.5f, 0f);
        itemScrollRect.anchorMax = new Vector2(0.5f, 0f);
        itemScrollRect.pivot = new Vector2(0.5f, 0f);
        itemScrollRect.anchoredPosition = new Vector2(0, 60);
        itemScrollRect.sizeDelta = new Vector2(1000, 500);
        itemListScrollView = itemScrollGO.GetComponent<ScrollRect>();
        itemListContent = itemContentRect;

        // Setup ShopUISetup
        ShopUISetup shopUISetup = canvasGO.AddComponent<ShopUISetup>();
        shopUISetup.autoSetup = false;
        shopUISetup.logoutButton = logoutButton;
        shopUISetup.backToMainMenuButton = backToMainMenuButton;
        shopUISetup.shopListScrollView = shopListScrollView;
        shopUISetup.shopListContent = shopListContent;
        shopUISetup.itemListScrollView = itemListScrollView;
        shopUISetup.itemListContent = itemListContent;

        Debug.Log("Shop UI created successfully!");
    }

    private void SetupUI()
    {
        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogoutClick);
        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClick);
    }

    private void OnLogoutClick()
    {
        APIManager.Instance.LogoutWithAPI();
    }

    private void OnBackToMainMenuClick()
    {
        SceneManager.LoadScene(mainMenuSceneName);
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

    GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject go = CreateUIElement(name, parent);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.5f, 0.8f, 1f);
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

    GameObject CreateScrollView(string name, Transform parent, out RectTransform contentRect)
    {
        GameObject scrollGO = CreateUIElement(name, parent);
        Image bg = scrollGO.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.25f, 0.8f);
        ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        // Viewport
        GameObject viewport = CreateUIElement("Viewport", scrollGO.transform);
        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0.05f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        scrollRect.viewport = viewportRect;
        // Content
        GameObject content = CreateUIElement("Content", viewport.transform);
        contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 600);
        scrollRect.content = contentRect;
        return scrollGO;
    }

    void SetFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
} 