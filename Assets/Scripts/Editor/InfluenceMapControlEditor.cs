using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InfluenceMapControl))]
public class InfluenceMapControlEditor : Editor
{
    bool showSaveLoadOptions = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InfluenceMapControl influenceMapControl = (InfluenceMapControl)target;

        showSaveLoadOptions = EditorGUILayout.Foldout(showSaveLoadOptions, "Save/Load Influence Map");
        if(showSaveLoadOptions)
        {
            if (GUILayout.Button("Save Influence Map"))
            {
                SaveLoadArray sla = new SaveLoadArray();
                sla.SaveLastInfluenceMap("./Assets/InfluenceMapData/" + influenceMapControl.m_InfluenceMapFileName, influenceMapControl.GetLastInfluenceMap());
            }
        }
    }

    private void OnSceneGUI()
    {
        if(Application.isPlaying)
        {
            InfluenceMapControl influenceMapControl = (InfluenceMapControl)target;

            for (int x = 0; x < influenceMapControl.GetInfluenceMap().GetLength(0); x++)
            {
                for (int y = 0; y < influenceMapControl.GetInfluenceMap().GetLength(1); y++)
                {
                    Handles.Label(influenceMapControl.transform.position + new Vector3(x, 0.0f, y), influenceMapControl.GetLastInfluence(x, y).ToString());
                }
            }
        }
    }
}
