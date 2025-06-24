using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerItemManager : SaiSingleton<PlayerItemManager>
{
    [Header("Settings")]
    [SerializeField] protected bool showDebugLog = true;
    [Tooltip("If enabled, Refresh will be automatically called when game authentication is successful")]
    [SerializeField] protected bool autoLoad = true;

    [Header("Player Items (Read Only)")]
    [SerializeField]
    private List<InventoryItem> playerItems = new List<InventoryItem>();
    public List<InventoryItem> PlayerItems => playerItems;

    // Biến lưu id item được chọn từ UI
    public string chooseItem;

    public event Action<List<InventoryItem>> OnPlayerItemsChanged;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        
        // Kiểm tra xem APIManager đã có token hợp lệ chưa
        if (autoLoad && APIManager.Instance != null && APIManager.Instance.HasValidToken())
        {
            GetPlayerItems();
        }
        
        // Đăng ký listener cho event authentication success
        if (APIManager.Instance != null)
        {
            APIManager.Instance.OnAuthenticationSuccess += OnAuthenticationSuccess;
        }
        
        // Bắt đầu coroutine kiểm tra định kỳ
        if (autoLoad)
        {
            StartCoroutine(PeriodicAutoLoadCheck());
        }
    }

    protected virtual void OnEnable()
    {
        // Đăng ký listener nếu chưa được đăng ký trong Start
        if (APIManager.Instance != null)
        {
            APIManager.Instance.OnAuthenticationSuccess += OnAuthenticationSuccess;
        }
    }

    protected virtual void OnDisable()
    {
        if (APIManager.Instance != null)
        {
            APIManager.Instance.OnAuthenticationSuccess -= OnAuthenticationSuccess;
        }
    }

    private void OnAuthenticationSuccess()
    {
        if (autoLoad)
        {
            GetPlayerItems();
        }
    }

    /// <summary>
    /// Kiểm tra và trigger AutoLoad nếu cần thiết
    /// Có thể gọi method này từ Inspector hoặc từ code khác
    /// </summary>
    [ContextMenu("Check and Trigger AutoLoad")]
    public void CheckAndTriggerAutoLoad()
    {
        if (autoLoad && APIManager.Instance != null && APIManager.Instance.HasValidToken())
        {
            if (showDebugLog) Debug.Log("[PlayerItemManager] Manual AutoLoad check: Found valid token, loading player items");
            GetPlayerItems();
        }
        else
        {
            if (showDebugLog) Debug.Log("[PlayerItemManager] Manual AutoLoad check: No valid token or autoLoad disabled");
        }
    }

    /// <summary>
    /// Test method để kiểm tra trạng thái AutoLoad
    /// </summary>
    [ContextMenu("Test AutoLoad Status")]
    public void TestAutoLoadStatus()
    {
        Debug.Log($"[PlayerItemManager] AutoLoad Status:");
        Debug.Log($"  - autoLoad enabled: {autoLoad}");
        Debug.Log($"  - APIManager exists: {APIManager.Instance != null}");
        
        if (APIManager.Instance != null)
        {
            Debug.Log($"  - Has valid token: {APIManager.Instance.HasValidToken()}");
            Debug.Log($"  - Current token: {(!string.IsNullOrEmpty(APIManager.Instance.GetAuthToken()) ? "Present" : "None")}");
        }
        
        Debug.Log($"  - Player items loaded: {playerItems.Count}");
        Debug.Log($"  - Event listeners: {(APIManager.Instance != null ? "Registered" : "Not registered")}");
    }

    public void GetPlayerItems(Action<List<InventoryItem>> onComplete = null)
    {
        if (APIManager.Instance == null)
        {
            if (showDebugLog) Debug.LogWarning("[PlayerItemManager] APIManager is null, cannot get player items");
            onComplete?.Invoke(new List<InventoryItem>());
            return;
        }

        if (!APIManager.Instance.HasValidToken())
        {
            if (showDebugLog) Debug.LogWarning("[PlayerItemManager] No valid token, cannot get player items");
            onComplete?.Invoke(new List<InventoryItem>());
            return;
        }

        string endpoint = $"/games/{APIManager.Instance.GameId}/items";
        StartCoroutine(GetPlayerItemsCoroutine(endpoint, onComplete));
    }

    private IEnumerator GetPlayerItemsCoroutine(string endpoint, Action<List<InventoryItem>> onComplete)
    {
        PlayerItemListResponse result = null;
        yield return StartCoroutine(APIManager.Instance.GetRequest<PlayerItemListResponse>(endpoint, (response) => {
            result = response;
        }));

        if (result != null && result.data != null)
        {
            playerItems = new List<InventoryItem>(result.data);
            
            OnPlayerItemsChanged?.Invoke(playerItems);
            onComplete?.Invoke(playerItems);
        }
        else
        {
            if (showDebugLog) Debug.LogWarning("[PlayerItemManager] No player items found in response.");
            playerItems = new List<InventoryItem>();
            OnPlayerItemsChanged?.Invoke(playerItems);
            onComplete?.Invoke(new List<InventoryItem>()); // Still callback with empty list
        }
    }

    /// <summary>
    /// Coroutine kiểm tra định kỳ để đảm bảo AutoLoad hoạt động
    /// </summary>
    private IEnumerator PeriodicAutoLoadCheck()
    {
        yield return new WaitForSeconds(1f); // Đợi 1 giây để các manager khác khởi tạo
        
        // Kiểm tra lần đầu
        if (autoLoad && APIManager.Instance != null && APIManager.Instance.HasValidToken() && playerItems.Count == 0)
        {
            if (showDebugLog) Debug.Log("[PlayerItemManager] Periodic check: Found valid token but no items loaded, loading now");
            GetPlayerItems();
        }
        
        // Kiểm tra định kỳ mỗi 5 giây
        while (autoLoad)
        {
            yield return new WaitForSeconds(5f);
            
            if (APIManager.Instance != null && APIManager.Instance.HasValidToken() && playerItems.Count == 0)
            {
                GetPlayerItems();
            }
        }
    }
}

[System.Serializable]
public class PlayerItemListResponse
{
    public string status;
    public string message;
    public string message_code;
    public List<InventoryItem> data;
}