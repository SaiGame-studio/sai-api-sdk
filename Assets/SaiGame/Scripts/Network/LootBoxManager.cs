using System.Collections;
using System;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;

public class LootBoxManager : SaiSingleton<LootBoxManager>
{
    [Header("LootBox Settings")]
    public string lootBoxId = "";
    public int quantity = 1;

    private void OnEnable()
    {
        SyncLootBoxId();
    }

    private void FixedUpdate()
    {
        SyncLootBoxId();
    }

    private void SyncLootBoxId()
    {
        if (PlayerItemManager.Instance != null)
        {
            if (lootBoxId != PlayerItemManager.Instance.chooseItem)
            {
                lootBoxId = PlayerItemManager.Instance.chooseItem;
            }
        }
    }

    public void OpenLootBox(string lootBoxId, int quantity, Action<string> onSuccess, Action<string> onError)
    {
        Debug.Log($"[LootBoxManager] OpenLootBox: lootBoxId={lootBoxId}, quantity={quantity}");
        if (string.IsNullOrEmpty(lootBoxId))
        {
            Debug.LogError("[LootBoxManager] LootBoxId is null or empty");
            onError?.Invoke("LootBoxId is null or empty");
            return;
        }
        string endpoint = $"/loot-box/{lootBoxId}/items";
        var body = new LootBoxOpenRequest { quantity = quantity };
        Debug.Log($"[LootBoxManager] Endpoint: {endpoint}");
        Debug.Log($"[LootBoxManager] Body: {JsonUtility.ToJson(body)}");
        APIManager.Instance.StartCoroutine(APIManager.Instance.PostRequest<LootBoxOpenResponse>(endpoint, body, (response) => {
            if (response != null) onSuccess?.Invoke(JsonUtility.ToJson(response));
            else onError?.Invoke("Response is null");
        }));
    }

    // Thay vì dùng lootBoxId nội bộ, truyền trực tiếp id đang chọn từ UI
    public void OpenLootBoxFromUIOrEditor(string lootBoxId, int quantity, Action<string> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(lootBoxId))
        {
            onError?.Invoke("Bạn chưa chọn item để mở loot box!");
            return;
        }
        OpenLootBox(lootBoxId, quantity,
            (result) => onSuccess?.Invoke(result),
            (err) => onError?.Invoke(err));
    }

    // Sửa lại hàm OpenLootBoxFromInspector chỉ gọi hàm mới với lootBoxId hiện tại
    public void OpenLootBoxFromInspector()
    {
        OpenLootBoxFromUIOrEditor(lootBoxId, quantity,
            (result) => Debug.Log($"[LootBoxManager] Success: {result}"),
            (err) => Debug.LogWarning($"[LootBoxManager] Error: {err}"));
    }

    [Serializable]
    public class LootBoxOpenRequest { public int quantity; }
    [Serializable]
    public class LootBoxOpenResponse {
        public string status;
        public string message;
        public string message_code;
        public Data data;
        public string raw;

        [Serializable]
        public class Data {
            public string loot_box_id;
            public string loot_box_type;
            public int quantity_opened;
            public int remaining_amount;
            public Reward[] rewards;
            public AddedItem[] added_items;
        }

        [Serializable]
        public class Reward {
            public string item_profile_id;
            public ItemProfile item_profile;
            public int quantity;
        }

        [Serializable]
        public class ItemProfile {
            public string name;
            public string code_name;
            public string status;
            public string description;
            public int create_on_registry;
            public int amount_on_registry;
            public string type;
            public int level_start;
            public int level_max;
            public bool stackable;
            public int stack_limit;
            public string game_id;
            public long updated_at;
            public long created_at;
            public CustomData custom_data;
            public string inventory_profile_id;
            public string id;
        }

        [Serializable]
        public class CustomData {
            public int hp_max;
            public int hp_current;
        }

        [Serializable]
        public class AddedItem {
            public string name;
            public string description;
            public CustomData custom_data;
            public string type;
            public int create_on_registry;
            public int amount_on_registry;
            public int level_max;
            public bool stackable;
            public int stack_limit;
            public int amount;
            public string game_id;
            public string user_profile_id;
            public string item_profile_id;
            public long updated_at;
            public long created_at;
            public string inventory_item_id;
            public string id;
        }
    }
}
