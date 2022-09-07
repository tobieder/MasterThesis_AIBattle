using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardSpot : MonoBehaviour
{
    bool m_Occupied = false;
    Transform m_Occupier;


    private void OnDrawGizmos()
    {
        if(m_Occupied)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }

        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    public void SetOccupier(Transform _occupier)
    {
        m_Occupier = _occupier;

        if (m_Occupier == null)
        {
            m_Occupied = false;
        }
        else
        {
            m_Occupied = true;
        }
    }

    public Transform GetOccupier()
    {
        return m_Occupier;
    }

    public bool IsOccupied()
    {
        return m_Occupied;
    }
}
