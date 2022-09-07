using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier_Cover : MonoBehaviour
{

    //CoverManager m_coverManager;

    //public Team m_myTeam;
    //public Vitals m_myVitals;
    //Transform m_myTransform;
    //public Transform m_Eyes;

    //public enum ai_states
    //{
    //    idle,
    //    //move,
    //    moveToCover,
    //    combat,
    //    investigate
    //}
    //public ai_states m_state = ai_states.idle;

    //Soldier_Cover m_target;
    //Vector3 m_target_lastKnownPosition;
    //Path m_currentPath = null;

    //CoverSpot m_currentCover = null;
    //float m_coverChangeCooldown = 5.0f;
    //float m_currentCoverChangeCooldown;

    //Animator animator;

    //[SerializeField] float m_minAttackDistance = 10.0f, m_maxAttackDistance = 25.0f;
    //[SerializeField] float m_moveSpeed = 15.0f;

    //[SerializeField] float m_damageDealt = 25.0f;
    //[SerializeField] float m_fireCooldown = 1.0f;
    //float m_currentCooldown = 0.0f;

    //void Start()
    //{
    //    m_myTransform = transform;
    //    m_myTeam = GetComponent<Team>();
    //    m_myVitals = GetComponent<Vitals>();
    //    animator = GetComponent<Animator>();

    //    m_coverManager = GameObject.FindObjectOfType<CoverManager>();
    //    m_currentCoverChangeCooldown = m_coverChangeCooldown;
    //}

    //void Update()
    //{
    //    if (m_myVitals.GetHealth() > 0)
    //    {
    //        switch (m_state)
    //        {
    //            case ai_states.idle:
    //                StateIdle();
    //                break;
    //            /*
    //            case ai_states.move:
    //                StateMove();
    //                break;
    //            */
    //            case ai_states.moveToCover:
    //                StateMoveToCover();
    //                break;
    //            case ai_states.combat:
    //                StateCombat();
    //                break;
    //            case ai_states.investigate:
    //                StateInvestigate();
    //                break;
    //            default:
    //                break;
    //        }
    //    }
    //    else
    //    {
    //        // die
    //        animator.SetBool("move", false);

    //        if (GetComponent<CapsuleCollider>() != null)
    //        {
    //            Destroy(GetComponent<CapsuleCollider>());
    //        }

    //        if (m_currentCover != null)
    //        {
    //            m_coverManager.ExitCover(m_currentCover);
    //        }

    //        Quaternion deathRotation = Quaternion.Euler(-90f, m_myTransform.rotation.eulerAngles.y, m_myTransform.rotation.eulerAngles.z);
    //        if (m_myTransform.rotation != deathRotation)
    //        {
    //            m_myTransform.rotation = deathRotation;
    //        }
    //    }
    //}

    //void StateIdle()
    //{
    //    animator.SetBool("move", false);
    //    if (m_target != null && m_target.GetComponent<Vitals>().GetHealth() > 0)
    //    {
    //        if (m_currentCover != null)
    //        {
    //            m_coverManager.ExitCover(m_currentCover);
    //        }

    //        m_currentCover = m_coverManager.GetCoverForEnemy(this, m_target.transform.position, m_maxAttackDistance, m_minAttackDistance, m_currentCover);

    //        if (m_currentCover != null)
    //        {
    //            if (Vector3.Distance(m_myTransform.position, m_currentCover.transform.position) > 0.2f)
    //            {
    //                m_currentPath = CalculatePath(m_myTransform.position, m_currentCover.transform.position);

    //                m_state = ai_states.moveToCover;
    //            }
    //            else
    //            {
    //                m_state = ai_states.combat;
    //            }
    //        }
    //        else
    //        {
    //            if (Vector3.Distance(m_myTransform.position, m_target.transform.position) <= m_maxAttackDistance &&
    //                Vector3.Distance(m_myTransform.position, m_target.transform.position) >= m_minAttackDistance)
    //            {
    //                // Attack enemy
    //                m_state = ai_states.combat;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Soldier_Cover bestTarget = GetNewTarget();

    //        if (bestTarget != null)
    //        {
    //            m_target = bestTarget;
    //        }
    //    }
    //}
    //void StateMove()
    //{
    //    animator.SetBool("move", true);

    //    if (m_target != null && m_target.GetComponent<Vitals>().GetHealth() > 0)
    //    {
    //        if (!IsTargetVisible(m_target))
    //        {
    //            // target no visible anymore -> investigate last known position
    //            m_target_lastKnownPosition = m_target.transform.position;
    //            m_state = ai_states.investigate;

    //            m_currentPath = CalculatePath(m_myTransform.position, m_target_lastKnownPosition);

    //            return;
    //        }

    //        m_myTransform.LookAt(m_target.transform);

    //        if (Vector3.Distance(m_myTransform.position, m_target.transform.position) > m_maxAttackDistance)
    //        {
    //            // move closer to target
    //            m_myTransform.Translate(Vector3.forward * m_moveSpeed * Time.deltaTime);
    //        }
    //        else if (Vector3.Distance(m_myTransform.position, m_target.transform.position) < m_minAttackDistance)
    //        {
    //            // move away from target
    //            m_myTransform.Translate(Vector3.back * m_moveSpeed * Time.deltaTime);
    //        }
    //        else
    //        {
    //            // attack
    //            m_state = ai_states.combat;
    //        }
    //    }
    //    else
    //    {
    //        m_state = ai_states.idle;
    //    }
    //}


    //void StateMoveToCover()
    //{
    //    animator.SetBool("move", true);

    //    if (m_target != null && m_currentCover != null && m_currentCover.IsCoveredFromEnemy(m_target.transform.position))
    //    {
    //        if (m_currentPath != null)
    //        {
    //            Soldier_Cover alternativeTarget = GetNewTarget();

    //            if (alternativeTarget != null && alternativeTarget != m_target)
    //            {
    //                float distanceToCurrentTarget = Vector3.Distance(m_myTransform.position, m_target.transform.position);
    //                float distanceToAlternativeTarget = Vector3.Distance(m_myTransform.position, alternativeTarget.transform.position);
    //                float distanceBetweenTargets = Vector3.Distance(m_target.transform.position, alternativeTarget.transform.position);

    //                if (Mathf.Abs(distanceToAlternativeTarget - distanceToCurrentTarget) > 5 && distanceBetweenTargets > 5)
    //                {
    //                    m_target = alternativeTarget;
    //                    m_coverManager.ExitCover(m_currentCover);
    //                    m_currentCover = m_coverManager.GetCoverForEnemy(this, m_target.transform.position, m_maxAttackDistance, m_minAttackDistance, m_currentCover);
    //                    m_currentPath = CalculatePath(m_myTransform.position, m_currentCover.transform.position);
    //                    return;
    //                }
    //            }

    //            if (m_currentPath.EndNodeReached())
    //            {
    //                m_currentPath = null;

    //                m_state = ai_states.combat;

    //                return;
    //            }

    //            Vector3 nodePosition = m_currentPath.GetNextNode();

    //            if (Vector3.Distance(m_myTransform.position, nodePosition) < 0.5)
    //            {
    //                m_currentPath.m_CurrentPathIndex++;
    //            }
    //            else
    //            {
    //                m_myTransform.LookAt(nodePosition);
    //                m_myTransform.Translate(Vector3.forward * m_moveSpeed * Time.deltaTime);
    //            }
    //        }
    //        else
    //        {
    //            m_currentPath = null;
    //            m_target = null;

    //            m_state = ai_states.idle;
    //        }
    //    }
    //    else
    //    {
    //        m_state = ai_states.idle;
    //    }
    //}
    //void StateCombat()
    //{
    //    animator.SetBool("move", false);
    //    if (m_target != null && m_target.GetComponent<Vitals>().GetHealth() > 0)
    //    {
    //        if (!IsTargetVisible(m_target))
    //        {
    //            Soldier_Cover alternateiveTarget = GetNewTarget();

    //            if (alternateiveTarget == null)
    //            {
    //                // target no visible anymore -> investigate last known position
    //                m_target_lastKnownPosition = m_target.transform.position;
    //                m_currentPath = CalculatePath(m_myTransform.position, m_target_lastKnownPosition);

    //                if (m_currentCover != null)
    //                {
    //                    m_coverManager.ExitCover(m_currentCover);
    //                }

    //                m_state = ai_states.investigate;

    //            }
    //            else
    //            {
    //                m_target = alternateiveTarget;
    //            }


    //            return;
    //        }

    //        m_myTransform.LookAt(m_target.transform);

    //        if (Vector3.Distance(m_myTransform.position, m_target.transform.position) <= m_maxAttackDistance &&
    //            Vector3.Distance(m_myTransform.position, m_target.transform.position) >= m_minAttackDistance)
    //        {
    //            // Attack enemy
    //            if (m_currentCooldown <= 0.0f)
    //            {
    //                animator.SetTrigger("fire");

    //                m_target.GetComponent<Vitals>().Hit(m_damageDealt);

    //                m_currentCooldown = m_fireCooldown;
    //            }
    //            else
    //            {
    //                m_currentCooldown -= Time.deltaTime;
    //            }
    //        }
    //        else
    //        {
    //            if (m_currentCoverChangeCooldown <= 0.0f)
    //            {
    //                m_currentCoverChangeCooldown = m_coverChangeCooldown;
    //                m_state = ai_states.idle;
    //            }
    //            else
    //            {
    //                m_currentCoverChangeCooldown -= Time.deltaTime;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        m_state = ai_states.idle;
    //        // investigate area of body for more enemies
    //        /*
    //        if(m_target != null && m_target.GetComponent<Vitals>().GetHealth() <= 0)
    //        {
    //            m_target_lastKnownPosition = m_target.transform.position;
    //            m_state = ai_states.investigate;

    //            m_currentPath = CalculatePath(m_myTransform.position, m_target_lastKnownPosition);
    //        }
    //        else
    //        {
    //            m_state = ai_states.idle;
    //        }
    //        */
    //    }
    //}
    //void StateInvestigate()
    //{
    //    animator.SetBool("move", true);

    //    if (m_currentPath != null)
    //    {
    //        Soldier_Cover alternativeTarget = GetNewTarget();

    //        if (m_currentPath.EndNodeReached() || alternativeTarget != null)
    //        {
    //            m_currentPath = null;
    //            m_target = alternativeTarget;

    //            m_state = ai_states.idle;

    //            return;
    //        }

    //        Vector3 nodePosition = m_currentPath.GetNextNode();

    //        if (Vector3.Distance(m_myTransform.position, nodePosition) < 1)
    //        {
    //            m_currentPath.m_CurrentPathIndex++;
    //        }
    //        else
    //        {
    //            m_myTransform.LookAt(nodePosition);
    //            m_myTransform.Translate(Vector3.forward * m_moveSpeed * Time.deltaTime);
    //        }
    //    }
    //    else
    //    {
    //        m_currentPath = null;
    //        m_target = null;

    //        m_state = ai_states.idle;
    //    }
    //}

    //Soldier_Cover GetNewTarget()
    //{
    //    Soldier_Cover[] allSoldiers = GameObject.FindObjectsOfType<Soldier_Cover>();
    //    Soldier_Cover bestTarget = null;
    //    for (int i = 0; i < allSoldiers.Length; i++)
    //    {
    //        Soldier_Cover currentSoldier = allSoldiers[i];

    //        // check if npcs are on the same team
    //        if (currentSoldier.GetComponent<Team>().GetTeamNumber() != this.m_myTeam.GetTeamNumber())
    //        {

    //            if (IsTargetVisible(currentSoldier))
    //            {
    //                if (bestTarget == null)
    //                {
    //                    bestTarget = currentSoldier;
    //                }
    //                else
    //                {
    //                    // check distance
    //                    if (Vector3.Distance(bestTarget.transform.position, m_myTransform.position) >
    //                        Vector3.Distance(currentSoldier.transform.position, m_myTransform.position))
    //                    {
    //                        bestTarget = currentSoldier;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    return bestTarget;
    //}

    //bool IsTargetVisible(Soldier_Cover _target)
    //{
    //    Vector3 directionToEnemy = _target.m_Eyes.position - m_Eyes.position;
    //    RaycastHit hit;

    //    if (Physics.Raycast(m_Eyes.position, directionToEnemy, out hit, Mathf.Infinity))
    //    {
    //        if (hit.transform == _target.transform)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    //Path CalculatePath(Vector3 _source, Vector3 _destination)
    //{
    //    NavMeshPath path = new NavMeshPath();
    //    NavMesh.CalculatePath(_source, _destination, NavMesh.AllAreas, path);

    //    return new Path(path.corners);
    //}

    //private void OnDrawGizmos()
    //{
    //    if (m_currentPath != null)
    //    {
    //        Vector3[] pathNodes = m_currentPath.GetPathNodes();
    //        for (int i = 0; i < pathNodes.Length - 1; i++)
    //        {
    //            Debug.DrawLine(pathNodes[i], pathNodes[i + 1]);
    //        }
    //    }
    //}
}
