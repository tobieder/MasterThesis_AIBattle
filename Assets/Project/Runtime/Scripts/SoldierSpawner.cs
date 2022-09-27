using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class SoldierSpawner : MonoBehaviour
{
    [SerializeField]
    int m_TeamIndex;

    [SerializeField]
    GameObject m_UnitPrefab;

    [SerializeField]
    Transform m_TeamParent;

    [SerializeField]
    GroupAI m_GroupAI;

    [SerializeField]
    private Canvas m_HealthBarCanvas;

    private int m_PositionCounter;
    private List<Vector3> m_SpawnPositions;

    private void Awake()
    {
        BoxCollider area = GetComponent<BoxCollider>();

        int m_StartX = Mathf.RoundToInt(area.transform.position.x - area.bounds.extents.x);
        int m_StartZ = Mathf.RoundToInt(area.transform.position.z - area.bounds.extents.z);
        int m_ExtentX = Mathf.RoundToInt(area.bounds.extents.x * 2.0f);
        int m_ExtentZ = Mathf.RoundToInt(area.bounds.extents.z * 2.0f);

        m_PositionCounter = 0;

        m_SpawnPositions = new List<Vector3>();

        for (int x = m_StartX; x < m_StartX + m_ExtentX; x++)
        {
            for (int z = m_StartZ; z < m_StartZ + m_ExtentZ; z++)
            {
                m_SpawnPositions.Add(new Vector3(x, 0.0f, z));
            }
        }

        for (int i = 0; i < m_SpawnPositions.Count; i++)
        {
            Vector3 temp = m_SpawnPositions[i];
            int randIndex = UnityEngine.Random.Range(i, m_SpawnPositions.Count);
            m_SpawnPositions[i] = m_SpawnPositions[randIndex];
            m_SpawnPositions[randIndex] = temp;
        }
    }


    public void SpawnUnit()
    {
        SpawnUnit(m_SpawnPositions[m_PositionCounter++]);
        m_PositionCounter %= m_SpawnPositions.Count;
    }

    public void SpawnUnit(Vector3 _position)
    {
        GameObject soldier = Instantiate(m_UnitPrefab, _position, UnityEngine.Random.rotation, transform);

        soldier.name = "Soldier_" + m_GroupAI.m_Team.Count + "_Team_" + m_TeamIndex;

        soldier.GetComponent<SoldierController>().SetGroupAI(m_GroupAI);
        soldier.GetComponent<Vitals>().SetupHealthBar(m_HealthBarCanvas);
    }
}
