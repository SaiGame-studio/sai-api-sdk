using UnityEngine;

public class BtnOpenLootBox : ButttonAbstract
{

    public override void OnClick()
    {
        string lootBoxId = PlayerItemManager.Instance != null ? PlayerItemManager.Instance.chooseItem : null;
        Debug.Log("[BtnOpenLootBox] Đang mở loot box...");
        LootBoxManager.Instance.OpenLootBoxFromUIOrEditor(
            lootBoxId, 1,
            (result) => {
                Debug.Log($"[BtnOpenLootBox] Loot box opened!\n{result}");
            },
            (error) => {
                Debug.LogWarning($"[BtnOpenLootBox] Lỗi mở loot box: {error}");
            }
        );
    }
}
