using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SoldierSpawner))]
public class SpawnEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SoldierSpawner soldierSpawner = (SoldierSpawner)target;

        if (GUILayout.Button("Spawn Soldier"))
        {
            soldierSpawner.SpawnUnit();
        }
    }
}
