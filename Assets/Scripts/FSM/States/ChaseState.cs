using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    public AttackState m_AttackState;
    public bool m_IsInAttackRange;

    public override State RunCurrentState()
    {
        if(m_IsInAttackRange)
        {
            return m_AttackState;
        }

        return this;
    }
}
