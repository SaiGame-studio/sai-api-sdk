#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[CustomEditor(typeof(MainMenuUISetup))]
public class MainMenuUISetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MainMenuUISetup mainMenuUISetup = (MainMenuUISetup)target;
        
        GUILayout.Space(10);
        
        // UI Management Section
        EditorGUILayout.LabelField("UI Management", EditorStyles.boldLabel);
        
        // Create và Delete buttons với màu sắc phù hợp
        using (new EditorGUILayout.HorizontalScope())
        {
            // Create Main Menu UI button - Màu xanh lá
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Xanh lá đẹp
            if (GUILayout.Button("Create Main Menu UI", GUILayout.Height(30)))
            {
                mainMenuUISetup.CreateMainMenuUI();
            }
            GUI.backgroundColor = Color.white;
            
            // Delete Main Menu UI button - Màu đỏ
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ đẹp
            if (GUILayout.Button("Delete Main Menu UI", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Delete Main Menu UI", 
                    "Are you sure you want to delete the Main Menu UI? This will remove the Canvas and all UI elements.", 
                    "Yes, Delete", "Cancel"))
                {
                    mainMenuUISetup.DeleteMainMenuUI();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        GUILayout.Space(10);
        
        // Test Features Section
        EditorGUILayout.LabelField("Test Features", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            // Show current APIManager status
            APIManager apiManager = FindFirstObjectByType<APIManager>();
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
                EditorGUILayout.HelpBox("APIManager not found!", MessageType.Warning);
            }
            
            GUILayout.Space(10);
            
            // Quick Actions Section
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // Shop button - Màu xanh dương
                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f); // Xanh dương
                if (GUILayout.Button("Go to Shop", GUILayout.Height(25)))
                {
                    mainMenuUISetup.OnShopClick();
                }
                GUI.backgroundColor = Color.white;
                
                // My Items button - Màu xanh lá nhạt
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f); // Xanh lá nhạt
                if (GUILayout.Button("Go to My Items", GUILayout.Height(25)))
                {
                    mainMenuUISetup.OnMyItemsClick();
                }
                GUI.backgroundColor = Color.white;
                
                // My Character button - Màu tím
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.8f, 1f); // Tím
                if (GUILayout.Button("Go to My Character", GUILayout.Height(25)))
                {
                    mainMenuUISetup.OnMyCharacterClick();
                }
                GUI.backgroundColor = Color.white;
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // Logout button - Màu đỏ
                GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ
                if (GUILayout.Button("Logout", GUILayout.Height(25)))
                {
                    mainMenuUISetup.OnLogoutClick();
                }
                GUI.backgroundColor = Color.white;
                
                // Update Welcome Text button - Màu vàng
                GUI.backgroundColor = new Color(1f, 0.8f, 0.2f, 1f); // Vàng
                if (GUILayout.Button("Update Welcome Text", GUILayout.Height(25)))
                {
                    mainMenuUISetup.UpdateWelcomeText();
                }
                GUI.backgroundColor = Color.white;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Test features are only available when playing.", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        // APIManager Integration Section
        EditorGUILayout.LabelField("APIManager Integration", EditorStyles.boldLabel);
        
        APIManager existingAPIManager = FindFirstObjectByType<APIManager>();
        if (existingAPIManager == null)
        {
            EditorGUILayout.HelpBox("APIManager is not found in scene. The UI will try to create one automatically.", MessageType.Warning);
            
            // Create APIManager button - Màu xanh dương nhạt
            GUI.backgroundColor = new Color(0.4f, 0.6f, 1f, 1f); // Xanh dương nhạt
            if (GUILayout.Button("Create APIManager", GUILayout.Height(25)))
            {
                GameObject apiManagerGO = new GameObject("APIManager");
                APIManager apiManager = apiManagerGO.AddComponent<APIManager>();
                Debug.Log("[MainMenuUISetupEditor] APIManager created.");
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("APIManager is present in scene.", MessageType.Info);
        }
        
        // UI Status Section
        GUILayout.Space(10);
        EditorGUILayout.LabelField("UI Status", EditorStyles.boldLabel);
        
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            EditorGUILayout.LabelField("Canvas Status:", "✓ Main Menu UI is present");
            EditorGUILayout.LabelField("Canvas Name:", existingCanvas.name);
            
            // Check for specific UI elements
            Button shopBtn = existingCanvas.GetComponentInChildren<Button>();
            if (shopBtn != null)
            {
                EditorGUILayout.LabelField("Shop Button:", "✓ Found");
            }
            else
            {
                EditorGUILayout.LabelField("Shop Button:", "✗ Not found");
            }
        }
        else
        {
            EditorGUILayout.LabelField("Canvas Status:", "✗ No Main Menu UI found");
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
    }
}
#endif 