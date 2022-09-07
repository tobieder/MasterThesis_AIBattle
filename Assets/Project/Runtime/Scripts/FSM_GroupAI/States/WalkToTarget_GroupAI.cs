using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkToTarget_GroupAI : GroupAIState
{
    public Idle_GroupAI m_IdleState;

    private Vector3 m_WalkTarget;

    public override GroupAIState RunCurrentState()
    {
        m_SoldierData.SetWalkAnimation(true);

        if(m_WalkTarget != m_SoldierData.GetWalkTarget())
        {
            m_WalkTarget = m_SoldierData.GetWalkTarget();
            
            if(!m_SoldierData.GetNavMeshAgent().SetDestination(m_WalkTarget))
            {
                Debug.LogWarning("No path found.");
            }
        }

        Vector3 distanceToDestination = transform.position - m_WalkTarget;
        if(distanceToDestination.magnitude < 1.0f)
        {
            return m_IdleState;
        }

        return this;
    }
}