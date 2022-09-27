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
    GroupAIManager m_GroupAIManager;
    [SerializeField]
    GroupAI m_GroupAI;

    private void Start()
    {
        if(m_GroupAI != null)
        {
            m_GroupAI.RegisterSoldier(GetComponent<Soldier>());
        }

        m_StateManager = GetComponent<StateManager>();
        m_GroupAIManager = GetComponent<GroupAIManager>();
    }

    public void RunSoldierController()
    {
        switch(m_Controller)
        {
            case Controller.FiniteStateMachine:
                m_StateManager.RunStateMachine();
                break;
            case Controller.GroupAI:
                if(m_GroupAI != null)
                {
                    m_GroupAIManager.Run();
                }
                break;
            case Controller.STOP:
                break;
        }
    }

    public void SetGroupAI(GroupAI _groupAI)
    {
        m_GroupAI = _groupAI;
    }

    public void SetControlMethod(Controller _controlMethod)
    {
        m_Controller = _controlMethod;
    }

    private void OnDestroy()
    {
        m_GroupAI.DeregisterSoldier(GetComponent<Soldier>());
    }
}
