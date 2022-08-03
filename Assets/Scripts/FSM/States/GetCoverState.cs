using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCoverState : State
{
    public MoveToCoverState m_MoveToCoverState;
    public AttackState m_AttackState;

    public override State RunCurrentState()
    {
        m_SoldierData.ResetCoverChangeCooldown();

        if(m_SoldierData.GetCurrentCoverSpot() != null)
        {
            CoverManager.Instance.ExitCover(m_SoldierData.GetCurrentCoverSpot());
        }

        /*
        CoverSpot coverSpot = CoverManager.Instance.GetCoverForEnemy(
            m_SoldierData,
            m_SoldierData.GetTarget().transform.position,
            m_SoldierData.GetMaxAttackDistance(),
            m_SoldierData.GetMinAttackDistance(),
            m_SoldierData.GetCurrentCoverSpot()
            );
        */

        // Getting a cover spot for the 3 closes enemies
        // TODO: make changable by skill
        CoverSpot coverSpot = CoverManager.Instance.GetCoverForClosestEnemies(
            m_SoldierData,
            ConvertSoldierArrayToVectorArray(m_SoldierData.GetXClosestTargets(3)), 
            m_SoldierData.GetMaxAttackDistance(),
            m_SoldierData.GetMinAttackDistance(),
            null //m_SoldierData.GetCurrentCoverSpot()
            );

        if (coverSpot != null)
        {
            // Cover available -> run to cover
            m_SoldierData.SetCurrentCoverSpot(coverSpot);
            return m_MoveToCoverState;
        }
        else
        {
            // No cover available -> Open Combat
            return m_AttackState;
        }
    }

    Vector3[] ConvertSoldierArrayToVectorArray(Soldier[] _soldiers)
    {
        Vector3[] output = new Vector3[_soldiers.Length];

        for(int i = 0; i < output.Length; i++)
        {
            output[i] = _soldiers[i].transform.position;
        }

        return output;
    }
}
