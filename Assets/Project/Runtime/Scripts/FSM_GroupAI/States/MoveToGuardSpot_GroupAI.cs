using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToGuardSpot_GroupAI : GroupAIState
{
    public Idle_GroupAI m_IdleState;
    public Guard_GroupAI m_GuardState;

    public override GroupAIState RunCurrentState()
    {
        m_SoldierData.SetGuardAnimation(false);
        m_SoldierData.SetWalkAnimation(true);

        GuardSpot currentGuardSpot = m_SoldierData.GetCurrentGuardSpot();

        if(currentGuardSpot != null)
        {
            if(Vector3.Distance(m_SoldierData.transform.position, currentGuardSpot.transform.position) > 0.2f)
            {
                m_SoldierData.GetNavMeshAgent().SetDestination(currentGuardSpot.transform.position);
                return this;
            }
            else
            {
                return m_GuardState;
            }
        }

        return m_IdleState;
    }
}
