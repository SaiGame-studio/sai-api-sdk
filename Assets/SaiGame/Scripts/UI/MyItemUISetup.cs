using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class MyItemUISetup : MonoBehaviour
{
    [Header("Auto Setup UI")]
    public bool autoSetup = true;

    [Header("Manager Integration")]
    public APIManager apiManager;
    public ItemProfileManager itemProfileManager;
    public PlayerItemManager playerItemManager;

    [Header("UI References (Auto-assigned)")]
    [SerializeField] public Transform buttonItemContainer;
    [SerializeField] public GameObject buttonItemPrefab;
    [SerializeField] public Button refreshButton;
    [SerializeField] public Button backToMainMenuButton;
    [SerializeField] public GameObject loadingPanel;
    [SerializeField] public ScrollRect itemScrollRect;

    [Header("Scene Management")]
    public string mainMenuSceneName = "2_MainMenu";

    [Header("Item Display Settings")]
    [SerializeField] public int numberOfItemsToDisplay = 6;

    // Private variables for tracking UI elements
    private List<GameObject> buttonItems = new List<GameObject>();
    private List<ItemProfileSimple> availableItemProfiles = new List<ItemProfileSimple>();
    private List<InventoryItem> playerInventoryItems = new List<InventoryItem>();

    void Start()
    {
        // Tự động tìm và liên kết Managers
        AutoLinkManagers();

        if (autoSetup)
        {
            CreateMyItemUI();
        }

        SetupUI();
        
        // Delay một frame để đảm bảo tất cả components đã được khởi tạo
        StartCoroutine(DelayedLoadData());
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

        // Tự động tìm và liên kết ItemProfileManager
        if (itemProfileManager == null)
        {
            itemProfileManager = FindFirstObjectByType<ItemProfileManager>();
            if (itemProfileManager == null)
            {
                Debug.LogWarning("[MyItemUISetup] ✗ ItemProfileManager not found in scene");
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

    private IEnumerator DelayedLoadData()
    {
        // Đợi một frame
        yield return null;
        
        // Kiểm tra lại Managers
        AutoLinkManagers();
        
        // Đợi thêm một frame nữa để đảm bảo managers đã sẵn sàng
        yield return null;
        
        // Kiểm tra xem có token hợp lệ không
        if (apiManager != null && apiManager.HasValidToken())
        {
            LoadData();
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
        LoadData();
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
        // Tìm và xóa Canvas có tên CanvasMyItem
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.name == "CanvasMyItem")
            {
                DestroyImmediate(canvas.gameObject);
                break;
            }
        }

        // Tìm và xóa EventSystem nếu không có UI nào khác sử dụng
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();
        if (existingEventSystem != null)
        {
            // Kiểm tra xem có Canvas nào khác không
            Canvas[] remainingCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (remainingCanvases.Length == 0)
            {
                DestroyImmediate(existingEventSystem.gameObject);
            }
        }

        // Xóa ButtonItemPrefab nếu tồn tại
        if (buttonItemPrefab != null)
        {
            DestroyImmediate(buttonItemPrefab);
        }

        // Reset references
        buttonItemContainer = null;
        buttonItemPrefab = null;
        refreshButton = null;
        backToMainMenuButton = null;
        loadingPanel = null;
        itemScrollRect = null;
    }

    public void CreateMyItemUI()
    {
        // Check if CanvasMyItem already exists
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas _canvas in allCanvases)
        {
            if (_canvas.name == "CanvasMyItem")
            {
                return; // Already exists
            }
        }

        // Check and create EventSystem if needed
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();
        if (existingEventSystem == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        // Create Canvas with specific name
        GameObject canvasGO = new GameObject("CanvasMyItem");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();

        // Create Main Panel
        GameObject mainPanel = CreateUIElement("MyItemPanel", canvasGO.transform);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.sizeDelta = new Vector2(1600, 900);

        // Create Top Buttons Panel
        GameObject topButtonsPanel = CreateUIElement("TopButtonsPanel", mainPanel.transform);
        RectTransform topButtonsRect = topButtonsPanel.GetComponent<RectTransform>();
        topButtonsRect.anchoredPosition = new Vector2(0, 340);
        topButtonsRect.sizeDelta = new Vector2(800, 80);

        // Create Refresh Button
        GameObject refreshBtn = CreateButton("RefreshButton", "REFRESH", topButtonsPanel.transform);
        RectTransform refreshRect = refreshBtn.GetComponent<RectTransform>();
        refreshRect.anchoredPosition = new Vector2(-200, 0);
        refreshRect.sizeDelta = new Vector2(200, 60);
        refreshButton = refreshBtn.GetComponent<Button>();

        // Set font size cho refresh button text
        TextMeshProUGUI refreshText = refreshBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (refreshText != null)
            refreshText.fontSize = 28;

        // Create Back to Main Menu Button
        GameObject backBtn = CreateButton("BackToMainMenuButton", "BACK TO MENU", topButtonsPanel.transform);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchoredPosition = new Vector2(200, 0);
        backRect.sizeDelta = new Vector2(200, 60);
        backToMainMenuButton = backBtn.GetComponent<Button>();

        // Set color cho back button
        Button backBtnComp = backBtn.GetComponent<Button>();
        ColorBlock backColors = backBtnComp.colors;
        backColors.normalColor = new Color(0.8f, 0.3f, 0.3f, 1f); // Red color
        backBtnComp.colors = backColors;

        // Set font size cho back button text
        TextMeshProUGUI backText = backBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (backText != null)
            backText.fontSize = 28;

        // Create Loading Panel
        GameObject loadingPanelGO = CreateUIElement("LoadingPanel", mainPanel.transform);
        Image loadingImage = loadingPanelGO.AddComponent<Image>();
        loadingImage.color = new Color(0, 0, 0, 0.7f);
        RectTransform loadingRect = loadingPanelGO.GetComponent<RectTransform>();
        loadingRect.sizeDelta = new Vector2(400, 200);
        loadingRect.anchoredPosition = new Vector2(0, 0);

        GameObject loadingTextGO = CreateText("LoadingText", "Loading...", loadingPanelGO.transform, 32);
        loadingTextGO.GetComponent<TextMeshProUGUI>().color = Color.white;
        loadingTextGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        loadingPanel = loadingPanelGO;
        loadingPanel.SetActive(false);

        // Create Item Grid Container (Top Right Corner)
        GameObject scrollViewGO = CreateUIElement("ItemScrollView", mainPanel.transform);
        RectTransform scrollViewRect = scrollViewGO.GetComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(1, 1);
        scrollViewRect.anchorMax = new Vector2(1, 1);
        scrollViewRect.anchoredPosition = new Vector2(-20, -20); // 20px margin from top-right corner
        scrollViewRect.sizeDelta = new Vector2(300, 600); // Narrower width for single column

        ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        Image scrollViewImage = scrollViewGO.AddComponent<Image>();
        scrollViewImage.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

        // Create Content for ScrollView
        GameObject contentGO = CreateUIElement("Content", scrollViewGO.transform);
        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.sizeDelta = Vector2.zero;
        contentRect.anchoredPosition = Vector2.zero;

        // Add GridLayoutGroup cho content (Single Column)
        GridLayoutGroup gridLayout = contentGO.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(260, 120);
        gridLayout.spacing = new Vector2(0, 10);
        gridLayout.padding = new RectOffset(10, 10, 10, 10);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 1; // Single column

        // Setup ScrollRect
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        itemScrollRect = scrollRect;
        buttonItemContainer = contentGO.transform;

        // Create ButtonItem Prefab
        CreateButtonItemPrefab();

        Debug.Log("[MyItemUISetup] ✓ My Item UI created successfully");
    }

    private void CreateButtonItemPrefab()
    {
        if (buttonItemPrefab != null)
        {
            DestroyImmediate(buttonItemPrefab);
        }

        // Create ButtonItem Prefab
        GameObject prefab = CreateUIElement("ButtonItemPrefab", null);
        
        // Add Image component for background
        Image bgImage = prefab.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);
        
        RectTransform prefabRect = prefab.GetComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(260, 120); // Match với cellSize của grid

        // Create Dropdown for ItemProfile selection
        GameObject dropdownGO = CreateUIElement("ItemProfileDropdown", prefab.transform);
        RectTransform dropdownRect = dropdownGO.GetComponent<RectTransform>();
        dropdownRect.anchoredPosition = new Vector2(0, 25);
        dropdownRect.sizeDelta = new Vector2(240, 30); // Slightly smaller to fit in 260px width

        TMP_Dropdown dropdown = dropdownGO.AddComponent<TMP_Dropdown>();
        Image dropdownImage = dropdownGO.AddComponent<Image>();
        dropdownImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        // Create simple dropdown label
        GameObject labelGO = CreateText("Label", "Select Item Profile", dropdownGO.transform, 14);
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(10, 0);
        labelRect.sizeDelta = new Vector2(-30, 0);
        SetFullScreen(labelRect);
        labelGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
        dropdown.captionText = labelGO.GetComponent<TextMeshProUGUI>();

        // Create Item Info Text
        GameObject itemInfoGO = CreateText("ItemInfo", "No item selected", prefab.transform, 14);
        RectTransform itemInfoRect = itemInfoGO.GetComponent<RectTransform>();
        itemInfoRect.anchoredPosition = new Vector2(0, -10);
        itemInfoRect.sizeDelta = new Vector2(240, 40); // Match với dropdown width
        TextMeshProUGUI itemInfoText = itemInfoGO.GetComponent<TextMeshProUGUI>();
        itemInfoText.color = Color.white;
        itemInfoText.alignment = TextAlignmentOptions.Center;

        // Add ButtonItemController component
        prefab.AddComponent<ButtonItemController>();

        buttonItemPrefab = prefab;
        buttonItemPrefab.SetActive(false);

        Debug.Log("[MyItemUISetup] ✓ ButtonItem Prefab created");
    }

    private void SetupUI()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(OnRefreshClick);
        }

        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.RemoveAllListeners();
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClick);
        }
    }

    public void OnRefreshClick()
    {
        LoadData();
    }

    public void OnBackToMainMenuClick()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneController.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("[MyItemUISetup] Main menu scene name is not set!");
        }
    }

    private void LoadData()
    {
        ShowLoading(true);
        ShowStatus("Loading data...");
        
        StartCoroutine(LoadDataCoroutine());
    }

    private IEnumerator LoadDataCoroutine()
    {
        // Load ItemProfiles first
        if (itemProfileManager != null)
        {
            itemProfileManager.FetchItemProfiles();
            
            // Wait for ItemProfiles to load
            float timeout = 10f;
            float elapsed = 0f;
            while (itemProfileManager.ItemProfiles.Count == 0 && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            availableItemProfiles = new List<ItemProfileSimple>(itemProfileManager.ItemProfiles);
        }

        // Load Player Items
        if (playerItemManager != null)
        {
            playerItemManager.FetchPlayerItems();
            
            // Wait for Player Items to load
            float timeout = 10f;
            float elapsed = 0f;
            while (playerItemManager.PlayerItems.Count == 0 && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            playerInventoryItems = new List<InventoryItem>(playerItemManager.PlayerItems);
        }

        PopulateButtonItems();
        ShowLoading(false);
        ShowStatus($"Loaded {availableItemProfiles.Count} item profiles");
    }

    private void PopulateButtonItems()
    {
        ClearButtonItems();

        // Create ButtonItem theo số lượng được thiết lập trong inspector
        int itemsToCreate = Mathf.Max(1, numberOfItemsToDisplay); // Đảm bảo ít nhất 1 item
        for (int i = 0; i < itemsToCreate; i++)
        {
            CreateButtonItem();
        }
    }

    private void CreateButtonItem()
    {
        if (buttonItemPrefab == null || buttonItemContainer == null) return;

        GameObject newItem = Instantiate(buttonItemPrefab, buttonItemContainer);
        newItem.SetActive(true);
        
        ButtonItemController controller = newItem.GetComponent<ButtonItemController>();
        if (controller != null)
        {
            controller.Initialize(availableItemProfiles, playerInventoryItems);
        }

        buttonItems.Add(newItem);
    }

    private void ClearButtonItems()
    {
        foreach (GameObject item in buttonItems)
        {
            if (item != null)
            {
                DestroyImmediate(item);
            }
        }
        buttonItems.Clear();
    }

    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.AddComponent<RectTransform>();
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }
        return go;
    }

    GameObject CreateText(string name, string text, Transform parent, int fontSize)
    {
        GameObject go = CreateUIElement(name, parent);
        TextMeshProUGUI textComp = go.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.color = Color.black;
        textComp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject go = CreateUIElement(name, parent);
        
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.7f, 0.7f, 0.8f, 1f);
        
        Button button = go.AddComponent<Button>();
        
        GameObject textGO = CreateText($"{name}_Text", text, go.transform, 18);
        textGO.GetComponent<TextMeshProUGUI>().color = Color.white;
        
        return go;
    }

    void SetFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        if (apiManager != null)
        {
            apiManager.OnAuthenticationSuccess -= OnAuthenticationSuccess;
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
        ShowStatus("Ready");
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
} 