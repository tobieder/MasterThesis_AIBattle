using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverState : State
{
    public RoamAreaState m_RoamAreaState;
    public GetCoverState m_GetCoverState;
    public AttackState m_AttackState;

    public override State RunCurrentState()
    {
        m_SoldierData.SetWalkAnimation(false);

        m_SoldierData.transform.localScale = new Vector3(1.0f, 0.5f, 1.0f);

        if(m_SoldierData.GetTarget() != null)
        {
            if (m_SoldierData.ReadyToChangeCover())
            {
                m_SoldierData.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                return m_GetCoverState;
            }

            if (m_SoldierData.ReadyToFire())
            {
                m_SoldierData.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                m_AttackState.SetPreviousState(this);
                m_AttackState.SetNextState(this);
                return m_AttackState;
            }
        }
        else
        {
            m_SoldierData.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            return m_RoamAreaState;
        }



        return this;
    }
}
