using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProfileManager : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] protected bool showDebugLog = true;

    [Header("Auto Load Settings")]
    [Tooltip("If enabled, Refresh will be automatically called when game authentication is successful")]
    [SerializeField] protected bool autoLoad = false;

    [Header("Item Profiles (Read Only)")]
    [SerializeField]
    private List<ItemProfileSimple> itemProfiles = new List<ItemProfileSimple>();
    public List<ItemProfileSimple> ItemProfiles => itemProfiles;

    public event System.Action<List<ItemProfileSimple>> OnItemProfilesChanged;

    protected virtual void Start()
    {
        if (autoLoad && APIManager.Instance != null && APIManager.Instance.HasValidToken())
        {
            FetchItemProfiles();
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
            Debug.LogWarning("ItemProfileManager: APIManager.Instance is null in OnEnable");
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
            FetchItemProfiles();
        }
    }

    [ContextMenu("Fetch Item Profiles")]
    public void FetchItemProfiles()
    {
        string endpoint = $"/games/{APIManager.Instance.GameId}/item-profiles";
        StartCoroutine(FetchItemProfilesCoroutine(endpoint));
    }

    private IEnumerator FetchItemProfilesCoroutine(string endpoint)
    {
        bool done = false;
        ItemProfileListResponse result = null;
        APIManager.Instance.StartCoroutine(GetItemProfilesFromAPI(endpoint, (ItemProfileListResponse response) => {
            result = response;
            done = true;
        }));
        while (!done) yield return null;
        if (result != null && result.data != null)
        {
            itemProfiles = new List<ItemProfileSimple>(result.data);
            OnItemProfilesChanged?.Invoke(itemProfiles);
        }
        else
        {
            Debug.LogWarning("No item profiles found in response.");
            itemProfiles = new List<ItemProfileSimple>();
            OnItemProfilesChanged?.Invoke(itemProfiles);
        }
    }

    private IEnumerator GetItemProfilesFromAPI(string endpoint, System.Action<ItemProfileListResponse> onComplete)
    {
        var method = typeof(APIManager).GetMethod("GetRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var coroutine = (IEnumerator)method.MakeGenericMethod(typeof(ItemProfileListResponse)).Invoke(APIManager.Instance, new object[] { endpoint, onComplete });
        yield return StartCoroutine(coroutine);
    }
} 