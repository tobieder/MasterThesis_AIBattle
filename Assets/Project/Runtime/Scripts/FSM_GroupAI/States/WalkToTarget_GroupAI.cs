using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkToTarget_GroupAI : GroupAIState
{
    public Idle_GroupAI m_IdleState;

    [SerializeField]
    private Vector3 m_WalkTarget;

    private float m_ResetTimer = 0.25f;
    [SerializeField]
    private float m_Timer = 0.0f;

    public override GroupAIState RunCurrentState()
    {
        m_SoldierData.SetGuardAnimation(false);
        m_SoldierData.SetWalkAnimation(true);

        if(m_WalkTarget != m_SoldierData.GetWalkTarget() || Time.time - m_Timer >= m_ResetTimer)
        {
            m_WalkTarget = m_SoldierData.GetWalkTarget();
            m_SoldierData.GetNavMeshAgent().SetDestination(m_WalkTarget);

            m_Timer = Time.time;
        }

        Vector3 distanceToDestination = transform.position - m_WalkTarget;
        if(distanceToDestination.magnitude <= 1.0f)
        {
            //Debug.Log(m_SoldierData.name + " has reached " + m_WalkTarget);
            return m_IdleState;
        }

        return this;
    }

    public Vector3 GetWalkTarget()
    {
        return m_WalkTarget;
    }
}
