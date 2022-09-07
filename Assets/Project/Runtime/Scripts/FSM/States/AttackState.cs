using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    State m_PreviousState;
    public WaitState m_WaitState;

    public override State RunCurrentState()
    {
        Soldier target = m_SoldierData.GetTarget();

        if(target == null)
        {
            Debug.LogWarning("No target");
            return m_PreviousState;
        }

        m_SoldierData.SetWalkAnimation(false);
        m_SoldierData.transform.LookAt(target.transform);

        if(!m_SoldierData.IsTargetVisible())
        {
            m_WaitState.SetTimer(0.25f);
            return m_WaitState;
            //return this;
        }

        m_SoldierData.TriggerShotAnimation();

        m_SoldierData.ResetPathfinding();

        float maxDeviation = 0.75f;
        Vector3 deviation3D = Random.insideUnitCircle * maxDeviation;
        Vector3 direction = m_SoldierData.GetTarget().GetEyes().position - m_SoldierData.GetEyes().position;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward * m_SoldierData.GetMaxAttackDistance() + deviation3D);
        //Vector3 forwardVector = transform.rotation * rot * Vector3.forward;

        Vector3 shootVector = direction * m_SoldierData.GetMaxAttackDistance() + deviation3D;

        RaycastHit hit;

        Debug.DrawRay(m_SoldierData.GetEyes().position, direction * m_SoldierData.GetMaxAttackDistance(), Color.blue, 4);
        Debug.DrawRay(m_SoldierData.GetEyes().position, shootVector, Color.red, 10);

        if (Physics.Raycast(m_SoldierData.GetEyes().position, shootVector, out hit, m_SoldierData.GetMaxAttackDistance()))
        {
            //Check if hit == target
            if(hit.transform.gameObject == target.gameObject)
            {
                target.GetComponent<Vitals>().Hit(m_SoldierData.GetDamage());
            }
            else
            {
                Debug.LogWarning(transform.parent.parent.gameObject.name + " hit not its intended target " + target.name);
            }
        }

        m_SoldierData.ResetFireCooldown();

        m_WaitState.SetTimer(0.5f);
        return m_WaitState;
    }

    public void SetPreviousState(State _prevState)
    {
        m_PreviousState = _prevState;
    }

    public void SetNextState(State _nextState)
    {
        m_WaitState.SetNextState(_nextState);
    }
}