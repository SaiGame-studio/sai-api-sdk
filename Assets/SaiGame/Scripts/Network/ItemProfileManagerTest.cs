using UnityEngine;

/// <summary>
/// Script test để kiểm tra AutoLoad của ItemProfileManager
/// </summary>
public class ItemProfileManagerTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private bool showDebugLog = true;

    private void Start()
    {
        if (runTestOnStart)
        {
            Invoke(nameof(RunTest), 2f); // Đợi 2 giây để các manager khởi tạo
        }
    }

    [ContextMenu("Run AutoLoad Test")]
    public void RunTest()
    {
        if (!showDebugLog) return;

        Debug.Log("=== ItemProfileManager AutoLoad Test ===");
        
        // Kiểm tra ItemProfileManager
        if (ItemProfileManager.Instance != null)
        {
            Debug.Log("✓ ItemProfileManager found");
            ItemProfileManager.Instance.TestAutoLoadStatus();
        }
        else
        {
            Debug.LogError("✗ ItemProfileManager not found!");
        }
        
        // Kiểm tra APIManager
        if (APIManager.Instance != null)
        {
            Debug.Log("✓ APIManager found");
            Debug.Log($"  - Has valid token: {APIManager.Instance.HasValidToken()}");
            Debug.Log($"  - Token length: {APIManager.Instance.GetAuthToken()?.Length ?? 0}");
        }
        else
        {
            Debug.LogError("✗ APIManager not found!");
        }
        
        Debug.Log("=== Test Complete ===");
    }

    [ContextMenu("Trigger Manual AutoLoad")]
    public void TriggerManualAutoLoad()
    {
        if (ItemProfileManager.Instance != null)
        {
            ItemProfileManager.Instance.CheckAndTriggerAutoLoad();
        }
        else
        {
            Debug.LogError("ItemProfileManager not found!");
        }
    }
} 