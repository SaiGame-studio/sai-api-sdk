using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaiGame.Enums;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShopManager : MonoBehaviour
{
    [Header("Shop List (Read Only)")]
    [SerializeField] protected bool showDebugLog = true;

    [SerializeField]
    private List<ShopData> shopList = new List<ShopData>();
    public List<ShopData> ShopList => shopList;

    [Header("Selected Shop Item Profiles (Read Only)")]
    [SerializeField]
    private List<ItemProfileData> currentShopItems = new List<ItemProfileData>();
    public List<ItemProfileData> CurrentShopItems => currentShopItems;

    public event System.Action<List<ShopData>> OnShopListChanged;
    public event System.Action<List<ItemProfileData>> OnShopItemsChanged;

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

#if UNITY_EDITOR
    [CustomEditor(typeof(ShopManager))]
    public class ShopManagerEditor : Editor
    {
        private int selectedShopIndex = -1;
        private string itemProfileId = "";
        private int number = 1;
        private Vector2 itemProfilesScrollPosition;
        private string lastSelectedShopId = null;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ShopManager shopManager = (ShopManager)target;

            // Đồng bộ dropdown nếu selectedShopIdForEditor thay đổi
            if (!string.IsNullOrEmpty(shopManager.selectedShopIdForEditor) && shopManager.selectedShopIdForEditor != lastSelectedShopId)
            {
                int idx = shopManager.ShopList.FindIndex(s => s.id == shopManager.selectedShopIdForEditor);
                if (idx >= 0)
                {
                    selectedShopIndex = idx;
                    lastSelectedShopId = shopManager.selectedShopIdForEditor;
                    shopManager.FetchShopItems(shopManager.ShopList[selectedShopIndex].id);
                }
            }

            if (GUILayout.Button("Load Shop"))
            {
                shopManager.FetchShopList();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Buy Item", EditorStyles.boldLabel);

            // Shop Dropdown
            if (shopManager.ShopList.Count > 0)
            {
                string[] shopNames = new string[shopManager.ShopList.Count];
                for (int i = 0; i < shopManager.ShopList.Count; i++)
                {
                    shopNames[i] = shopManager.ShopList[i].name;
                }

                int newSelectedIndex = EditorGUILayout.Popup("Select Shop", selectedShopIndex, shopNames);
                if (newSelectedIndex != selectedShopIndex)
                {
                    selectedShopIndex = newSelectedIndex;
                    if (selectedShopIndex >= 0)
                    {
                        shopManager.FetchShopItems(shopManager.ShopList[selectedShopIndex].id);
                        shopManager.selectedShopIdForEditor = shopManager.ShopList[selectedShopIndex].id;
                        lastSelectedShopId = shopManager.selectedShopIdForEditor;
                    }
                }

                itemProfileId = EditorGUILayout.TextField("Item Profile ID", itemProfileId);
                number = EditorGUILayout.IntField("Number", number);

                if (GUILayout.Button("Buy Item"))
                {
                    if (string.IsNullOrEmpty(itemProfileId))
                    {
                        EditorUtility.DisplayDialog("Error", "Please enter an Item Profile ID", "OK");
                        return;
                    }
                    if (number <= 0)
                    {
                        EditorUtility.DisplayDialog("Error", "Number must be greater than 0", "OK");
                        return;
                    }
                    shopManager.BuyItem(shopManager.ShopList[selectedShopIndex].id, itemProfileId, number);
                }

                // Display Item Profiles List
                if (selectedShopIndex >= 0 && shopManager.CurrentShopItems.Count > 0)
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Shop Item Profiles", EditorStyles.boldLabel);
                    
                    itemProfilesScrollPosition = EditorGUILayout.BeginScrollView(itemProfilesScrollPosition, GUILayout.Height(200));
                    foreach (var item in shopManager.CurrentShopItems)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        
                        EditorGUILayout.LabelField($"Name: {item.item_profile.name}");
                        EditorGUILayout.LabelField($"Type: {item.item_profile.type}");
                        EditorGUILayout.LabelField($"Price: {item.price_current} (Old: {item.price_old})");
                        EditorGUILayout.LabelField($"Item Profile ID: {item.item_profile_id}");
                        
                        if (GUILayout.Button("Use This Item Profile ID"))
                        {
                            itemProfileId = item.item_profile_id;
                        }
                        
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No shops available. Please load shops first.", MessageType.Info);
            }
        }
    }

    public string selectedShopIdForEditor = null;
    public void SelectShopById(string shopId)
    {
        selectedShopIdForEditor = shopId;
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
#endif
} 