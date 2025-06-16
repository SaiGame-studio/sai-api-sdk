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
        }
        else
        {
            Debug.LogWarning("No shop data found in response.");
            shopList = new List<ShopData>();
        }
    }

    private IEnumerator GetShopListFromAPI(string endpoint, System.Action<ShopListResponse> onComplete)
    {
        var method = typeof(APIManager).GetMethod("GetRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var coroutine = (IEnumerator)method.MakeGenericMethod(typeof(ShopListResponse)).Invoke(APIManager.Instance, new object[] { endpoint, onComplete });
        yield return StartCoroutine(coroutine);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ShopManager))]
    public class ShopManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ShopManager shopManager = (ShopManager)target;
            if (GUILayout.Button("Load Shop"))
            {
                shopManager.FetchShopList();
            }
        }
    }
#endif
} 