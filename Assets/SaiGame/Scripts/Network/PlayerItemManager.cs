using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemManager : MonoBehaviour
{
    [Header("Player Items (Read Only)")]
    [SerializeField] protected bool showDebugLog = true;

    [SerializeField]
    private List<InventoryItem> playerItems = new List<InventoryItem>();
    public List<InventoryItem> PlayerItems => playerItems;

    public event System.Action<List<InventoryItem>> OnPlayerItemsChanged;

    [ContextMenu("Fetch Player Items")]
    public void FetchPlayerItems()
    {
        string endpoint = $"/games/{APIManager.Instance.GameId}/items";
        StartCoroutine(FetchPlayerItemsCoroutine(endpoint));
    }

    private IEnumerator FetchPlayerItemsCoroutine(string endpoint)
    {
        bool done = false;
        PlayerItemListResponse result = null;
        APIManager.Instance.StartCoroutine(GetPlayerItemsFromAPI(endpoint, (PlayerItemListResponse response) => {
            result = response;
            done = true;
        }));
        while (!done) yield return null;
        if (result != null && result.data != null)
        {
            playerItems = new List<InventoryItem>(result.data);
            OnPlayerItemsChanged?.Invoke(playerItems);
        }
        else
        {
            Debug.LogWarning("No player items found in response.");
            playerItems = new List<InventoryItem>();
            OnPlayerItemsChanged?.Invoke(playerItems);
        }
    }

    private IEnumerator GetPlayerItemsFromAPI(string endpoint, System.Action<PlayerItemListResponse> onComplete)
    {
        var method = typeof(APIManager).GetMethod("GetRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var coroutine = (IEnumerator)method.MakeGenericMethod(typeof(PlayerItemListResponse)).Invoke(APIManager.Instance, new object[] { endpoint, onComplete });
        yield return StartCoroutine(coroutine);
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