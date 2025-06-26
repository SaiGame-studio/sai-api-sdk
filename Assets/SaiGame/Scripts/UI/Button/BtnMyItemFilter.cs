using UnityEngine;

public class BtnMyItemFilter : ButttonAbstract
{
    public ItemType? filterType;

    public override void OnClick()
    {
        // Lấy tên object và bỏ tiền tố "TypeBtn_", sau đó chuyển thành ItemType nếu có thể
        string objectName = gameObject.name;
        string typeName = objectName;
        const string prefix = "TypeBtn_";
        if (objectName.StartsWith(prefix))
        {
            typeName = objectName.Substring(prefix.Length);
        }

        var uiSetup = FindObjectOfType<MyItemUISetup>();
        if (uiSetup != null)
        {
            uiSetup.OnFilterButtonClicked(typeName);
        }
        else
        {
            Debug.LogWarning("[BtnMyItemFilter] MyItemUISetup not found!");
        }
    }
}
