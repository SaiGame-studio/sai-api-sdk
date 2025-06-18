using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShopManager))]
public class ShopManagerEditor : Editor
{
    private int selectedShopIndex = -1;
    private string itemProfileId = "";
    private int number = 1;
    private Vector2 itemProfilesScrollPosition;
    private string lastSelectedShopId = null;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ShopManager shopManager = (ShopManager)target;

        // Đồng bộ dropdown nếu selectedShopIdForEditor thay đổi
        if (!string.IsNullOrEmpty(shopManager.selectedShopIdForEditor) && shopManager.selectedShopIdForEditor != lastSelectedShopId)
        {
            int idx = shopManager.ShopList.FindIndex(s => s.id == shopManager.selectedShopIdForEditor);
            if (idx >= 0)
            {
                selectedShopIndex = idx;
                lastSelectedShopId = shopManager.selectedShopIdForEditor;
                shopManager.FetchShopItems(shopManager.ShopList[selectedShopIndex].id);
            }
        }

        if (GUILayout.Button("Refresh Shops"))
        {
            shopManager.FetchShopList();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Buy Item", EditorStyles.boldLabel);

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
                    shopManager.FetchShopItems(shopManager.ShopList[selectedShopIndex].id);
                    shopManager.selectedShopIdForEditor = shopManager.ShopList[selectedShopIndex].id;
                    lastSelectedShopId = shopManager.selectedShopIdForEditor;
                }
            }

            itemProfileId = EditorGUILayout.TextField("Item Profile ID", itemProfileId);
            number = EditorGUILayout.IntField("Number", number);

            if (GUILayout.Button("Buy Item"))
            {
                if (string.IsNullOrEmpty(itemProfileId))
                {
                    EditorUtility.DisplayDialog("Error", "Please enter an Item Profile ID", "OK");
                    return;
                }
                if (number <= 0)
                {
                    EditorUtility.DisplayDialog("Error", "Number must be greater than 0", "OK");
                    return;
                }
                if (selectedShopIndex < 0)
                {
                    EditorUtility.DisplayDialog("Error", "Please select a shop", "OK");
                    return;
                }
                shopManager.BuyItem(shopManager.ShopList[selectedShopIndex].id, itemProfileId, number);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No shops available. Click 'Refresh Shops' to fetch shop data.", MessageType.Info);
        }
    }
} 