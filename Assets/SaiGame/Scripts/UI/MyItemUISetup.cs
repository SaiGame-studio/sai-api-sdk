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
    public string shopSceneName = "3_Shop";

    [Header("Dummy Data (Editor Only)")]
    public bool showDummyData = true;
    public int dummyItemCount = 8;

    // Public methods for Inspector buttons
    public void ShowDummyDataButton()
    {
        if (Application.isEditor)
        {
            LoadDummyData();
        }
        else
        {
            Debug.LogWarning("[MyItemUISetup] Dummy data only available in Editor mode");
        }
    }

    public void DeleteDummyDataButton()
    {
        ClearItems();
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
            ClearItems();
            ShowStatus($"Dummy data {(showDummyData ? "enabled" : "disabled")}");
        }
    }

    // Private variables for tracking UI elements
    private List<GameObject> itemObjects = new List<GameObject>();
    private ItemType? currentFilterType = null;

    void Start()
    {
        // Clear dummy data if we're in play mode
        if (Application.isPlaying)
        {
            ClearItems();
            showDummyData = false; // Disable dummy data in play mode
        }

        if (autoSetup)
        {
            CreateMyItemUI();
        }

        SetupUI();
        
        // Delay một frame để đảm bảo tất cả components đã được khởi tạo
        StartCoroutine(DelayedLoadPlayerItems());
    }

    private IEnumerator DelayedLoadPlayerItems()
    {
        // Đợi một frame
        yield return null;
        
        // Kiểm tra xem có token hợp lệ không
        if (APIManager.Instance != null && APIManager.Instance.HasValidToken())
        {
            LoadPlayerItems();
        }
        else
        {
            Debug.LogWarning("[MyItemUISetup] APIManager not found or no valid token. Loading dummy data...");
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
            if (APIManager.Instance != null)
            {
                APIManager.Instance.OnAuthenticationSuccess += OnAuthenticationSuccess;
            }
        }
    }

    private void OnAuthenticationSuccess()
    {
        if (APIManager.Instance != null)
        {
            APIManager.Instance.OnAuthenticationSuccess -= OnAuthenticationSuccess;
        }
        
        // Clear dummy data and load real data
        ClearItems();
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
        titleText.alignment = TextAlignmentOptions.Left;
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.9f);
        titleRect.anchorMax = new Vector2(0.4f, 0.98f);
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

        // Add GridLayoutGroup to container (thay thế VerticalLayoutGroup)
        GridLayoutGroup gridLayout = container.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(400, 150);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.padding = new RectOffset(20, 20, 20, 20);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 4; // Tăng lên 4 cột
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;

        // Set ScrollRect references
        itemScrollRect.viewport = viewportRect;
        itemScrollRect.content = containerRect;
        itemScrollRect.vertical = true;
        itemScrollRect.horizontal = false;

        // Create Main Menu button (changed from "Back to Main Menu")
        backToMainMenuButton = CreateButton("BackButton", "Main Menu", mainPanel.transform);
        TextMeshProUGUI backButtonText = backToMainMenuButton.GetComponentInChildren<TextMeshProUGUI>();
        if (backButtonText != null)
        {
            backButtonText.alignment = TextAlignmentOptions.Center;
            backButtonText.enableWordWrapping = true;
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
        // Position to the right of title with some spacing
        backRect.anchoredPosition = new Vector2(450, -20);
        backRect.sizeDelta = new Vector2(150, 80);

        // Create Shops button (positioned to the right of Main Menu)
        Button shopsButton = CreateButton("ShopsButton", "Shops", mainPanel.transform);
        TextMeshProUGUI shopsButtonText = shopsButton.GetComponentInChildren<TextMeshProUGUI>();
        if (shopsButtonText != null)
        {
            shopsButtonText.alignment = TextAlignmentOptions.Center;
            shopsButtonText.enableWordWrapping = true;
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
        // Position next to the main menu button: main menu button pos x (450) + main menu button width (150) + spacing (10)
        shopsRect.anchoredPosition = new Vector2(610, -20);
        shopsRect.sizeDelta = new Vector2(120, 80);

        // Create Open LootBox button bên phải Shops
        Button openLootBoxButton = CreateButton("OpenLootBoxButton", "Open\nLootBox", mainPanel.transform);
        TextMeshProUGUI lootBoxButtonText = openLootBoxButton.GetComponentInChildren<TextMeshProUGUI>();
        if (lootBoxButtonText != null)
        {
            lootBoxButtonText.alignment = TextAlignmentOptions.Center;
            lootBoxButtonText.textWrappingMode = TextWrappingModes.PreserveWhitespace;
        }
        // Style giống các nút khác (màu xanh blue)
        Image lootBoxImage = openLootBoxButton.GetComponent<Image>();
        if (lootBoxImage != null)
        {
            lootBoxImage.color = new Color(0.2f, 0.6f, 1f, 1f); // Unified blue background
        }
        ColorBlock lootBoxCb = openLootBoxButton.colors;
        lootBoxCb.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        lootBoxCb.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        lootBoxCb.pressedColor = new Color(0.15f, 0.5f, 0.9f, 1f);
        openLootBoxButton.colors = lootBoxCb;
        RectTransform lootBoxRect = openLootBoxButton.GetComponent<RectTransform>();
        lootBoxRect.anchorMin = new Vector2(0, 1);
        lootBoxRect.anchorMax = new Vector2(0, 1);
        lootBoxRect.pivot = new Vector2(0, 1);
        // Position next to the shops button: shops pos x (610) + shops width (120) + spacing (10)
        lootBoxRect.anchoredPosition = new Vector2(740, -20);
        lootBoxRect.sizeDelta = new Vector2(160, 80);
        // Gán script BtnOpenLootBox vào nút này
        openLootBoxButton.gameObject.AddComponent<BtnOpenLootBox>();

        // Create Refresh button and position it at top-right corner
        refreshButton = CreateButton("RefreshButton", "Refresh", mainPanel.transform);
        
        // Center the text in the refresh button
        TextMeshProUGUI refreshButtonText = refreshButton.GetComponentInChildren<TextMeshProUGUI>();
        if (refreshButtonText != null)
        {
            refreshButtonText.alignment = TextAlignmentOptions.Center;
        }

        // Style the refresh button - Unified blue color
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
        // Position at top-right corner
        refreshRect.anchoredPosition = new Vector2(-20, -20); 
        refreshRect.sizeDelta = new Vector2(120, 80);
        
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

        // Load dummy data nếu được bật (chỉ trong editor)
        if (Application.isEditor && showDummyData)
        {
            LoadDummyData();
        }
        
        // Tạo group chứa các button filter ở hàng dưới các button chính
        GameObject filterGroup = CreateUIElement("ItemTypeFilterGroup", mainPanel.transform);
        RectTransform filterGroupRect = filterGroup.GetComponent<RectTransform>();
        // Đặt anchorMin/anchorMax để cạnh trái filterGroup thẳng với titleText (anchorMin.x = 0.05)
        filterGroupRect.anchorMin = new Vector2(0.05f, 0.78f); // Dưới hàng button chính, dịch phải
        filterGroupRect.anchorMax = new Vector2(0.95f, 0.83f);
        filterGroupRect.offsetMin = Vector2.zero;
        filterGroupRect.offsetMax = Vector2.zero;
        filterGroupRect.pivot = new Vector2(0, 1);

        HorizontalLayoutGroup hLayout = filterGroup.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10f;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.padding = new RectOffset(0, 0, 0, 0);

        // Style cho các button filter giống các button chính
        Color filterBtnColor = new Color(0.2f, 0.6f, 1f, 1f);
        Color filterBtnHighlight = new Color(0.3f, 0.7f, 1f, 1f);
        Color filterBtnPressed = new Color(0.15f, 0.5f, 0.9f, 1f);
        Color filterBtnSelected = Color.yellow;

        // Button "Tất cả"
        Button allBtn = CreateButton("TypeBtn_AllType", "Show All", filterGroup.transform);
        SetButtonColors(allBtn, filterBtnColor, filterBtnHighlight, filterBtnPressed);
        // Gắn BtnMyItemFilter cho button "Tất cả"
        var allBtnFilter = allBtn.gameObject.AddComponent<BtnMyItemFilter>();
        allBtnFilter.filterType = null;
        allBtn.onClick.AddListener(() => OnFilterButtonClicked(null));
        // Đặt lại chiều rộng cố định cho button
        var allBtnRect = allBtn.GetComponent<RectTransform>();
        allBtnRect.sizeDelta = new Vector2(140, 60);
        var allBtnText = allBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (allBtnText != null) allBtnText.alignment = TextAlignmentOptions.Center;

        // Các button cho từng ItemType
        foreach (var type in System.Enum.GetValues(typeof(ItemType)))
        {
            var typeVal = (ItemType)type;
            Button btn = CreateButton($"TypeBtn_{typeVal}", typeVal.ToString(), filterGroup.transform);
            SetButtonColors(btn, currentFilterType == typeVal ? filterBtnSelected : filterBtnColor, filterBtnHighlight, filterBtnPressed);
            // Gắn BtnMyItemFilter cho từng button filter
            var btnFilter = btn.gameObject.AddComponent<BtnMyItemFilter>();
            btnFilter.filterType = typeVal;
            btn.onClick.AddListener(() => OnFilterButtonClicked(typeVal.ToString()));
            var btnRect = btn.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(160, 60);
            var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.alignment = TextAlignmentOptions.Center;
        }
        // Dời Grid Item xuống để tránh đè lên filter
        scrollRect.anchorMin = new Vector2(0.05f, 0.10f); // thấp hơn
        scrollRect.anchorMax = new Vector2(0.95f, 0.75f); // thấp hơn
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
    }

    private void CreateItemPrefab()
    {
        if (itemPrefab != null) return;

        itemPrefab = CreateUIElement("ItemPrefab", null);
        RectTransform prefabRect = itemPrefab.GetComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(400, 150); // Tăng kích thước lên 400x150

        // Add background image
        Image bgImage = itemPrefab.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        // Tạo tên item ở góc trái trên
        GameObject nameText = CreateText("NameText", "Item Name", itemPrefab.transform, 32);
        TextMeshProUGUI nameTMP = nameText.GetComponent<TextMeshProUGUI>();
        nameTMP.color = Color.white;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.alignment = TextAlignmentOptions.TopLeft;
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = new Vector2(10, 5);
        nameRect.offsetMax = new Vector2(-10, -5);

        // Tạo số lượng item ở góc trái dưới
        GameObject amountText = CreateText("AmountText", "Amount: 0", itemPrefab.transform, 28);
        TextMeshProUGUI amountTMP = amountText.GetComponent<TextMeshProUGUI>();
        amountTMP.color = Color.cyan;
        amountTMP.alignment = TextAlignmentOptions.BottomLeft;
        RectTransform amountRect = amountText.GetComponent<RectTransform>();
        amountRect.anchorMin = new Vector2(0, 0);
        amountRect.anchorMax = new Vector2(1, 0.5f);
        amountRect.offsetMin = new Vector2(10, 5);
        amountRect.offsetMax = new Vector2(-10, -5);

        // Add button component
        Button button = itemPrefab.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 0.9f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        button.colors = colors;

        // Gắn BtnMyItemPrefab
        itemPrefab.AddComponent<BtnMyItemPrefab>();

        itemPrefab.SetActive(false);
    }

    private void SetupUI()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshClick);

        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClick);

        // Find and setup shops button
        Button shopsButton = GameObject.Find("ShopsButton")?.GetComponent<Button>();
        if (shopsButton != null)
            shopsButton.onClick.AddListener(OnShopsClick);
    }

    private void LoadPlayerItems()
    {
        ShowLoading(true);
        ShowStatus("Loading player items...");

        PlayerItemManager.Instance.GetPlayerItems(OnPlayerItemsLoaded);
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

        // Áp dụng filter nếu có
        var filtered = ItemTypeFilter.FilterByType(items, currentFilterType);
        ShowStatus($"Loaded {filtered.Count} items");
        PopulateItems(filtered);
    }

    private void PopulateItems(List<InventoryItem> items)
    {
        ClearItems();

        if (itemPrefab == null)
        {
            Debug.LogError("[MyItemUISetup] Item prefab is null. Cannot populate items.");
            return;
        }

        foreach (var item in items)
        {
            GameObject itemObj = Instantiate(itemPrefab, itemContainer);
            itemObj.SetActive(true);

            // Gắn dữ liệu vào BtnMyItemPrefab
            var btnMyItemPrefab = itemObj.GetComponent<BtnMyItemPrefab>();
            if (btnMyItemPrefab != null)
            {
                btnMyItemPrefab.SetData(item);
            }

            // Set item name
            TextMeshProUGUI nameText = itemObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = item.name;

            // Set item amount
            TextMeshProUGUI amountText = itemObj.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();
            amountText.text = $"Amount: {item.amount}";

            // Add click handler (giữ lại để tương thích)
            Button itemButton = itemObj.GetComponent<Button>();
            itemButton.onClick.AddListener(() => OnItemSelected(item));

            itemObjects.Add(itemObj);
        }
    }

    private void ClearItems()
    {
        if (itemContainer != null)
        {
            // Iterate backwards to safely remove children
            for (int i = itemContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = itemContainer.GetChild(i);
                if (child != null && child.gameObject != null)
                {
                    // Use DestroyImmediate in the editor when not playing, otherwise use Destroy
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        DestroyImmediate(child.gameObject);
                    }
                    else
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        itemObjects.Clear();
    }

    public void OnItemSelected(InventoryItem item)
    {
        ShowStatus($"Selected: {item.name} (Amount: {item.amount})");
        // Lưu id item vào PlayerItemManager
        if (PlayerItemManager.Instance != null)
        {
            PlayerItemManager.Instance.chooseItem = item.id;
        }
    }

    public void OnRefreshClick()
    {
        ShowLoading(true);
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

    public void OnShopsClick()
    {
        if (!string.IsNullOrEmpty(shopSceneName))
        {
            SceneController.LoadScene(shopSceneName);
        }
        else
        {
            Debug.LogWarning("[MyItemUISetup] Shop scene name not set");
        }
    }

    // Thay vì gọi trực tiếp OpenLootBox, chỉ gọi LootBoxManager.Instance.OpenLootBoxFromUIOrEditor
    public void OnOpenLootBoxClick()
    {
        string lootBoxId = PlayerItemManager.Instance != null ? PlayerItemManager.Instance.chooseItem : null;
        ShowStatus("Đang mở loot box...");
        LootBoxManager.Instance.OpenLootBoxFromUIOrEditor(
            lootBoxId, 1,
            (result) => {
                ShowStatus($"Loot box opened!\n{result}");
            },
            (error) => {
                ShowStatus($"Lỗi mở loot box: {error}");
            }
        );
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        //Debug.Log($"[MyItemUISetup] {message}");
    }

    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }
    }

    // Thêm hàm để set filter từ UI (ví dụ dropdown)
    public void SetItemTypeFilter(ItemType? type)
    {
        currentFilterType = type;
        // Sau khi set filter, reload lại danh sách
        LoadPlayerItems();
    }

    [ContextMenu("Test Refresh")]
    public void TestRefresh()
    {
        OnRefreshClick();
    }

    [ContextMenu("Load Dummy Data")]
    public void TestLoadDummyData()
    {
        LoadDummyData();
    }

    [ContextMenu("Clear Dummy Data")]
    public void TestClearDummyData()
    {
        ClearItems();
        ShowStatus("Dummy data cleared");
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

    private void LoadDummyData()
    {
        // Chỉ load dummy data trong editor mode
        if (!Application.isEditor || !showDummyData) return;

        List<InventoryItem> dummyItems = new List<InventoryItem>();
        
        string[] itemNames = {
            "Sword of Light",
            "Magic Staff",
            "Golden Shield", 
            "Health Potion",
            "Mana Crystal",
            "Steel Armor",
            "Fire Scroll",
            "Diamond Ring",
            "Poison Dagger",
            "Holy Book",
            "Thunder Bow",
            "Ice Wand"
        };

        for (int i = 0; i < dummyItemCount && i < itemNames.Length; i++)
        {
            InventoryItem dummyItem = new InventoryItem
            {
                id = $"dummy_{i}",
                name = itemNames[i],
                amount = Random.Range(1, 100)
            };
            dummyItems.Add(dummyItem);
        }

        ShowStatus($"Loaded {dummyItems.Count} dummy items (Editor Mode)");
        PopulateItems(dummyItems);
    }

    // Đảm bảo filter button click luôn hoạt động đúng
    public void OnFilterButtonClicked(string type)
    {
        string filterTypeStr = null;
        ItemTypeExtensions.TryParseItemType(type, out var parsedType);
        filterTypeStr = ItemTypeExtensions.ToItemTypeString(parsedType);
        for (int i = 0; i < itemContainer.childCount; i++)
        {
            var itemObj = itemContainer.GetChild(i).gameObject;
            BtnMyItemPrefab btnMyItemPrefab = itemObj.GetComponent<BtnMyItemPrefab>();
            string itemTypeStr = btnMyItemPrefab.itemData.type?.ToString() ?? string.Empty;
            bool show = filterTypeStr == null || itemTypeStr.Contains(filterTypeStr);
            itemObj.SetActive(show);
        }
    }

    // Helper để set màu cho button
    private void SetButtonColors(Button btn, Color normal, Color highlight, Color pressed)
    {
        var colors = btn.colors;
        colors.normalColor = normal;
        colors.highlightedColor = highlight;
        colors.pressedColor = pressed;
        btn.colors = colors;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = normal;
    }
}