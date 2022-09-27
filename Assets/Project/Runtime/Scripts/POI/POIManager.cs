using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class POIManager : MonoBehaviour
{
    private static POIManager _instance;
    public static POIManager Instance { get { return _instance; } }

    [SerializeField] List<POI> m_POIs = new List<POI>();
    [SerializeField] List<POI> m_POIsTeam1 = new List<POI>();
    [SerializeField] List<POI> m_POIsTeam2 = new List<POI>();

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Debug.LogError("POI Manager destroyed.");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        m_POIs = new List<POI>(GameObject.FindObjectsOfType<POI>());

        InvokeRepeating("UpdatePOILists", 1.0f, 0.5f);
    }

    private void UpdatePOILists()
    {
        foreach (POI poi in m_POIs)
        {
            if(poi.GetTeamOccupation() > 0)
            {
                if(!m_POIsTeam1.Contains(poi))
                {
                    m_POIsTeam2.Remove(poi);
                    m_POIsTeam1.Add(poi);
                }
            }
            else if(poi.GetTeamOccupation() < 0)
            {
                if(!m_POIsTeam2.Contains(poi))
                {
                    m_POIsTeam1.Remove(poi);
                    m_POIsTeam2.Add(poi);
                }
            }
            else
            {
                m_POIsTeam1.Remove(poi);
                m_POIsTeam2.Remove(poi);
            }
        }
    }

    public POI GetClosestUnoccupiedPOI(Vector3 _position)
    {
        POI closestPOI = null;
        float distanceClosesPOI = float.PositiveInfinity;

        foreach(POI poi in m_POIs)
        {
            if(poi.GetTeamOccupation() == 0)
            {
                float currentDistance = Vector3.Distance(_position, poi.GetComponent<BoxCollider>().ClosestPointOnBounds(_position));

                if (currentDistance < distanceClosesPOI)
                {
                    closestPOI = poi;
                    distanceClosesPOI = currentDistance;
                }
            }
        }

        return closestPOI;
    }

    public POI GetClosestUnclaimedOrEnemyPOI(Soldier _soldier)
    {
        POI closestPOI = null;
        float distanceClosesPOI = float.PositiveInfinity;

        foreach (POI poi in m_POIs)
        {
            if (poi.GetTeamOccupation() != _soldier.GetComponent<Team>().GetTeamNumber())
            {
                float currentDistance = Vector3.Distance(_soldier.transform.position, poi.GetComponent<BoxCollider>().ClosestPointOnBounds(_soldier.transform.position));

                if (currentDistance < distanceClosesPOI)
                {
                    closestPOI = poi;
                    distanceClosesPOI = currentDistance;
                }
            }
        }

        return closestPOI;
    }

    public POI GetClosestTeamPOI(Soldier _soldier)
    {
        POI closestPOI = null;
        float distanceClosesPOI = float.PositiveInfinity;

        foreach (POI poi in m_POIs)
        {
            if (poi.GetTeamOccupation() == _soldier.GetComponent<Team>().GetTeamNumber())
            {
                float currentDistance = Vector3.Distance(_soldier.transform.position, poi.GetComponent<BoxCollider>().ClosestPointOnBounds(_soldier.transform.position));

                if (currentDistance < distanceClosesPOI)
                {
                    closestPOI = poi;
                    distanceClosesPOI = currentDistance;
                }
            }
        }

        return closestPOI;
    }

    public List<POI> GetPOIs()
    {
        return m_POIs;
    }

    public List<POI> GetPOIsTeam1()
    {
        return m_POIsTeam1;
    }

    public List<POI> GetPOIsTeam2()
    {
        return m_POIsTeam2;
    }
}
