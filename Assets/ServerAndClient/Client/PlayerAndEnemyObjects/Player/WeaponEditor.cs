#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Player_Weapon))]
public class WeaponEditor : Editor {

    public override void OnInspectorGUI()
    {
        Player_Weapon pw = (Player_Weapon)target;
        base.OnInspectorGUI();

        if(GUILayout.Button("Set position in model", GUILayout.Width(150), GUILayout.Height(20)))
        {
            pw.SetModelPoint();
        }
    }
}
#endif
