using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PlayerInventoryManager : SaiSingleton<PlayerInventoryManager>
{
    [Header("Settings")]
    [SerializeField] protected bool showDebugLog = true;

    [Header("Filtered Inventory Items (Type = Inventory)")]
    [SerializeField]
    private List<InventoryItem> filteredInventoryItems = new List<InventoryItem>();
    public List<InventoryItem> FilteredInventoryItems => filteredInventoryItems;

    [Header("Items Inside Selected Inventory")]
    [SerializeField]
    private List<InventoryItem> inventoryItems = new List<InventoryItem>();
    public List<InventoryItem> InventoryItems => inventoryItems;

    [Header("Selected Inventory Info")]
    [SerializeField] private string selectedInventoryId = "";
    [SerializeField] private string selectedInventoryName = "";
    [SerializeField] private bool isLoadingInventoryItems = false;

    public event Action<List<InventoryItem>> OnFilteredInventoryItemsChanged;
    public event Action<List<InventoryItem>> OnInventoryItemsChanged;

    // Editor-only field
    public string selectedItemIdForEditor = null;
    
    // Flag to track if event is registered
    private bool isEventRegistered = false;

    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void Start()
    {
        // Initialize with existing data from PlayerItemManager
        EnsureEventRegistration();
        RefreshInventory();
    }

    protected virtual void OnEnable()
    {
        // Reset flag to ensure re-registration if needed
        isEventRegistered = false;
    }

    protected virtual void OnDisable()
    {
        UnregisterFromEvents();
    }

    /// <summary>
    /// Đảm bảo event được đăng ký đúng cách với PlayerItemManager
    /// </summary>
    private void EnsureEventRegistration()
    {
        if (!isEventRegistered && PlayerItemManager.Instance != null)
        {
            PlayerItemManager.Instance.OnPlayerItemsChanged += OnPlayerItemsUpdated;
            isEventRegistered = true;
            
            // Sync with existing data immediately if available
            if (PlayerItemManager.Instance.PlayerItems.Count > 0)
            {
                if (showDebugLog) Debug.Log("[PlayerInventoryManager] Syncing with existing PlayerItems data");
                UpdateFilteredInventoryItems();
            }
        }
    }

    /// <summary>
    /// Hủy đăng ký event với PlayerItemManager
    /// </summary>
    private void UnregisterFromEvents()
    {
        if (isEventRegistered && PlayerItemManager.Instance != null)
        {
            PlayerItemManager.Instance.OnPlayerItemsChanged -= OnPlayerItemsUpdated;
            isEventRegistered = false;
            
            if (showDebugLog) Debug.Log("[PlayerInventoryManager] Event unregistered from PlayerItemManager");
        }
    }

    private void OnPlayerItemsUpdated(List<InventoryItem> items)
    {
        if (showDebugLog) Debug.Log("[PlayerInventoryManager] PlayerItems updated, refreshing inventory...");
        RefreshInventory();
    }

    public void RefreshInventory(Action<List<InventoryItem>> onComplete = null)
    {
        // Đảm bảo event được đăng ký trước khi refresh
        EnsureEventRegistration();
        
        if (PlayerItemManager.Instance != null)
        {
            // Filter directly from PlayerItemManager's existing data
            UpdateFilteredInventoryItems();
            onComplete?.Invoke(filteredInventoryItems);
        }
        else
        {
            if (showDebugLog) Debug.LogWarning("[PlayerInventoryManager] PlayerItemManager instance not found!");
            onComplete?.Invoke(new List<InventoryItem>());
        }
    }

    private void UpdateFilteredInventoryItems()
    {
        // Đảm bảo event được đăng ký
        EnsureEventRegistration();
        
        if (PlayerItemManager.Instance != null)
        {
            // Filter items with type "Inventory" directly from PlayerItemManager
            filteredInventoryItems = PlayerItemManager.Instance.PlayerItems.Where(item => 
                !string.IsNullOrEmpty(item.type) && 
                item.type.Equals("Inventory", StringComparison.OrdinalIgnoreCase)
            ).ToList();

            if (showDebugLog && filteredInventoryItems.Count > 0)
            {
                Debug.Log($"[PlayerInventoryManager] Filtered {filteredInventoryItems.Count} inventory items from {PlayerItemManager.Instance.PlayerItems.Count} total items");
            }

            OnFilteredInventoryItemsChanged?.Invoke(filteredInventoryItems);
        }
        else
        {
            filteredInventoryItems.Clear();
            OnFilteredInventoryItemsChanged?.Invoke(filteredInventoryItems);
        }
    }

    public void SelectItemById(string itemId)
    {
        selectedItemIdForEditor = itemId;
        
        // Tìm inventory item được chọn
        var selectedInventory = filteredInventoryItems.FirstOrDefault(item => item.id == itemId);
        if (selectedInventory != null)
        {
            selectedInventoryId = selectedInventory.id;
            selectedInventoryName = selectedInventory.name;
            
            if (showDebugLog) Debug.Log($"Selected Inventory: {selectedInventoryName} (ID: {selectedInventoryId})");
            
            // Gọi API để lấy items trong inventory này
            LoadInventoryItems(selectedInventoryId);
        }
        else
        {
            if (showDebugLog) Debug.LogWarning($"Could not find inventory with ID: {itemId}");
        }
    }

    /// <summary>
    /// Gọi API để lấy danh sách items trong inventory được chọn
    /// </summary>
    /// <param name="inventoryId">ID của inventory</param>
    public void LoadInventoryItems(string inventoryId)
    {
        if (string.IsNullOrEmpty(inventoryId))
        {
            Debug.LogError("Cannot load inventory items: Inventory ID is null or empty");
            return;
        }

        if (APIManager.Instance == null)
        {
            Debug.LogError("Cannot load inventory items: APIManager instance not found");
            return;
        }

        isLoadingInventoryItems = true;
        inventoryItems.Clear();
        OnInventoryItemsChanged?.Invoke(inventoryItems);

        if (showDebugLog) Debug.Log($"Loading items for inventory: {inventoryId}");

        APIManager.Instance.GetInventoryItems(inventoryId, OnInventoryItemsLoaded);
    }

    /// <summary>
    /// Callback khi API trả về danh sách items trong inventory
    /// </summary>
    /// <param name="response">Response từ API</param>
    private void OnInventoryItemsLoaded(InventoryItemsResponse response)
    {
        isLoadingInventoryItems = false;

        if (response != null && response.data != null)
        {
            inventoryItems = response.data;
            
            if (showDebugLog) 
            {
                Debug.Log($"Loaded {inventoryItems.Count} items for inventory '{selectedInventoryName}'");
                
                // Log chi tiết từng item để debug
                foreach (var item in inventoryItems)
                {
                    Debug.Log($"- {item.name} (x{item.amount}) - Type: {item.type}");
                }
            }
        }
        else
        {
            inventoryItems.Clear();
            Debug.LogWarning("Failed to load inventory items or received null response");
        }

        OnInventoryItemsChanged?.Invoke(inventoryItems);
    }

    /// <summary>
    /// Lấy thông tin inventory được chọn hiện tại
    /// </summary>
    public string GetSelectedInventoryId() => selectedInventoryId;

    /// <summary>
    /// Lấy tên inventory được chọn hiện tại
    /// </summary>
    public string GetSelectedInventoryName() => selectedInventoryName;

    /// <summary>
    /// Kiểm tra xem có đang load items không
    /// </summary>
    public bool IsLoadingInventoryItems() => isLoadingInventoryItems;

    public InventoryItem GetItemById(string itemId)
    {
        return filteredInventoryItems.FirstOrDefault(item => item.id == itemId);
    }

    public List<InventoryItem> GetItemsByType(string type)
    {
        if (PlayerItemManager.Instance != null)
        {
            return PlayerItemManager.Instance.PlayerItems.Where(item => 
                !string.IsNullOrEmpty(item.type) && 
                item.type.Equals(type, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        return new List<InventoryItem>();
    }

    public int GetTotalItemCount()
    {
        return filteredInventoryItems.Sum(item => item.amount);
    }

    public int GetUniqueItemCount()
    {
        return filteredInventoryItems.Count;
    }
} 