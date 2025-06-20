using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class MyItemUISetup : MonoBehaviour
{
    [Header("Auto Setup UI")]
    public bool autoSetup = true;

    [Header("APIManager Integration")]
    public APIManager apiManager;
    public PlayerItemManager playerItemManager;

    [Header("UI References (Auto-assigned)")]
    [SerializeField] public Transform itemContainer;
    [SerializeField] public GameObject itemPrefab;
    [SerializeField] public Button refreshButton;
    [SerializeField] public Button backToMainMenuButton;
    [SerializeField] public TextMeshProUGUI statusText;
    [SerializeField] public GameObject loadingPanel;
    [SerializeField] public ScrollRect itemScrollRect;
    [SerializeField] public TextMeshProUGUI titleText;

    [Header("Scene Management")]
    public string mainMenuSceneName = "2_MainMenu";

    // Private variables for tracking UI elements
    private List<GameObject> itemObjects = new List<GameObject>();

    void Start()
    {
        // Tự động tìm và liên kết APIManager và PlayerItemManager một lần duy nhất
        AutoLinkManagers();

        if (autoSetup)
        {
            CreateMyItemUI();
        }

        SetupUI();
        
        // Delay một frame để đảm bảo tất cả components đã được khởi tạo
        StartCoroutine(DelayedLoadPlayerItems());
    }

    private void AutoLinkManagers()
    {
        // Tự động tìm và liên kết APIManager
        if (apiManager == null)
        {
            apiManager = FindFirstObjectByType<APIManager>();
            if (apiManager == null)
            {
                Debug.LogWarning("[MyItemUISetup] ✗ APIManager not found in scene");
            }
        }

        // Tự động tìm và liên kết PlayerItemManager
        if (playerItemManager == null)
        {
            playerItemManager = FindFirstObjectByType<PlayerItemManager>();
            if (playerItemManager == null)
            {
                Debug.LogWarning("[MyItemUISetup] ✗ PlayerItemManager not found in scene");
            }
        }
    }

    private IEnumerator DelayedLoadPlayerItems()
    {
        // Đợi một frame
        yield return null;
        
        // Kiểm tra lại APIManager và PlayerItemManager
        AutoLinkManagers();
        
        // Đợi thêm một frame nữa để đảm bảo managers đã sẵn sàng
        yield return null;
        
        // Kiểm tra xem có token hợp lệ không
        if (apiManager != null && apiManager.HasValidToken())
        {
            LoadPlayerItems();
        }
        else
        {
            Debug.LogWarning("[MyItemUISetup] APIManager not found or no valid token. Waiting for authentication...");
            ShowStatus("Waiting for authentication...");
            
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
        LoadPlayerItems();
    }

    void Reset()
    {
        // Gọi hàm tạo UI khi nhấn nút Reset trong Inspector
        CreateMyItemUI();
    }

    [ContextMenu("Create My Item UI")]
    public void CreateMyItemUIFromMenu()
    {
        CreateMyItemUI();
    }

    [ContextMenu("Delete My Item UI")]
    public void DeleteMyItemUI()
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

        // Xóa ItemPrefab nếu tồn tại
        if (itemPrefab != null)
        {
            DestroyImmediate(itemPrefab);
        }

        // Reset references
        itemContainer = null;
        itemPrefab = null;
        refreshButton = null;
        backToMainMenuButton = null;
        statusText = null;
        loadingPanel = null;
        itemScrollRect = null;
        titleText = null;
    }

    public void CreateMyItemUI()
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

        canvasGO.AddComponent<GraphicRaycaster>();

        // Create main panel
        GameObject mainPanel = CreateUIElement("MainPanel", canvasGO.transform);
        SetFullScreen(mainPanel.GetComponent<RectTransform>());

        // Create background
        GameObject background = CreateUIElement("Background", mainPanel.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        SetFullScreen(background.GetComponent<RectTransform>());

        // Create title
        titleText = CreateText("TitleText", "My Items", mainPanel.transform, 48).GetComponent<TextMeshProUGUI>();
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Create item container with ScrollRect
        GameObject scrollView = CreateUIElement("ScrollView", mainPanel.transform);
        itemScrollRect = scrollView.AddComponent<ScrollRect>();
        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.05f, 0.15f);
        scrollRect.anchorMax = new Vector2(0.95f, 0.85f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        // Create viewport
        GameObject viewport = CreateUIElement("Viewport", scrollView.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.AddComponent<Mask>();
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        // Create item container
        GameObject container = CreateUIElement("ItemContainer", viewport.transform);
        itemContainer = container.transform;
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0.5f, 1);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        // Add ContentSizeFitter to container
        ContentSizeFitter contentFitter = container.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Add VerticalLayoutGroup to container
        VerticalLayoutGroup layoutGroup = container.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;

        // Set ScrollRect references
        itemScrollRect.viewport = viewportRect;
        itemScrollRect.content = containerRect;
        itemScrollRect.vertical = true;
        itemScrollRect.horizontal = false;

        // Create button panel
        GameObject buttonPanel = CreateUIElement("ButtonPanel", mainPanel.transform);
        RectTransform buttonPanelRect = buttonPanel.GetComponent<RectTransform>();
        buttonPanelRect.anchorMin = new Vector2(0, 0);
        buttonPanelRect.anchorMax = new Vector2(1, 0.1f);
        buttonPanelRect.offsetMin = Vector2.zero;
        buttonPanelRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup buttonLayout = buttonPanel.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 20;
        buttonLayout.padding = new RectOffset(20, 20, 10, 10);
        buttonLayout.childControlWidth = false;
        buttonLayout.childControlHeight = true;

        // Create buttons
        refreshButton = CreateButton("RefreshButton", "Refresh", buttonPanel.transform);
        backToMainMenuButton = CreateButton("BackButton", "Back to Main Menu", buttonPanel.transform);
        
        // Create status text
        statusText = CreateText("StatusText", "", mainPanel.transform, 24).GetComponent<TextMeshProUGUI>();
        statusText.color = Color.yellow;
        statusText.alignment = TextAlignmentOptions.Center;
        RectTransform statusRect = statusText.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0.05f);
        statusRect.anchorMax = new Vector2(1, 0.12f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;

        // Create loading panel
        loadingPanel = CreateUIElement("LoadingPanel", mainPanel.transform);
        SetFullScreen(loadingPanel.GetComponent<RectTransform>());
        Image loadingBg = loadingPanel.AddComponent<Image>();
        loadingBg.color = new Color(0, 0, 0, 0.7f);

        GameObject loadingText = CreateText("LoadingText", "Loading...", loadingPanel.transform, 36);
        loadingText.GetComponent<TextMeshProUGUI>().color = Color.white;
        RectTransform loadingTextRect = loadingText.GetComponent<RectTransform>();
        loadingTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        loadingTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        loadingTextRect.anchoredPosition = Vector2.zero;

        loadingPanel.SetActive(false);

        // Create item prefab
        CreateItemPrefab();
    }

    private void CreateItemPrefab()
    {
        if (itemPrefab != null) return;

        itemPrefab = CreateUIElement("ItemPrefab", null);
        RectTransform prefabRect = itemPrefab.GetComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(0, 120);

        // Add background image
        Image bgImage = itemPrefab.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        // Add horizontal layout group
        HorizontalLayoutGroup layout = itemPrefab.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 15;
        layout.padding = new RectOffset(15, 15, 10, 10);
        layout.childControlWidth = false;
        layout.childControlHeight = true;

        // Create item info panel
        GameObject infoPanel = CreateUIElement("InfoPanel", itemPrefab.transform);
        RectTransform infoRect = infoPanel.GetComponent<RectTransform>();
        infoRect.sizeDelta = new Vector2(0, 0);
        infoRect.anchorMin = new Vector2(0, 0);
        infoRect.anchorMax = new Vector2(1, 1);

        VerticalLayoutGroup infoLayout = infoPanel.AddComponent<VerticalLayoutGroup>();
        infoLayout.spacing = 5;
        infoLayout.childControlWidth = true;
        infoLayout.childControlHeight = false;

        // Create item name text
        GameObject nameText = CreateText("NameText", "Item Name", infoPanel.transform, 24);
        nameText.GetComponent<TextMeshProUGUI>().color = Color.white;
        nameText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Create item description text
        GameObject descText = CreateText("DescriptionText", "Item Description", infoPanel.transform, 18);
        descText.GetComponent<TextMeshProUGUI>().color = Color.gray;

        // Create item details text
        GameObject detailsText = CreateText("DetailsText", "Type: None | Amount: 0 | Level: 0", infoPanel.transform, 16);
        detailsText.GetComponent<TextMeshProUGUI>().color = Color.cyan;

        // Add button component
        Button button = itemPrefab.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 0.9f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        button.colors = colors;

        itemPrefab.SetActive(false);
    }

    private void SetupUI()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshClick);

        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClick);

        if (playerItemManager != null)
            playerItemManager.OnPlayerItemsChanged += OnPlayerItemsLoaded;
    }

    private void LoadPlayerItems()
    {
        if (playerItemManager == null)
        {
            ShowStatus("PlayerItemManager not found");
            return;
        }

        ShowLoading(true);
        ShowStatus("Loading player items...");
        playerItemManager.FetchPlayerItems();
    }

    private void OnPlayerItemsLoaded(List<InventoryItem> items)
    {
        ShowLoading(false);
        
        if (items == null || items.Count == 0)
        {
            ShowStatus("No items found");
            ClearItems();
            return;
        }

        ShowStatus($"Loaded {items.Count} items");
        PopulateItems(items);
    }

    private void PopulateItems(List<InventoryItem> items)
    {
        ClearItems();

        foreach (var item in items)
        {
            GameObject itemObj = Instantiate(itemPrefab, itemContainer);
            itemObj.SetActive(true);

            // Set item name
            TextMeshProUGUI nameText = itemObj.transform.Find("InfoPanel/NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = item.name;

            // Set item description
            TextMeshProUGUI descText = itemObj.transform.Find("InfoPanel/DescriptionText").GetComponent<TextMeshProUGUI>();
            descText.text = item.description;

            // Set item details
            TextMeshProUGUI detailsText = itemObj.transform.Find("InfoPanel/DetailsText").GetComponent<TextMeshProUGUI>();
            detailsText.text = $"Type: {item.type} | Amount: {item.amount} | Level Max: {item.level_max} | Stackable: {(item.stackable == 1 ? "Yes" : "No")}";

            // Add click handler
            Button itemButton = itemObj.GetComponent<Button>();
            itemButton.onClick.AddListener(() => OnItemSelected(item));

            itemObjects.Add(itemObj);
        }
    }

    private void ClearItems()
    {
        foreach (var item in itemObjects)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        itemObjects.Clear();
    }

    private void OnItemSelected(InventoryItem item)
    {
        ShowStatus($"Selected: {item.name} (Amount: {item.amount})");
        Debug.Log($"[MyItemUISetup] Selected item: {item.name}, ID: {item.id}, Amount: {item.amount}");
    }

    public void OnRefreshClick()
    {
        ShowStatus("Refreshing items...");
        LoadPlayerItems();
    }

    public void OnBackToMainMenuClick()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneController.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("[MyItemUISetup] Main menu scene name not set");
        }
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[MyItemUISetup] {message}");
    }

    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }
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
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return go;
    }

    Button CreateButton(string name, string text, Transform parent)
    {
        GameObject go = CreateUIElement(name, parent);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        Button button = go.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        button.colors = colors;

        GameObject textGO = CreateText("Text", text, go.transform, 24);
        textGO.GetComponent<TextMeshProUGUI>().color = Color.white;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    void SetFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    [ContextMenu("Retry Load Player Items")]
    public void RetryLoadPlayerItems()
    {
        LoadPlayerItems();
    }

    [ContextMenu("Force Load Player Items")]
    public void ForceLoadPlayerItems()
    {
        if (apiManager != null)
        {
            LoadPlayerItems();
        }
    }

    private void OnDestroy()
    {
        if (playerItemManager != null)
            playerItemManager.OnPlayerItemsChanged -= OnPlayerItemsLoaded;
    }
} 