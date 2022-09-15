using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AssemblySpot : MonoBehaviour
{
    int m_StartX;
    int m_StartZ;
    int m_ExtentX;
    int m_ExtentZ;

    private int counter;

    List<Vector2Int> targetPositions;
    BoxCollider area;

    public void Start()
    {
        area = GetComponent<BoxCollider>();

        m_StartX = Mathf.RoundToInt(area.transform.position.x - area.bounds.extents.x - InfluenceMapControl.Instance.transform.position.x);
        m_StartZ = Mathf.RoundToInt(area.transform.position.z - area.bounds.extents.z - InfluenceMapControl.Instance.transform.position.z);
        m_ExtentX = Mathf.RoundToInt(area.bounds.extents.x * 2.0f);
        m_ExtentZ = Mathf.RoundToInt(area.bounds.extents.z * 2.0f);

        counter = 0;

        targetPositions = new List<Vector2Int>();

        for (int x = m_StartX; x < m_StartX + m_ExtentX; x++)
        {
            for (int z = m_StartZ; z < m_StartZ + m_ExtentZ; z++)
            {
                targetPositions.Add(new Vector2Int(x, z));
            }
        }

        for (int i = 0; i < targetPositions.Count; i++)
        {
            Vector2Int temp = targetPositions[i];
            int randIndex = Random.Range(i, targetPositions.Count);
            targetPositions[i] = targetPositions[randIndex];
            targetPositions[randIndex] = temp;
        }
    }

    public Vector2Int GetRandomTargetPosition()
    {
        if(counter < targetPositions.Count)
        {
            return targetPositions[counter++];
        }

        Debug.LogWarning("No more TargetPositions available.");
        return Vector2Int.zero;
    }

    public int GetMaxTargetPositions()
    {
        return targetPositions.Count;
    }

    public bool IsAtTarget(Soldier _soldier)
    {
        if (area.bounds.Contains(_soldier.transform.position))
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(158f / 255f, 66f / 255f, 245f / 255f);
        BoxCollider bc = GetComponent<BoxCollider>();
        Gizmos.DrawWireCube(transform.position, new Vector3(bc.size.x, bc.size.z, bc.size.y));
    }
}
