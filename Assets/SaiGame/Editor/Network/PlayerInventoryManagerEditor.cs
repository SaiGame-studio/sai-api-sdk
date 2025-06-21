using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerInventoryManager))]
public class PlayerInventoryManagerEditor : Editor
{
    private int selectedInventoryIndex = -1;
    private Vector2 inventoryItemsScrollPosition;
    private Vector2 inventoryContentsScrollPosition;
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

            // Hiển thị danh sách inventory items (containers)
            EditorGUILayout.LabelField("Available Inventories", EditorStyles.boldLabel);
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

                // Hiển thị thông tin chi tiết của inventory được chọn
                if (selectedInventoryIndex >= 0 && selectedInventoryIndex < inventoryManager.FilteredInventoryItems.Count)
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Selected Inventory Details", EditorStyles.boldLabel);
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

            // Hiển thị nội dung bên trong inventory được chọn
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Items Inside Selected Inventory", EditorStyles.boldLabel);

            if (inventoryManager.IsLoadingInventoryItems())
            {
                EditorGUILayout.HelpBox("Loading inventory items...", MessageType.Info);
            }
            else if (!string.IsNullOrEmpty(inventoryManager.GetSelectedInventoryId()))
            {
                // Hiển thị thông tin inventory được chọn
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Selected Inventory: {inventoryManager.GetSelectedInventoryName()}", GUILayout.Width(300));
                EditorGUILayout.LabelField($"ID: {inventoryManager.GetSelectedInventoryId()}", GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();

                // Button để refresh inventory items
                if (GUILayout.Button("Refresh Inventory Items"))
                {
                    inventoryManager.LoadInventoryItems(inventoryManager.GetSelectedInventoryId());
                }

                // Hiển thị danh sách items trong inventory
                if (inventoryManager.InventoryItems != null && inventoryManager.InventoryItems.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField($"Total Items in Inventory: {inventoryManager.InventoryItems.Count}", EditorStyles.miniLabel);

                    inventoryContentsScrollPosition = EditorGUILayout.BeginScrollView(inventoryContentsScrollPosition, GUILayout.Height(200));
                    
                    foreach (var item in inventoryManager.InventoryItems)
                    {
                        EditorGUILayout.BeginVertical("box");
                        
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Name: {item.name}", GUILayout.Width(200));
                        EditorGUILayout.LabelField($"Amount: {item.amount}", GUILayout.Width(100));
                        EditorGUILayout.LabelField($"Type: {item.type}", GUILayout.Width(100));
                        EditorGUILayout.EndHorizontal();

                        if (!string.IsNullOrEmpty(item.description))
                        {
                            EditorGUILayout.LabelField($"Description: {item.description}", EditorStyles.wordWrappedMiniLabel);
                        }

                        // Hiển thị custom_data nếu có
                        if (item.custom_data != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"HP: {item.custom_data.hp_current}/{item.custom_data.hp_max}", GUILayout.Width(150));
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Stack Limit: {item.stack_limit}", GUILayout.Width(120));
                        EditorGUILayout.LabelField($"Level Max: {item.level_max}", GUILayout.Width(120));
                        EditorGUILayout.LabelField($"Stackable: {(item.stackable == 1 ? "Yes" : "No")}", GUILayout.Width(100));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.LabelField($"ID: {item.id}", EditorStyles.miniLabel);
                        
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(2);
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    EditorGUILayout.HelpBox("No items found in this inventory or inventory is empty.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select an inventory to view its contents.", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No inventory items available. Click 'Refresh Inventory' or make sure PlayerItemManager has items with type 'Inventory'.", MessageType.Info);
        }
    }
} 