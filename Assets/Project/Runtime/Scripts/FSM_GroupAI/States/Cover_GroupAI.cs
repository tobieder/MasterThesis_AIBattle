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

        //m_SoldierData.transform.localScale = new Vector3(1.0f, 0.5f, 1.0f);

        return this;
    }
}
