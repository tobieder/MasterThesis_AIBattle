using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

public struct EnemyVisionData
{
    public EnemyVisionData(Vector3 _position, float _lastSeenTime)
    {
        position = _position;
        lastSeenTime = _lastSeenTime;
    }

    public Vector3 position;
    public float lastSeenTime;
}

public class GroupAI : MonoBehaviour
{
    public List<Soldier> m_Team = new List<Soldier>();
    public List<Soldier> m_VisibleEnemies = new List<Soldier>();
    public Dictionary<Soldier, EnemyVisionData> m_LastSeenPositionEnemies = new Dictionary<Soldier, EnemyVisionData>();

    public List<CapturePOISoldierGroup> m_SoldierGroups = new List<CapturePOISoldierGroup>();


    // --- SETTINGS ---
    public int m_TeamIndex;
    public LayerMask m_EnemyLayer;
    public float m_IgnoreEnemiesAfter = 60.0f;
    // --- SETTINGS ---

    public GameObject m_ArrowFormation3;
    public Formation m_CurrentFormation;

    private void Awake()
    {
        InvokeRepeating("UpdateVisibleEnemies", 0.5f, 1.0f / 10.0f);
        InvokeRepeating("UpdateAgents", 1f, 1.0f / 10.0f);
        InvokeRepeating("UpdateGroups", 1.5f, 5.0f / 10.0f);
    }

    private void Update()
    {
    }

    private void UpdateVisibleEnemies()
    {
        m_VisibleEnemies.Clear();

        foreach (Soldier soldier in m_Team)
        {
            foreach (Collider enemyCollider in Physics.OverlapSphere(soldier.transform.position, soldier.GetViewDistance(), m_EnemyLayer))
            {
                Soldier enemy = enemyCollider.GetComponent<Soldier>();
                if (!m_VisibleEnemies.Contains(enemy))
                {
                    // Check if enemy is in fov
                    float angleSoldierToEnemy = Vector3.Angle(enemy.transform.position - soldier.transform.position, soldier.transform.forward);
                    if (angleSoldierToEnemy < soldier.GetFieldOfView() / 2.0f)
                    {
                        RaycastHit hit;
                        // Check for obstacles
                        if(Physics.Raycast(soldier.m_Eyes.position, enemy.m_Eyes.position - soldier.m_Eyes.position, out hit, soldier.GetViewDistance()))
                        {
                            if(hit.collider == enemyCollider)
                            {
                                Debug.DrawLine(soldier.m_Eyes.position, hit.point, Color.magenta, 1f/10f);
                                m_VisibleEnemies.Add(enemy);
                            }
                        }
                    }

                    if(!m_LastSeenPositionEnemies.ContainsKey(enemy))
                    {
                        EnemyVisionData evd = new EnemyVisionData(enemy.transform.position, Time.time);
                        m_LastSeenPositionEnemies.Add(enemy, evd);
                    }
                    else
                    {
                        EnemyVisionData evd = new EnemyVisionData(enemy.transform.position, Time.time);
                        m_LastSeenPositionEnemies[enemy] = evd;
                    }
                }
            }
        }

        Soldier soldierToRemove = null;
        foreach (KeyValuePair<Soldier, EnemyVisionData> lastSeenPositionEnemy in m_LastSeenPositionEnemies)
        {
            if(Time.time - lastSeenPositionEnemy.Value.lastSeenTime > m_IgnoreEnemiesAfter)
            {
                //m_LastSeenPositionEnemies.Remove(lastSeenPositionEnemy.Key);
                soldierToRemove = lastSeenPositionEnemy.Key;
            }
        }

        if (soldierToRemove != null)
        {
            m_LastSeenPositionEnemies.Remove(soldierToRemove);
        }
    }

    private void UpdateAgents()
    {
        foreach (Soldier soldier in m_Team)
        {
            GroupAIManager gAIManager = soldier.GetComponent<GroupAIManager>();

            if(m_VisibleEnemies.Count > 0)
            {
                // --- Enemy - SELECT ---
                Soldier closestVisibleEnemy = null;
                float closestVisibleEnemyDistance = float.PositiveInfinity;

                foreach (Soldier enemy in m_VisibleEnemies)
                {
                    if (closestVisibleEnemy == null)
                    {
                        closestVisibleEnemy = enemy;
                        closestVisibleEnemyDistance = Vector3.Distance(soldier.transform.position, enemy.transform.position);
                    }
                    else
                    {
                        float currentEnemyDistance = Vector3.Distance(soldier.transform.position, enemy.transform.position);
                        if (currentEnemyDistance < closestVisibleEnemyDistance)
                        {
                            closestVisibleEnemy = enemy;
                            closestVisibleEnemyDistance = currentEnemyDistance;
                        }
                    }
                }
                // --- Enemy - SELECT ---

                /*
                if(closestVisibleEnemy != null)
                {
                    if(soldier.ReadyToFire() && closestVisibleEnemyDistance < soldier.GetMaxAttackDistance())
                    {
                        if (soldier.IsInCover())
                        {
                            // Attack from cover
                            SetCoverAttackState(soldier, closestVisibleEnemy);
                        }
                        else
                        {
                            // Open ground attack
                            gAIManager.m_AttackState.SetNextState(gAIManager.m_MoveToCoverState);
                            SetAttackState(soldier, closestVisibleEnemy);
                        }
                    }

                    if (gAIManager.m_CurrentState != gAIManager.m_MoveToCoverState && gAIManager.m_CurrentState != gAIManager.m_CoverState)
                    {
                        if (soldier.ReadyToChangeCover())
                        {
                            //SetMoveToCoverState(soldier, CoverManager.Instance.GetCoverForEnemy(soldier, closestVisibleEnemy.transform.position, soldier.GetMaxAttackDistance(), soldier.GetMinAttackDistance(), soldier.GetCurrentCoverSpot()));
                            SetMoveToCoverState(soldier, CoverManager.Instance.GetCoverForEnemy(soldier, closestVisibleEnemy.transform.position, soldier.GetMaxAttackDistance(), soldier.GetMinAttackDistance(), null));
                            soldier.ResetCoverChangeCooldown();
                        }
                    }
                }
                */
            }
            else
            {
                // check if NPC is part of a group
                bool hasAJob = false;
                foreach(CapturePOISoldierGroup soldierGroup in m_SoldierGroups)
                {
                    if(soldierGroup.GetSoldiers().Contains(soldier))
                    {
                        hasAJob = true;
                        break;
                    }
                }

                if(!hasAJob)
                {
                    // check if NPC has no job currently
                    if (gAIManager.m_CurrentState == gAIManager.m_IdleState || gAIManager.m_CurrentState == gAIManager.m_CoverState)
                    {
                        if (InfluenceMapControl.Instance.AreAllValuesTheSame())
                        {
                            // Team has won
                        }
                        else if (m_LastSeenPositionEnemies.Count > 0)
                        {
                            KeyValuePair<Soldier, EnemyVisionData> closestLastSeenPositionEnemy = new KeyValuePair<Soldier, EnemyVisionData>();
                            float distanceClosestLastSeenPositionEnenmy = float.PositiveInfinity;
                            // check for enemies that are no longer visible
                            foreach (KeyValuePair<Soldier, EnemyVisionData> lastSeenPositionEnemy in m_LastSeenPositionEnemies)
                            {
                                float currentDistance = Vector3.Distance(soldier.transform.position, lastSeenPositionEnemy.Key.transform.position);
                                if (currentDistance < distanceClosestLastSeenPositionEnenmy)
                                {
                                    closestLastSeenPositionEnemy = lastSeenPositionEnemy;
                                    distanceClosestLastSeenPositionEnenmy = currentDistance;
                                }
                            }

                            if (Vector3.Distance(soldier.transform.position, closestLastSeenPositionEnemy.Key.transform.position) < soldier.GetViewDistance())
                            {
                                // Enemy is still in view -> Look at enemy to start attack
                                soldier.transform.LookAt(closestLastSeenPositionEnemy.Key.transform.position);
                            }
                            else
                            {
                                // No enemy in range, but some enemies have been spotted earlier -> investigate
                                SetWalkTarget(soldier, closestLastSeenPositionEnemy.Key.transform.position, Priority.low);
                            }
                        }
                        else
                        {
                            Vector2 walkTarget = InfluenceMapControl.Instance.GetClosestUnclaimedOrEnemyClaimed(new Vector2(soldier.transform.position.x, soldier.transform.position.z), soldier.GetComponent<Propagator>().Value);
                            NavMeshHit hit;
                            if (NavMesh.SamplePosition(new Vector3(walkTarget.x, 0.0f, walkTarget.y), out hit, 100f, NavMesh.AllAreas))
                            {
                                SetWalkTarget(soldier, hit.position, Priority.low);
                            }
                        }
                    }
                }
            }
        }
    }

    private void UpdateGroups()
    {
        CapturePOISoldierGroup groupToRemove = null;
        foreach (CapturePOISoldierGroup soldierGroup in m_SoldierGroups)
        {
            switch (soldierGroup.GetCapturePOIStatus())
            {
                case CapturePOIStatus.AssembleSoldiers:
                    if (soldierGroup.AllSoldiersAtTarget())
                    {
                        // All soldiers ready to capture POI
                        Debug.Log(soldierGroup.m_Name + " ready to capture poi");
                        soldierGroup.BeginCapture();
                    }
                    break;
                case CapturePOIStatus.Capture:
                    // TODO: check if clear of enemies!
                    //if (soldierGroup.m_TargetPOI.GetTeamOccupation() == m_TeamIndex)
                    if (soldierGroup.Capture())
                    {
                        soldierGroup.BeginGuard(this);
                    }
                    break;
                case CapturePOIStatus.Guard:
                    // Sent all units to their guard positions -> destroy group
                    groupToRemove = soldierGroup;
                    break;
            }
        }
        m_SoldierGroups.Remove(groupToRemove);
    }

    // Functionality
    public Soldier GetFreeSoldier()
    {
        foreach(Soldier soldier in m_Team)
        {
            GroupAIManager gAIManager = soldier.GetComponent<GroupAIManager>();
            if(gAIManager.IsFree())
            {
                return soldier;
            }
        }

        return null;
    }

    public Soldier GetClosestFreeSoldier(Vector3 _position)
    {
        float closestDistance = float.MaxValue;
        Soldier closestSoldier = null;
        foreach (Soldier soldier in m_Team)
        {
            GroupAIManager gAIManager = soldier.GetComponent<GroupAIManager>();
            if (gAIManager.IsFree())
            {
                float currDistance = Vector3.Distance(_position, soldier.transform.position);
                if (currDistance < closestDistance)
                {
                    closestDistance = currDistance;
                    closestSoldier = soldier;
                }
            }
        }

        return closestSoldier;
    }

    // Soldier Registration
    public void RegisterSoldier(Soldier _soldier)
    {
        m_Team.Add(_soldier);
    }

    public void DeregisterSoldier(Soldier _soldier)
    {
        m_Team.Remove(_soldier);
    }

    // State Managment
    public void SetWalkTarget(Soldier _soldier, Vector3 _target, Priority _priority)
    {
        _soldier.GetComponent<GroupAIManager>().SetWalkToTargetState(_target, _priority);
    }

    public void SetAttackState(Soldier _soldier, Soldier _target)
    {
        _soldier.GetComponent<GroupAIManager>().SetAttackState(_target);
    }

    public void SetMoveToCoverState(Soldier _soldier, CoverSpot _coverSpot)
    {
        _soldier.GetComponent<GroupAIManager>().SetMoveToCoverState(_coverSpot);
    }

    public void SetMoveToGuardSpotState(Soldier _soldier, GuardSpot _guardSpot)
    {
        _soldier.GetComponent<GroupAIManager>().SetMoveToGuardSpotState(_guardSpot);
    }

    public void SetCoverAttackState(Soldier _soldier, Soldier _target)
    {
        _soldier.GetComponent<GroupAIManager>().SetCoverAttackState(_target);
    }

    public void SetFormationState(Soldier _soldier, Formation _formation)
    {
        _soldier.GetComponent<GroupAIManager>().SetFormationState(_formation);
    }

    // Debug
    private void OnDrawGizmosSelected()
    {
        if(m_VisibleEnemies != null)
        {
            Gizmos.color = Color.green;
            foreach (Soldier enemy in m_VisibleEnemies)
            {
                Gizmos.DrawSphere(enemy.transform.position, 1.0f);
            }
        }

        if(m_LastSeenPositionEnemies != null)
        {
            Gizmos.color = Color.yellow;
            foreach (KeyValuePair<Soldier, EnemyVisionData> keyValuePair in m_LastSeenPositionEnemies)
            {
                if(!m_VisibleEnemies.Contains(keyValuePair.Key))
                {
                    Gizmos.DrawSphere(keyValuePair.Value.position, 1.0f);
                    Handles.Label(keyValuePair.Value.position, (Time.time - keyValuePair.Value.lastSeenTime).ToString("F2"));
                }
            }
        }

        if(m_Team != null)
        {
            Gizmos.color = Color.blue;
            foreach (Soldier soldier in m_Team)
            {
                Gizmos.DrawSphere(soldier.transform.position, 1.0f);
            }
        }
    }
}
