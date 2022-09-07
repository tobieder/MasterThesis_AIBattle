using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCover_GroupAI : GroupAIState
{
    public Idle_GroupAI m_IdleState;
    public Cover_GroupAI m_CoverState;

    public override GroupAIState RunCurrentState()
    {
        m_SoldierData.SetWalkAnimation(true);

        CoverSpot currentCoverSpot = m_SoldierData.GetCurrentCoverSpot();

        if(currentCoverSpot != null)
        {
            if(Vector3.Distance(m_SoldierData.transform.position, currentCoverSpot.transform.position) > 0.2f)
            {
                // TODO: Allow for attack durin walk?

                m_SoldierData.GetNavMeshAgent().SetDestination(currentCoverSpot.transform.position);
                return this;
            }
            else
            {
                // Cover spot reached
                Debug.Log(m_SoldierData.name + " has reached its cover spot.");
                return m_CoverState;
            }
        }

        return m_IdleState;
    }
}
