using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover_GroupAI : GroupAIState
{
    public override GroupAIState RunCurrentState()
    {
        m_SoldierData.SetGuardAnimation(false);
        m_SoldierData.SetWalkAnimation(false);
        m_SoldierData.SetCrouchAnimation(true);

        if(m_SoldierData.GetTarget() != null)
        {
            m_SoldierData.transform.LookAt(m_SoldierData.GetTarget().transform.position);
        }

        //m_SoldierData.transform.localScale = new Vector3(1.0f, 0.5f, 1.0f);

        return this;
    }
}
