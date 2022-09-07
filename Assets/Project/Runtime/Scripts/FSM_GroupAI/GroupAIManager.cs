using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupAIManager : MonoBehaviour
{
    public GroupAIState m_CurrentState;

    public Idle_GroupAI m_IdleState;
    public WalkToTarget_GroupAI m_WalkToTarget;
    public Formation_GroupAI m_FormationState;
    public Attack_GroupAI m_AttackState;
    public MoveToCover_GroupAI m_MoveToCoverState;
    public Cover_GroupAI m_CoverState;
    public CoverAttack_GroupAI m_CoverAttackState;

    public void Run()
    {
        GroupAIState nextState = m_CurrentState?.RunCurrentState();

        if(nextState != null && nextState != m_CurrentState)
        {
            SetState(nextState);
        }
    }

    private void SetState(GroupAIState _newState)
    {
        if(m_CurrentState == m_CoverState && m_CurrentState != m_CoverAttackState)
        {
            // Exit Cover
            Debug.Log("Exit Cover");
            m_CoverState.m_SoldierData.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            m_CoverState.m_SoldierData.SetCrouchAnimation(false);
        }

        m_CurrentState = _newState;
    }

    public void SetIdleState()
    {
        SetState(m_IdleState);
    }

    public void SetWalkToTargetState(Vector3 _walkTarget)
    {
        m_WalkToTarget.m_SoldierData.SetWalkTarget(_walkTarget);
        SetState(m_WalkToTarget);
    }

    public void SetFormationState(Formation _formation)
    {
        Transform formationPosition = _formation.RegisterSoldierToFormation(m_FormationState.m_SoldierData);
        m_FormationState.SetFormation(_formation);
        m_FormationState.SetFormationPosition(formationPosition);
        SetState(m_FormationState);
    }

    public void SetAttackState(Soldier _target)
    {
        m_AttackState.m_SoldierData.SetTarget(_target);
        SetState(m_AttackState);
    }

    public void SetMoveToCoverState(CoverSpot _coverSpot)
    {
        m_MoveToCoverState.m_SoldierData.SetCurrentCoverSpot(_coverSpot);
        SetState(m_MoveToCoverState);
    }

    public void SetCoverAttackState(Soldier _target)
    {
        m_CoverAttackState.m_SoldierData.SetTarget(_target);
        SetState(m_CoverAttackState);
    }
}
