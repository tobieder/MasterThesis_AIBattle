using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public ChaseState m_ChaseState;
    public bool m_CanSeeThePlayer;

    public override State RunCurrentState()
    {
        /*
        if(m_CanSeeThePlayer)
        {
            return m_ChaseState;
        }
        */

        m_SoldierData.SetWalkAnimation(false);
        m_SoldierData.ResetPathfinding();

        return this;
    }
}
