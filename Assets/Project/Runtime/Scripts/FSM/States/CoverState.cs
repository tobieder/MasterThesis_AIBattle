using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverState : State
{
    public RoamAreaState m_RoamAreaState;
    public GetCoverState m_GetCoverState;
    public CoverAttackState m_CoverAttackState;

    public override State RunCurrentState()
    {
        m_SoldierData.SetWalkAnimation(false);
        m_SoldierData.SetCrouchAnimation(true);

        if(m_SoldierData.GetTarget() != null)
        {
            m_SoldierData.transform.LookAt(m_SoldierData.GetTarget().transform);

            if (m_SoldierData.ReadyToChangeCover())
            {
                m_SoldierData.SetCrouchAnimation(false);
                return m_GetCoverState;
            }

            if (m_SoldierData.ReadyToFire())
            {
                m_SoldierData.SetCrouchAnimation(false);

                return m_CoverAttackState;
            }
        }
        else
        {
            m_SoldierData.SetCrouchAnimation(false);
            CoverManager.Instance.ExitCover(m_SoldierData.GetCurrentCoverSpot());
            return m_RoamAreaState;
        }



        return this;
    }
}
