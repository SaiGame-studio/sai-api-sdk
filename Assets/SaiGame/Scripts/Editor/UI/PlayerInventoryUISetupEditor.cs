#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

[CustomEditor(typeof(PlayerInventoryUISetup))]
public class PlayerInventoryUISetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all properties except the ones we want to move
        DrawPropertiesExcluding(serializedObject, "showDummyData", "dummyInventoryCount", "dummyItemCount");
        
        PlayerInventoryUISetup inventoryUISetup = (PlayerInventoryUISetup)target;
        
        GUILayout.Space(10);
        
        // UI Management Section
        EditorGUILayout.LabelField("UI Management", EditorStyles.boldLabel);
        
        // Create và Delete buttons với màu sắc phù hợp
        using (new EditorGUILayout.HorizontalScope())
        {
            // Create Inventory UI button - Màu xanh lá
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Xanh lá đẹp
            if (GUILayout.Button("Create My Item UI", GUILayout.Height(30)))
            {
                inventoryUISetup.CreateInventoryUI();
            }
            GUI.backgroundColor = Color.white;
            
            // Delete Inventory UI button - Màu đỏ
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ đẹp
            if (GUILayout.Button("Delete My Item UI", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Delete My Item UI", 
                    "Are you sure you want to delete the My Item UI? This will remove the Canvas and all UI elements.", 
                    "Yes, Delete", "Cancel"))
                {
                    inventoryUISetup.DeleteInventoryUI();
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

            // Show PlayerInventoryManager status
            if (PlayerInventoryManager.Instance != null)
            {
                EditorGUILayout.LabelField("PlayerInventoryManager Status:", "✓ PlayerInventoryManager instance is ready");
            }
            else
            {
                EditorGUILayout.HelpBox("PlayerInventoryManager instance not found!", MessageType.Warning);
            }
            
            GUILayout.Space(10);
            
            // Quick Actions Section
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // Refresh Inventory button - Màu xanh dương
                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f); // Xanh dương
                if (GUILayout.Button("Refresh Inventory", GUILayout.Height(25)))
                {
                    inventoryUISetup.OnRefreshClick();
                }
                GUI.backgroundColor = Color.white;
                
                // Clear Status button - Màu vàng
                GUI.backgroundColor = new Color(1f, 0.8f, 0.2f, 1f); // Vàng đẹp
                if (GUILayout.Button("Clear Status", GUILayout.Height(25)))
                {
                    inventoryUISetup.ClearStatus();
                }
                GUI.backgroundColor = Color.white;
                
                // Show Loading button - Màu xanh lá nhạt
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 1f); // Xanh lá nhạt
                if (GUILayout.Button("Show Loading", GUILayout.Height(25)))
                {
                    inventoryUISetup.ShowLoadingTest();
                }
                GUI.backgroundColor = Color.white;
                
                // Hide Loading button - Màu xám
                GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Xám
                if (GUILayout.Button("Hide Loading", GUILayout.Height(25)))
                {
                    inventoryUISetup.HideLoadingTest();
                }
                GUI.backgroundColor = Color.white;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                // Use Item button - Màu tím
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.8f, 1f); // Tím
                if (GUILayout.Button("Test Use Item", GUILayout.Height(25)))
                {
                    inventoryUISetup.OnUseItemClick();
                }
                GUI.backgroundColor = Color.white;
                
                // Back to Main Menu button - Màu tím
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.8f, 1f); // Tím
                if (GUILayout.Button("Back to Main Menu", GUILayout.Height(25)))
                {
                    inventoryUISetup.OnBackToMainMenuClick();
                }
                GUI.backgroundColor = Color.white;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Test features are only available when playing.", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        // UI Status Section
        EditorGUILayout.LabelField("UI Status", EditorStyles.boldLabel);
        
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            EditorGUILayout.LabelField("Canvas Status:", "✓ My Item UI is present");
            EditorGUILayout.LabelField("Canvas Name:", existingCanvas.name);
        }
        else
        {
            EditorGUILayout.LabelField("Canvas Status:", "✗ No My Item UI found");
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

        // Inventory Items Status
        if (inventoryUISetup.inventoryItemContainer != null)
        {
            int itemCount = inventoryUISetup.inventoryItemContainer.childCount;
            EditorGUILayout.LabelField("Inventory Items Count:", itemCount.ToString());
        }

        // Inventory Selection Status
        if (inventoryUISetup.inventorySelectionContainer != null)
        {
            int inventoryCount = inventoryUISetup.inventorySelectionContainer.childCount;
            EditorGUILayout.LabelField("Inventory Selection Count:", inventoryCount.ToString());
        }

        // Selected Inventory Status
        if (Application.isPlaying)
        {
            var selectedInv = inventoryUISetup.GetSelectedInventory();
            if (selectedInv != null)
            {
                EditorGUILayout.LabelField("Selected Inventory:", selectedInv.name);
            }
            else
            {
                EditorGUILayout.LabelField("Selected Inventory:", "None");
            }
        }

        GUILayout.Space(10);
        
        // Dummy Data Section
        EditorGUILayout.LabelField("Dummy Data (Editor Only)", EditorStyles.boldLabel);
        
        if (Application.isEditor)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showDummyData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dummyInventoryCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dummyItemCount"));

            using (new EditorGUILayout.HorizontalScope())
            {
                // Show Dummy Data button - Màu xanh lá
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f);
                if (GUILayout.Button("Show Dummy Data", GUILayout.Height(25)))
                {
                    inventoryUISetup.ShowDummyDataButton();
                }
                GUI.backgroundColor = Color.white;

                // Delete Dummy Data button - Màu đỏ
                GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f);
                if (GUILayout.Button("Delete Dummy Data", GUILayout.Height(25)))
                {
                    inventoryUISetup.DeleteDummyDataButton();
                }
                GUI.backgroundColor = Color.white;
            }

            // Toggle Dummy Data button - Màu cam
            GUI.backgroundColor = new Color(1f, 0.6f, 0.2f, 1f);
            if (GUILayout.Button("Toggle Dummy Data", GUILayout.Height(25)))
            {
                inventoryUISetup.ToggleDummyDataButton();
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