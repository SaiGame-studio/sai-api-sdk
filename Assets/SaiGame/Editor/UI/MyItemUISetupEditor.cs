#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

[CustomEditor(typeof(MyItemUISetup))]
public class MyItemUISetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MyItemUISetup myItemUISetup = (MyItemUISetup)target;
        
        GUILayout.Space(10);
        
        // UI Management Section
        EditorGUILayout.LabelField("UI Management", EditorStyles.boldLabel);
        
        // Create và Delete buttons với màu sắc phù hợp
        using (new EditorGUILayout.HorizontalScope())
        {
            // Create My Item UI button - Màu xanh lá
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Xanh lá đẹp
            if (GUILayout.Button("Create My Item UI", GUILayout.Height(30)))
            {
                myItemUISetup.CreateMyItemUI();
            }
            GUI.backgroundColor = Color.white;
            
            // Delete My Item UI button - Màu đỏ
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ đẹp
            if (GUILayout.Button("Delete My Item UI", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Delete My Item UI", 
                    "Are you sure you want to delete the My Item UI? This will remove the CanvasMyItem and all UI elements.", 
                    "Yes, Delete", "Cancel"))
                {
                    myItemUISetup.DeleteMyItemUI();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        GUILayout.Space(10);
        
        // Test My Items Section
        EditorGUILayout.LabelField("Test My Items", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            // Show current APIManager status
            APIManager apiManager = myItemUISetup.apiManager;
            if (apiManager != null)
            {
                bool hasToken = apiManager.HasValidToken();
                string statusText = hasToken ? "User is logged in" : "No active session";
                EditorGUILayout.LabelField("APIManager Status:", statusText);
                
                if (hasToken)
                {
                    string currentToken = apiManager.GetAuthToken();
                    string tokenPreview = !string.IsNullOrEmpty(currentToken) && currentToken.Length > 20 
                        ? currentToken.Substring(0, 20) + "..." 
                        : currentToken;
                    EditorGUILayout.LabelField("Token:", tokenPreview);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("APIManager not assigned!", MessageType.Warning);
            }

            // Show ItemProfileManager status
            ItemProfileManager itemProfileManager = myItemUISetup.itemProfileManager;
            if (itemProfileManager != null)
            {
                EditorGUILayout.LabelField("ItemProfileManager Status:", "✓ ItemProfileManager is ready");
                EditorGUILayout.LabelField("Available Item Profiles:", itemProfileManager.ItemProfiles.Count.ToString());
            }
            else
            {
                EditorGUILayout.HelpBox("ItemProfileManager not assigned!", MessageType.Warning);
            }

            // Show PlayerItemManager status
            PlayerItemManager playerItemManager = myItemUISetup.playerItemManager;
            if (playerItemManager != null)
            {
                EditorGUILayout.LabelField("PlayerItemManager Status:", "✓ PlayerItemManager is ready");
                EditorGUILayout.LabelField("Player Items Count:", playerItemManager.PlayerItems.Count.ToString());
            }
            else
            {
                EditorGUILayout.HelpBox("PlayerItemManager not assigned!", MessageType.Warning);
            }
            
            GUILayout.Space(10);
            
            // Quick Actions Section
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // Refresh Data button - Màu xanh dương
                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f); // Xanh dương
                if (GUILayout.Button("Refresh Data", GUILayout.Height(25)))
                {
                    myItemUISetup.OnRefreshClick();
                }
                GUI.backgroundColor = Color.white;
                
                // Show Loading button - Màu xanh lá nhạt
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f); // Xanh lá nhạt
                if (GUILayout.Button("Show Loading", GUILayout.Height(25)))
                {
                    myItemUISetup.ShowLoadingTest();
                }
                GUI.backgroundColor = Color.white;
                
                // Hide Loading button - Màu xám
                GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Xám
                if (GUILayout.Button("Hide Loading", GUILayout.Height(25)))
                {
                    myItemUISetup.HideLoadingTest();
                }
                GUI.backgroundColor = Color.white;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                // Back to Main Menu button - Màu tím
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.8f, 1f); // Tím
                if (GUILayout.Button("Back to Main Menu", GUILayout.Height(25)))
                {
                    myItemUISetup.OnBackToMainMenuClick();
                }
                GUI.backgroundColor = Color.white;

                // Test Refresh button - Màu cam
                GUI.backgroundColor = new Color(1f, 0.6f, 0.2f, 1f); // Cam
                if (GUILayout.Button("Test Refresh", GUILayout.Height(25)))
                {
                    myItemUISetup.TestRefresh();
                }
                GUI.backgroundColor = Color.white;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                // Update Items button - Màu xanh lá nhạt  
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f); // Xanh lá nhạt
                if (GUILayout.Button($"Update Items ({myItemUISetup.numberOfItemsToDisplay})", GUILayout.Height(25)))
                {
                    myItemUISetup.OnRefreshClick(); // This will recreate items with new count
                }
                GUI.backgroundColor = Color.white;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Test features are only available when playing.", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        // Manager Integration Section
        EditorGUILayout.LabelField("Manager Integration", EditorStyles.boldLabel);
        
        if (myItemUISetup.apiManager == null)
        {
            EditorGUILayout.HelpBox("APIManager is not assigned. The UI will try to find or create one automatically.", MessageType.Warning);
            
            // Find APIManager button - Màu xanh dương nhạt
            GUI.backgroundColor = new Color(0.4f, 0.6f, 1f, 1f); // Xanh dương nhạt
            if (GUILayout.Button("Find APIManager", GUILayout.Height(25)))
            {
                APIManager foundManager = FindFirstObjectByType<APIManager>();
                if (foundManager != null)
                {
                    SerializedProperty apiManagerProp = serializedObject.FindProperty("apiManager");
                    apiManagerProp.objectReferenceValue = foundManager;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log("[MyItemUISetupEditor] APIManager found and assigned.");
                }
                else
                {
                    Debug.LogWarning("[MyItemUISetupEditor] No APIManager found in scene.");
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("APIManager is properly assigned.", MessageType.Info);
        }

        if (myItemUISetup.itemProfileManager == null)
        {
            EditorGUILayout.HelpBox("ItemProfileManager is not assigned. The UI will try to find or create one automatically.", MessageType.Warning);
            
            // Find ItemProfileManager button - Màu xanh lá nhạt
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f); // Xanh lá nhạt
            if (GUILayout.Button("Find ItemProfileManager", GUILayout.Height(25)))
            {
                ItemProfileManager foundManager = FindFirstObjectByType<ItemProfileManager>();
                if (foundManager != null)
                {
                    SerializedProperty itemProfileManagerProp = serializedObject.FindProperty("itemProfileManager");
                    itemProfileManagerProp.objectReferenceValue = foundManager;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log("[MyItemUISetupEditor] ItemProfileManager found and assigned.");
                }
                else
                {
                    Debug.LogWarning("[MyItemUISetupEditor] No ItemProfileManager found in scene.");
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("ItemProfileManager is properly assigned.", MessageType.Info);
        }

        if (myItemUISetup.playerItemManager == null)
        {
            EditorGUILayout.HelpBox("PlayerItemManager is not assigned. The UI will try to find or create one automatically.", MessageType.Warning);
            
            // Find PlayerItemManager button - Màu vàng nhạt
            GUI.backgroundColor = new Color(1f, 0.8f, 0.4f, 1f); // Vàng nhạt
            if (GUILayout.Button("Find PlayerItemManager", GUILayout.Height(25)))
            {
                PlayerItemManager foundManager = FindFirstObjectByType<PlayerItemManager>();
                if (foundManager != null)
                {
                    SerializedProperty playerItemManagerProp = serializedObject.FindProperty("playerItemManager");
                    playerItemManagerProp.objectReferenceValue = foundManager;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log("[MyItemUISetupEditor] PlayerItemManager found and assigned.");
                }
                else
                {
                    Debug.LogWarning("[MyItemUISetupEditor] No PlayerItemManager found in scene.");
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("PlayerItemManager is properly assigned.", MessageType.Info);
        }
        
        // UI Status Section
        GUILayout.Space(10);
        EditorGUILayout.LabelField("UI Status", EditorStyles.boldLabel);
        
        // Check for CanvasMyItem specifically
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas myItemCanvas = null;
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.name == "CanvasMyItem")
            {
                myItemCanvas = canvas;
                break;
            }
        }

        if (myItemCanvas != null)
        {
            EditorGUILayout.LabelField("CanvasMyItem Status:", "✓ My Item UI is present");
            EditorGUILayout.LabelField("Canvas Name:", myItemCanvas.name);
        }
        else
        {
            EditorGUILayout.LabelField("CanvasMyItem Status:", "✗ No My Item UI found");
        }
        
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();
        if (existingEventSystem != null)
        {
            EditorGUILayout.LabelField("EventSystem Status:", "✓ EventSystem is present");
        }
        else
        {
            EditorGUILayout.LabelField("EventSystem Status:", "✗ No EventSystem found");
        }

        // Button Items Status
        EditorGUILayout.LabelField("Items to Display:", myItemUISetup.numberOfItemsToDisplay.ToString());
        
        if (myItemUISetup.buttonItemContainer != null)
        {
            int itemCount = myItemUISetup.buttonItemContainer.childCount;
            EditorGUILayout.LabelField("Current Button Items:", itemCount.ToString());
        }
        else
        {
            EditorGUILayout.LabelField("Button Items Container:", "Not assigned");
        }

        // Prefab Status
        if (myItemUISetup.buttonItemPrefab != null)
        {
            EditorGUILayout.LabelField("ButtonItem Prefab:", "✓ Prefab is ready");
        }
        else
        {
            EditorGUILayout.LabelField("ButtonItem Prefab:", "✗ Prefab not found");
        }

        GUILayout.Space(10);
        
        // Development Notes
        EditorGUILayout.LabelField("Development Notes", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This UI displays player inventory items with dropdowns to select from available ItemProfiles. " +
            "When an ItemProfile is selected, it shows the total quantity owned by the player across all instances.",
            MessageType.Info);
    }
}
#endif 