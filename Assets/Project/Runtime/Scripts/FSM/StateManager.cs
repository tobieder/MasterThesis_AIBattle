using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public State m_CurrentState;

    public void RunStateMachine()
    {
        State nextState = m_CurrentState?.RunCurrentState();

        if(nextState != null)
        {
            // Switch to the next state
            SwitchToTheNextState(nextState);
        }
    }

    private void SwitchToTheNextState(State _nextState)
    {
        m_CurrentState = _nextState;
    }
}
