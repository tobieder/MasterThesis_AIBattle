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

    ScriptedMovementManager m_SMM;
    //List<Formation> m_Formations;
    //Dictionary<Soldier, Formation> m_AssignedFormation = new Dictionary<Soldier, Formation>();

    public CapturePOISoldierGroup(string _name, int _teamIndex, POI _targetPOI)
    {
        m_Name = _name;
        m_TargetPOI = _targetPOI;
        m_TeamIndex = _teamIndex;

        capturePOIStatus = CapturePOIStatus.AssembleSoldiers;

        m_SMM = m_TargetPOI.GetScriptedMovement();
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
        m_AssignedWalkTargets.Remove(_soldier);
        m_AssignedAssemblySpots.Remove(_soldier);

        if(m_SMM != null)
        {
            m_SMM.Deregister(_soldier);
        }

        if(soldiers.Count < m_TargetPOI.GetMinNumerOfRequiredSoldiersToCapture())
        {
            capturePOIStatus = CapturePOIStatus.EOL;
        }
    }

    public void BeginAssembleSoldiers(GroupAI _groupAI)
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

        /*
        List<Soldier> soldiersInFormation = new List<Soldier>();

        foreach(AssemblySpot assemblySpot in assemblySpots)
        {
            List<Soldier> soldiersWithSameAssemblySpot = new List<Soldier>();

            foreach(KeyValuePair<Soldier, AssemblySpot> assignedAssemblySpot in m_AssignedAssemblySpots)
            {
                if(assignedAssemblySpot.Value == assemblySpot)
                {
                    soldiersWithSameAssemblySpot.Add(assignedAssemblySpot.Key);
                }
            }

            foreach(Soldier soldier in soldiersWithSameAssemblySpot)
            {
                float distanceToAssemblySpot = Vector3.Distance(soldier.transform.position, assemblySpot.transform.position);

                if(distanceToAssemblySpot < 1000.0f && !soldiersInFormation.Contains(soldier))
                {
                    List<Soldier> possibleFormationMembers = new List<Soldier>();
                    possibleFormationMembers.Add(soldier);

                    foreach (Soldier soldierTeammate in soldiersWithSameAssemblySpot)
                    {
                        if (soldier != soldierTeammate)
                        {

                            float distance = Vector3.Distance(soldier.transform.position, soldierTeammate.transform.position);
                            if (distance < 15.0f)
                            {
                                possibleFormationMembers.Add(soldierTeammate);
                            }
                        }
                    }

                    if (possibleFormationMembers.Count >= 3)
                    {
                        List<Soldier> formationMembers = new List<Soldier>();

                        formationMembers.Add(possibleFormationMembers[0]);
                        formationMembers.Add(possibleFormationMembers[1]);
                        formationMembers.Add(possibleFormationMembers[2]);

                        Debug.Log(formationMembers.Count + ", " + assemblySpot.transform.position);

                        Formation currentFormation = _groupAI.CreateFormation(formationMembers, assemblySpot.transform.position);
                        m_Formations.Add(currentFormation);

                        foreach(Soldier soldierToAssign in formationMembers)
                        {
                            m_AssignedFormation[soldierToAssign] = currentFormation;
                        }
                    }
                }
            }
        }
        */
    }

    public void AssembleSoldiers()
    {
        foreach (KeyValuePair<Soldier, Vector3> assignedWalkTarget in m_AssignedWalkTargets)
        {
            assignedWalkTarget.Key.GetComponent<GroupAIManager>().SetWalkToTargetState(assignedWalkTarget.Value, Priority.job);

            /*
            if (!IsPartOfAnyFormation(assignedWalkTarget.Key))
            {
            }
            else
            {
                if (m_AssignedFormation[assignedWalkTarget.Key] != null)
                {
                    assignedWalkTarget.Key.GetComponent<GroupAIManager>().SetFormationState(m_AssignedFormation[assignedWalkTarget.Key]);
                }
                else
                {
                    m_AssignedFormation.Remove(assignedWalkTarget.Key);
                }
            }
            */
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

        if(m_SMM != null)
        {
            m_SMM.ResetScriptedMovement();

            for (int i = 0; i < m_SMM.m_SoldiersNeeded; i++)
            {
                m_SMM.Subscribe(soldiers[i]);
            }
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
                    if (m_SMM != null)
                    {
                        if (!m_SMM.m_SubscribedSoldiers.Contains(soldier))
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
                    if (m_SMM != null)
                    {
                        if (!m_SMM.m_SubscribedSoldiers.Contains(soldier))
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

        if(m_SMM != null)
        {
            if (!m_SMM.Run())
            {
                if(m_SMM.m_SubscribedSoldiers.Count < m_SMM.m_SoldiersNeeded)
                {
                    Debug.Log("SMM doesn't habe enough units.");
                    foreach(Soldier soldier in soldiers)
                    {
                        if(!m_SMM.m_SubscribedSoldiers.Contains(soldier))
                        {
                            m_SMM.Subscribe(soldier);
                            break;
                        }
                    }
                }

                ret = false;
            }
            else
            {
                m_SMM = null;
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
                if(!guardSpot.IsOccupied())
                {
                    _groupAI.SetMoveToGuardSpotState(soldier, guardSpot);
                }
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

    /*
    private bool IsPartOfAnyFormation(Soldier _soldier)
    {
        foreach(Formation formation in m_Formations)
        {
            if(formation.GetFormationSoldiers().Contains(_soldier))
            {
                return true;
            }
        }

        return false;
    }
    */

    public CapturePOIStatus GetCapturePOIStatus()
    {
        return capturePOIStatus;
    }
}
