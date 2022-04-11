using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{
    public Transform m_Destination;
    protected NavMeshAgent m_NavMeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_NavMeshAgent.destination = m_Destination.position;
    }

    // Update is called once per frame
    void Update()
    {
        m_NavMeshAgent.destination = m_Destination.position;
    }
}
