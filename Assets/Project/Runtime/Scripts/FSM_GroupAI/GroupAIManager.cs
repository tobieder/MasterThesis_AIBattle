using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

public enum Priority
{
    none,
    low,
    job,
    formation,
    action
}

public class GroupAIManager : MonoBehaviour
{
    public GroupAIState m_CurrentState;

    public Idle_GroupAI m_IdleState;
    public Wait_GroupAI m_WaitState;
    public WalkToTarget_GroupAI m_WalkToTarget;
    public Formation_GroupAI m_FormationState;
    public Attack_GroupAI m_AttackState;
    public MoveToGuardSpot_GroupAI m_MoveToGuardSpotState;
    public Guard_GroupAI m_GuardGroupAI;
    public MoveToCover_GroupAI m_MoveToCoverState;
    public Cover_GroupAI m_CoverState;
    public CoverAttack_GroupAI m_CoverAttackState;

    [SerializeField]
    private float m_TimeSinceNonePriority;

    [SerializeField]
    private Priority m_Priority = Priority.none;

    public void Run()
    {
        if(m_CurrentState == m_IdleState)
        {
            m_TimeSinceNonePriority += Time.deltaTime;
        }
        else
        {
            m_TimeSinceNonePriority = 0.0f;
        }

        GroupAIState nextState = m_CurrentState?.RunCurrentState();

        if(nextState != null && nextState != m_CurrentState)
        {
            if(m_CurrentState == m_GuardGroupAI)
            {
                m_CurrentState.m_SoldierData.GetCurrentGuardSpot().SetOccupier(null);
            }

            SetState(nextState);
        }
    }

    // Functionality
    public bool DoesNothing()
    {
        if(m_Priority == Priority.none)
        {
            return true;
        }

        return false;
    }

    public float GetTimeSinceNonePriority()
    {
        return m_TimeSinceNonePriority;
    }

    public bool IsFree()
    {
        if(m_Priority < Priority.job)
        {
            return true;
        }

        return false;
    }

    public bool IsInAction()
    {
        if(m_Priority == Priority.action)
        {
            return true;
        }

        return false;
    }

    public bool IsInCover()
    {
        if(m_CurrentState == m_MoveToCoverState ||
            m_CurrentState == m_CoverState ||
            m_CurrentState == m_CoverAttackState)
        {
            return true;
        }

        return false;
    }

    // State Managment
    private void SetState(GroupAIState _newState)
    {
        if(m_CurrentState == m_CoverState && m_CurrentState != m_CoverAttackState)
        {
            // Exit Cover
            //Debug.Log("Exit Cover");
            m_CoverState.m_SoldierData.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            m_CoverState.m_SoldierData.SetCrouchAnimation(false);
            m_CoverState.m_SoldierData.ResetCurrentCover();
        }

        if(_newState == m_IdleState)
        {
            m_Priority = Priority.none;
        }

        m_CurrentState = _newState;
    }

    public void SetIdleState()
    {
        m_Priority = Priority.none;
        SetState(m_IdleState);
    }

    public void SetWalkToTargetState(Vector3 _walkTarget, Priority _priority)
    {
        m_Priority = _priority;
        m_WalkToTarget.m_SoldierData.SetWalkTarget(_walkTarget);
        SetState(m_WalkToTarget);
    }

    public void SetFormationState(Formation _formation)
    {
        m_Priority = Priority.formation;
        Transform formationPosition = _formation.RegisterSoldierToFormation(m_FormationState.m_SoldierData);
        m_FormationState.SetFormation(_formation);
        m_FormationState.SetFormationPosition(formationPosition);
        SetState(m_FormationState);
    }

    public void SetAttackState(Soldier _target)
    {
        m_Priority = Priority.action;
        m_AttackState.m_SoldierData.SetTarget(_target);
        SetState(m_AttackState);
    }

    public void SetMoveToCoverState(CoverSpot _coverSpot)
    {
        m_Priority = Priority.job;
        m_MoveToCoverState.m_SoldierData.SetCurrentCoverSpot(_coverSpot);
        SetState(m_MoveToCoverState);
    }

    public void SetMoveToGuardSpotState(GuardSpot _guardSpot)
    {
        m_Priority = Priority.job;
        m_MoveToCoverState.m_SoldierData.SetCurrentGuardSpot(_guardSpot);
        SetState(m_MoveToGuardSpotState);
    }

    public void SetCoverAttackState(Soldier _target)
    {
        m_Priority = Priority.action;
        m_CoverAttackState.m_SoldierData.SetTarget(_target);
        SetState(m_CoverAttackState);
    }

    private void OnDrawGizmos()
    {
        if(m_CurrentState == m_WalkToTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(m_WalkToTarget.GetWalkTarget(), Vector3.one);
            //Handles.Label(m_WalkToTarget.GetWalkTarget(), gameObject.name);
        }
    }
}
