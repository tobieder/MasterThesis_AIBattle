using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedMovementManager : MonoBehaviour
{
    public int m_SoldiersNeeded;

    public List<TacticalPointList> m_TacticalPointList;
    public List<Soldier_Scripted> m_SubscribedSoldiers;

    private int m_currentTacticalPoints = 0;

    private void Update()
    {
        if(m_SubscribedSoldiers.Count == m_SoldiersNeeded)
        {
            bool allSoldiersAtCurrentTacticalPoint = true;
            for(int i = 0; i < m_SubscribedSoldiers.Count; i++)
            {
                m_SubscribedSoldiers[i].SetTarget(m_TacticalPointList[m_currentTacticalPoints].m_Points[i]);

                if (Vector3.Distance(m_SubscribedSoldiers[i].transform.position, m_TacticalPointList[m_currentTacticalPoints].m_Points[i].transform.position) > 0.5f)
                {
                    allSoldiersAtCurrentTacticalPoint=false;
                }
            }
            if(allSoldiersAtCurrentTacticalPoint)
            {
                m_currentTacticalPoints++;
                m_currentTacticalPoints = Mathf.Clamp(m_currentTacticalPoints, 0, 1);
            }
        }
    }

    public void SubscribeToScriptedMovement(Soldier_Scripted _soldier)
    {
        if(m_SubscribedSoldiers.Count < m_SoldiersNeeded)
        {
            m_SubscribedSoldiers.Add(_soldier);
        }
    }
}

[System.Serializable]
public class TacticalPointList
{
    public List<TacticalPoint> m_Points;
}
