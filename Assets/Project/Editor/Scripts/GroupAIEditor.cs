using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Microsoft.Win32;
using NUnit.Framework;

[CustomEditor(typeof(GroupAI))]
public class GroupAIEditor : Editor
{
    IEnumerator AttachSoldiersToFormation(int _index)
    {
        GroupAI groupAI = (GroupAI)target;

        yield return new WaitForSeconds(1);

        groupAI.SetFormationState(groupAI.m_Team[_index], groupAI.m_CurrentFormation);
        _index = (_index + 1) % groupAI.m_Team.Count;
        groupAI.SetFormationState(groupAI.m_Team[_index], groupAI.m_CurrentFormation);
        _index = (_index + 1) % groupAI.m_Team.Count;
        groupAI.SetFormationState(groupAI.m_Team[_index], groupAI.m_CurrentFormation);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GroupAI groupAI = (GroupAI)target;

        if (GUILayout.Button("Spawn Arrow Formation 3 at Random Unit"))
        {
            int randIndex = Random.Range(0, groupAI.m_Team.Count - 1);

            GameObject formation = Instantiate(groupAI.m_ArrowFormation3, groupAI.m_Team[randIndex].transform.position, groupAI.m_Team[randIndex].transform.rotation);
            groupAI.m_CurrentFormation = formation.GetComponent<Formation>();
            groupAI.m_CurrentFormation.SetTarget(new Vector3(0, 0, 0));
        }

        if (GUILayout.Button("Spawn and user Arrow Formation 3 at Random Unit"))
        {
            int randIndex = Random.Range(0, groupAI.m_Team.Count - 1);

            GameObject formation = Instantiate(groupAI.m_ArrowFormation3, groupAI.m_Team[randIndex].transform.position, groupAI.m_Team[randIndex].transform.rotation);
            groupAI.m_CurrentFormation = formation.GetComponent<Formation>();
            groupAI.m_CurrentFormation.SetTarget(new Vector3(0, 0, 0));

            EditorCoroutineUtility.StartCoroutine(AttachSoldiersToFormation(randIndex), this);
        }

        if(POIManager.Instance != null)
        {
            foreach (POI poi in POIManager.Instance.GetPOIs())
            {
                GUILayout.Label(poi.name, EditorStyles.boldLabel);

                List<POI> teamPOIs;
                if(groupAI.m_TeamIndex == 1)
                {
                    teamPOIs = POIManager.Instance.GetPOIsTeam1();
                }
                else
                {
                    teamPOIs = POIManager.Instance.GetPOIsTeam2();
                }

                if (!teamPOIs.Contains(poi))
                {
                    if (GUILayout.Button("Capture " + poi.name + " POI"))
                    {
                        CapturePOISoldierGroup soldierGroup = new CapturePOISoldierGroup(poi.name + " capture group", groupAI.m_TeamIndex, poi);
                        foreach(Soldier soldier in groupAI.m_Team)
                        {
                            if(soldier.GetComponent<GroupAIManager>().IsFree())
                            {
                                soldierGroup.RegisterSoldier(soldier);
                            }
                        }

                        soldierGroup.BeginAssembleSoldiers();
                        groupAI.m_SoldierGroups.Add(soldierGroup);
                    }
                }
                else
                {
                    if (GUILayout.Button("Set Guard Spots"))
                    {
                        Debug.Log("Set Guard Spots");
                        foreach (GuardSpot guardSpot in poi.GetGuardSpots())
                        {
                            Soldier soldier = groupAI.GetFreeSoldier();
                            if(soldier != null)
                            {
                                groupAI.SetMoveToGuardSpotState(soldier, guardSpot);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        foreach (Soldier soldier in groupAI.m_Team)
        {
            GUILayout.Label(soldier.name, EditorStyles.boldLabel);

            if(GUILayout.Button("Get closest POI."))
            {
                Debug.Log(POIManager.Instance.GetClosestUnoccupiedPOI(soldier.transform.position).name);
            }

            if(GUILayout.Button("Move To Random Position"))
            {
                Vector3 bottomLeft = InfluenceMapControl.Instance.GetBottomLeft();
                Vector3 topRight = InfluenceMapControl.Instance.GetTopRight();
                while(true)
                {

                    float randomX = Random.Range(bottomLeft.x, topRight.x);
                    float randomZ = Random.Range(bottomLeft.z, topRight.z);

                    Vector3 walkTarget = new Vector3(randomX, soldier.transform.position.y, randomZ);

                    NavMeshHit hit;

                    if (NavMesh.SamplePosition(walkTarget, out hit, 0.5f, NavMesh.AllAreas))
                    {
                        groupAI.SetWalkTarget(soldier, walkTarget, Priority.low);
                        return;
                    }
                }
            }

            if (GUILayout.Button("Move To closest enemy or unclaimed position"))
            {
                Vector2 walkTarget = InfluenceMapControl.Instance.GetClosestUnclaimedOrEnemyClaimed(new Vector2(soldier.transform.position.x, soldier.transform.position.z), soldier.GetComponent<Propagator>().Value);
                Debug.Log(soldier.GetComponent<Propagator>().Value);
                Debug.Log(soldier.transform.position + " -> " + walkTarget);
                groupAI.SetWalkTarget(soldier, new Vector3(walkTarget.x, 0.0f, walkTarget.y), Priority.low);
            }

            if (GUILayout.Button("Register To Formation."))
            {
                if(groupAI.m_CurrentFormation != null)
                {
                    groupAI.SetFormationState(soldier, groupAI.m_CurrentFormation);
                }
            }

            if (GUILayout.Button("Attack Enemy"))
            {
                if(soldier.IsInCover())
                {
                    // Attack from cover
                    groupAI.SetCoverAttackState(soldier, groupAI.m_VisibleEnemies[0]);
                }
                else
                {
                    // Open ground attack
                    groupAI.SetAttackState(soldier, groupAI.m_VisibleEnemies[0]);
                }
            }

            if (GUILayout.Button("Move To Cover"))
            {
                CoverManager.Instance.ExitCover(soldier.GetCurrentCoverSpot());
                groupAI.SetMoveToCoverState(soldier, CoverManager.Instance.GetCoverForClosestEnemies(soldier, groupAI.m_VisibleEnemies.ToArray(), 100, 1, soldier.GetCurrentCoverSpot()));
            }
        }
    }
}
