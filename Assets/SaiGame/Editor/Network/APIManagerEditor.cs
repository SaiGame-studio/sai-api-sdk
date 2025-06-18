#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(APIManager))]
public class APIManagerEditor : Editor
{
    private string loginEmail = "";
    private string loginPassword = "";
    private string registerEmail = "";
    private string registerPassword = "";
    private string registerPasswordConfirm = "";

    private void OnEnable()
    {
        // Load remembered email when inspector is enabled
        // Không tự động đồng bộ loginEmail với rememberedEmail nữa
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        APIManager apiManager = (APIManager)target;
        
        // Không còn tự động sync loginEmail với rememberedEmail
        
        GUILayout.Space(10);
        
        // Login Section
        EditorGUILayout.LabelField("Login Management", EditorStyles.boldLabel);
        
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
        
        // Login inputs and button
        if (!hasToken && Application.isPlaying)
        {
            EditorGUILayout.LabelField("Login Form:", EditorStyles.boldLabel);
            
            // Email field with manual sync button
            using (new EditorGUILayout.HorizontalScope())
            {
                loginEmail = EditorGUILayout.TextField("Email:", loginEmail);
                if (GUILayout.Button("Sync", GUILayout.Width(40)))
                {
                    string remembered = apiManager.GetRememberedEmail();
                    if (!string.IsNullOrEmpty(remembered))
                    {
                        loginEmail = remembered;
                        Debug.Log("[APIManagerEditor] Synced login email with Remembered Email.");
                    }
                    else
                    {
                        Debug.LogWarning("[APIManagerEditor] No Remembered Email to sync.");
                    }
                }
            }
            
            loginPassword = EditorGUILayout.PasswordField("Password:", loginPassword);
            
            GUILayout.Space(5);
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("LOGIN", GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(loginEmail) || string.IsNullOrEmpty(loginPassword))
                {
                    Debug.LogWarning("[APIManagerEditor] Please enter both email and password.");
                }
                else
                {
                    APIManager.Instance.LoginWithToken(loginEmail, loginPassword, null);
                    APIManager.Instance.SaveRememberedEmail(loginEmail);
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Login form is only available when playing.", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        // Register Section
        EditorGUILayout.LabelField("Register Management", EditorStyles.boldLabel);
        
        if (!hasToken && Application.isPlaying)
        {
            EditorGUILayout.LabelField("Register Form:", EditorStyles.boldLabel);
            
            // Email field with random button
            using (new EditorGUILayout.HorizontalScope())
            {
                registerEmail = EditorGUILayout.TextField("Email:", registerEmail);
                if (GUILayout.Button("Random Email", GUILayout.Width(100)))
                {
                    // Sinh email ngẫu nhiên
                    string random = System.Guid.NewGuid().ToString("N").Substring(0, 8);
                    registerEmail = $"user{random}@example.com";
                    APIManager.Instance.SaveRememberedEmail(registerEmail);
                }
            }
            
            registerPassword = EditorGUILayout.PasswordField("Password:", registerPassword);
            registerPasswordConfirm = EditorGUILayout.PasswordField("Confirm Password:", registerPasswordConfirm);
            
            GUILayout.Space(5);
            
            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("REGISTER", GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(registerEmail) || string.IsNullOrEmpty(registerPassword) || string.IsNullOrEmpty(registerPasswordConfirm))
                {
                    Debug.LogWarning("[APIManagerEditor] Please fill in all fields.");
                }
                else if (registerPassword != registerPasswordConfirm)
                {
                    Debug.LogWarning("[APIManagerEditor] Passwords do not match.");
                }
                else
                {
                    apiManager.Register(registerEmail, registerPassword, registerPasswordConfirm, (response) =>
                    {
                        string json = JsonUtility.ToJson(response, true);
                        if (response.success)
                        {
                            Debug.Log($"[APIManagerEditor] Registration successful! Response: {json}");
                            // Clear register form
                            registerEmail = "";
                            registerPassword = "";
                            registerPasswordConfirm = "";
                        }
                        else
                        {
                            Debug.LogWarning($"[APIManagerEditor] Registration failed: {response.message}\nResponse: {json}");
                        }
                    });
                }
            }
            GUI.backgroundColor = Color.white;
        }
        else if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Register form is only available when playing.", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        // Logout Section
        EditorGUILayout.LabelField("Logout Management", EditorStyles.boldLabel);
        
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
        
        // Clear Local Token button - available even when not playing
        GUILayout.Space(5);
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("CLEAR LOCAL TOKEN", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Clear Local Token Confirmation", 
                "Are you sure you want to clear the local token from storage? This will remove the saved authentication token but won't call the logout API.", 
                "Yes, Clear Token", "Cancel"))
            {
                apiManager.ClearAuthToken();
                apiManager.UpdateTokenDisplayInfo();
                EditorUtility.SetDirty(apiManager);
                EditorUtility.DisplayDialog("Token Cleared", "Local token has been cleared from storage.", "OK");
            }
        }
        GUI.backgroundColor = Color.white;

        // Clear Remembered Email button - ngay dưới Clear Local Token
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("CLEAR REMEMBERED EMAIL", GUILayout.Height(25)))
        {
            apiManager.ClearRememberedEmail();
            loginEmail = "";
            registerEmail = "";
            EditorUtility.SetDirty(apiManager);
        }
        GUI.backgroundColor = Color.white;

        // Hiển thị Remembered Email
        string rememberedEmail = apiManager.GetRememberedEmail();
        if (!string.IsNullOrEmpty(rememberedEmail))
        {
            EditorGUILayout.LabelField("Current Remembered Email:", rememberedEmail);
        }
        else
        {
            EditorGUILayout.LabelField("Current Remembered Email:", "No email remembered");
        }
    }
}
#endif 