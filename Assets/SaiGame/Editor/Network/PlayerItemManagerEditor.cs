using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerItemManager))]
public class PlayerItemManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PlayerItemManager manager = (PlayerItemManager)target;
        if (GUILayout.Button("Refresh Items"))
        {
            manager.GetPlayerItems(null);
        }
    }
} 