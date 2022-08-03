using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoamAreaState : State
{
    public bool m_WalkTargetSet;
    Vector3 m_WalkTarget;

    Vector3 m_BottomLeft, m_TopRight;

    public AttackState m_AttackState;
    public GetCoverState m_GetCoverState;

    private void Start()
    {
        m_WalkTargetSet = false;

        m_BottomLeft = InfluenceMapControl.Instance.GetBottomLeft();
        m_TopRight = InfluenceMapControl.Instance.GetTopRight();
    }

    public override State RunCurrentState()
    {
        if(m_SoldierData.ReadyToFire() && m_SoldierData.GetTarget() != null)
        {
            return m_GetCoverState;
        }

        m_SoldierData.SetWalkAnimation(true);

        if (!m_WalkTargetSet)
        {
            float randomX = Random.Range(m_BottomLeft.x, m_TopRight.x);
            float randomZ = Random.Range(m_BottomLeft.z, m_TopRight.z);

            m_WalkTarget = new Vector3(randomX, transform.position.y, randomZ);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(m_WalkTarget, out hit, 0.5f, NavMesh.AllAreas))
            {
                m_WalkTargetSet = true;
            }
        }
        else
        {
            m_SoldierData.GetNavMeshAgent().SetDestination(m_WalkTarget);

            Vector3 distanceToWalkpoint = transform.position - m_WalkTarget;
            if (distanceToWalkpoint.magnitude < 1.0f)
            {
                m_WalkTargetSet = false;
            }
        }

        return this;
    }

    private void OnDrawGizmos()
    {
        if(m_WalkTargetSet)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_WalkTarget, 0.5f);
        }
    }
}
