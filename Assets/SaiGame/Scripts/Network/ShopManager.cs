using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaiGame.Enums;
using UnityEngine.Playables;

public class ShopManager : MonoBehaviour
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

    public event System.Action<List<ShopData>> OnShopListChanged;
    public event System.Action<List<ItemProfileData>> OnShopItemsChanged;

    // Editor-only field
    public string selectedShopIdForEditor = null;

    protected virtual void Start()
    {
        if (autoLoad && APIManager.Instance != null && APIManager.Instance.HasValidToken())
        {
            FetchShopList();
        }
    }

    protected virtual void OnEnable()
    {
        if (APIManager.Instance != null)
        {
            APIManager.Instance.OnAuthenticationSuccess += OnAuthenticationSuccess;
        }
        else
        {
            Debug.LogWarning("ShopManager: APIManager.Instance is null in OnEnable");
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
            FetchShopList();
        }
    }

    [ContextMenu("Fetch Shop List")]
    public void FetchShopList()
    {
        string endpoint = $"/games/{APIManager.Instance.GameId}/shops";
        StartCoroutine(FetchShopListCoroutine(endpoint));
    }

    public void FetchShopItems(string shopId)
    {
        string endpoint = $"/shops/{shopId}/item-profiles";
        StartCoroutine(FetchShopItemsCoroutine(endpoint));
    }

    private IEnumerator FetchShopItemsCoroutine(string endpoint)
    {
        bool done = false;
        ItemProfileResponse result = null;
        APIManager.Instance.StartCoroutine(GetShopItemsFromAPI(endpoint, (ItemProfileResponse response) => {
            result = response;
            done = true;
        }));
        while (!done) yield return null;
        if (result != null && result.data != null)
        {
            currentShopItems = new List<ItemProfileData>(result.data);
            OnShopItemsChanged?.Invoke(currentShopItems);
        }
        else
        {
            Debug.LogWarning("No item profiles found in response.");
            currentShopItems = new List<ItemProfileData>();
            OnShopItemsChanged?.Invoke(currentShopItems);
        }
    }

    private IEnumerator GetShopItemsFromAPI(string endpoint, System.Action<ItemProfileResponse> onComplete)
    {
        var method = typeof(APIManager).GetMethod("GetRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var coroutine = (IEnumerator)method.MakeGenericMethod(typeof(ItemProfileResponse)).Invoke(APIManager.Instance, new object[] { endpoint, onComplete });
        yield return StartCoroutine(coroutine);
    }

    private IEnumerator FetchShopListCoroutine(string endpoint)
    {
        bool done = false;
        ShopListResponse result = null;
        APIManager.Instance.StartCoroutine(GetShopListFromAPI(endpoint, (ShopListResponse response) => {
            result = response;
            done = true;
        }));
        while (!done) yield return null;
        if (result != null && result.data != null)
        {
            shopList = result.data;
            OnShopListChanged?.Invoke(shopList);
        }
        else
        {
            Debug.LogWarning("No shop data found in response.");
            shopList = new List<ShopData>();
            OnShopListChanged?.Invoke(shopList);
        }
    }

    private IEnumerator GetShopListFromAPI(string endpoint, System.Action<ShopListResponse> onComplete)
    {
        var method = typeof(APIManager).GetMethod("GetRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var coroutine = (IEnumerator)method.MakeGenericMethod(typeof(ShopListResponse)).Invoke(APIManager.Instance, new object[] { endpoint, onComplete });
        yield return StartCoroutine(coroutine);
    }

    public void BuyItem(string shopId, string itemProfileId, int number)
    {
        string endpoint = $"/shops/{shopId}/item-profiles/{itemProfileId}";
        var requestBody = new BuyItemRequest { number = number };
        StartCoroutine(BuyItemCoroutine(endpoint, requestBody));
    }

    private IEnumerator BuyItemCoroutine(string endpoint, BuyItemRequest requestBody)
    {
        bool done = false;
        BuyItemResponse result = null;
        APIManager.Instance.StartCoroutine(PostRequestToAPI(endpoint, requestBody, (BuyItemResponse response) => {
            result = response;
            done = true;
        }));
        while (!done) yield return null;
        if (result != null)
        {
            if(this.showDebugLog) Debug.Log($"Item purchased successfully: {result.message}");
            if (result.data?.my_items?.Count > 0)
            {
                var item = result.data.my_items[0];
                if (this.showDebugLog) Debug.Log($"Received item: {item.name}, Amount: {item.amount}");
            }
        }
        else
        {
            Debug.LogWarning("Failed to purchase item.");
        }
    }

    private IEnumerator PostRequestToAPI<T>(string endpoint, object data, System.Action<T> onComplete)
    {
        var method = typeof(APIManager).GetMethod("PostRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var coroutine = (IEnumerator)method.MakeGenericMethod(typeof(T)).Invoke(APIManager.Instance, new object[] { endpoint, data, onComplete });
        yield return StartCoroutine(coroutine);
    }

    [System.Serializable]
    private class BuyItemRequest
    {
        public int number;
    }

    public void SelectShopById(string shopId)
    {
        selectedShopIdForEditor = shopId;
        FetchShopItems(shopId);
    }
} 