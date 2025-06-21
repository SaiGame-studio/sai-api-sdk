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

    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void Start()
    {
        // Initialize with existing data from PlayerItemManager
        if (PlayerItemManager.Instance != null)
        {
            RefreshInventory();
        }
    }

    protected virtual void OnEnable()
    {
        if (PlayerItemManager.Instance != null)
        {
            PlayerItemManager.Instance.OnPlayerItemsChanged += OnPlayerItemsUpdated;
            // Sync with existing data immediately when enabled
            if (PlayerItemManager.Instance.PlayerItems.Count > 0)
            {
                UpdateFilteredInventoryItems();
            }
        }
    }

    protected virtual void OnDisable()
    {
        if (PlayerItemManager.Instance != null)
        {
            PlayerItemManager.Instance.OnPlayerItemsChanged -= OnPlayerItemsUpdated;
        }
    }

    private void OnPlayerItemsUpdated(List<InventoryItem> items)
    {
        UpdateFilteredInventoryItems();
    }

    public void RefreshInventory(Action<List<InventoryItem>> onComplete = null)
    {
        if (PlayerItemManager.Instance != null)
        {
            // Filter directly from PlayerItemManager's existing data
            UpdateFilteredInventoryItems();
            onComplete?.Invoke(filteredInventoryItems);
        }
        else
        {
            if (showDebugLog) Debug.LogWarning("PlayerItemManager instance not found!");
            onComplete?.Invoke(new List<InventoryItem>());
        }
    }

    private void UpdateFilteredInventoryItems()
    {
        if (PlayerItemManager.Instance != null)
        {
            // Filter items with type "Inventory" directly from PlayerItemManager
            filteredInventoryItems = PlayerItemManager.Instance.PlayerItems.Where(item => 
                !string.IsNullOrEmpty(item.type) && 
                item.type.Equals("Inventory", StringComparison.OrdinalIgnoreCase)
            ).ToList();

            if (showDebugLog) 
            {
                Debug.Log($"Total items in PlayerItemManager: {PlayerItemManager.Instance.PlayerItems.Count}, Filtered Inventory items: {filteredInventoryItems.Count}");
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