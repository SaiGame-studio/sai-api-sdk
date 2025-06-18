using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemProfileManager))]
public class ItemProfileManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ItemProfileManager manager = (ItemProfileManager)target;
        if (GUILayout.Button("Refresh Items"))
        {
            manager.FetchItemProfiles();
        }
    }
} 