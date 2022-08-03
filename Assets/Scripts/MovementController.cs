using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{
    // Properties
    public float m_Health;

    public Transform m_EnemyTransform;

    // Navigation
    protected NavMeshAgent m_NavMeshAgent;
    public LayerMask m_GroundLayer, m_PlayerLayer;

    // Patrol
    public Vector3 m_WalkPoint;
    bool m_WalkPointSet;
    public float m_WalkPointRange;

    // Attack
    public ParticleSystem m_MuzzleFlash;
    public float m_TimeBetweenAttacks;
    bool m_AlreadyAttacked;

    // States
    public float m_SightRange, m_AttackRange;
    public bool m_PlayerInSightRange, m_PlayerInAttackRange;

    private void Awake()
    {
        // Set variables by code.
        //m_PlayerTransform = GameObject.Find("Player").transform;
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Check for current state.
        m_PlayerInSightRange = Physics.CheckSphere(transform.position, m_SightRange, m_PlayerLayer);
        m_PlayerInAttackRange = false;

        if (m_PlayerInSightRange)
        {
            // If there is no enemy save the enemy transform.
            if (m_EnemyTransform == null)
            {
                m_EnemyTransform = Physics.OverlapSphere(transform.position, m_SightRange, m_PlayerLayer)[0].gameObject.transform;
            }

            // Check if the enemy is in front of the player (Field of View)
            Vector3 heading = m_EnemyTransform.position - transform.position;
            float dot_product = Vector3.Dot(heading, transform.forward);

            // Dot Product > 0 => Enemy is in front of the npc
            if (dot_product > 0)
            {
                m_PlayerInAttackRange = Physics.CheckSphere(transform.position, m_AttackRange, m_PlayerLayer);
            }
            else
            {
                m_EnemyTransform = null;
                m_PlayerInSightRange = false;
            }
        }


        if (!m_PlayerInSightRange && !m_PlayerInAttackRange)
        {
            m_EnemyTransform = null;

            Patroling();
        }
        else if(m_PlayerInSightRange && !m_PlayerInAttackRange)
        {
            ChasePlayer();
        }
        else if(m_PlayerInSightRange && m_PlayerInAttackRange)
        {
            AttackPlayer();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_AttackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_SightRange);
    }

    void Patroling()
    {
        if(!m_WalkPointSet)
        {
            SearchWalkPoint();
        }
        else
        {
            m_NavMeshAgent.SetDestination(m_WalkPoint);

            Vector3 distanceToWalkpoint = transform.position - m_WalkPoint;
            if(distanceToWalkpoint.magnitude < 1.0f)
            {
                m_WalkPointSet = false;
            }
        }
    }

    void ChasePlayer()
    {
        m_NavMeshAgent.SetDestination(m_EnemyTransform.position);
    }

    void AttackPlayer()
    {
        m_NavMeshAgent.SetDestination(transform.position);
        transform.LookAt(m_EnemyTransform);

        if (!m_AlreadyAttacked)
        {
            // Attack Code goes HERE!
            m_EnemyTransform.parent.parent.GetComponent<MovementController>().TakeDamage(20);
            m_MuzzleFlash.Play();

            m_AlreadyAttacked = true;
            Invoke(nameof(ResetAttack), m_TimeBetweenAttacks);
        }
    }

    private void SearchWalkPoint()
    {
        // Generate random patrol point
        float randomZ = Random.Range(-m_WalkPointRange, m_WalkPointRange);
        float randomX = Random.Range(-m_WalkPointRange, m_WalkPointRange);

        m_WalkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if(Physics.Raycast(m_WalkPoint, -transform.up, 2f, m_GroundLayer))
        {
            m_WalkPointSet = true;
        }
    }

    private void ResetAttack()
    {
        m_AlreadyAttacked = false;
    }

    public void TakeDamage(int _damage)
    {
        m_Health -= _damage;

        if(m_Health <= 0)
        {
            //Invoke(nameof(Die), 0.5f);
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
