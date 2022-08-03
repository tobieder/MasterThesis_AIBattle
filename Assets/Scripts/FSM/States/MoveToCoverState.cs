using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCoverState : State
{
    public IdleState m_IdleState;

    public CoverState m_CoverState;
    public AttackState m_AttackState;

    public override State RunCurrentState()
    {
        m_SoldierData.SetWalkAnimation(true);

        CoverSpot currentCoverSpot = m_SoldierData.GetCurrentCoverSpot();

        if(currentCoverSpot != null)
        {
            if (Vector3.Distance(m_SoldierData.transform.position, currentCoverSpot.transform.position) > 0.2F)
            {
                if (m_SoldierData.ReadyToFire() && m_SoldierData.GetTarget() != null)
                {
                    m_AttackState.SetPreviousState(this);
                    m_AttackState.SetNextState(this);
                    return m_AttackState;
                }

                // Move to Cover Spot
                m_SoldierData.GetNavMeshAgent().SetDestination(currentCoverSpot.transform.position);
                return this;
            }
            else
            {
                // Cover Spot reached -> Begin Combat
                return m_CoverState;
            }
        }

        return m_IdleState;
    }
}
