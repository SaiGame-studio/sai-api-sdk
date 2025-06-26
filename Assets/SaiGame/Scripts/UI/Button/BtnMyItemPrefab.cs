using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BtnMyItemPrefab : MonoBehaviour
{
    public InventoryItem itemData;
    private Button button;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI amountText;
    private MyItemUISetup uiSetup;

    void Awake()
    {
        button = GetComponent<Button>();
        nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        amountText = transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();
        uiSetup = FindObjectOfType<MyItemUISetup>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void SetData(InventoryItem item)
    {
        itemData = item;
        if (nameText != null) nameText.text = item.name;
        if (amountText != null) amountText.text = $"Amount: {item.amount}";
    }

    private void OnClick()
    {
        if (uiSetup != null && itemData != null)
        {
            uiSetup.OnItemSelected(itemData);
        }
    }
}
