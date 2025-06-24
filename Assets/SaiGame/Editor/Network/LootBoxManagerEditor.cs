#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LootBoxManager))]
public class LootBoxManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Open LootBox", EditorStyles.boldLabel);
        LootBoxManager manager = (LootBoxManager)target;
        GUI.enabled = !string.IsNullOrEmpty(manager.lootBoxId);
        if (GUILayout.Button("Open LootBox", GUILayout.Height(30)))
        {
            manager.OpenLootBoxFromInspector();
        }
        GUI.enabled = true;
    }
}
#endif
