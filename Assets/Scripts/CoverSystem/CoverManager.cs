using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverManager : MonoBehaviour
{
    private static CoverManager _instance;
    public static CoverManager Instance { get { return _instance; } }

    [SerializeField] List<CoverSpot> m_OccupiedCoverSpots = new List<CoverSpot>();
    [SerializeField] List<CoverSpot> m_UnoccupiedCoverSpots = new List<CoverSpot>();

    List<Soldier> m_soldiers = new List<Soldier>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("Influence Map Control destroyed.");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        m_UnoccupiedCoverSpots = new List<CoverSpot>(GameObject.FindObjectsOfType<CoverSpot>());

        m_soldiers = new List<Soldier>(GameObject.FindObjectsOfType<Soldier>());
    }

    void AddToOccupied(CoverSpot _coverSpot)
    {
        if (m_UnoccupiedCoverSpots.Contains(_coverSpot))
        {
            m_UnoccupiedCoverSpots.Remove(_coverSpot);
        }
        if (!m_OccupiedCoverSpots.Contains(_coverSpot))
        {
            m_OccupiedCoverSpots.Add(_coverSpot);
        }
    }

    void AddToUnoccupied(CoverSpot _coverSpot)
    {
        if (m_OccupiedCoverSpots.Contains(_coverSpot))
        {
            m_OccupiedCoverSpots.Remove(_coverSpot);
        }
        if (!m_UnoccupiedCoverSpots.Contains(_coverSpot))
        {
            m_UnoccupiedCoverSpots.Add(_coverSpot);
        }
    }

    public CoverSpot GetCoverForEnemy(Soldier _soldier, Vector3 _enemyPosition, float _maxAttackDistance, float _minAttackDistance, CoverSpot _prevCoverSpot)
    {
        CoverSpot bestCover = null;
        Vector3 soldierPoisition = _soldier.transform.position;

        CoverSpot[] possibleCoverSpots = m_UnoccupiedCoverSpots.ToArray();

        for (int i = 0; i < possibleCoverSpots.Length; i++)
        {
            CoverSpot currentCoverSpot = possibleCoverSpots[i];

            if (!currentCoverSpot.IsOccupied() && currentCoverSpot.IsCoveredFromEnemy(_enemyPosition) && Vector3.Distance(currentCoverSpot.transform.position, _enemyPosition) >= _minAttackDistance && !IsCoverPastEnemyLine(_soldier, currentCoverSpot))
            {
                if (bestCover == null)
                {
                    bestCover = currentCoverSpot;
                }
                else if (currentCoverSpot != _prevCoverSpot &&
                    Vector3.Distance(bestCover.transform.position, soldierPoisition) > Vector3.Distance(currentCoverSpot.transform.position, soldierPoisition) &&
                    Vector3.Distance(currentCoverSpot.transform.position, _enemyPosition) < Vector3.Distance(soldierPoisition, _enemyPosition))
                {
                    if (Vector3.Distance(currentCoverSpot.transform.position, soldierPoisition) < Vector3.Distance(_enemyPosition, soldierPoisition))
                    {
                        bestCover = currentCoverSpot;
                    }
                }
            }
        }

        if (bestCover != null)
        {
            bestCover.SetOccupier(_soldier.transform);
            AddToOccupied(bestCover);
        }

        return bestCover;
    }

    public CoverSpot GetCoverForClosestEnemies(Soldier _soldier, Vector3[] _enemyPositions, float _maxAttackDistance, float _minAttackDistance, CoverSpot _prevCoverSpot)
    {
        CoverSpot bestCover = null;
        Vector3 soldierPosition = _soldier.transform.position;

        CoverSpot[] possibleCoverSpots = m_UnoccupiedCoverSpots.ToArray();

        for (int i = 0; i < possibleCoverSpots.Length; i++)
        {
            CoverSpot currentCoverSpot = possibleCoverSpots[i];

            if(!currentCoverSpot.IsOccupied() && currentCoverSpot.IsCoveredFromEnemies(_enemyPositions))
            {
                if (bestCover == null)
                {
                    bestCover = currentCoverSpot;
                }
                else if (
                    currentCoverSpot != _prevCoverSpot &&
                    Vector3.Distance(bestCover.transform.position, soldierPosition) > Vector3.Distance(currentCoverSpot.transform.position, soldierPosition)
                    )
                {
                    bestCover = currentCoverSpot;
                }
            }
        }

        if (bestCover != null)
        {
            bestCover.SetOccupier(_soldier.transform);
            AddToOccupied(bestCover);
        }

        return bestCover;
    }

    public void ExitCover(CoverSpot _coverSpot)
    {
        if (_coverSpot != null)
        {
            _coverSpot.SetOccupier(null);

            AddToUnoccupied(_coverSpot);
        }
    }

    public bool IsCoverPastEnemyLine(Soldier _solider, CoverSpot _coverSpot)
    {
        foreach (Soldier solider in m_soldiers)
        {
            if (_solider.m_Team.GetTeamNumber() != solider.m_Team.GetTeamNumber() && solider.m_Vitals.GetHealth() > 0)
            {
                if (_coverSpot.IsBehindEnemyPosition(_solider.transform.position, solider.transform.position))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
