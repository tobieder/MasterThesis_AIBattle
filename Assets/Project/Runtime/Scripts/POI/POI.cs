using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class POI : MonoBehaviour
{
    [SerializeField] int m_TeamOccupation = 0;
    [SerializeField] List<GuardSpot> m_GuardSpots;

    // Values represented relative to InfluenceMap Grid
    private int m_StartX;
    private int m_StartZ;
    private int m_ExtentX;
    private int m_ExtentZ;

    private void Start()
    {
        Vector3 colliderExtents = GetComponent<BoxCollider>().bounds.extents;

        m_StartX = Mathf.RoundToInt(transform.position.x - colliderExtents.x - InfluenceMapControl.Instance.transform.position.x);
        m_StartZ = Mathf.RoundToInt(transform.position.z - colliderExtents.z - InfluenceMapControl.Instance.transform.position.z);
        m_ExtentX = Mathf.RoundToInt(colliderExtents.x * 2.0f);
        m_ExtentZ = Mathf.RoundToInt(colliderExtents.z * 2.0f);

        Debug.Log(gameObject.name + ": (" + m_StartX + ", " + m_StartZ + ") -> (" + m_ExtentX + ", " + m_ExtentZ + ")");

        Invoke("CalculateTeamOccupation", 5.0f);

        m_GuardSpots = new List<GuardSpot>(GetComponentsInChildren<GuardSpot>());
    }

    private void OnTriggerStay(Collider other)
    {
        CalculateTeamOccupation();
    }

    private void CalculateTeamOccupation()
    {
        float influence = InfluenceMapControl.Instance.GetLastInfluenceInArea(m_StartX, m_StartZ, new Vector2(m_ExtentX, m_ExtentZ));

        if (influence >= 0.5f)
        {
            m_TeamOccupation = 1;
        }
        else if (influence <= -0.5f)
        {
            m_TeamOccupation = -1;
        }
        else
        {
            m_TeamOccupation = 0;
        }

        Debug.Log(gameObject.name + ": " + influence);
    }

    public int GetTeamOccupation()
    {
        return m_TeamOccupation;
    }

    public int GetNumberOfRequiredGuards()
    {
        return m_GuardSpots.Count;
    }

    public List<GuardSpot> GetGuardSpots()
    {
        return m_GuardSpots;
    }
}
