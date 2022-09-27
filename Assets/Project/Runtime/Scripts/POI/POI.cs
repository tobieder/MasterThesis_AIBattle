using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class POI : MonoBehaviour
{
    [SerializeField] int m_TeamOccupation = 0;

    [SerializeField] int m_MinNumberOfRequiredSoldiersToCapture = 5;

    [SerializeField] Transform m_GuardSpotsHolder;
    [SerializeField] List<GuardSpot> m_GuardSpots;
    [SerializeField] Transform m_AssemblySpotsHolder;
    [SerializeField] List<AssemblySpot> m_AssemblySpots;
    [SerializeField] ScriptedMovementManager m_ScriptedMovementManager;

    // Values represented relative to InfluenceMap Grid
    private int m_StartX;
    private int m_StartZ;
    private int m_ExtentX;
    private int m_ExtentZ;

    // Debug values
    private float m_Influence;

    private void Start()
    {
        Vector3 colliderExtents = GetComponent<BoxCollider>().bounds.extents;

        m_StartX = Mathf.RoundToInt(transform.position.x - colliderExtents.x - InfluenceMapControl.Instance.transform.position.x);
        m_StartZ = Mathf.RoundToInt(transform.position.z - colliderExtents.z - InfluenceMapControl.Instance.transform.position.z);
        m_ExtentX = Mathf.RoundToInt(colliderExtents.x * 2.0f);
        m_ExtentZ = Mathf.RoundToInt(colliderExtents.z * 2.0f);

        //Debug.Log(gameObject.name + ": (" + m_StartX + ", " + m_StartZ + ") -> (" + m_ExtentX + ", " + m_ExtentZ + ")");

        Invoke("CalculateTeamOccupation", 5.0f);

        m_GuardSpots = new List<GuardSpot>(m_GuardSpotsHolder.GetComponentsInChildren<GuardSpot>());
        m_AssemblySpots = new List<AssemblySpot>(m_AssemblySpotsHolder.GetComponentsInChildren<AssemblySpot>());
    }

    private void OnTriggerStay(Collider other)
    {
        CalculateTeamOccupation();
    }

    private void CalculateTeamOccupation()
    {
        float influence = InfluenceMapControl.Instance.GetLastInfluenceInArea(m_StartX, m_StartZ, new Vector2(m_ExtentX, m_ExtentZ));

        if (influence >= 0.9f)
        {
            m_TeamOccupation = 1;
        }
        else if (influence <= -0.9f)
        {
            m_TeamOccupation = -1;
        }
        else
        {
            m_TeamOccupation = 0;
        }

        //Debug.Log(gameObject.name + ": " + influence);
        m_Influence = influence;
    }

    public int GetTeamOccupation()
    {
        return m_TeamOccupation;
    }

    public Vector3 GetRandomWalkablePosition()
    {
        Vector3 bottomLeft = new Vector3(InfluenceMapControl.Instance.transform.position.x + m_StartX, 0.0f, InfluenceMapControl.Instance.transform.position.z + m_StartZ);
        Vector3 topRight = new Vector3(InfluenceMapControl.Instance.transform.position.x + m_StartX + m_ExtentX, 0.0f, InfluenceMapControl.Instance.transform.position.z + m_StartZ + m_ExtentZ);


        int areaMask = (1 << NavMesh.GetAreaFromName("Walkable")) | (1 << NavMesh.GetAreaFromName("Roads"));

        while (true)
        {

            float randomX = Random.Range(bottomLeft.x, topRight.x);
            float randomZ = Random.Range(bottomLeft.z, topRight.z);

            Vector3 walkTarget = new Vector3(randomX, 0.0f, randomZ);
            NavMeshHit yPositionHit;
            if(NavMesh.SamplePosition(walkTarget, out yPositionHit, 2.0f, areaMask))
            {
                walkTarget.y = yPositionHit.position.y;
            }

            NavMeshHit hit;

            if (NavMesh.SamplePosition(walkTarget, out hit, 0.5f, areaMask))
            {
                return walkTarget;
            }
        }
    }

    public Vector3 GetRandomWalkableUnoocupiedOrEnemyPosition(int _teamIndex)
    {
        Vector3 bottomLeft = new Vector3(InfluenceMapControl.Instance.transform.position.x + m_StartX, 0.0f, InfluenceMapControl.Instance.transform.position.z + m_StartZ);
        Vector3 topRight = new Vector3(InfluenceMapControl.Instance.transform.position.x + m_StartX + m_ExtentX, 0.0f, InfluenceMapControl.Instance.transform.position.z + m_StartZ + m_ExtentZ);

        int counter = 0;
        int areaMask = (1 << NavMesh.GetAreaFromName("Walkable")) | (1 << NavMesh.GetAreaFromName("Roads"));

        while (counter < 100000)
        {
            float randomX = Random.Range(bottomLeft.x, topRight.x);
            float randomZ = Random.Range(bottomLeft.z, topRight.z);

            if (InfluenceMapControl.Instance.GetLastInfluence(
                Mathf.RoundToInt(randomX - InfluenceMapControl.Instance.transform.position.x), 
                Mathf.RoundToInt(randomZ - InfluenceMapControl.Instance.transform.position.z)) 
                != _teamIndex
                )
            {
                Vector3 walkTarget = new Vector3(randomX, 0.0f, randomZ);
                NavMeshHit yPositionHit;
                if (NavMesh.SamplePosition(walkTarget, out yPositionHit, 2.0f, areaMask))
                {
                    walkTarget.y = yPositionHit.position.y;
                }

                NavMeshHit hit;

                if (NavMesh.SamplePosition(walkTarget, out hit, 0.5f, areaMask))
                {
                    return walkTarget;
                }
            }

            counter++;
        }

        return Vector3.zero;
    }

    public int GetMinNumerOfRequiredSoldiersToCapture()
    {
        return m_MinNumberOfRequiredSoldiersToCapture;
    }

    public int GetNumberOfRequiredGuards()
    {
        return m_GuardSpots.Count;
    }

    public List<GuardSpot> GetGuardSpots()
    {
        return m_GuardSpots;
    }

    public List<AssemblySpot> GetAssemblySpots()
    {
        return m_AssemblySpots;
    }

    public ScriptedMovementManager GetScriptedMovement()
    {
        return m_ScriptedMovementManager;
    }

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position, m_Influence.ToString("F2"));
    }
}
