#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

[CustomEditor(typeof(MyItemUISetup))]
public class MyItemUISetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all properties except the ones we want to move
        DrawPropertiesExcluding(serializedObject, "showDummyData", "dummyItemCount");
        
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
            if (APIManager.Instance != null)
            {
                bool hasToken = APIManager.Instance.HasValidToken();
                string statusText = hasToken ? "User is logged in" : "No active session";
                EditorGUILayout.LabelField("APIManager Status:", statusText);
                if (hasToken)
                {
                    string currentToken = APIManager.Instance.GetAuthToken();
                    string tokenPreview = !string.IsNullOrEmpty(currentToken) && currentToken.Length > 20 
                        ? currentToken.Substring(0, 20) + "..." 
                        : currentToken;
                    EditorGUILayout.LabelField("Token:", tokenPreview);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("APIManager instance not found!", MessageType.Warning);
            }

            // PlayerItemManager status
            if (PlayerItemManager.Instance != null)
            {
                EditorGUILayout.LabelField("PlayerItemManager Status:", "✓ PlayerItemManager instance is ready");
            }
            else
            {
                EditorGUILayout.HelpBox("PlayerItemManager instance not found!", MessageType.Warning);
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
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Test features are only available when playing.", MessageType.Info);
        }
        
        GUILayout.Space(10);

        if (Application.isEditor)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showDummyData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dummyItemCount"));

            using (new EditorGUILayout.HorizontalScope())
            {
                // Show Dummy Data button - Xanh lá
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f);
                if (GUILayout.Button("Show Dummy Data", GUILayout.Height(25)))
                {
                    myItemUISetup.ShowDummyDataButton();
                }
                GUI.backgroundColor = Color.white;

                // Delete Dummy Data button - Đỏ
                GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f);
                if (GUILayout.Button("Delete Dummy Data", GUILayout.Height(25)))
                {
                    myItemUISetup.DeleteDummyDataButton();
                }
                GUI.backgroundColor = Color.white;
            }

            // Toggle Dummy Data button - Cam
            GUI.backgroundColor = new Color(1f, 0.6f, 0.2f, 1f);
            if (GUILayout.Button("Toggle Dummy Data", GUILayout.Height(25)))
            {
                myItemUISetup.ToggleDummyDataButton();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.HelpBox("Dummy data helps you preview the UI layout without needing to play the game.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Dummy data controls are only available in Editor mode.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif 