using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverSpot : MonoBehaviour
{
    public enum CoverType { Top, Side_Left, Side_Right};
    public CoverType m_CoverType;

    bool m_Occupied = false;
    Transform m_Occupier;

    Transform m_Cover;

    private void Start()
    {
        m_Cover = transform.parent;
    }

    private void OnDrawGizmos()
    {
        if (m_Occupied)
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

    public bool IsCoveredFromEnemy(Vector3 _enemyPosition)
    {
        Vector3 enemyDirection = _enemyPosition - transform.position;
        Vector3 coverDirection = m_Cover.position - transform.position;

        if (Vector3.Dot(coverDirection, enemyDirection) > 0.95f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsCoveredFromEnemies(Vector3[] _enemyPositions)
    {
        bool coveredFromAllEnemies = true;
        foreach(Vector3 enemyPosition in _enemyPositions)
        {
            Vector3 enemyDirection = enemyPosition - transform.position;
            Vector3 coverDirection = m_Cover.position - transform.position;

            if (Vector3.Dot(coverDirection, enemyDirection) <= 0.95f)
            {
                coveredFromAllEnemies = false;
            }
        }

        return coveredFromAllEnemies;
    }

    public bool IsBehindEnemyPosition(Vector3 _soldierPosition, Vector3 _enemyPosition)
    {
        Vector3 soldierToTargetDirection = _enemyPosition - _soldierPosition;
        Vector3 soldierToCoverDirection = transform.position - _soldierPosition;

        float soldierToTargetDistance = Vector3.Distance(_soldierPosition, _enemyPosition);
        float soldierToCoverDistance = Vector3.Distance(_soldierPosition, transform.position);

        if ((soldierToCoverDistance + 1.0f) < soldierToTargetDistance)
        {
            // cover is not behin target
            return false;
        }
        if (Vector3.Dot(soldierToTargetDirection, soldierToCoverDirection) < 0.7F)
        {
            return false;
        }

        return true;
    }
}
