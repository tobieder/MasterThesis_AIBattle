using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Formation : MonoBehaviour
{
    [SerializeField]
    Vector3 m_Target;
    [SerializeField]
    float m_MoveSpeed;

    Path m_CurrentPath;

    [SerializeField]
    List<Transform> m_FormationPositions = new List<Transform>();
    [SerializeField]
    Dictionary<Transform, Soldier> m_FormationOccupancy = new Dictionary<Transform, Soldier>();

    private void Start()
    {
        foreach(Transform formationPosition in m_FormationPositions)
        {
            m_FormationOccupancy.Add(formationPosition, null);
        }
    }

    private void Update()
    {
        if (m_Target == null)
            return;

        if(Vector3.Distance(transform.position, m_Target) <= 1.0f)
        {
            m_CurrentPath = null;
        }
        else
        {
            if(AreAllUnitsInPosition(2.0f))
            {
                if(m_CurrentPath == null)
                {
                    m_CurrentPath = CalculatePath(transform.position, m_Target);
                }
                else
                {
                    Vector3 nodePosition = m_CurrentPath.GetNextNode();
                    if(Vector3.Distance(transform.position, nodePosition) < 0.5f)
                    {
                        m_CurrentPath.m_CurrentPathIndex++;
                    }
                    else
                    {
                        transform.LookAt(nodePosition);
                        if(AreAllUnitsInPosition(0.75f))
                        {
                            transform.Translate(Vector3.forward * m_MoveSpeed * Time.deltaTime);
                        }
                        else
                        {
                            transform.Translate(Vector3.forward * m_MoveSpeed * 0.25f * Time.deltaTime);
                        }
                    }
                }
            }
            else
            {
                if(m_CurrentPath != null)
                {
                    Vector3 nodePosition = m_CurrentPath.GetNextNode();
                    if (Vector3.Distance(transform.position, nodePosition) < 0.5f)
                    {
                        m_CurrentPath.m_CurrentPathIndex++;
                    }
                    else
                    {
                        transform.LookAt(nodePosition);
                        transform.Translate(Vector3.forward * m_MoveSpeed * 0.25f * Time.deltaTime);
                    }
                }
            }
        }
    }

    public void SetTarget(Vector3 _position)
    {
        m_Target = _position;
    }

    public Transform RegisterSoldierToFormation(Soldier _soldier)
    {
        foreach (KeyValuePair<Transform, Soldier> occupancy in m_FormationOccupancy)
        {
            if(m_FormationOccupancy[occupancy.Key] == null)
            {
                m_FormationOccupancy[occupancy.Key] = _soldier;
                Debug.Log(_soldier.name + " has been registered to formation " + name);
                return occupancy.Key;
            }
        }

        Debug.LogWarning(name + " formation is already full.");
        return null;
    }

    public void RegisterSoldierToFormation(Transform _formationPosition, Soldier _soldier)
    {
        if(m_FormationOccupancy.ContainsKey(_formationPosition))
        {
            m_FormationOccupancy[_formationPosition] = _soldier;
        }
        else
        {
            Debug.LogWarning("Formation Position is not available as a key in the dictionary.");
        }
    }

    public bool IsUnitInPosition(Transform _formationPosition, Soldier _soldier, float _distance)
    {
        if(Vector3.Distance(_formationPosition.position, _soldier.transform.position) < _distance)
        {
            return true;
        }

        return false;
    }

    public bool AreAllUnitsInPosition(float _distance)
    {
        bool allUnitsInPosition = true;

        foreach(KeyValuePair<Transform, Soldier> occupancy in m_FormationOccupancy)
        {
            if(occupancy.Value != null)
            {
                if(!IsUnitInPosition(occupancy.Key, occupancy.Value, _distance))
                {
                    allUnitsInPosition = false;
                }
            }
            else
            {
                allUnitsInPosition = false;
            }
        }

        return allUnitsInPosition;
    }

    Path CalculatePath(Vector3 _source, Vector3 _destination)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(_source, _destination, NavMesh.AllAreas, path);

        return new Path(path.corners);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.color = Color.red;
        foreach (Transform formationPosition in m_FormationPositions)
        {
            Gizmos.DrawWireSphere(formationPosition.position, 0.15f);
        }

        if (m_CurrentPath != null)
        {
            Vector3[] pathNodes = m_CurrentPath.GetPathNodes();
            for (int i = 0; i < pathNodes.Length - 1; i++)
            {
                Debug.DrawLine(pathNodes[i], pathNodes[i + 1]);
            }
        }
    }
}
