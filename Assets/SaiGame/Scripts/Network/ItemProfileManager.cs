using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemProfileManager : SaiSingleton<ItemProfileManager>
{
    [Header("Settings")]
    [SerializeField] protected bool showDebugLog = true;
    [Tooltip("If enabled, Refresh will be automatically called when game authentication is successful")]
    [SerializeField] protected bool autoLoad = true;

    [Header("Item Profiles (Read Only)")]
    [SerializeField]
    private List<ItemProfileData> itemProfiles = new List<ItemProfileData>();
    public List<ItemProfileData> ItemProfiles => itemProfiles;

    public event Action<List<ItemProfileData>> OnItemProfilesChanged;

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
            if (showDebugLog) Debug.Log("[ItemProfileManager] AutoLoad: Found valid token, loading item profiles");
            GetItemProfiles();
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
            if (showDebugLog) Debug.Log("[ItemProfileManager] AutoLoad: Authentication success, loading item profiles");
            GetItemProfiles();
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
            if (showDebugLog) Debug.Log("[ItemProfileManager] Manual AutoLoad check: Found valid token, loading item profiles");
            GetItemProfiles();
        }
        else
        {
            if (showDebugLog) Debug.Log("[ItemProfileManager] Manual AutoLoad check: No valid token or autoLoad disabled");
        }
    }

    /// <summary>
    /// Test method để kiểm tra trạng thái AutoLoad
    /// </summary>
    [ContextMenu("Test AutoLoad Status")]
    public void TestAutoLoadStatus()
    {
        Debug.Log($"[ItemProfileManager] AutoLoad Status:");
        Debug.Log($"  - autoLoad enabled: {autoLoad}");
        Debug.Log($"  - APIManager exists: {APIManager.Instance != null}");
        
        if (APIManager.Instance != null)
        {
            Debug.Log($"  - Has valid token: {APIManager.Instance.HasValidToken()}");
            Debug.Log($"  - Current token: {(!string.IsNullOrEmpty(APIManager.Instance.GetAuthToken()) ? "Present" : "None")}");
        }
        
        Debug.Log($"  - Item profiles loaded: {itemProfiles.Count}");
        Debug.Log($"  - Event listeners: {(APIManager.Instance != null ? "Registered" : "Not registered")}");
    }

    public void GetItemProfiles(Action<List<ItemProfileData>> onComplete = null)
    {
        if (APIManager.Instance == null)
        {
            if (showDebugLog) Debug.LogWarning("[ItemProfileManager] APIManager is null, cannot get item profiles");
            onComplete?.Invoke(new List<ItemProfileData>());
            return;
        }

        if (!APIManager.Instance.HasValidToken())
        {
            if (showDebugLog) Debug.LogWarning("[ItemProfileManager] No valid token, cannot get item profiles");
            onComplete?.Invoke(new List<ItemProfileData>());
            return;
        }

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

            itemProfiles = newItemProfiles;

            if (showDebugLog) Debug.Log($"[ItemProfileManager] Successfully loaded and mapped {itemProfiles.Count} item profiles");
            OnItemProfilesChanged?.Invoke(itemProfiles);
            onComplete?.Invoke(itemProfiles);
        }
        else
        {
            if(showDebugLog) Debug.LogWarning("[ItemProfileManager] No item profiles found in response.");
            itemProfiles.Clear();
            OnItemProfilesChanged?.Invoke(itemProfiles);
            onComplete?.Invoke(itemProfiles);
        }
    }

    /// <summary>
    /// Coroutine kiểm tra định kỳ để đảm bảo AutoLoad hoạt động
    /// </summary>
    private IEnumerator PeriodicAutoLoadCheck()
    {
        yield return new WaitForSeconds(1f); // Đợi 1 giây để các manager khác khởi tạo
        
        // Kiểm tra lần đầu
        if (autoLoad && APIManager.Instance != null && APIManager.Instance.HasValidToken() && itemProfiles.Count == 0)
        {
            if (showDebugLog) Debug.Log("[ItemProfileManager] Periodic check: Found valid token but no profiles loaded, loading now");
            GetItemProfiles();
        }
        
        // Kiểm tra định kỳ mỗi 5 giây
        while (autoLoad)
        {
            yield return new WaitForSeconds(5f);
            
            if (APIManager.Instance != null && APIManager.Instance.HasValidToken() && itemProfiles.Count == 0)
            {
                if (showDebugLog) Debug.Log("[ItemProfileManager] Periodic check: Found valid token but no profiles loaded, loading now");
                GetItemProfiles();
            }
        }
    }
} 