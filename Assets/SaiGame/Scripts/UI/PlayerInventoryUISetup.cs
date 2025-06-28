using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
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
    [SerializeField] public Button shopsButton;
    [SerializeField] public Button useItemButton;
    [SerializeField] public TextMeshProUGUI statusText;
    [SerializeField] public GameObject loadingPanel;

    [Header("Scene Management")]
    public string mainMenuSceneName = SceneNames.MAIN_MENU;
    public string shopSceneName = SceneNames.SHOP;
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
        // Tương tự như ShopUISetup
        if (Application.isPlaying)
        {
            
            // Tắt showDummyData khi ở Play mode để tránh load dummy data sau này
            showDummyData = false;
            
            int clearedInventorySelection = 0;
            int clearedInventoryItems = 0;
            
            // Xóa các items trong inventory selection (Left panel)
            if (inventorySelectionContainer != null)
            {
                clearedInventorySelection = inventorySelectionContainer.childCount;
                // Sử dụng DestroyImmediate để đảm bảo xóa ngay lập tức
                List<Transform> childrenToDestroy = new List<Transform>();
                foreach (Transform child in inventorySelectionContainer)
                {
                    childrenToDestroy.Add(child);
                }
                foreach (Transform child in childrenToDestroy)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            if (inventorySelectionItems != null) inventorySelectionItems.Clear();

            // Xóa các Grid Items trong inventory (Right panel)
            if (inventoryItemContainer != null)
            {
                clearedInventoryItems = inventoryItemContainer.childCount;
                // Sử dụng DestroyImmediate để đảm bảo xóa ngay lập tức
                List<Transform> childrenToDestroy = new List<Transform>();
                foreach (Transform child in inventoryItemContainer)
                {
                    childrenToDestroy.Add(child);
                }
                foreach (Transform child in childrenToDestroy)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            if (inventoryItems != null) inventoryItems.Clear();
            
            // Reset selected inventory
            selectedInventory = null;
        }
    }

    void Start()
    {
        // Auto setup UI nếu được bật
        if (autoSetup)
        {
            CreateInventoryUI();
        }

        SetupEventListeners();
        
        // Delay một frame để đảm bảo tất cả components đã được khởi tạo
        StartCoroutine(DelayedLoadInventoryData());
        
        // Kiểm tra xem có data sẵn từ PlayerInventoryManager không
        CheckForExistingInventoryData();
    }

    private void OnValidate()
    {
        // Tạo UI và dummy data khi ở Editor mode và không play game
        // Và chỉ khi showDummyData = true
        if (showDummyData && !Application.isPlaying && autoSetup && Application.isEditor)
        {
            CreateInventoryUI();
            LoadDummyDataSafe(true);
        }
    }

    private void SetupEventListeners()
    {
        // Setup UI button listeners
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshClick);
        
        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClick);
        
        if (shopsButton != null)
            shopsButton.onClick.AddListener(OnShopsClick);
        
        if (useItemButton != null)
            useItemButton.onClick.AddListener(OnUseItemClick);

        // Setup PlayerInventoryManager event listeners
        // Đăng ký event để lắng nghe khi inventory được chọn từ dropdown
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.OnInventorySelectedFromDropdown += OnInventorySelectedFromDropdown;
        }
        else
        {
            Debug.LogWarning("[PlayerInventoryUISetup] PlayerInventoryManager.Instance is null during SetupEventListeners!");
        }
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
            
            // Chỉ load dummy data trong Editor mode và không phải Play mode
            // Và chỉ khi showDummyData = true (đã được set false trong Play mode)
            if (showDummyData && Application.isEditor && !Application.isPlaying)
            {
                LoadDummyData();
                Debug.Log("[PlayerInventoryUISetup] Loading dummy data in Editor mode (not playing)");
            }
            else
            {
                Debug.Log("[PlayerInventoryUISetup] Dummy data disabled - waiting for real authentication in Play mode");
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
                    button.onClick.AddListener(() => OnInventoryButtonClicked(inventory));
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

    /// <summary>
    /// Event handler cho dropdown selection từ PlayerInventoryManager
    /// Method này được gọi khi user chọn inventory từ dropdown hoặc bấm button
    /// </summary>
    /// <param name="inventory">Inventory được chọn từ dropdown</param>
    private void OnInventorySelectedFromDropdown(InventoryItem inventory)
    {
        if (inventory == null)
        {
            ShowStatus("Invalid inventory selected from dropdown");
            return;
        }

        // Cập nhật selectedInventory để đồng bộ với PlayerInventoryManager
        selectedInventory = inventory;
        ShowStatus($"Loading items from '{inventory.name}' (via dropdown)...");
        ShowLoading(true);
        
        // Clear current Grid Items before loading new ones
        ClearInventoryItems();
        
        // Unsubscribe from previous events to prevent multiple subscriptions
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.OnInventoryItemsChanged -= OnInventoryItemsLoaded;
        }
        
        // Subscribe to inventory items loaded event
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.OnInventoryItemsChanged += OnInventoryItemsLoaded;
            
            // Kiểm tra xem data đã có sẵn chưa (tránh race condition)
            if (PlayerInventoryManager.Instance.InventoryItems.Count > 0 && 
                PlayerInventoryManager.Instance.GetSelectedInventoryId() == inventory.id)
            {
                // Data đã có sẵn, gọi callback ngay lập tức
                OnInventoryItemsLoaded(PlayerInventoryManager.Instance.InventoryItems);
            }
        }
        else
        {
            ShowLoading(false);
            ShowStatus("PlayerInventoryManager not found!");
        }
    }

    /// <summary>
    /// Handler cho việc bấm vào Button của Inventory trong Grid
    /// Thay vì load items trực tiếp, nó sẽ thay đổi selection trong PlayerInventoryManager
    /// </summary>
    /// <param name="inventory">Inventory được chọn</param>
    private void OnInventoryButtonClicked(InventoryItem inventory)
    {
        if (inventory == null)
        {
            ShowStatus("Invalid inventory selected");
            return;
        }

        // Gọi PlayerInventoryManager để thay đổi selection
        // Điều này sẽ trigger dropdown selection và tự động load items
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.SelectInventory(inventory);
        }
        else
        {
            ShowStatus("PlayerInventoryManager not found!");
            Debug.LogError("[PlayerInventoryUISetup] PlayerInventoryManager.Instance is null!");
        }
    }

    private void OnInventoryItemsLoaded(List<InventoryItem> items)
    {
        ShowLoading(false); // Hide loading panel
        
        // KHÔNG unsubscribe ngay lập tức để tránh mất event
        // Chỉ unsubscribe khi có inventory mới được chọn trong OnInventorySelectedFromDropdown
        
        // Get inventory name once to avoid scope conflicts
        string currentInventoryName = selectedInventory?.name ?? "Selected Inventory";
        
        // Display ALL items from the selected inventory
        if (items != null && items.Count > 0)
        {
            // Populate Grid with ALL items
            PopulateAllInventoryItems(items);
            
            // Log chi tiết từng item để debug
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
            }
        }
        else
        {
            ClearInventoryItems();
        }
    }

    /// <summary>
    /// Hiển thị TẤT CẢ items từ inventory đã chọn lên Grid Items
    /// Method này được gọi khi user bấm vào Button Inventory
    /// </summary>
    private void PopulateAllInventoryItems(List<InventoryItem> items)
    {
        // Clear existing items trước khi populate mới
        ClearInventoryItems();

        // Get inventory name to avoid scope conflicts
        string selectedInventoryName = selectedInventory?.name ?? "Selected Inventory";

        if (items == null || items.Count == 0)
        {
            ShowStatus($"No items found in '{selectedInventoryName}'");
            Debug.Log($"[PlayerInventoryUISetup] PopulateAllInventoryItems: No items to display for '{selectedInventoryName}'");
            return;
        }

        if (inventoryItemPrefab == null)
        {
            ShowStatus("Error: Inventory item prefab not found!");
            Debug.LogError("[PlayerInventoryUISetup] inventoryItemPrefab is null!");
            return;
        }

        if (inventoryItemContainer == null)
        {
            ShowStatus("Error: Inventory item container not found!");
            Debug.LogError("[PlayerInventoryUISetup] inventoryItemContainer is null!");
            return;
        }

        // Hiển thị TẤT CẢ items lên Grid
        int itemCount = 0;
        foreach (var item in items)
        {
            try
            {
                GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryItemContainer);
                itemObj.SetActive(true);
                
                // Set data cho Grid Item
                SetInventoryItemData(itemObj, item);
                
                // Add click event cho Grid Item
                Button button = itemObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnGridItemSelected(item));
                }
                
                inventoryItems.Add(itemObj);
                itemCount++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayerInventoryUISetup] Error creating Grid Item for {item.name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Backward compatibility - redirect to PopulateAllInventoryItems
    /// </summary>
    private void PopulateInventoryItems(List<InventoryItem> items)
    {
        PopulateAllInventoryItems(items);
    }

    private void SetInventoryItemData(GameObject itemObject, InventoryItem item)
    {
        // Set item name - tương tự như ShopUISetup
        TextMeshProUGUI nameText = itemObject.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = item.name ?? "Unknown Item";
        }

        // Set item amount - thay vì PriceText như Shop, hiển thị Amount
        TextMeshProUGUI amountText = itemObject.transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();
        if (amountText != null)
        {
            amountText.text = $"x{item.amount}";
        }

        // Set item type - thay vì DateText như Shop, hiển thị Type
        TextMeshProUGUI typeText = itemObject.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
        if (typeText != null)
        {
            string typeDisplay = !string.IsNullOrEmpty(item.type) ? item.type : "N/A";
            typeText.text = $"Type: {typeDisplay}";
        }

        // Fallback cho cấu trúc cũ nếu có
        TextMeshProUGUI[] texts = itemObject.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            switch (text.name)
            {
                case "Name":
                    text.text = item.name ?? "Unknown Item";
                    break;
                case "Amount":
                    text.text = $"x{item.amount}";
                    break;
                case "Type":
                    text.text = !string.IsNullOrEmpty(item.type) ? item.type : "N/A";
                    break;
            }
        }
    }

    /// <summary>
    /// Được gọi khi user bấm vào một Grid Item
    /// </summary>
    private void OnGridItemSelected(InventoryItem item)
    {
        if (item == null)
        {
            ShowStatus("Invalid item selected");
            return;
        }

        // Enable Use Item button
        if (useItemButton != null)
            useItemButton.interactable = true;
    }

    /// <summary>
    /// Backward compatibility
    /// </summary>
    private void OnItemSelected(InventoryItem item)
    {
        OnGridItemSelected(item);
    }

    /// <summary>
    /// Refresh toàn bộ UI - clear selections và reload data
    /// </summary>
    public void OnRefreshClick()
    {
        ShowStatus("Refreshing inventory data...");
        ShowLoading(true);
        
        // Clear current selections
        selectedInventory = null;
        ClearInventoryItems();
        
        // Disable Use Item button
        if (useItemButton != null)
            useItemButton.interactable = false;
        
        // Reload inventory data
        LoadInventoryData();
        
        Debug.Log("[PlayerInventoryUISetup] Manual refresh triggered - clearing Grid Items and reloading inventories");
    }

    public void OnBackToMainMenuClick()
    {
        SceneController.LoadScene(mainMenuSceneName);
    }

    public void OnShopsClick()
    {
        SceneController.LoadScene(shopSceneName);
    }

    public void OnUseItemClick()
    {
        SceneController.LoadScene(myItemSceneName);
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
        shopsButton = null;
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
        // Bảo vệ: không load dummy data trong Play mode
        if (Application.isPlaying)
        {
            Debug.LogWarning("[PlayerInventoryUISetup] Cannot load dummy data in Play mode");
            return;
        }
        
        LoadDummyDataSafe(false);
    }

    [ContextMenu("Force Refresh Everything")]
    public void ForceRefreshEverything()
    {
        Debug.Log("[PlayerInventoryUISetup] Force refreshing everything...");
        
        // Clear all data
        ClearAllData();
        
        // Ensure UI exists
        if (inventoryItemPrefab == null || inventoryItemContainer == null)
        {
            CreateInventoryUI();
        }
        
        // Load dummy data with Grid Items
        LoadDummyData();
        
        Debug.Log("[PlayerInventoryUISetup] Force refresh completed");
    }

    [ContextMenu("Test Dropdown Event Flow")]
    public void TestDropdownEventFlow()
    {
        Debug.Log("[PlayerInventoryUISetup] Testing dropdown event flow...");
        
        if (PlayerInventoryManager.Instance == null)
        {
            Debug.LogError("[PlayerInventoryUISetup] PlayerInventoryManager.Instance is null!");
            return;
        }

        // Ensure UI exists
        if (inventoryItemPrefab == null || inventoryItemContainer == null)
        {
            CreateInventoryUI();
        }

        // Create a test inventory
        InventoryItem testInventory = new InventoryItem
        {
            id = "test_inventory_dropdown",
            name = "Test Inventory (Dropdown)",
            type = "Inventory",
            amount = 1
        };

        // Simulate dropdown selection via PlayerInventoryManager
        // Điều này sẽ fire event mà UI đã đăng ký
        Debug.Log("[PlayerInventoryUISetup] Simulating dropdown selection via PlayerInventoryManager...");
        PlayerInventoryManager.Instance.SelectInventory(testInventory);
        
        Debug.Log("[PlayerInventoryUISetup] Dropdown event test completed - check Grid Items should update automatically!");
    }

    [ContextMenu("Test Inventory Button Click Flow")]
    public void TestInventoryButtonClickFlow()
    {
        Debug.Log("[PlayerInventoryUISetup] Testing inventory button click flow...");
        
        if (PlayerInventoryManager.Instance == null)
        {
            Debug.LogError("[PlayerInventoryUISetup] PlayerInventoryManager.Instance is null!");
            return;
        }

        // Ensure UI exists
        if (inventoryItemPrefab == null || inventoryItemContainer == null)
        {
            CreateInventoryUI();
        }

        // Create a test inventory
        InventoryItem testInventory = new InventoryItem
        {
            id = "test_inventory_button_click",
            name = "Test Inventory (Button Click)",
            type = "Inventory",
            amount = 1
        };

        // Simulate button click - this should trigger the new logic
        Debug.Log("[PlayerInventoryUISetup] Simulating inventory button click...");
        OnInventoryButtonClicked(testInventory);
        
        Debug.Log("[PlayerInventoryUISetup] Button click test completed - check if dropdown selection was triggered!");
    }

    [ContextMenu("Test Clear Dummy Data (Simulate Play Mode)")]
    public void TestClearDummyData()
    {
        Debug.Log("[PlayerInventoryUISetup] Testing dummy data clearing (simulating Play mode)...");
        
        int clearedInventorySelection = 0;
        int clearedInventoryItems = 0;
        
        // Xóa các items trong inventory selection (Left panel)
        if (inventorySelectionContainer != null)
        {
            clearedInventorySelection = inventorySelectionContainer.childCount;
            List<Transform> childrenToDestroy = new List<Transform>();
            foreach (Transform child in inventorySelectionContainer)
            {
                childrenToDestroy.Add(child);
            }
            foreach (Transform child in childrenToDestroy)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        if (inventorySelectionItems != null) inventorySelectionItems.Clear();

        // Xóa các Grid Items trong inventory (Right panel)
        if (inventoryItemContainer != null)
        {
            clearedInventoryItems = inventoryItemContainer.childCount;
            List<Transform> childrenToDestroy = new List<Transform>();
            foreach (Transform child in inventoryItemContainer)
            {
                childrenToDestroy.Add(child);
            }
            foreach (Transform child in childrenToDestroy)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        if (inventoryItems != null) inventoryItems.Clear();
        
        Debug.Log($"[PlayerInventoryUISetup] Test clear completed: {clearedInventorySelection} inventory selection items, {clearedInventoryItems} Grid Items cleared");
        ShowStatus("Dummy data cleared (test)");
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
        // Bảo vệ: không load dummy data trong Play mode
        if (Application.isPlaying)
        {
            Debug.LogWarning("[PlayerInventoryUISetup] Attempted to load dummy data in Play mode - ignored");
            return;
        }
        
        // Create dummy inventories
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
        
        // Also create dummy Grid Items to test display
        if (dummyInventories.Count > 0)
        {
            // Select first inventory and create dummy items
            selectedInventory = dummyInventories[0];
            
            List<InventoryItem> dummyGridItems = new List<InventoryItem>();
            for (int i = 1; i <= dummyItemCount; i++)
            {
                InventoryItem item = new InventoryItem
                {
                    id = $"item_{i}",
                    name = $"Grid Item {i}",
                    type = $"Type {(i % 3) + 1}",
                    amount = Random.Range(1, 10)
                };
                dummyGridItems.Add(item);
            }
            
            // Populate Grid Items to show immediately
            PopulateAllInventoryItems(dummyGridItems);
            Debug.Log($"[PlayerInventoryUISetup] Created {dummyGridItems.Count} dummy Grid Items");
        }
        
        ShowStatus($"Dummy data loaded: {dummyInventories.Count} inventories, {dummyItemCount} Grid Items");
        Debug.Log("[PlayerInventoryUISetup] Dummy data loading completed");
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
            useItemButton.interactable = true;
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
        if (Application.isEditor)
        {
            showDummyData = !showDummyData;
            Debug.Log($"[PlayerInventoryUISetup] Dummy data toggled: {showDummyData}");
            
            if (showDummyData)
            {
                LoadDummyData();
            }
            else
            {
                ClearAllData();
            }
        }
        else
        {
            Debug.LogWarning("[PlayerInventoryUISetup] Dummy data toggle only available in Editor mode");
        }
    }

    [ContextMenu("Reset Dummy Data Settings")]
    public void ResetDummyDataSettings()
    {
        if (Application.isEditor)
        {
            showDummyData = true;
            Debug.Log("[PlayerInventoryUISetup] Dummy data settings reset to default");
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

    [ContextMenu("Test Grid Items Display")]
    public void TestGridItemsDisplay()
    {
        if (selectedInventory == null)
        {
            Debug.LogWarning("[PlayerInventoryUISetup] No inventory selected for testing. Please select an inventory first.");
            ShowStatus("No inventory selected for testing");
            return;
        }

        Debug.Log($"[PlayerInventoryUISetup] Testing Grid Items display for inventory: {selectedInventory.name}");
        ShowStatus($"Testing display for '{selectedInventory.name}'...");
        ShowLoading(true);
        
        // Force reload items for current selected inventory using new logic
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.Instance.SelectInventory(selectedInventory);
        }
        else
        {
            ShowLoading(false);
            ShowStatus("PlayerInventoryManager not found!");
        }
    }

    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log("=== PlayerInventoryUISetup Debug State ===");
        
        // Debug UI state
        Debug.Log($"UI State: selectedInventory={(selectedInventory?.name ?? "null")}, " +
                 $"inventoryItems.Count={inventoryItems.Count}, " +
                 $"inventorySelectionItems.Count={inventorySelectionItems.Count}");
        
        // Debug PlayerInventoryManager state
        if (PlayerInventoryManager.Instance != null)
        {
            Debug.Log($"PlayerInventoryManager State: " +
                     $"SelectedInventoryId='{PlayerInventoryManager.Instance.GetSelectedInventoryId()}', " +
                     $"SelectedInventoryName='{PlayerInventoryManager.Instance.GetSelectedInventoryName()}', " +
                     $"InventoryItemsCount={PlayerInventoryManager.Instance.InventoryItems.Count}, " +
                     $"FilteredInventoryItemsCount={PlayerInventoryManager.Instance.FilteredInventoryItems.Count}, " +
                     $"IsLoading={PlayerInventoryManager.Instance.IsLoadingInventoryItems()}");
            
            // Debug inventory items detail
            if (PlayerInventoryManager.Instance.InventoryItems.Count > 0)
            {
                Debug.Log("Current Inventory Items:");
                for (int i = 0; i < PlayerInventoryManager.Instance.InventoryItems.Count; i++)
                {
                    var item = PlayerInventoryManager.Instance.InventoryItems[i];
                    Debug.Log($"  {i+1}. {item.name} (Amount: {item.amount}, Type: {item.type})");
                }
            }
        }
        else
        {
            Debug.LogWarning("PlayerInventoryManager.Instance is null!");
        }
        
        // Debug APIManager state
        if (APIManager.Instance != null)
        {
            Debug.Log($"APIManager State: HasValidToken={APIManager.Instance.HasValidToken()}");
        }
        else
        {
            Debug.LogWarning("APIManager.Instance is null!");
        }
        
        Debug.Log("=== End Debug State ===");
    }

    [ContextMenu("Force Refresh Inventory Data")]
    public void ForceRefreshInventoryData()
    {
        Debug.Log("[PlayerInventoryUISetup] Force refreshing inventory data...");
        
        if (PlayerInventoryManager.Instance != null)
        {
            // Force refresh từ PlayerItemManager
            if (PlayerItemManager.Instance != null)
            {
                PlayerItemManager.Instance.GetPlayerItems((items) => {
                    Debug.Log($"[PlayerInventoryUISetup] PlayerItems refreshed: {items.Count} items");
                    
                    // Force refresh inventory
                    PlayerInventoryManager.Instance.RefreshInventory((inventories) => {
                        Debug.Log($"[PlayerInventoryUISetup] Inventories refreshed: {inventories.Count} inventories");
                        
                        // Nếu có inventory được chọn, force load items
                        if (!string.IsNullOrEmpty(PlayerInventoryManager.Instance.GetSelectedInventoryId()))
                        {
                            Debug.Log($"[PlayerInventoryUISetup] Force loading items for selected inventory: {PlayerInventoryManager.Instance.GetSelectedInventoryName()}");
                            PlayerInventoryManager.Instance.LoadInventoryItems(PlayerInventoryManager.Instance.GetSelectedInventoryId());
                        }
                    });
                });
            }
        }
    }

    [ContextMenu("Test Event Flow")]
    public void TestEventFlow()
    {
        Debug.Log("[PlayerInventoryUISetup] Testing event flow...");
        
        if (PlayerInventoryManager.Instance != null && PlayerInventoryManager.Instance.FilteredInventoryItems.Count > 0)
        {
            // Test với inventory đầu tiên
            var testInventory = PlayerInventoryManager.Instance.FilteredInventoryItems[0];
            Debug.Log($"[PlayerInventoryUISetup] Testing with inventory: {testInventory.name} (ID: {testInventory.id})");
            
            // Simulate inventory selection
            OnInventorySelectedFromDropdown(testInventory);
        }
        else
        {
            Debug.LogWarning("[PlayerInventoryUISetup] No inventories available for testing");
        }
    }
    
    [ContextMenu("Test Manual Inventory Selection")]
    public void TestManualInventorySelection()
    {
        Debug.Log("[PlayerInventoryUISetup] Testing manual inventory selection...");
        
        if (PlayerInventoryManager.Instance != null && PlayerInventoryManager.Instance.FilteredInventoryItems.Count > 0)
        {
            // Test với inventory đầu tiên
            var testInventory = PlayerInventoryManager.Instance.FilteredInventoryItems[0];
            Debug.Log($"[PlayerInventoryUISetup] Manually selecting inventory: {testInventory.name}");
            
            // Gọi PlayerInventoryManager trực tiếp
            PlayerInventoryManager.Instance.SelectInventory(testInventory);
        }
        else
        {
            Debug.LogWarning("[PlayerInventoryUISetup] No inventories available for manual selection test");
        }
    }

    [ContextMenu("Check and Fix Event Registration")]
    public void CheckAndFixEventRegistration()
    {
        Debug.Log("[PlayerInventoryUISetup] Checking and fixing event registration...");
        
        bool needsFix = false;
        
        // Kiểm tra PlayerInventoryManager
        if (PlayerInventoryManager.Instance == null)
        {
            Debug.LogError("[PlayerInventoryUISetup] PlayerInventoryManager.Instance is null!");
            return;
        }
        
        // Kiểm tra xem có đang load không
        if (PlayerInventoryManager.Instance.IsLoadingInventoryItems())
        {
            Debug.Log("[PlayerInventoryUISetup] PlayerInventoryManager is currently loading items, waiting...");
            StartCoroutine(WaitForLoadingComplete());
            return;
        }
        
        // Kiểm tra xem có inventory được chọn và có items không
        if (!string.IsNullOrEmpty(PlayerInventoryManager.Instance.GetSelectedInventoryId()))
        {
            if (PlayerInventoryManager.Instance.InventoryItems.Count == 0)
            {
                Debug.Log("[PlayerInventoryUISetup] Selected inventory has no items, this might be the issue!");
                needsFix = true;
            }
            else
            {
                Debug.Log($"[PlayerInventoryUISetup] Selected inventory has {PlayerInventoryManager.Instance.InventoryItems.Count} items");
                
                // Kiểm tra xem UI đã hiển thị items chưa
                if (inventoryItems.Count == 0)
                {
                    Debug.Log("[PlayerInventoryUISetup] UI has no displayed items but PlayerInventoryManager has data - fixing...");
                    needsFix = true;
                }
            }
        }
        else
        {
            Debug.Log("[PlayerInventoryUISetup] No inventory is currently selected");
        }
        
        // Apply fix if needed
        if (needsFix)
        {
            Debug.Log("[PlayerInventoryUISetup] Applying fix...");
            
            // Re-register events
            SetupEventListeners();
            
            // Check for existing data
            CheckForExistingInventoryData();
            
            // Force refresh if needed
            if (PlayerInventoryManager.Instance.FilteredInventoryItems.Count == 0)
            {
                Debug.Log("[PlayerInventoryUISetup] No filtered inventories, forcing refresh...");
                ForceRefreshInventoryData();
            }
        }
        else
        {
            Debug.Log("[PlayerInventoryUISetup] Event registration looks good!");
        }
    }
    
    private IEnumerator WaitForLoadingComplete()
    {
        Debug.Log("[PlayerInventoryUISetup] Waiting for loading to complete...");
        
        while (PlayerInventoryManager.Instance != null && PlayerInventoryManager.Instance.IsLoadingInventoryItems())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("[PlayerInventoryUISetup] Loading completed, checking event registration...");
        CheckAndFixEventRegistration();
    }

    #region UI Creation Helper Methods

    private void CreateInventoryButtons(GameObject mainPanel)
    {
        // Create Back to Main Menu button
        GameObject backButtonGO = CreateButton("BackButton", "Main Menu", mainPanel.transform);
        backToMainMenuButton = backButtonGO.GetComponent<Button>();
        
        // Style the back to main menu button - Unified blue color
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
            backImage.color = new Color(0.2f, 0.6f, 1f, 1f); // Unified blue background
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
        backRect.anchoredPosition = new Vector2(320, -20);
        backRect.sizeDelta = new Vector2(200, 80);

        // Create Shops button
        GameObject shopsButtonGO = CreateButton("ShopsButton", "Shops", mainPanel.transform);
        shopsButton = shopsButtonGO.GetComponent<Button>();
        
        // Style the shops button - Unified blue color
        TextMeshProUGUI shopsButtonText = shopsButton.GetComponentInChildren<TextMeshProUGUI>();
        if (shopsButtonText != null)
        {
            shopsButtonText.alignment = TextAlignmentOptions.Center;
            shopsButtonText.fontSize = 24;
        }
        Image shopsImage = shopsButton.GetComponent<Image>();
        if (shopsImage != null)
        {
            shopsImage.color = new Color(0.2f, 0.6f, 1f, 1f); // Unified blue background
        }
        ColorBlock shopsCb = shopsButton.colors;
        shopsCb.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        shopsCb.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        shopsCb.pressedColor = new Color(0.15f, 0.5f, 0.9f, 1f);
        shopsButton.colors = shopsCb;
        
        RectTransform shopsRect = shopsButton.GetComponent<RectTransform>();
        shopsRect.anchorMin = new Vector2(0, 1);
        shopsRect.anchorMax = new Vector2(0, 1);
        shopsRect.pivot = new Vector2(0, 1);
        shopsRect.anchoredPosition = new Vector2(530, -20);
        shopsRect.sizeDelta = new Vector2(150, 80);

        // Create Use Item button - moved to the right to avoid overlap
        GameObject useButtonGO = CreateButton("UseItemButton", "My Item", mainPanel.transform);
        useItemButton = useButtonGO.GetComponent<Button>();
        useItemButton.interactable = true;
        
        // Style the use item button - Unified blue color
        TextMeshProUGUI useButtonText = useItemButton.GetComponentInChildren<TextMeshProUGUI>();
        if (useButtonText != null)
        {
            useButtonText.alignment = TextAlignmentOptions.Center;
            useButtonText.fontSize = 24;
        }
        Image useImage = useItemButton.GetComponent<Image>();
        if (useImage != null)
        {
            useImage.color = new Color(0.2f, 0.6f, 1f, 1f); // Unified blue background
        }
        ColorBlock useCb = useItemButton.colors;
        useCb.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        useCb.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        useCb.pressedColor = new Color(0.15f, 0.5f, 0.9f, 1f);
        useItemButton.colors = useCb;
        
        RectTransform useRect = useItemButton.GetComponent<RectTransform>();
        useRect.anchorMin = new Vector2(0, 1);
        useRect.anchorMax = new Vector2(0, 1);
        useRect.pivot = new Vector2(0, 1);
        useRect.anchoredPosition = new Vector2(690, -20);
        useRect.sizeDelta = new Vector2(150, 80);

        // Create Refresh button
        GameObject refreshButtonGO = CreateButton("RefreshButton", "Refresh", mainPanel.transform);
        refreshButton = refreshButtonGO.GetComponent<Button>();
        
        // Style the refresh button - Unified blue color
        TextMeshProUGUI refreshButtonText = refreshButton.GetComponentInChildren<TextMeshProUGUI>();
        if (refreshButtonText != null)
        {
            refreshButtonText.alignment = TextAlignmentOptions.Center;
            refreshButtonText.fontSize = 24;
        }
        Image refreshImage = refreshButton.GetComponent<Image>();
        if (refreshImage != null)
        {
            refreshImage.color = new Color(0.2f, 0.6f, 1f, 1f); // Unified blue background
        }
        ColorBlock cb = refreshButton.colors;
        cb.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        cb.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f); // Lighter blue on highlight
        cb.pressedColor = new Color(0.15f, 0.5f, 0.9f, 1f); // Darker blue on press
        refreshButton.colors = cb;
        
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

        // Add Grid Layout Group - Updated for new 400x200 item size
        GridLayoutGroup layoutGroup = contentGO.AddComponent<GridLayoutGroup>();
        layoutGroup.cellSize = new Vector2(400, 200); // Match new item size
        layoutGroup.spacing = new Vector2(15, 15);
        layoutGroup.padding = new RectOffset(15, 15, 15, 15);
        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = 3; // Changed from 2 to 3 for inventory items

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
        // Create inventory item prefab as a 400x200 button - increased height for better text fit
        GameObject itemPrefab = CreateUIElement("InventoryItemPrefab", null);
        RectTransform itemRect = itemPrefab.GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(400, 200);

        // Add background image
        Image itemBg = itemPrefab.AddComponent<Image>();
        itemBg.color = new Color(0.2f, 0.4f, 0.3f, 0.8f); // Green tint to distinguish from shop

        // Add Button component
        Button itemButton = itemPrefab.AddComponent<Button>();
        ColorBlock buttonColors = itemButton.colors;
        buttonColors.normalColor = new Color(0.2f, 0.4f, 0.3f, 0.8f);
        buttonColors.highlightedColor = new Color(0.3f, 0.5f, 0.4f, 0.9f);
        buttonColors.pressedColor = new Color(0.1f, 0.3f, 0.2f, 0.8f);
        itemButton.colors = buttonColors;

        // Add Vertical Layout Group for content
        VerticalLayoutGroup itemLayout = itemPrefab.AddComponent<VerticalLayoutGroup>();
        itemLayout.spacing = 5f;
        itemLayout.padding = new RectOffset(10, 10, 10, 10);
        itemLayout.childControlWidth = true;
        itemLayout.childControlHeight = false;
        itemLayout.childForceExpandWidth = false;
        itemLayout.childForceExpandHeight = false;

        // Create Item Name - tương tự ShopUISetup
        GameObject nameGO = CreateText("ItemName", "Item Name", itemPrefab.transform, 32);
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(380, 60);
        TextMeshProUGUI nameText = nameGO.GetComponent<TextMeshProUGUI>();
        nameText.alignment = TextAlignmentOptions.Center;

        // Create Amount Text - thay vì PriceText như Shop
        GameObject amountGO = CreateText("AmountText", "x1", itemPrefab.transform, 36);
        RectTransform amountTextRect = amountGO.GetComponent<RectTransform>();
        amountTextRect.sizeDelta = new Vector2(380, 50);
        TextMeshProUGUI amountText = amountGO.GetComponent<TextMeshProUGUI>();
        amountText.color = new Color(0.2f, 1f, 0.8f, 1f); // Cyan color for amount
        amountText.alignment = TextAlignmentOptions.Center;

        // Create Type Text - thay vì DateText như Shop
        GameObject typeGO = CreateText("TypeText", "Type: N/A", itemPrefab.transform, 24);
        RectTransform typeTextRect = typeGO.GetComponent<RectTransform>();
        typeTextRect.sizeDelta = new Vector2(380, 40);
        TextMeshProUGUI typeText = typeGO.GetComponent<TextMeshProUGUI>();
        typeText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        typeText.alignment = TextAlignmentOptions.Center;

        // Store as prefab
        inventoryItemPrefab = itemPrefab;
        inventoryItemPrefab.SetActive(false); // Hide prefab
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
            // Unsubscribe từ tất cả events để tránh memory leaks
            PlayerInventoryManager.Instance.OnFilteredInventoryItemsChanged -= OnInventoriesLoaded;
            PlayerInventoryManager.Instance.OnInventoryItemsChanged -= OnInventoryItemsLoaded;
            PlayerInventoryManager.Instance.OnInventorySelectedFromDropdown -= OnInventorySelectedFromDropdown;
        }
    }

    /// <summary>
    /// Kiểm tra xem PlayerInventoryManager đã có data sẵn chưa
    /// Nếu có thì hiển thị ngay lập tức
    /// </summary>
    private void CheckForExistingInventoryData()
    {
        if (PlayerInventoryManager.Instance != null)
        {
            // Kiểm tra xem có inventory được chọn và có items không
            if (!string.IsNullOrEmpty(PlayerInventoryManager.Instance.GetSelectedInventoryId()) &&
                PlayerInventoryManager.Instance.InventoryItems.Count > 0)
            {
                // Có data sẵn, hiển thị ngay
                selectedInventory = PlayerInventoryManager.Instance.FilteredInventoryItems
                    .FirstOrDefault(item => item.id == PlayerInventoryManager.Instance.GetSelectedInventoryId());
                
                if (selectedInventory != null)
                {
                    Debug.Log($"[PlayerInventoryUISetup] Found existing inventory data for '{selectedInventory.name}', displaying immediately");
                    OnInventoryItemsLoaded(PlayerInventoryManager.Instance.InventoryItems);
                }
            }
        }
    }
} 