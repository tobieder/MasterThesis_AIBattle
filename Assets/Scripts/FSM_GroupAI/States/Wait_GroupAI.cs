using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wait_GroupAI : GroupAIState
{
    [SerializeField]
    float m_WaitTime = 0.0f;

    GroupAIState m_NextState;

    public override GroupAIState RunCurrentState()
    {
        if(m_NextState == null)
        {
            Debug.LogError("No next state set.");
        }

        if (m_WaitTime <= 0.0f)
        {
            return m_NextState;
        }

        m_WaitTime -= Time.deltaTime;

        return this;
    }

    public void SetNextState(GroupAIState _nextState)
    {
        m_NextState = _nextState;
    }

    public void SetTimer(float _time)
    {
        m_WaitTime = _time;
    }
}
