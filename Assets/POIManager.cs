using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class POIManager : MonoBehaviour
{
    private static POIManager _instance;
    public static POIManager Instance { get { return _instance; } }

    [SerializeField] List<POI> m_POIs = new List<POI>();

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
}
