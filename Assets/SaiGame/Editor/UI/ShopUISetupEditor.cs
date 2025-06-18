#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

[CustomEditor(typeof(ShopUISetup))]
public class ShopUISetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ShopUISetup shopUISetup = (ShopUISetup)target;
        
        GUILayout.Space(10);
        
        // UI Management Section
        EditorGUILayout.LabelField("UI Management", EditorStyles.boldLabel);
        
        // Create và Delete buttons với màu sắc phù hợp
        using (new EditorGUILayout.HorizontalScope())
        {
            // Create Shop UI button - Màu xanh lá
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Xanh lá đẹp
            if (GUILayout.Button("Create Shop UI", GUILayout.Height(30)))
            {
                shopUISetup.CreateShopUI();
            }
            GUI.backgroundColor = Color.white;
            
            // Delete Shop UI button - Màu đỏ
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ đẹp
            if (GUILayout.Button("Delete Shop UI", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Delete Shop UI", 
                    "Are you sure you want to delete the Shop UI? This will remove the Canvas and all UI elements.", 
                    "Yes, Delete", "Cancel"))
                {
                    shopUISetup.DeleteShopUI();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        GUILayout.Space(10);
        
        // Test Shop Section
        EditorGUILayout.LabelField("Test Shop", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            // Show current APIManager status
            APIManager apiManager = shopUISetup.apiManager;
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

            // Show ShopManager status
            ShopManager shopManager = shopUISetup.shopManager;
            if (shopManager != null)
            {
                EditorGUILayout.LabelField("ShopManager Status:", "✓ ShopManager is ready");
            }
            else
            {
                EditorGUILayout.HelpBox("ShopManager not assigned!", MessageType.Warning);
            }
            
            GUILayout.Space(10);
            
            // Quick Actions Section
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // Refresh Shop button - Màu xanh dương
                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f); // Xanh dương
                if (GUILayout.Button("Refresh Shop", GUILayout.Height(25)))
                {
                    shopUISetup.OnRefreshClick();
                }
                GUI.backgroundColor = Color.white;
                
                // Clear Status button - Màu vàng
                GUI.backgroundColor = new Color(1f, 0.8f, 0.2f, 1f); // Vàng đẹp
                if (GUILayout.Button("Clear Status", GUILayout.Height(25)))
                {
                    shopUISetup.ClearStatus();
                }
                GUI.backgroundColor = Color.white;
                
                // Show Loading button - Màu xanh lá nhạt
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f); // Xanh lá nhạt
                if (GUILayout.Button("Show Loading", GUILayout.Height(25)))
                {
                    shopUISetup.ShowLoadingTest();
                }
                GUI.backgroundColor = Color.white;
                
                // Hide Loading button - Màu xám
                GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Xám
                if (GUILayout.Button("Hide Loading", GUILayout.Height(25)))
                {
                    shopUISetup.HideLoadingTest();
                }
                GUI.backgroundColor = Color.white;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                // Back to Main Menu button - Màu tím
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.8f, 1f); // Tím
                if (GUILayout.Button("Back to Main Menu", GUILayout.Height(25)))
                {
                    shopUISetup.OnBackToMainMenuClick();
                }
                GUI.backgroundColor = Color.white;

                // Retry Load Data button - Màu cam
                GUI.backgroundColor = new Color(1f, 0.6f, 0.2f, 1f); // Cam
                if (GUILayout.Button("Retry Load Data", GUILayout.Height(25)))
                {
                    shopUISetup.RetryLoadShopData();
                }
                GUI.backgroundColor = Color.white;

                // Force Load Data button - Màu đỏ nhạt
                GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f, 1f); // Đỏ nhạt
                if (GUILayout.Button("Force Load Data", GUILayout.Height(25)))
                {
                    shopUISetup.ForceLoadShopData();
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
        
        if (shopUISetup.apiManager == null)
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
                    Debug.Log("[ShopUISetupEditor] APIManager found and assigned.");
                }
                else
                {
                    Debug.LogWarning("[ShopUISetupEditor] No APIManager found in scene.");
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("APIManager is properly assigned.", MessageType.Info);
        }

        if (shopUISetup.shopManager == null)
        {
            EditorGUILayout.HelpBox("ShopManager is not assigned. The UI will try to find or create one automatically.", MessageType.Warning);
            
            // Find ShopManager button - Màu xanh lá nhạt
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f); // Xanh lá nhạt
            if (GUILayout.Button("Find ShopManager", GUILayout.Height(25)))
            {
                ShopManager foundManager = FindFirstObjectByType<ShopManager>();
                if (foundManager != null)
                {
                    SerializedProperty shopManagerProp = serializedObject.FindProperty("shopManager");
                    shopManagerProp.objectReferenceValue = foundManager;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log("[ShopUISetupEditor] ShopManager found and assigned.");
                }
                else
                {
                    Debug.LogWarning("[ShopUISetupEditor] No ShopManager found in scene.");
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("ShopManager is properly assigned.", MessageType.Info);
        }
        
        // UI Status Section
        GUILayout.Space(10);
        EditorGUILayout.LabelField("UI Status", EditorStyles.boldLabel);
        
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            EditorGUILayout.LabelField("Canvas Status:", "✓ Shop UI is present");
            EditorGUILayout.LabelField("Canvas Name:", existingCanvas.name);
        }
        else
        {
            EditorGUILayout.LabelField("Canvas Status:", "✗ No Shop UI found");
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

        // Shop Items Status
        if (shopUISetup.shopItemContainer != null)
        {
            int itemCount = shopUISetup.shopItemContainer.childCount;
            EditorGUILayout.LabelField("Shop Items Count:", itemCount.ToString());
        }
        else
        {
            EditorGUILayout.LabelField("Shop Items Container:", "✗ Not found");
        }
    }
}
#endif 