using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShipManager))]
public class ShipManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if(GUILayout.Button("Set Ship in Control"))
        {
            ShipManager.SetControl(true);
        }
        
        if(GUILayout.Button("Set Player in Control"))
        {
            ShipManager.SetControl(false);
        }
    }
}
