using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard_GroupAI : GroupAIState
{
    public override GroupAIState RunCurrentState()
    {
        m_SoldierData.SetWalkAnimation(false);
        m_SoldierData.SetCrouchAnimation(false);
        m_SoldierData.SetGuardAnimation(true);

        // Look in direction of guard spot.
        m_SoldierData.transform.rotation = m_SoldierData.GetCurrentGuardSpot().transform.rotation;

        return this;
    }
}
