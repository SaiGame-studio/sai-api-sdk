using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemProfileManager : SaiSingleton<ItemProfileManager>
{
    [Header("Debug Settings")]
    [SerializeField] protected bool showDebugLog = true;

    [Header("Auto Load Settings")]
    [Tooltip("If enabled, Refresh will be automatically called when game authentication is successful")]
    [SerializeField] protected bool autoLoad = false;

    [Header("Item Profiles (Read Only)")]
    [SerializeField]
    private List<ItemProfileData> itemProfiles = new List<ItemProfileData>();
    public List<ItemProfileData> ItemProfiles => itemProfiles;

    public event Action<List<ItemProfileData>> OnItemProfilesChanged;

    protected override void Awake()
    {
        base.Awake();
        if (autoLoad && APIManager.Instance != null && APIManager.Instance.HasValidToken())
        {
            GetItemProfiles();
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
            if(showDebugLog) Debug.LogWarning("ItemProfileManager: APIManager.Instance is null in OnEnable");
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
            GetItemProfiles();
        }
    }

    public void GetItemProfiles(Action<List<ItemProfileData>> onComplete = null)
    {
        string endpoint = $"/games/{APIManager.Instance.GameId}/item-profiles";
        StartCoroutine(GetItemProfilesCoroutine(endpoint, onComplete));
    }

    private IEnumerator GetItemProfilesCoroutine(string endpoint, Action<List<ItemProfileData>> onComplete)
    {
        ItemProfileListResponse result = null;
        yield return StartCoroutine(APIManager.Instance.GetRequest<ItemProfileListResponse>(endpoint, (response) => {
            result = response;
        }));
        
        if (result != null && result.data != null)
        {
            itemProfiles = new List<ItemProfileData>(result.data);
            OnItemProfilesChanged?.Invoke(itemProfiles);
            onComplete?.Invoke(itemProfiles);
        }
        else
        {
            if(showDebugLog) Debug.LogWarning("No item profiles found in response.");
            itemProfiles = new List<ItemProfileData>();
            OnItemProfilesChanged?.Invoke(itemProfiles);
            onComplete?.Invoke(new List<ItemProfileData>());
        }
    }
} 