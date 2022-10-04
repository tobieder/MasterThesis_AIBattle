using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptedMovementManager : MonoBehaviour
{
    public int m_SoldiersNeeded;

    public List<TacticalPointList> m_TacticalPointList;
    //public List<Soldier_Scripted> m_SubscribedSoldiers;
    public List<Soldier> m_SubscribedSoldiers;

    private int m_currentTacticalPoint = 0;

    public void ResetScriptedMovement()
    {
        m_SubscribedSoldiers.Clear();
        m_currentTacticalPoint = 0;
    }

    public bool Run()
    {
        if (m_currentTacticalPoint < m_TacticalPointList.Count)
        {
            if (m_SubscribedSoldiers.Count == m_SoldiersNeeded)
            {
                bool allSoldiersAtCurrentTacticalPoint = true;
                for (int i = 0; i < m_SubscribedSoldiers.Count; i++)
                {
                    //m_SubscribedSoldiers[i].SetTarget(m_TacticalPointList[m_currentTacticalPoints].m_Points[i]);
                    m_SubscribedSoldiers[i].GetComponent<GroupAIManager>().SetWalkToTargetState(m_TacticalPointList[m_currentTacticalPoint].m_Points[i].transform.position, Priority.job);

                    if (Vector3.Distance(m_SubscribedSoldiers[i].transform.position, m_TacticalPointList[m_currentTacticalPoint].m_Points[i].transform.position) > 0.5f)
                    {
                        allSoldiersAtCurrentTacticalPoint = false;
                    }
                }

                if (allSoldiersAtCurrentTacticalPoint)
                {
                    m_currentTacticalPoint++;
                }
            }
            else
            {
                // Request a new soldier
                Debug.Log("Need a new soldier");
            }

            return false;
        }
        else
        {
            // End reached
            Debug.Log("End reached");
            return true;
        }
    }

    public void Subscribe(Soldier _soldier)
    {
        if(m_SubscribedSoldiers.Count < m_SoldiersNeeded && !m_SubscribedSoldiers.Contains(_soldier))
        {
            m_SubscribedSoldiers.Add(_soldier);
        }
    }

    public void Deregister(Soldier _solder)
    {
        m_SubscribedSoldiers.Remove(_solder);
    }
}

[System.Serializable]
public class TacticalPointList
{
    public List<TacticalPoint> m_Points;
}
