using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(NavMeshObstacle))]
public class ObstacleAgent : MonoBehaviour
{
    [SerializeField]
    private float m_CarvingTime = 0.5f;
    [SerializeField]
    private float m_CarvingMoveThreshold = 0.1f;

    private NavMeshAgent m_Agent;
    private NavMeshObstacle m_Obstacle;

    private float m_LastMoveTime;
    private Vector3 m_LastPosition;

    private void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Obstacle = GetComponent<NavMeshObstacle>();

        m_Obstacle.enabled = false;
        m_Obstacle.carveOnlyStationary = false;
        m_Obstacle.carving = true;

        m_LastPosition = transform.position;
    }

    private void Update()
    {
        if(Vector3.Distance(m_LastPosition, transform.position) > m_CarvingMoveThreshold)
        {
            m_LastMoveTime = Time.time;
            m_LastPosition = transform.position;
        }

        if(m_LastMoveTime + m_CarvingTime < Time.time)
        {
            m_Agent.enabled = false;
            m_Obstacle.enabled = true;
        }
    }

    public void SetDestination(Vector3 _position)
    {
        m_Obstacle.enabled = false;

        m_LastMoveTime = Time.time;
        m_LastPosition = transform.position;

        StartCoroutine(MoveAgent(_position));
    }

    public IEnumerator MoveAgent(Vector3 _position)
    {
        yield return null;

        m_Agent.enabled = true;
        m_Agent.SetDestination(_position);
    }
}
