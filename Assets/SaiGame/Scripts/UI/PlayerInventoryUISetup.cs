using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UI Setup cho Player Inventory - tương tự như ShopUISetup nhưng dành cho inventory
/// Grid Shop -> Grid Inventories
/// Grid Item -> Grid ItemInInventory
/// </summary>
public class PlayerInventoryUISetup : MonoBehaviour
{
    [Header("Auto Setup UI")]
    public bool autoSetup = true;

    [Header("APIManager Integration")]
    public APIManager apiManager;

    [Header("UI References - Inventory Items (Right Panel)")]
    [SerializeField] public Transform inventoryItemContainer;
    [SerializeField] public GameObject inventoryItemPrefab;
    [SerializeField] public ScrollRect inventoryScrollRect;
    [SerializeField] public GameObject inventoryItemsPanel;

    [Header("UI References - Inventory Selection (Left Panel)")] 
    [SerializeField] public Transform inventorySelectionContainer;
    [SerializeField] public GameObject inventorySelectionPrefab;
    [SerializeField] public GameObject inventorySelectionPanel;

    [Header("UI References - Controls")]
    [SerializeField] public Button refreshButton;
    [SerializeField] public Button backToMainMenuButton;
    [SerializeField] public Button useItemButton;
    [SerializeField] public TextMeshProUGUI statusText;
    [SerializeField] public GameObject loadingPanel;

    [Header("Scene Management")]
    public string mainMenuSceneName = SceneNames.MAIN_MENU;
    public string myItemSceneName = SceneNames.MY_ITEMS;

    [Header("Dummy Data")]
    public bool showDummyData = true;
    public int dummyInventoryCount = 6;
    public int dummyItemCount = 12;

    // Private variables for tracking UI elements
    private List<GameObject> inventoryItems = new List<GameObject>();
    private List<GameObject> inventorySelectionItems = new List<GameObject>();
    private InventoryItem selectedInventory = null;

    void Awake()
    {
        // Khi play game thì ẩn dummy data đi bằng cách xóa các item con
        if (Application.isPlaying)
        {
            // Xóa các items trong inventory selection
            if (inventorySelectionContainer != null)
            {
                foreach (Transform child in inventorySelectionContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            if (inventorySelectionItems != null) inventorySelectionItems.Clear();

            // Xóa các items trong inventory
            if (inventoryItemContainer != null)
            {
                foreach (Transform child in inventoryItemContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            if (inventoryItems != null) inventoryItems.Clear();
        }
    }

    void Start()
    {
        if (apiManager == null)
            apiManager = APIManager.Instance;

        SetupEventListeners();
        StartCoroutine(DelayedLoadInventoryData());
    }

    private void OnValidate()
    {
        // Tạo dummy data khi ở Editor mode và không play game
        if (showDummyData && !Application.isPlaying && Application.isEditor)
        {
            LoadDummyDataSafe(true);
        }
    }

    private void SetupEventListeners()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshClick);
        
        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClick);
        
        if (useItemButton != null)
            useItemButton.onClick.AddListener(OnUseItemClick);
    }

    private IEnumerator DelayedLoadInventoryData()
    {
        yield return null;
        
        if (APIManager.Instance != null && APIManager.Instance.HasValidToken())
        {
            LoadInventoryData();
        }
        else
        {
            ShowStatus("Waiting for authentication...");
            if (showDummyData && Application.isEditor)
            {
                LoadDummyData();
            }
        }
    }

    private void LoadInventoryData()
    {
        ShowLoading(true);
        ShowStatus("Loading inventories...");
        
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.OnFilteredInventoryItemsChanged += OnInventoriesLoaded;
            PlayerInventoryManager.Instance.RefreshInventory();
        }
        else
        {
            ShowLoading(false);
            ShowStatus("PlayerInventoryManager not found");
        }
    }

    private void OnInventoriesLoaded(List<InventoryItem> inventories)
    {
        ShowLoading(false);
        
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.OnFilteredInventoryItemsChanged -= OnInventoriesLoaded;
        }
        
        if (inventories != null && inventories.Count > 0)
        {
            ShowStatus($"Loaded {inventories.Count} inventories");
            PopulateInventorySelection(inventories);
        }
        else
        {
            ShowStatus("No inventories found");
        }
    }

    private void PopulateInventorySelection(List<InventoryItem> inventories)
    {
        ClearInventorySelectionItems();
        
        foreach (var inventory in inventories)
        {
            if (inventorySelectionPrefab != null && inventorySelectionContainer != null)
            {
                GameObject item = Instantiate(inventorySelectionPrefab, inventorySelectionContainer);
                item.SetActive(true);
                
                SetInventorySelectionData(item, inventory);
                
                Button button = item.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnInventorySelected(inventory));
                }
                
                inventorySelectionItems.Add(item);
            }
        }
    }

    private void SetInventorySelectionData(GameObject itemObject, InventoryItem inventory)
    {
        TextMeshProUGUI[] texts = itemObject.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            switch (text.name)
            {
                case "Name":
                    text.text = inventory.name;
                    break;
                case "Amount":
                    text.text = $"x{inventory.amount}";
                    break;
                case "Type":
                    text.text = inventory.type;
                    break;
            }
        }
    }

    private void OnInventorySelected(InventoryItem inventory)
    {
        selectedInventory = inventory;
        ShowStatus($"Selected: {inventory.name}");
        
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.OnInventoryItemsChanged += OnInventoryItemsLoaded;
            PlayerInventoryManager.Instance.LoadInventoryItems(inventory.id);
        }
    }

    private void OnInventoryItemsLoaded(List<InventoryItem> items)
    {
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.OnInventoryItemsChanged -= OnInventoryItemsLoaded;
        }
        
        if (items != null && items.Count > 0)
        {
            ShowStatus($"Loaded {items.Count} items");
            PopulateInventoryItems(items);
        }
        else
        {
            ShowStatus("No items in inventory");
            ClearInventoryItems();
        }
    }

    private void PopulateInventoryItems(List<InventoryItem> items)
    {
        ClearInventoryItems();
        
        foreach (var item in items)
        {
            if (inventoryItemPrefab != null && inventoryItemContainer != null)
            {
                GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryItemContainer);
                itemObj.SetActive(true);
                
                SetInventoryItemData(itemObj, item);
                
                Button button = itemObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnItemSelected(item));
                }
                
                inventoryItems.Add(itemObj);
            }
        }
    }

    private void SetInventoryItemData(GameObject itemObject, InventoryItem item)
    {
        TextMeshProUGUI[] texts = itemObject.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            switch (text.name)
            {
                case "Name":
                    text.text = item.name;
                    break;
                case "Amount":
                    text.text = $"x{item.amount}";
                    break;
                case "Type":
                    text.text = item.type;
                    break;
            }
        }
    }

    private void OnItemSelected(InventoryItem item)
    {
        ShowStatus($"Selected item: {item.name}");
        if (useItemButton != null)
            useItemButton.interactable = true;
    }

    public void OnRefreshClick()
    {
        LoadInventoryData();
    }

    public void OnBackToMainMenuClick()
    {
        SceneController.LoadScene(mainMenuSceneName);
    }

    public void OnUseItemClick()
    {
        ShowStatus("Use item not implemented yet");
    }

    private void ClearInventorySelectionItems()
    {
        // Xóa các GameObject theo dõi trong list
        foreach (GameObject item in inventorySelectionItems)
        {
            if (item != null) 
            {
                if (Application.isPlaying)
                {
                    Destroy(item);
                }
                else
                {
#if UNITY_EDITOR
                    // Sử dụng delayCall để tránh lỗi trong OnValidate
                    UnityEditor.EditorApplication.delayCall += () => 
                    {
                        if (item != null)
                            DestroyImmediate(item);
                    };
#endif
                }
            }
        }
        inventorySelectionItems.Clear();
        
        // Xóa thêm bất kỳ GameObject con nào còn sót lại trong container
        if (inventorySelectionContainer != null)
        {
            List<Transform> childrenToDestroy = new List<Transform>();
            foreach (Transform child in inventorySelectionContainer)
            {
                childrenToDestroy.Add(child);
            }
            
            foreach (Transform child in childrenToDestroy)
            {
                if (child != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.delayCall += () => 
                        {
                            if (child != null)
                                DestroyImmediate(child.gameObject);
                        };
#endif
                    }
                }
            }
        }
    }

    private void ClearInventoryItems()
    {
        // Xóa các GameObject theo dõi trong list
        foreach (GameObject item in inventoryItems)
        {
            if (item != null) 
            {
                if (Application.isPlaying)
                {
                    Destroy(item);
                }
                else
                {
#if UNITY_EDITOR
                    // Sử dụng delayCall để tránh lỗi trong OnValidate
                    UnityEditor.EditorApplication.delayCall += () => 
                    {
                        if (item != null)
                            DestroyImmediate(item);
                    };
#endif
                }
            }
        }
        inventoryItems.Clear();
        
        // Xóa thêm bất kỳ GameObject con nào còn sót lại trong container
        if (inventoryItemContainer != null)
        {
            List<Transform> childrenToDestroy = new List<Transform>();
            foreach (Transform child in inventoryItemContainer)
            {
                childrenToDestroy.Add(child);
            }
            
            foreach (Transform child in childrenToDestroy)
            {
                if (child != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.delayCall += () => 
                        {
                            if (child != null)
                                DestroyImmediate(child.gameObject);
                        };
#endif
                    }
                }
            }
        }
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(show);
    }

    // Expose selectedInventory for editor  
    public InventoryItem GetSelectedInventory() { return selectedInventory; }

    [ContextMenu("Create Inventory UI")]
    public void CreateInventoryUI()
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

        // Create Main Panel
        GameObject mainPanel = CreateUIElement("InventoryPanel", canvasGO.transform);
        SetFullScreen(mainPanel.GetComponent<RectTransform>());

        // Create buttons at top
        CreateInventoryButtons(mainPanel);

        // Create Inventory Selection Panel (Left side)
        CreateInventorySelectionPanel(mainPanel);

        // Create Inventory Items Panel (Right side)  
        CreateInventoryItemsPanel(mainPanel);

        // Create Status and Loading elements
        CreateStatusAndLoadingElements(mainPanel);

        // Create prefabs
        CreateInventorySelectionPrefab();
        CreateInventoryItemPrefab();

        // Create Scene Title
        GameObject sceneTitleGO = CreateText("SceneTitle", "My Inventory", mainPanel.transform, 48);
        RectTransform sceneTitleRect = sceneTitleGO.GetComponent<RectTransform>();
        sceneTitleRect.anchorMin = new Vector2(0, 1);
        sceneTitleRect.anchorMax = new Vector2(0, 1);
        sceneTitleRect.pivot = new Vector2(0, 1);
        sceneTitleRect.anchoredPosition = new Vector2(20, -20);
        sceneTitleRect.sizeDelta = new Vector2(300, 80);
        TextMeshProUGUI sceneTitleText = sceneTitleGO.GetComponent<TextMeshProUGUI>();
        sceneTitleText.color = Color.white;
        sceneTitleText.alignment = TextAlignmentOptions.TopLeft;
        sceneTitleText.fontStyle = FontStyles.Bold;

        // Load dummy data if in editor
        if (showDummyData && !Application.isPlaying)
        {
            LoadDummyDataSafe(true);
        }

        ShowStatus("Inventory UI created successfully");
        Debug.Log("PlayerInventoryUISetup: UI created successfully");
    }

    [ContextMenu("Delete Inventory UI")]
    public void DeleteInventoryUI()
    {
        // Find and delete Canvas
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            DestroyImmediate(existingCanvas.gameObject);
        }

        // Find and delete EventSystem if no other UI is using it
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();
        if (existingEventSystem != null)
        {
            // Check if there are other Canvases
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (allCanvases.Length == 0)
            {
                DestroyImmediate(existingEventSystem.gameObject);
            }
        }

        // Delete prefabs if they exist
        if (inventorySelectionPrefab != null)
        {
            DestroyImmediate(inventorySelectionPrefab);
        }

        if (inventoryItemPrefab != null)
        {
            DestroyImmediate(inventoryItemPrefab);
        }

        // Reset references
        inventoryItemContainer = null;
        inventoryItemPrefab = null;
        refreshButton = null;
        backToMainMenuButton = null;
        useItemButton = null;
        statusText = null;
        loadingPanel = null;
        inventoryScrollRect = null;
        inventorySelectionContainer = null;
        inventorySelectionPrefab = null;
        inventorySelectionPanel = null;
        inventoryItemsPanel = null;

        Debug.Log("PlayerInventoryUISetup: Inventory UI deleted successfully");
    }

    [ContextMenu("Clear All Data")]
    public void ClearAllData()
    {
        ClearInventorySelectionItems();
        ClearInventoryItems();
        selectedInventory = null;
        
        if (useItemButton != null)
        {
            useItemButton.interactable = false;
        }
        
        ShowStatus("All data cleared");
    }

    [ContextMenu("Refresh Inventory Data")]
    public void RefreshInventoryData()
    {
        LoadInventoryData();
    }

    [ContextMenu("Load Dummy Data")]
    public void LoadDummyData()
    {
        LoadDummyDataSafe(false);
    }
    
    private void LoadDummyDataSafe(bool forceDelay = false)
    {
        // Nếu được gọi từ OnValidate hoặc context khác cần delay
        if (forceDelay || !Application.isPlaying)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += LoadDummyDataInternal;
#endif
        }
        else
        {
            LoadDummyDataInternal();
        }
    }
    
    private void LoadDummyDataInternal()
    {
        List<InventoryItem> dummyInventories = new List<InventoryItem>();
        
        for (int i = 1; i <= dummyInventoryCount; i++)
        {
            InventoryItem inventory = new InventoryItem
            {
                id = $"inv_{i}",
                name = $"Inventory {i}",
                type = "Inventory",
                amount = Random.Range(1, 20)
            };
            dummyInventories.Add(inventory);
        }
        
        PopulateInventorySelection(dummyInventories);
        ShowStatus("Dummy data loaded");
    }

    // Methods for Editor buttons
    public void ShowDummyDataButton()
    {
        if (Application.isEditor)
        {
            LoadDummyData();
        }
        else
        {
            Debug.LogWarning("[PlayerInventoryUISetup] Dummy data only available in Editor mode");
        }
    }

    public void DeleteDummyDataButton()
    {
        ClearInventorySelectionItems();
        ClearInventoryItems();
        
        // Reset selected inventory
        selectedInventory = null;
        if (useItemButton != null)
        {
            useItemButton.interactable = false;
        }
        
#if UNITY_EDITOR
        // Delay status message để đảm bảo việc xóa hoàn thành
        UnityEditor.EditorApplication.delayCall += () => 
        {
            ShowStatus("Dummy data deleted");
        };
#else
        ShowStatus("Dummy data deleted");
#endif
    }

    public void ToggleDummyDataButton()
    {
        showDummyData = !showDummyData;
        if (showDummyData && Application.isEditor)
        {
            LoadDummyDataSafe(true);
        }
        else
        {
            ClearInventorySelectionItems();
            ClearInventoryItems();
            ShowStatus($"Dummy data {(showDummyData ? "enabled" : "disabled")}");
        }
    }

    // Test methods for Editor
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

    #region UI Creation Helper Methods

    private void CreateInventoryButtons(GameObject mainPanel)
    {
        // Create Back to Main Menu button
        GameObject backButtonGO = CreateButton("BackButton", "Main Menu", mainPanel.transform);
        backToMainMenuButton = backButtonGO.GetComponent<Button>();
        
        RectTransform backRect = backToMainMenuButton.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 1);
        backRect.anchorMax = new Vector2(0, 1);
        backRect.pivot = new Vector2(0, 1);
        backRect.anchoredPosition = new Vector2(320, -20);
        backRect.sizeDelta = new Vector2(200, 80);

        // Create Use Item button
        GameObject useButtonGO = CreateButton("UseItemButton", "Use Item", mainPanel.transform);
        useItemButton = useButtonGO.GetComponent<Button>();
        useItemButton.interactable = false;
        
        RectTransform useRect = useItemButton.GetComponent<RectTransform>();
        useRect.anchorMin = new Vector2(0, 1);
        useRect.anchorMax = new Vector2(0, 1);
        useRect.pivot = new Vector2(0, 1);
        useRect.anchoredPosition = new Vector2(530, -20);
        useRect.sizeDelta = new Vector2(150, 80);

        // Create Refresh button
        GameObject refreshButtonGO = CreateButton("RefreshButton", "Refresh", mainPanel.transform);
        refreshButton = refreshButtonGO.GetComponent<Button>();
        
        RectTransform refreshRect = refreshButton.GetComponent<RectTransform>();
        refreshRect.anchorMin = new Vector2(1, 1);
        refreshRect.anchorMax = new Vector2(1, 1);
        refreshRect.pivot = new Vector2(1, 1);
        refreshRect.anchoredPosition = new Vector2(-20, -20);
        refreshRect.sizeDelta = new Vector2(120, 80);
    }

    private void CreateInventorySelectionPanel(GameObject mainPanel)
    {
        // Create Inventory Selection Panel (Left side)
        GameObject inventorySelectionPanelGO = CreateUIElement("InventorySelectionPanel", mainPanel.transform);
        RectTransform inventorySelectionRect = inventorySelectionPanelGO.GetComponent<RectTransform>();
        inventorySelectionRect.anchorMin = new Vector2(0, 1);
        inventorySelectionRect.anchorMax = new Vector2(0, 1);
        inventorySelectionRect.pivot = new Vector2(0, 1);
        inventorySelectionRect.anchoredPosition = new Vector2(20, -150);
        inventorySelectionRect.sizeDelta = new Vector2(430, 600);
        inventorySelectionPanel = inventorySelectionPanelGO;

        // Create Container with ScrollRect
        GameObject containerGO = CreateUIElement("InventorySelectionContainer", inventorySelectionPanelGO.transform);
        RectTransform containerRect = containerGO.GetComponent<RectTransform>();
        containerRect.anchoredPosition = new Vector2(0, 0);
        containerRect.sizeDelta = new Vector2(430, 600);

        ScrollRect scrollRect = containerGO.AddComponent<ScrollRect>();

        // Create Content for ScrollRect
        GameObject contentGO = CreateUIElement("InventorySelectionContent", containerGO.transform);
        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(430, 600);
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(0, 1);
        contentRect.pivot = new Vector2(0, 1);

        // Setup ScrollRect
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10f;

        // Add Grid Layout Group
        GridLayoutGroup gridGroup = contentGO.AddComponent<GridLayoutGroup>();
        gridGroup.cellSize = new Vector2(400, 100);
        gridGroup.spacing = new Vector2(10, 10);
        gridGroup.padding = new RectOffset(10, 10, 10, 10);
        gridGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridGroup.constraintCount = 1;

        // Add Content Size Fitter
        ContentSizeFitter sizeFitter = contentGO.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        inventorySelectionContainer = contentGO.transform;
    }

    private void CreateInventoryItemsPanel(GameObject mainPanel)
    {
        // Create Inventory Items Panel (Right side)
        GameObject inventoryItemsPanelGO = CreateUIElement("InventoryItemsPanel", mainPanel.transform);
        RectTransform inventoryItemsRect = inventoryItemsPanelGO.GetComponent<RectTransform>();
        inventoryItemsRect.anchorMin = new Vector2(0, 1);
        inventoryItemsRect.anchorMax = new Vector2(0, 1);
        inventoryItemsRect.pivot = new Vector2(0, 1);
        inventoryItemsRect.anchoredPosition = new Vector2(460, -150);
        inventoryItemsRect.sizeDelta = new Vector2(800, 600);
        inventoryItemsPanel = inventoryItemsPanelGO;

        // Create Container with ScrollRect
        GameObject containerGO = CreateUIElement("InventoryItemsContainer", inventoryItemsPanelGO.transform);
        RectTransform containerRect = containerGO.GetComponent<RectTransform>();
        containerRect.anchoredPosition = new Vector2(0, 0);
        containerRect.sizeDelta = new Vector2(800, 600);

        ScrollRect scrollRect = containerGO.AddComponent<ScrollRect>();
        inventoryScrollRect = scrollRect;

        // Create Content for ScrollRect
        GameObject contentGO = CreateUIElement("Content", containerGO.transform);
        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(800, 600);
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = new Vector2(0, contentRect.offsetMin.y);
        contentRect.offsetMax = new Vector2(0, contentRect.offsetMax.y);

        // Setup ScrollRect
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10f;

        // Add Grid Layout Group
        GridLayoutGroup layoutGroup = contentGO.AddComponent<GridLayoutGroup>();
        layoutGroup.cellSize = new Vector2(150, 180);
        layoutGroup.spacing = new Vector2(15, 15);
        layoutGroup.padding = new RectOffset(15, 15, 15, 15);
        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = 4;

        // Add Content Size Fitter
        ContentSizeFitter sizeFitter = contentGO.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        inventoryItemContainer = contentGO.transform;
    }

    private void CreateStatusAndLoadingElements(GameObject mainPanel)
    {
        // Create Status Text
        GameObject statusGO = CreateText("StatusText", "", mainPanel.transform, 24);
        RectTransform statusRect = statusGO.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0);
        statusRect.anchorMax = new Vector2(1, 0.1f);
        statusRect.offsetMin = new Vector2(20, 10);
        statusRect.offsetMax = new Vector2(-20, -10);
        statusText = statusGO.GetComponent<TextMeshProUGUI>();
        statusText.color = Color.yellow;
        statusText.alignment = TextAlignmentOptions.Center;

        // Create Loading Panel
        GameObject loadingGO = CreateUIElement("LoadingPanel", mainPanel.transform);
        RectTransform loadingRect = loadingGO.GetComponent<RectTransform>();
        loadingRect.sizeDelta = new Vector2(400, 200);
        loadingRect.anchoredPosition = new Vector2(0, 0);
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
    }

    private void CreateInventorySelectionPrefab()
    {
        GameObject prefab = CreateUIElement("InventorySelectionPrefab", null);
        RectTransform prefabRect = prefab.GetComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(400, 100);

        // Add background
        Image bg = prefab.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.4f, 0.8f);

        // Add button
        Button button = prefab.AddComponent<Button>();

        // Create name text - Font size gấp đôi: 20 -> 40, với Auto Size
        GameObject nameGO = CreateText("Name", "Inventory Name", prefab.transform, 40, true);
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = new Vector2(10, 0);
        nameRect.offsetMax = new Vector2(-10, -5);

        // Create amount text - Font size gấp đôi: 16 -> 32, với Auto Size
        GameObject amountGO = CreateText("Amount", "x0", prefab.transform, 32, true);
        RectTransform amountRect = amountGO.GetComponent<RectTransform>();
        amountRect.anchorMin = new Vector2(0, 0);
        amountRect.anchorMax = new Vector2(1, 0.5f);
        amountRect.offsetMin = new Vector2(10, 5);
        amountRect.offsetMax = new Vector2(-10, 0);

        inventorySelectionPrefab = prefab;
        prefab.SetActive(false);
    }

    private void CreateInventoryItemPrefab()
    {
        GameObject prefab = CreateUIElement("InventoryItemPrefab", null);
        RectTransform prefabRect = prefab.GetComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(150, 180);

        // Add background
        Image bg = prefab.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

        // Add button
        Button button = prefab.AddComponent<Button>();

        // Create name text với Auto Size
        GameObject nameGO = CreateText("Name", "Item Name", prefab.transform, 14, true);
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.6f);
        nameRect.anchorMax = new Vector2(1, 0.8f);
        nameRect.offsetMin = new Vector2(5, 0);
        nameRect.offsetMax = new Vector2(-5, 0);

        // Create amount text với Auto Size
        GameObject amountGO = CreateText("Amount", "x1", prefab.transform, 12, true);
        RectTransform amountRect = amountGO.GetComponent<RectTransform>();
        amountRect.anchorMin = new Vector2(0, 0.4f);
        amountRect.anchorMax = new Vector2(1, 0.6f);
        amountRect.offsetMin = new Vector2(5, 0);
        amountRect.offsetMax = new Vector2(-5, 0);

        // Create type text với Auto Size
        GameObject typeGO = CreateText("Type", "Type", prefab.transform, 10, true);
        RectTransform typeRect = typeGO.GetComponent<RectTransform>();
        typeRect.anchorMin = new Vector2(0, 0.2f);
        typeRect.anchorMax = new Vector2(1, 0.4f);
        typeRect.offsetMin = new Vector2(5, 0);
        typeRect.offsetMax = new Vector2(-5, 0);

        inventoryItemPrefab = prefab;
        prefab.SetActive(false);
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
        return CreateText(name, text, parent, fontSize, false);
    }
    
    GameObject CreateText(string name, string text, Transform parent, int fontSize, bool enableAutoSize)
    {
        GameObject go = CreateUIElement(name, parent);
        TextMeshProUGUI tmpText = go.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.Center;
        
        // Cài đặt Auto Size nếu được yêu cầu
        if (enableAutoSize)
        {
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 8f;  // Kích thước font tối thiểu
            tmpText.fontSizeMax = fontSize; // Kích thước font tối đa (dùng fontSize ban đầu)
        }
        
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

    #endregion

    private void OnDestroy()
    {
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.OnFilteredInventoryItemsChanged -= OnInventoriesLoaded;
            PlayerInventoryManager.Instance.OnInventoryItemsChanged -= OnInventoryItemsLoaded;
        }
    }
} 