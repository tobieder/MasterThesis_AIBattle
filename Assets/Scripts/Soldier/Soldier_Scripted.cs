using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier_Scripted : MonoBehaviour
{
    public int m_Position;

    TacticalPoint m_CurrentTarget = null;
    NavMeshAgent m_NavMeshAgent;

    private void Start()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if(m_CurrentTarget != null)
        {
            m_NavMeshAgent.SetDestination(m_CurrentTarget.transform.position);
        }
    }

    public void SetTarget(TacticalPoint _tacticalPoint)
    {
        m_CurrentTarget = _tacticalPoint;
    }

    Path CalculatePath(Vector3 _source, Vector3 _destination)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(_source, _destination, NavMesh.AllAreas, path);

        return new Path(path.corners);
    }
}
