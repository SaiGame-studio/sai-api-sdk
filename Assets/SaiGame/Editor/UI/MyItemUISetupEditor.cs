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
        
        using (new EditorGUILayout.HorizontalScope())
        {
            // Create UI button - Xanh lá
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f);
            if (GUILayout.Button("Create My Item UI", GUILayout.Height(30)))
            {
                myItemUISetup.CreateMyItemUI();
            }
            GUI.backgroundColor = Color.white;

            // Delete UI button - Đỏ
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f);
            if (GUILayout.Button("Delete My Item UI", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Delete My Item UI", 
                    "Are you sure you want to delete the My Item UI? This will remove the Canvas and all UI elements.", 
                    "Yes, Delete", "Cancel"))
                {
                    myItemUISetup.DeleteMyItemUI();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        GUILayout.Space(10);
        
        // Test Section
        EditorGUILayout.LabelField("Test My Items", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            // APIManager status
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

            // PlayerItemManager status
            PlayerItemManager playerItemManager = myItemUISetup.playerItemManager;
            if (playerItemManager != null)
            {
                EditorGUILayout.LabelField("PlayerItemManager Status:", "✓ PlayerItemManager is ready");
            }
            else
            {
                EditorGUILayout.HelpBox("PlayerItemManager not assigned!", MessageType.Warning);
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                // Refresh button - Xanh dương
                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f);
                if (GUILayout.Button("Refresh Items", GUILayout.Height(25)))
                {
                    myItemUISetup.OnRefreshClick();
                }
                GUI.backgroundColor = Color.white;

                // Clear Status button - Vàng
                GUI.backgroundColor = new Color(1f, 0.8f, 0.2f, 1f);
                if (GUILayout.Button("Clear Status", GUILayout.Height(25)))
                {
                    myItemUISetup.ClearStatus();
                }
                GUI.backgroundColor = Color.white;

                // Show Loading button - Xanh lá nhạt
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f);
                if (GUILayout.Button("Show Loading", GUILayout.Height(25)))
                {
                    myItemUISetup.ShowLoadingTest();
                }
                GUI.backgroundColor = Color.white;

                // Hide Loading button - Xám
                GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                if (GUILayout.Button("Hide Loading", GUILayout.Height(25)))
                {
                    myItemUISetup.HideLoadingTest();
                }
                GUI.backgroundColor = Color.white;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                // Back to Main Menu button - Tím
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.8f, 1f);
                if (GUILayout.Button("Back to Main Menu", GUILayout.Height(25)))
                {
                    myItemUISetup.OnBackToMainMenuClick();
                }
                GUI.backgroundColor = Color.white;

                // Retry Load Data button - Cam
                GUI.backgroundColor = new Color(1f, 0.6f, 0.2f, 1f);
                if (GUILayout.Button("Retry Load Data", GUILayout.Height(25)))
                {
                    myItemUISetup.RetryLoadPlayerItems();
                }
                GUI.backgroundColor = Color.white;

                // Force Load Data button - Đỏ nhạt
                GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f, 1f);
                if (GUILayout.Button("Force Load Data", GUILayout.Height(25)))
                {
                    myItemUISetup.ForceLoadPlayerItems();
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
            GUI.backgroundColor = new Color(0.4f, 0.6f, 1f, 1f);
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

        if (myItemUISetup.playerItemManager == null)
        {
            EditorGUILayout.HelpBox("PlayerItemManager is not assigned. The UI will try to find or create one automatically.", MessageType.Warning);
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f);
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
    }
}
#endif 