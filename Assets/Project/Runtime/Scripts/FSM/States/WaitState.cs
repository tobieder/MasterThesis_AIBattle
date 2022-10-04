using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitState : State
{
    public float m_WaitTime = 0.0f;

    State m_NextState;

    public override State RunCurrentState()
    {
        m_SoldierData.ResetPathfinding();

        if(m_NextState == null)
        {
            Debug.LogError("No next State set.");
        }

        if (m_WaitTime <= 0.0f)
        {
            return m_NextState;
        }

        m_WaitTime -= Time.deltaTime;

        return this;
    }

    public void SetNextState(State _nextState)
    {
        m_NextState = _nextState;
    }

    public void SetTimer(float _time)
    {
        m_WaitTime = _time;
    }
}
