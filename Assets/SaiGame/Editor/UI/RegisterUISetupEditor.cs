#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

[CustomEditor(typeof(RegisterUISetup))]
public class RegisterUISetupEditor : Editor
{
    private string testEmail = "";
    private string testPassword = "";
    private string testConfirmPassword = "";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        RegisterUISetup registerUISetup = (RegisterUISetup)target;
        
        GUILayout.Space(10);
        
        // UI Management Section
        EditorGUILayout.LabelField("UI Management", EditorStyles.boldLabel);
        
        // Create và Delete buttons với màu sắc phù hợp
        using (new EditorGUILayout.HorizontalScope())
        {
            // Create Register UI button - Màu xanh lá
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Xanh lá đẹp
            if (GUILayout.Button("Create Register UI", GUILayout.Height(30)))
            {
                registerUISetup.CreateRegisterUI();
            }
            GUI.backgroundColor = Color.white;
            
            // Delete Register UI button - Màu đỏ
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ đẹp
            if (GUILayout.Button("Delete Register UI", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Delete Register UI", 
                    "Are you sure you want to delete the Register UI? This will remove the Canvas and all UI elements.", 
                    "Yes, Delete", "Cancel"))
                {
                    registerUISetup.DeleteRegisterUI();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        GUILayout.Space(10);
        
        // Test Register Section
        // EditorGUILayout.LabelField("Test Register", EditorStyles.boldLabel);
        // if (Application.isPlaying) { ... }
        // XOÁ toàn bộ phần Test Register, Test Register Form, TEST REGISTER button
        
        GUILayout.Space(10);
        
        // APIManager Integration Section
        EditorGUILayout.LabelField("APIManager Integration", EditorStyles.boldLabel);
        
        if (registerUISetup.apiManager == null)
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
                    Debug.Log("[RegisterUISetupEditor] APIManager found and assigned.");
                }
                else
                {
                    Debug.LogWarning("[RegisterUISetupEditor] No APIManager found in scene.");
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
            EditorGUILayout.LabelField("Canvas Status:", "✓ Register UI is present");
            EditorGUILayout.LabelField("Canvas Name:", existingCanvas.name);
        }
        else
        {
            EditorGUILayout.LabelField("Canvas Status:", "✗ No Register UI found");
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