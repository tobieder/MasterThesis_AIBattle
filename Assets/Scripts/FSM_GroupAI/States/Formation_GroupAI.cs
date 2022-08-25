using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation_GroupAI : GroupAIState
{
    public Formation m_Formation;
    public Transform m_FormationPosition;

    public override GroupAIState RunCurrentState()
    {
        if(m_SoldierData.GetVelocity() > 0.1f)
        {
            m_SoldierData.SetWalkAnimation(true);
        }
        else
        {
            m_SoldierData.SetWalkAnimation(false);
        }

        m_SoldierData.GetNavMeshAgent().SetDestination(m_FormationPosition.position);

        if(m_Formation.IsUnitInPosition(m_FormationPosition, m_SoldierData, 0.5f))
        {
            m_SoldierData.transform.rotation = m_FormationPosition.rotation;
        }

        return this;
    }

    public void SetFormation(Formation _formation)
    {
        m_Formation = _formation;
    }

    public void SetFormationPosition(Transform _formationPosition)
    {
        m_FormationPosition = _formationPosition;
    }
}
