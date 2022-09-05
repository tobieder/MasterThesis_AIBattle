using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

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

    // --- SETTINGS ---
    public LayerMask m_EnemyLayer;
    public float m_IgnoreEnemiesAfter = 60.0f;
    // --- SETTINGS ---

    public GameObject m_ArrowFormation3;
    public Formation m_CurrentFormation;

    private void Awake()
    {
        InvokeRepeating("UpdateVisibleEnemies", 0.001f, 1.0f / 10.0f);
        InvokeRepeating("UpdateAgents", 2f, 1.0f / 10.0f);
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
                        m_VisibleEnemies.Add(enemy);
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

        foreach (KeyValuePair<Soldier, EnemyVisionData> lastSeenPositionEnemy in m_LastSeenPositionEnemies)
        {
            if(Time.time - lastSeenPositionEnemy.Value.lastSeenTime > m_IgnoreEnemiesAfter)
            {
                m_LastSeenPositionEnemies.Remove(lastSeenPositionEnemy.Key);
            }
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
            }
            else
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
                            SetWalkTarget(soldier, closestLastSeenPositionEnemy.Key.transform.position);
                        }
                    }
                    else
                    {
                        Vector2 walkTarget = InfluenceMapControl.Instance.GetClosestUnclaimedOrEnemyClaimed(new Vector2(soldier.transform.position.x, soldier.transform.position.z), soldier.GetComponent<Propagator>().Value);
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(new Vector3(walkTarget.x, 0.0f, walkTarget.y), out hit, 100f, NavMesh.AllAreas))
                        {
                            SetWalkTarget(soldier, hit.position);
                        }
                    }
                }
            }
        }
    }

    public void RegisterSoldier(Soldier _soldier)
    {
        m_Team.Add(_soldier);
    }

    public void DeregisterSoldier(Soldier _soldier)
    {
        m_Team.Remove(_soldier);
    }

    public void SetWalkTarget(Soldier _soldier, Vector3 _target)
    {
        _soldier.GetComponent<GroupAIManager>().SetWalkToTargetState(_target);
    }

    public void SetAttackState(Soldier _soldier, Soldier _target)
    {
        _soldier.GetComponent<GroupAIManager>().SetAttackState(_target);
    }

    public void SetMoveToCoverState(Soldier _soldier, CoverSpot _coverSpot)
    {
        _soldier.GetComponent<GroupAIManager>().SetMoveToCoverState(_coverSpot);
    }

    public void SetCoverAttackState(Soldier _soldier, Soldier _target)
    {
        _soldier.GetComponent<GroupAIManager>().SetCoverAttackState(_target);
    }

    public void SetFormationState(Soldier _soldier, Formation _formation)
    {
        _soldier.GetComponent<GroupAIManager>().SetFormationState(_formation);
    }

    private void OnDrawGizmosSelected()
    {
        if(m_VisibleEnemies != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Soldier enemy in m_VisibleEnemies)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, 1.0f);
            }
        }

        if(m_LastSeenPositionEnemies != null)
        {
            Gizmos.color = Color.green;
            foreach (KeyValuePair<Soldier, EnemyVisionData> keyValuePair in m_LastSeenPositionEnemies)
            {
                Gizmos.DrawWireSphere(keyValuePair.Value.position, 1.0f);
                Handles.Label(keyValuePair.Value.position, (Time.time - keyValuePair.Value.lastSeenTime).ToString());
            }
        }

        if(m_Team != null)
        {
            Gizmos.color = Color.blue;
            foreach (Soldier soldier in m_Team)
            {
                Gizmos.DrawWireSphere(soldier.transform.position, 1.0f);
            }
        }
    }
}
