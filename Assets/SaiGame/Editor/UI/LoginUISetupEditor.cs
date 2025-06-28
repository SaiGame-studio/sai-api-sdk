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