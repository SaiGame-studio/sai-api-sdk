#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(APIManager))]
public class APIManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        APIManager apiManager = (APIManager)target;
        
        GUILayout.Space(10);
        
        // Logout Section
        EditorGUILayout.LabelField("Logout Management", EditorStyles.boldLabel);
        
        // Show current login status
        bool hasToken = apiManager.HasValidToken();
        string statusText = hasToken ? "User is logged in" : "No active session";
        EditorGUILayout.LabelField("Status:", statusText);
        
        if (hasToken)
        {
            string currentToken = apiManager.GetAuthToken();
            string tokenPreview = !string.IsNullOrEmpty(currentToken) && currentToken.Length > 20 
                ? currentToken.Substring(0, 20) + "..." 
                : currentToken;
            EditorGUILayout.LabelField("Token:", tokenPreview);
        }
        
        GUILayout.Space(5);
        
        // Logout button - only show if has token
        if (hasToken && Application.isPlaying)
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("LOGOUT", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Logout Confirmation", 
                    "Are you sure you want to logout? This will clear your session and return to login scene.", 
                    "Yes, Logout", "Cancel"))
                {
                    apiManager.LogoutWithAPI();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Logout button is only available when playing.", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        // Remembered Email Section
        EditorGUILayout.LabelField("Remembered Email Management", EditorStyles.boldLabel);
        
        string rememberedEmail = apiManager.GetRememberedEmail();
        if (!string.IsNullOrEmpty(rememberedEmail))
        {
            EditorGUILayout.LabelField("Current Remembered Email:", rememberedEmail);
        }
        else
        {
            EditorGUILayout.LabelField("Current Remembered Email:", "No email remembered");
        }
        
        GUILayout.Space(5);
        
        // Buttons for email management
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Clear Remembered Email"))
            {
                apiManager.ClearRememberedEmail();
                EditorUtility.SetDirty(apiManager);
            }
            
            if (GUILayout.Button("Refresh Display"))
            {
                apiManager.UpdateTokenDisplayInfo();
                EditorUtility.SetDirty(apiManager);
            }
        }
        
        GUILayout.Space(5);
        
        // Test email input
        EditorGUILayout.LabelField("Test Email Input:", EditorStyles.boldLabel);
        string testEmail = EditorGUILayout.TextField("Test Email:", "");
        
        if (!string.IsNullOrEmpty(testEmail) && GUILayout.Button("Save Test Email"))
        {
            apiManager.SaveRememberedEmail(testEmail);
            EditorUtility.SetDirty(apiManager);
        }
    }
}
#endif 