using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle_GroupAI : GroupAIState
{
    public override GroupAIState RunCurrentState()
    {
        m_SoldierData.SetWalkAnimation(false);
        m_SoldierData.ResetPathfinding();

        return this;
    }
}
