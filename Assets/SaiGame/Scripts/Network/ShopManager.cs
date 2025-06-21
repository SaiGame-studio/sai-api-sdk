using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShopManager : SaiSingleton<ShopManager>
{
    [Header("Settings")]
    [SerializeField] protected bool showDebugLog = true;
    [Tooltip("If enabled, Refresh will be automatically called when game authentication is successful")]
    [SerializeField] protected bool autoLoad = true;

    [Header("Shop List (Read Only)")]
    [SerializeField]
    private List<ShopData> shopList = new List<ShopData>();
    public List<ShopData> ShopList => shopList;

    [Header("Selected Shop Item Profiles (Read Only)")]
    [SerializeField]
    private List<ItemProfileData> currentShopItems = new List<ItemProfileData>();
    public List<ItemProfileData> CurrentShopItems => currentShopItems;

    public event Action<List<ShopData>> OnShopListChanged;
    public event Action<List<ItemProfileData>> OnShopItemsChanged;
    public event Action<BuyItemResponse> OnItemBought;

    // Editor-only field
    public string selectedShopIdForEditor = null;
    
    [Header("Selected Item Profile (Read Only)")]
    [SerializeField]
    public string itemProfileIdForEditor = null;

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
            if (showDebugLog) Debug.Log("[ShopManager] AutoLoad: Found valid token, loading shop list");
            GetShopList();
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
            if (showDebugLog) Debug.Log("[ShopManager] AutoLoad: Authentication success, loading shop list");
            GetShopList();
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
            if (showDebugLog) Debug.Log("[ShopManager] Manual AutoLoad check: Found valid token, loading shop list");
            GetShopList();
        }
        else
        {
            if (showDebugLog) Debug.Log("[ShopManager] Manual AutoLoad check: No valid token or autoLoad disabled");
        }
    }

    /// <summary>
    /// Test method để kiểm tra trạng thái AutoLoad
    /// </summary>
    [ContextMenu("Test AutoLoad Status")]
    public void TestAutoLoadStatus()
    {
        Debug.Log($"[ShopManager] AutoLoad Status:");
        Debug.Log($"  - autoLoad enabled: {autoLoad}");
        Debug.Log($"  - APIManager exists: {APIManager.Instance != null}");
        
        if (APIManager.Instance != null)
        {
            Debug.Log($"  - Has valid token: {APIManager.Instance.HasValidToken()}");
            Debug.Log($"  - Current token: {(!string.IsNullOrEmpty(APIManager.Instance.GetAuthToken()) ? "Present" : "None")}");
        }
        
        Debug.Log($"  - Shop list loaded: {shopList.Count}");
        Debug.Log($"  - Current shop items: {currentShopItems.Count}");
        Debug.Log($"  - Event listeners: {(APIManager.Instance != null ? "Registered" : "Not registered")}");
    }

    public void GetShopList(Action<List<ShopData>> onComplete = null)
    {
        if (APIManager.Instance == null)
        {
            if (showDebugLog) Debug.LogWarning("[ShopManager] APIManager is null, cannot get shop list");
            onComplete?.Invoke(new List<ShopData>());
            return;
        }

        if (!APIManager.Instance.HasValidToken())
        {
            if (showDebugLog) Debug.LogWarning("[ShopManager] No valid token, cannot get shop list");
            onComplete?.Invoke(new List<ShopData>());
            return;
        }

        string endpoint = $"/games/{APIManager.Instance.GameId}/shops";
        StartCoroutine(GetShopListCoroutine(endpoint, onComplete));
    }

    public void GetShopItems(string shopId, Action<List<ItemProfileData>> onComplete = null)
    {
        if (APIManager.Instance == null)
        {
            if (showDebugLog) Debug.LogWarning("[ShopManager] APIManager is null, cannot get shop items");
            onComplete?.Invoke(new List<ItemProfileData>());
            return;
        }

        if (!APIManager.Instance.HasValidToken())
        {
            if (showDebugLog) Debug.LogWarning("[ShopManager] No valid token, cannot get shop items");
            onComplete?.Invoke(new List<ItemProfileData>());
            return;
        }

        string endpoint = $"/shops/{shopId}/item-profiles";
        StartCoroutine(GetShopItemsCoroutine(endpoint, onComplete));
    }

    private IEnumerator GetShopItemsCoroutine(string endpoint, Action<List<ItemProfileData>> onComplete)
    {
        ItemProfileListResponse result = null;
        yield return StartCoroutine(APIManager.Instance.GetRequest<ItemProfileListResponse>(endpoint, (response) => {
            result = response;
        }));

        if (result != null && result.data != null)
        {
            var newItemProfiles = new List<ItemProfileData>();
            foreach (var profile in result.data)
            {
                var profileData = new ItemProfileData
                {
                    item_profile = profile,
                    id = profile.id,
                    item_profile_id = profile.id,
                    game_id = profile.game_id,
                    created_at = profile.created_at,
                    updated_at = profile.updated_at
                };
                newItemProfiles.Add(profileData);
            }

            currentShopItems = newItemProfiles;
            
            if (showDebugLog) Debug.Log($"[ShopManager] Successfully loaded and mapped {currentShopItems.Count} shop items");
            OnShopItemsChanged?.Invoke(currentShopItems);
            onComplete?.Invoke(currentShopItems);
        }
        else
        {
            if (showDebugLog) Debug.LogWarning("[ShopManager] No shop items found in response.");
            currentShopItems.Clear();
            OnShopItemsChanged?.Invoke(currentShopItems);
            onComplete?.Invoke(currentShopItems);
        }
    }

    private IEnumerator GetShopListCoroutine(string endpoint, Action<List<ShopData>> onComplete)
    {
        ShopListResponse result = null;
        yield return StartCoroutine(APIManager.Instance.GetRequest<ShopListResponse>(endpoint, (response) => {
            result = response;
        }));

        if (result != null && result.data != null)
        {
            shopList = result.data;
            
            if (showDebugLog) Debug.Log($"[ShopManager] Successfully loaded {shopList.Count} shops");
            OnShopListChanged?.Invoke(shopList);
            onComplete?.Invoke(shopList);
        }
        else
        {
            if (showDebugLog) Debug.LogWarning("[ShopManager] No shop data found in response.");
            shopList = new List<ShopData>();
            OnShopListChanged?.Invoke(shopList);
            onComplete?.Invoke(new List<ShopData>());
        }
    }

    public void BuyItem(string shopId, string itemProfileId, int number, Action<BuyItemResponse> onComplete = null)
    {
        if (APIManager.Instance == null)
        {
            if (showDebugLog) Debug.LogWarning("[ShopManager] APIManager is null, cannot buy item");
            onComplete?.Invoke(null);
            return;
        }

        if (!APIManager.Instance.HasValidToken())
        {
            if (showDebugLog) Debug.LogWarning("[ShopManager] No valid token, cannot buy item");
            onComplete?.Invoke(null);
            return;
        }

        string endpoint = $"/shops/{shopId}/item-profiles/{itemProfileId}";
        var requestBody = new BuyItemRequest { number = number };
        StartCoroutine(BuyItemCoroutine(endpoint, requestBody, onComplete));
    }

    private IEnumerator BuyItemCoroutine(string endpoint, BuyItemRequest requestBody, Action<BuyItemResponse> onComplete)
    {
        BuyItemResponse result = null;
        yield return StartCoroutine(APIManager.Instance.PostRequest<BuyItemResponse>(endpoint, requestBody, (response) => {
            result = response;
        }));
        
        if (result != null)
        {
            if (showDebugLog) Debug.Log($"[ShopManager] Item purchased successfully: {result.message}");
            if (result.data?.my_items?.Count > 0)
            {
                var item = result.data.my_items[0];
                if (showDebugLog) Debug.Log($"[ShopManager] Received item: {item.name}, Amount: {item.amount}");
            }
            OnItemBought?.Invoke(result);
            onComplete?.Invoke(result);
        }
        else
        {
            if (showDebugLog) Debug.LogWarning("[ShopManager] Failed to purchase item.");
            onComplete?.Invoke(null);
        }
    }

    /// <summary>
    /// Coroutine kiểm tra định kỳ để đảm bảo AutoLoad hoạt động
    /// </summary>
    private IEnumerator PeriodicAutoLoadCheck()
    {
        yield return new WaitForSeconds(1f); // Đợi 1 giây để các manager khác khởi tạo
        
        // Kiểm tra lần đầu
        if (autoLoad && APIManager.Instance != null && APIManager.Instance.HasValidToken() && shopList.Count == 0)
        {
            if (showDebugLog) Debug.Log("[ShopManager] Periodic check: Found valid token but no shops loaded, loading now");
            GetShopList();
        }
        
        // Kiểm tra định kỳ mỗi 5 giây
        while (autoLoad)
        {
            yield return new WaitForSeconds(5f);
            
            if (APIManager.Instance != null && APIManager.Instance.HasValidToken() && shopList.Count == 0)
            {
                if (showDebugLog) Debug.Log("[ShopManager] Periodic check: Found valid token but no shops loaded, loading now");
                GetShopList();
            }
        }
    }

    [System.Serializable]
    private class BuyItemRequest
    {
        public int number;
    }

    public void SelectShopById(string shopId)
    {
        selectedShopIdForEditor = shopId;
        GetShopItems(shopId);
    }
    
    public void UpdateItemProfileId(string shopId, string itemProfileId)
    {
        selectedShopIdForEditor = shopId;
        itemProfileIdForEditor = itemProfileId;
        if (showDebugLog) Debug.Log($"[ShopManager] Selected Item Profile ID: {itemProfileId} from Shop ID: {shopId}");
    }
} 