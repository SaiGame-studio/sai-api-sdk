using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaiGame.Enums;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShopManager : MonoBehaviour
{
    [Header("Shop List (Read Only)")]
    [SerializeField]
    private List<ShopData> shopList = new List<ShopData>();
    public List<ShopData> ShopList => shopList;

    public event System.Action<List<ShopData>> OnShopListChanged;

    [ContextMenu("Fetch Shop List")]
    public void FetchShopList()
    {
        string endpoint = $"/games/{APIManager.Instance.GameId}/shops";
        StartCoroutine(FetchShopListCoroutine(endpoint));
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
        string endpoint = $"/api/shops/{shopId}/item-profiles/{itemProfileId}";
        StartCoroutine(BuyItemCoroutine(endpoint, number));
    }

    private IEnumerator BuyItemCoroutine(string endpoint, int number)
    {
        bool done = false;
        BuyItemResponse result = null;
        // Prepare JSON body
        var jsonBody = JsonUtility.ToJson(new BuyItemRequest { number = number });
        APIManager.Instance.StartCoroutine(PostRequestToAPI(endpoint, jsonBody, (BuyItemResponse response) => {
            result = response;
            done = true;
        }));
        while (!done) yield return null;
        if (result != null)
        {
            Debug.Log($"Item purchased successfully: {result.message}");
        }
        else
        {
            Debug.LogWarning("Failed to purchase item.");
        }
    }

    [System.Serializable]
    private class BuyItemRequest
    {
        public int number;
    }

    private IEnumerator PostRequestToAPI<T>(string endpoint, string jsonBody, System.Action<T> onComplete)
    {
        var method = typeof(APIManager).GetMethod("PostRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var coroutine = (IEnumerator)method.MakeGenericMethod(typeof(T)).Invoke(APIManager.Instance, new object[] { endpoint, jsonBody, onComplete });
        yield return StartCoroutine(coroutine);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ShopManager))]
    public class ShopManagerEditor : Editor
    {
        private int selectedShopIndex = 0;
        private string itemProfileId = "";
        private int number = 1;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ShopManager shopManager = (ShopManager)target;
            
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

                selectedShopIndex = EditorGUILayout.Popup("Select Shop", selectedShopIndex, shopNames);
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
            }
            else
            {
                EditorGUILayout.HelpBox("No shops available. Please load shops first.", MessageType.Info);
            }
        }
    }
#endif
} 