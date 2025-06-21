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

    public event Action<List<InventoryItem>> OnFilteredInventoryItemsChanged;

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
        if (showDebugLog) Debug.Log($"Selected Inventory Item ID: {itemId}");
    }

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