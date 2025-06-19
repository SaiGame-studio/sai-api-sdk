using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ButtonItemController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown itemProfileDropdown;
    [SerializeField] private TextMeshProUGUI itemInfoText;

    [Header("Data")]
    private List<ItemProfileSimple> availableItemProfiles = new List<ItemProfileSimple>();
    private List<InventoryItem> playerInventoryItems = new List<InventoryItem>();
    private ItemProfileSimple selectedItemProfile;

    private void Awake()
    {
        // Tự động tìm các UI components
        itemProfileDropdown = GetComponentInChildren<TMP_Dropdown>();
        itemInfoText = transform.Find("ItemInfo")?.GetComponent<TextMeshProUGUI>();

        if (itemProfileDropdown == null)
        {
            Debug.LogError("[ButtonItemController] TMP_Dropdown not found!");
        }

        if (itemInfoText == null)
        {
            Debug.LogError("[ButtonItemController] ItemInfo TextMeshProUGUI not found!");
        }
    }

    private void Start()
    {
        SetupDropdown();
    }

    public void Initialize(List<ItemProfileSimple> itemProfiles, List<InventoryItem> inventoryItems)
    {
        availableItemProfiles = new List<ItemProfileSimple>(itemProfiles);
        playerInventoryItems = new List<InventoryItem>(inventoryItems);
        
        PopulateDropdown();
        UpdateItemInfo();
    }

    private void SetupDropdown()
    {
        if (itemProfileDropdown != null)
        {
            itemProfileDropdown.onValueChanged.RemoveAllListeners();
            itemProfileDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
    }

    private void PopulateDropdown()
    {
        if (itemProfileDropdown == null) return;

        itemProfileDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add("Select Item Profile"); // Default option

        foreach (ItemProfileSimple profile in availableItemProfiles)
        {
            string optionText = $"{profile.name}";
            if (!string.IsNullOrEmpty(profile.code_name))
            {
                optionText += $" ({profile.code_name})";
            }
            options.Add(optionText);
        }

        itemProfileDropdown.AddOptions(options);
        itemProfileDropdown.value = 0; // Set to default
        itemProfileDropdown.RefreshShownValue();
    }

    private void OnDropdownValueChanged(int index)
    {
        if (index == 0)
        {
            // Default option selected
            selectedItemProfile = null;
        }
        else if (index - 1 < availableItemProfiles.Count)
        {
            // Valid ItemProfile selected
            selectedItemProfile = availableItemProfiles[index - 1];
        }

        UpdateItemInfo();
    }

    private void UpdateItemInfo()
    {
        if (itemInfoText == null) return;

        if (selectedItemProfile == null)
        {
            itemInfoText.text = "No item selected";
            itemInfoText.color = Color.gray;
            return;
        }

        // Tính tổng số lượng item mà player đang sở hữu
        int totalQuantity = CalculateTotalItemQuantity(selectedItemProfile.id);
        
        string itemName = selectedItemProfile.name;
        
        if (totalQuantity > 0)
        {
            itemInfoText.text = $"{itemName}\nOwned: {totalQuantity}";
            itemInfoText.color = Color.green;
        }
        else
        {
            itemInfoText.text = $"{itemName}\nOwned: 0";
            itemInfoText.color = Color.yellow;
        }
    }

    private int CalculateTotalItemQuantity(string itemProfileId)
    {
        int totalQuantity = 0;

        // Tìm tất cả InventoryItem có item_profile_id trùng với itemProfileId
        var matchingItems = playerInventoryItems.Where(item => item.item_profile_id == itemProfileId);

        foreach (InventoryItem item in matchingItems)
        {
            // Cộng dồn số lượng từ tất cả instance
            totalQuantity += item.amount;
        }

        return totalQuantity;
    }

    public ItemProfileSimple GetSelectedItemProfile()
    {
        return selectedItemProfile;
    }

    public int GetSelectedItemQuantity()
    {
        if (selectedItemProfile == null) return 0;
        return CalculateTotalItemQuantity(selectedItemProfile.id);
    }

    // Method để refresh data khi cần thiết
    public void RefreshData(List<ItemProfileSimple> itemProfiles, List<InventoryItem> inventoryItems)
    {
        Initialize(itemProfiles, inventoryItems);
    }

    [ContextMenu("Debug Selected Item")]
    public void DebugSelectedItem()
    {
        if (selectedItemProfile != null)
        {
            int quantity = GetSelectedItemQuantity();
            Debug.Log($"[ButtonItemController] Selected: {selectedItemProfile.name} | Quantity: {quantity}");
            
            // Debug all matching inventory items
            var matchingItems = playerInventoryItems.Where(item => item.item_profile_id == selectedItemProfile.id);
            Debug.Log($"[ButtonItemController] Found {matchingItems.Count()} inventory instances:");
            
            foreach (InventoryItem item in matchingItems)
            {
                Debug.Log($"  - Instance ID: {item.id} | Amount: {item.amount}");
            }
        }
        else
        {
            Debug.Log("[ButtonItemController] No item selected");
        }
    }
} 