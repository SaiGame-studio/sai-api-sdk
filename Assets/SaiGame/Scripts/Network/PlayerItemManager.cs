using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerItemManager : SaiSingleton<PlayerItemManager>
{
    [Header("Player Items (Read Only)")]
    [SerializeField] protected bool showDebugLog = true;

    [SerializeField]
    private List<InventoryItem> playerItems = new List<InventoryItem>();
    public List<InventoryItem> PlayerItems => playerItems;

    public event Action<List<InventoryItem>> OnPlayerItemsChanged;

    public void GetPlayerItems(Action<List<InventoryItem>> onComplete)
    {
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
            if (showDebugLog) Debug.LogWarning("No player items found in response.");
            playerItems = new List<InventoryItem>();
            OnPlayerItemsChanged?.Invoke(playerItems);
            onComplete?.Invoke(new List<InventoryItem>()); // Still callback with empty list
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