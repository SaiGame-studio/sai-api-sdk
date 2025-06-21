using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerInventoryManager))]
public class PlayerInventoryManagerEditor : Editor
{
    private int selectedInventoryIndex = -1;
    private Vector2 inventoryItemsScrollPosition;
    private string lastSelectedItemId = null;

    public override void OnInspectorGUI()
    {
        PlayerInventoryManager inventoryManager = (PlayerInventoryManager)target;

        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Inventory Management", EditorStyles.boldLabel);

        // Đồng bộ dropdown nếu selectedItemIdForEditor thay đổi
        if (!string.IsNullOrEmpty(inventoryManager.selectedItemIdForEditor) && inventoryManager.selectedItemIdForEditor != lastSelectedItemId)
        {
            int idx = inventoryManager.FilteredInventoryItems.FindIndex(item => item.id == inventoryManager.selectedItemIdForEditor);
            if (idx >= 0)
            {
                selectedInventoryIndex = idx;
                lastSelectedItemId = inventoryManager.selectedItemIdForEditor;
            }
        }

        if (GUILayout.Button("Refresh Player Items"))
        {
            if (PlayerItemManager.Instance != null)
            {
                PlayerItemManager.Instance.GetPlayerItems(null);
            }
            else
            {
                Debug.LogWarning("PlayerItemManager instance not found!");
            }
        }

        if (GUILayout.Button("Refresh Inventory"))
        {
            inventoryManager.RefreshInventory();
        }

        // Inventory Dropdown
        if (inventoryManager.FilteredInventoryItems.Count > 0)
        {
            string[] itemNames = new string[inventoryManager.FilteredInventoryItems.Count];
            for (int i = 0; i < inventoryManager.FilteredInventoryItems.Count; i++)
            {
                var item = inventoryManager.FilteredInventoryItems[i];
                itemNames[i] = $"{item.name} (x{item.amount})";
            }

            int newSelectedIndex = EditorGUILayout.Popup("Select Inventory", selectedInventoryIndex, itemNames);
            if (newSelectedIndex != selectedInventoryIndex)
            {
                selectedInventoryIndex = newSelectedIndex;
                if (selectedInventoryIndex >= 0)
                {
                    inventoryManager.selectedItemIdForEditor = inventoryManager.FilteredInventoryItems[selectedInventoryIndex].id;
                    inventoryManager.SelectItemById(inventoryManager.selectedItemIdForEditor);
                    lastSelectedItemId = inventoryManager.selectedItemIdForEditor;
                }
            }

            // Hiển thị danh sách inventory items
            EditorGUILayout.LabelField("Current Inventory Items", EditorStyles.boldLabel);
            if (inventoryManager.FilteredInventoryItems != null && inventoryManager.FilteredInventoryItems.Count > 0)
            {
                // Thông tin tổng quan
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Total Unique Items: {inventoryManager.GetUniqueItemCount()}", GUILayout.Width(200));
                EditorGUILayout.LabelField($"Total Items Count: {inventoryManager.GetTotalItemCount()}", GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                inventoryItemsScrollPosition = EditorGUILayout.BeginScrollView(inventoryItemsScrollPosition, GUILayout.Height(150));
                foreach (var item in inventoryManager.FilteredInventoryItems)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // Button để select item
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        inventoryManager.selectedItemIdForEditor = item.id;
                        inventoryManager.SelectItemById(item.id);
                        selectedInventoryIndex = inventoryManager.FilteredInventoryItems.FindIndex(i => i.id == item.id);
                        lastSelectedItemId = item.id;
                        GUI.FocusControl(null);
                    }
                    
                    // Hiển thị thông tin item
                    EditorGUILayout.LabelField($"Name: {item.name}", GUILayout.Width(200));
                    EditorGUILayout.LabelField($"Amount: {item.amount}", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Type: {item.type}", GUILayout.Width(100));
                    
                    // Hiển thị item selected
                    if (item.id == inventoryManager.selectedItemIdForEditor)
                    {
                        EditorGUILayout.LabelField("← Selected", GUILayout.Width(80));
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                // Hiển thị thông tin chi tiết của item được chọn
                if (selectedInventoryIndex >= 0 && selectedInventoryIndex < inventoryManager.FilteredInventoryItems.Count)
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Selected Item Details", EditorStyles.boldLabel);
                    var selectedItem = inventoryManager.FilteredInventoryItems[selectedInventoryIndex];
                    
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"ID: {selectedItem.id}");
                    EditorGUILayout.LabelField($"Name: {selectedItem.name}");
                    EditorGUILayout.LabelField($"Description: {selectedItem.description}");
                    EditorGUILayout.LabelField($"Type: {selectedItem.type}");
                    EditorGUILayout.LabelField($"Amount: {selectedItem.amount}");
                    EditorGUILayout.LabelField($"Stack Limit: {selectedItem.stack_limit}");
                    EditorGUILayout.LabelField($"Level Max: {selectedItem.level_max}");
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No Inventory Items", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No inventory items available. Click 'Refresh Inventory' or make sure PlayerItemManager has items with type 'Inventory'.", MessageType.Info);
        }
    }
} 