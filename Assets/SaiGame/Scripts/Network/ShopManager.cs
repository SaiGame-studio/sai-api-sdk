using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShopManager : SaiSingleton<ShopManager>
{
    [Header("Debug Settings")]
    [SerializeField] protected bool showDebugLog = true;

    [Header("Auto Load Settings")]
    [Tooltip("If enabled, Refresh will be automatically called when game authentication is successful")]
    [SerializeField] protected bool autoLoad = false;

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
        if (autoLoad && APIManager.Instance != null && APIManager.Instance.HasValidToken())
        {
            GetShopList();
        }
    }

    protected virtual void OnEnable()
    {
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
            GetShopList();
        }
    }

    public void GetShopList(Action<List<ShopData>> onComplete = null)
    {
        string endpoint = $"/games/{APIManager.Instance.GameId}/shops";
        StartCoroutine(GetShopListCoroutine(endpoint, onComplete));
    }

    public void GetShopItems(string shopId, Action<List<ItemProfileData>> onComplete = null)
    {
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
            currentShopItems = new List<ItemProfileData>(result.data);
            OnShopItemsChanged?.Invoke(currentShopItems);
            onComplete?.Invoke(currentShopItems);
        }
        else
        {
            if (showDebugLog) Debug.LogWarning("No item profiles found in response.");
            currentShopItems = new List<ItemProfileData>();
            OnShopItemsChanged?.Invoke(currentShopItems);
            onComplete?.Invoke(new List<ItemProfileData>());
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
            OnShopListChanged?.Invoke(shopList);
            onComplete?.Invoke(shopList);
        }
        else
        {
            if (showDebugLog) Debug.LogWarning("No shop data found in response.");
            shopList = new List<ShopData>();
            OnShopListChanged?.Invoke(shopList);
            onComplete?.Invoke(new List<ShopData>());
        }
    }

    public void BuyItem(string shopId, string itemProfileId, int number, Action<BuyItemResponse> onComplete = null)
    {
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
            if(this.showDebugLog) Debug.Log($"Item purchased successfully: {result.message}");
            if (result.data?.my_items?.Count > 0)
            {
                var item = result.data.my_items[0];
                if (this.showDebugLog) Debug.Log($"Received item: {item.name}, Amount: {item.amount}");
            }
            OnItemBought?.Invoke(result);
            onComplete?.Invoke(result);
        }
        else
        {
            Debug.LogWarning("Failed to purchase item.");
            onComplete?.Invoke(null);
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
        if (showDebugLog) Debug.Log($"Selected Item Profile ID: {itemProfileId} from Shop ID: {shopId}");
    }
} 