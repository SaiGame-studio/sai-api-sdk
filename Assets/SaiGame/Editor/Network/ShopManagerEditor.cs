using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShopManager))]
public class ShopManagerEditor : Editor
{
    private int selectedShopIndex = -1;
    private int number = 1;
    private Vector2 itemProfilesScrollPosition;
    private string lastSelectedShopId = null;

    public override void OnInspectorGUI()
    {
        ShopManager shopManager = (ShopManager)target;

        // Draw default inspector up to selectedShopIdForEditor
        DrawDefaultInspector();
        
        // Move BuyItem section up right after selectedShopIdForEditor
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Buy Item", EditorStyles.boldLabel);

        // BuyItem fields moved up - use itemProfileIdForEditor from ShopManager
        shopManager.itemProfileIdForEditor = EditorGUILayout.TextField("Item Profile ID", shopManager.itemProfileIdForEditor);
        number = EditorGUILayout.IntField("Number", number);

        if (GUILayout.Button("Buy Item"))
        {
            if (string.IsNullOrEmpty(shopManager.itemProfileIdForEditor))
            {
                EditorUtility.DisplayDialog("Error", "Please enter an Item Profile ID", "OK");
                return;
            }
            if (number <= 0)
            {
                EditorUtility.DisplayDialog("Error", "Number must be greater than 0", "OK");
                return;
            }
            if (string.IsNullOrEmpty(shopManager.selectedShopIdForEditor))
            {
                EditorUtility.DisplayDialog("Error", "Please select a shop first", "OK");
                return;
            }
            shopManager.BuyItem(shopManager.selectedShopIdForEditor, shopManager.itemProfileIdForEditor, number);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Shop Management", EditorStyles.boldLabel);

        // Đồng bộ dropdown nếu selectedShopIdForEditor thay đổi
        if (!string.IsNullOrEmpty(shopManager.selectedShopIdForEditor) && shopManager.selectedShopIdForEditor != lastSelectedShopId)
        {
            int idx = shopManager.ShopList.FindIndex(s => s.id == shopManager.selectedShopIdForEditor);
            if (idx >= 0)
            {
                selectedShopIndex = idx;
                lastSelectedShopId = shopManager.selectedShopIdForEditor;
                shopManager.GetShopItems(shopManager.ShopList[selectedShopIndex].id);
            }
        }

        if (GUILayout.Button("Refresh Shops"))
        {
            shopManager.GetShopList();
        }

        // Shop Dropdown
        if (shopManager.ShopList.Count > 0)
        {
            string[] shopNames = new string[shopManager.ShopList.Count];
            for (int i = 0; i < shopManager.ShopList.Count; i++)
            {
                shopNames[i] = shopManager.ShopList[i].name;
            }

            int newSelectedIndex = EditorGUILayout.Popup("Select Shop", selectedShopIndex, shopNames);
            if (newSelectedIndex != selectedShopIndex)
            {
                selectedShopIndex = newSelectedIndex;
                if (selectedShopIndex >= 0)
                {
                    shopManager.GetShopItems(shopManager.ShopList[selectedShopIndex].id);
                    shopManager.selectedShopIdForEditor = shopManager.ShopList[selectedShopIndex].id;
                    lastSelectedShopId = shopManager.selectedShopIdForEditor;
                }
            }

            // Hiển thị danh sách item của shop
            EditorGUILayout.LabelField("Current Shop Items", EditorStyles.boldLabel);
            if (shopManager.CurrentShopItems != null && shopManager.CurrentShopItems.Count > 0)
            {
                itemProfilesScrollPosition = EditorGUILayout.BeginScrollView(itemProfilesScrollPosition, GUILayout.Height(100));
                foreach (var item in shopManager.CurrentShopItems)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Copy ID", GUILayout.Width(70)))
                    {
                        shopManager.itemProfileIdForEditor = item.item_profile.id;
                        GUI.FocusControl(null); // Để cập nhật ngay vào field
                    }
                    EditorGUILayout.LabelField($"Name: {item.item_profile.name}", GUILayout.Width(250));
                    EditorGUILayout.LabelField($"Price: {item.price_current}", GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("No Item", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No shops available. Click 'Refresh Shops' to fetch shop data.", MessageType.Info);
        }
    }
} 