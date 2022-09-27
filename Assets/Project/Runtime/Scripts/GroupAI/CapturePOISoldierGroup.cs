using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum CapturePOIStatus
{
    AssembleSoldiers,
    Capture,
    Guard,
    EOL
}

public class CapturePOISoldierGroup
{
    public string m_Name;
    public CapturePOIStatus capturePOIStatus;
    public POI m_TargetPOI;
    private List<Soldier> soldiers = new List<Soldier>();
    private int m_TeamIndex;

    private Dictionary<Soldier, AssemblySpot> m_AssignedAssemblySpots = new Dictionary<Soldier, AssemblySpot>();
    private Dictionary<Soldier, Vector3> m_AssignedWalkTargets = new Dictionary<Soldier, Vector3>();

    ScriptedMovementManager smm;

    public CapturePOISoldierGroup(string _name, int _teamIndex, POI _targetPOI)
    {
        m_Name = _name;
        m_TargetPOI = _targetPOI;
        m_TeamIndex = _teamIndex;

        capturePOIStatus = CapturePOIStatus.AssembleSoldiers;

        smm = m_TargetPOI.GetScriptedMovement();
    }

    public void RegisterSoldier(Soldier _soldier)
    {
        soldiers.Add(_soldier);
    }

    public void RegisterSoldiers(List<Soldier> _soldiers)
    {
        foreach(Soldier soldier in _soldiers)
        {
            RegisterSoldier(soldier);
        }
    }

    public void DeregisterSoldier(Soldier _soldier)
    {
        soldiers.Remove(_soldier);
    }

    public void BeginAssembleSoldiers()
    {
        List<AssemblySpot> assemblySpots = m_TargetPOI.GetAssemblySpots();

        int counter = 0;
        foreach (Soldier soldier in soldiers)
        {
            Vector2Int randPos = assemblySpots[counter%assemblySpots.Count].GetRandomTargetPosition();
            Vector3 position = new Vector3(InfluenceMapControl.Instance.transform.position.x + randPos.x, 0.0f, InfluenceMapControl.Instance.transform.position.x + randPos.y);
            soldier.GetComponent<GroupAIManager>().SetWalkToTargetState(position, Priority.job);
            m_AssignedAssemblySpots.Add(soldier, assemblySpots[counter % assemblySpots.Count]);
            m_AssignedWalkTargets.Add(soldier, position);
            counter++;
        }
    }

    public void AssembleSoldiers()
    {
        foreach (KeyValuePair<Soldier, Vector3> assignedWalkTarget in m_AssignedWalkTargets)
        {
            assignedWalkTarget.Key.GetComponent<GroupAIManager>().SetWalkToTargetState(assignedWalkTarget.Value, Priority.job);
        }
    }

    public void BeginCapture()
    {
        capturePOIStatus = CapturePOIStatus.Capture;

        foreach (Soldier soldier in soldiers)
        {
            Vector3 position = m_TargetPOI.GetRandomWalkableUnoocupiedOrEnemyPosition(soldier.GetComponent<Team>().GetTeamNumber());
            soldier.GetComponent<GroupAIManager>().SetWalkToTargetState(position, Priority.job);
            m_AssignedWalkTargets[soldier] = position;
        }

        if(smm != null)
        {
            smm.ResetScriptedMovement();
            smm.Subscribe(soldiers[0]);
            smm.Subscribe(soldiers[1]);
        }
    }

    public bool Capture()
    {
        bool ret = true;

        if(m_TargetPOI.GetTeamOccupation() != m_TeamIndex)
        {
            foreach (Soldier soldier in soldiers)
            {
                if(!soldier.GetComponent<GroupAIManager>().IsInAction())
                {
                    if (smm != null)
                    {
                        if (!smm.m_SubscribedSoldiers.Contains(soldier))
                        {
                            if (Vector3.Distance(soldier.transform.position, m_AssignedWalkTargets[soldier]) < 0.5f)
                            {
                                Vector3 position = m_TargetPOI.GetRandomWalkableUnoocupiedOrEnemyPosition(soldier.GetComponent<Team>().GetTeamNumber());
                                m_AssignedWalkTargets[soldier] = position;
                            }
                            soldier.GetComponent<GroupAIManager>().SetWalkToTargetState(m_AssignedWalkTargets[soldier], Priority.job);
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(soldier.transform.position, m_AssignedWalkTargets[soldier]) < 0.5f)
                        {
                            Vector3 position = m_TargetPOI.GetRandomWalkableUnoocupiedOrEnemyPosition(soldier.GetComponent<Team>().GetTeamNumber());
                            //soldier.GetComponent<GroupAIManager>().SetWalkToTargetState(position, Priority.job);
                            m_AssignedWalkTargets[soldier] = position;
                        }
                        soldier.GetComponent<GroupAIManager>().SetWalkToTargetState(m_AssignedWalkTargets[soldier], Priority.job);
                    }
                }
            }

            ret = false;
        }
        else
        {
            foreach (Soldier soldier in soldiers)
            {
                if (!soldier.GetComponent<GroupAIManager>().IsInAction())
                {
                    if (smm != null)
                    {
                        if (!smm.m_SubscribedSoldiers.Contains(soldier))
                        {
                            if (Vector3.Distance(soldier.transform.position, m_AssignedWalkTargets[soldier]) < 0.5f)
                            {
                                Vector3 position = m_TargetPOI.GetRandomWalkablePosition();
                                m_AssignedWalkTargets[soldier] = position;
                            }
                            soldier.GetComponent<GroupAIManager>().SetWalkToTargetState(m_AssignedWalkTargets[soldier], Priority.job);
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(soldier.transform.position, m_AssignedWalkTargets[soldier]) < 0.5f)
                        {
                            Vector3 position = m_TargetPOI.GetRandomWalkablePosition();
                            m_AssignedWalkTargets[soldier] = position;
                        }
                        soldier.GetComponent<GroupAIManager>().SetWalkToTargetState(m_AssignedWalkTargets[soldier], Priority.job);
                    }
                }
            }
        }

        if(smm != null)
        {
            if (!smm.Run())
            {
                ret = false;
            }
        }

        return ret;
    }

    public void BeginGuard(GroupAI _groupAI)
    {
        capturePOIStatus = CapturePOIStatus.Guard;
        ResetSoldierPriority();

        foreach (GuardSpot guardSpot in m_TargetPOI.GetGuardSpots())
        {
            Soldier soldier = _groupAI.GetClosestFreeSoldier(guardSpot.transform.position);
            if (soldier != null)
            {
                _groupAI.SetMoveToGuardSpotState(soldier, guardSpot);
            }
            else
            {
                return;
            }
        }
    }

    public bool AllSoldiersAtTarget()
    {
        foreach (Soldier soldier in soldiers)
        {
            if (!m_AssignedAssemblySpots[soldier].IsAtTarget(soldier))
            {
                return false;
            }
            else
            {
            }
        }

        return true;
    }

    private void ResetSoldierPriority()
    {
        foreach(Soldier soldier in soldiers)
        {
            soldier.GetComponent<GroupAIManager>().SetIdleState();
        }
    }

    public List<Soldier> GetSoldiers()
    {
        return soldiers;
    }


    public CapturePOIStatus GetCapturePOIStatus()
    {
        return capturePOIStatus;
    }
}
