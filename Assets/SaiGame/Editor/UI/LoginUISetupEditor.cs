#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

[CustomEditor(typeof(LoginUISetup))]
public class LoginUISetupEditor : Editor
{
    private string testEmail = "";
    private string testPassword = "";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LoginUISetup loginUISetup = (LoginUISetup)target;
        
        GUILayout.Space(10);
        
        // UI Management Section
        EditorGUILayout.LabelField("UI Management", EditorStyles.boldLabel);
        
        // Create và Delete buttons với màu sắc phù hợp
        using (new EditorGUILayout.HorizontalScope())
        {
            // Create Login UI button - Màu xanh lá
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Xanh lá đẹp
            if (GUILayout.Button("Create Login UI", GUILayout.Height(30)))
            {
                loginUISetup.CreateLoginUI();
            }
            GUI.backgroundColor = Color.white;
            
            // Delete Login UI button - Màu đỏ
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ đẹp
            if (GUILayout.Button("Delete Login UI", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Delete Login UI", 
                    "Are you sure you want to delete the Login UI? This will remove the Canvas and all UI elements.", 
                    "Yes, Delete", "Cancel"))
                {
                    loginUISetup.DeleteLoginUI();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        GUILayout.Space(10);
        
        // Test Login Section
        EditorGUILayout.LabelField("Test Login", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            // Show current APIManager status
            APIManager apiManager = loginUISetup.apiManager;
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
            
            GUILayout.Space(10);
            
            // Quick Actions Section + Go to Register
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // Clear Status button - Màu vàng
                GUI.backgroundColor = new Color(1f, 0.8f, 0.2f, 1f); // Vàng đẹp
                if (GUILayout.Button("Clear Status", GUILayout.Height(25)))
                {
                    loginUISetup.ClearStatus();
                }
                GUI.backgroundColor = Color.white;
                
                // Show Loading button - Màu xanh lá nhạt
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f); // Xanh lá nhạt
                if (GUILayout.Button("Show Loading", GUILayout.Height(25)))
                {
                    loginUISetup.ShowLoadingTest();
                }
                GUI.backgroundColor = Color.white;
                
                // Hide Loading button - Màu xám
                GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Xám
                if (GUILayout.Button("Hide Loading", GUILayout.Height(25)))
                {
                    loginUISetup.HideLoadingTest();
                }
                GUI.backgroundColor = Color.white;

                // Go to Register button - Màu tím
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.8f, 1f); // Tím
                if (GUILayout.Button("Go to Register", GUILayout.Height(25)))
                {
                    loginUISetup.OnGoToRegisterClick();
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
        
        if (loginUISetup.apiManager == null)
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
                    Debug.Log("[LoginUISetupEditor] APIManager found and assigned.");
                }
                else
                {
                    Debug.LogWarning("[LoginUISetupEditor] No APIManager found in scene.");
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("APIManager is properly assigned.", MessageType.Info);
        }
        
        // UI Status Section
        GUILayout.Space(10);
        EditorGUILayout.LabelField("UI Status", EditorStyles.boldLabel);
        
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            EditorGUILayout.LabelField("Canvas Status:", "✓ Login UI is present");
            EditorGUILayout.LabelField("Canvas Name:", existingCanvas.name);
        }
        else
        {
            EditorGUILayout.LabelField("Canvas Status:", "✗ No Login UI found");
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