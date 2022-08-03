using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierController : MonoBehaviour
{
    public enum Controller
    {
        FiniteStateMachine,
        GroupAI,
        STOP
    }
    public Controller m_Controller = Controller.FiniteStateMachine;

    // Controllers
    // Finite State Machine
    StateManager m_StateManager;
    [SerializeField]
    GroupAI m_GroupAI;

    private void Start()
    {
        m_GroupAI.RegisterSoldier(GetComponent<Soldier>());

        m_StateManager = GetComponent<StateManager>();
    }

    public void RunSoldierController()
    {
        switch(m_Controller)
        {
            case Controller.FiniteStateMachine:
                m_StateManager.RunStateMachine();
                break;
            case Controller.GroupAI:
                break;
            case Controller.STOP:
                break;
        }
    }

    public void SetControlMethod(Controller _controlMethod)
    {
        m_Controller = _controlMethod;
    }
}
