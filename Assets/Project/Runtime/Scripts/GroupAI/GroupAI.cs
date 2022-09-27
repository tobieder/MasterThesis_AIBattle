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
    private List<Soldier> m_VisibleEnemies = new List<Soldier>();
    public Dictionary<Soldier, EnemyVisionData> m_LastSeenPositionEnemies = new Dictionary<Soldier, EnemyVisionData>();

    private CapturePOISoldierGroup m_SoldierGroup;

    public SoldierSpawner m_SoldierSpawner;


    // --- SETTINGS ---
    public int m_TeamIndex;
    public int m_MaxSoldiers = 50;
    public int m_MaxConcrurrentSoldiers = 10;
    public LayerMask m_EnemyLayer;
    public float m_IgnoreEnemiesAfter = 60.0f;
    public float m_MaxHelpDistance = 60.0f;
    // --- SETTINGS ---

    private int m_SpawnedSoldiers;

    public GameObject m_ArrowFormation3;
    public Formation m_CurrentFormation;

    private void Awake()
    {
        m_SpawnedSoldiers = 0;
        m_SoldierGroup = null;

        InvokeRepeating("UpdateSoldierSpawn", 2.0f, 2.0f);

        //InvokeRepeating("UpdateGoals", 5.0f, 5.0f);

        InvokeRepeating("UpdateVisibleEnemies", 0.5f, 1.0f / 10.0f);
        InvokeRepeating("UpdateAgents", 1f, 1.0f / 10.0f);
        InvokeRepeating("UpdateGroups", 1.5f, 5.0f / 10.0f);

        m_SoldierSpawner.SpawnUnit();
        m_SoldierSpawner.SpawnUnit();
        m_SoldierSpawner.SpawnUnit();
    }

    private void UpdateSoldierSpawn()
    {
        if(m_SpawnedSoldiers < m_MaxSoldiers)
        {
            if(m_Team.Count < m_MaxConcrurrentSoldiers)
            {
                //m_SoldierSpawner.SpawnUnit();
                m_SpawnedSoldiers++;
            }
        }
    }

    private void UpdateGoals()
    {
        if(m_SoldierGroup == null)
        {
            // select random free soldier
            Soldier groupLeader = null;
            foreach(Soldier soldier in m_Team)
            {
                if(soldier.GetComponent<GroupAIManager>().IsFree())
                {
                    groupLeader = soldier;
                    break;
                }
            }

            if(groupLeader != null)
            {
                // Get all available Soldiers
                List<Soldier> availableSoldiers = new List<Soldier>();
                foreach(Soldier soldier in m_Team)
                {
                    if(soldier.GetComponent<GroupAIManager>().IsFree())
                    {
                        availableSoldiers.Add(soldier);
                    }
                }

                // select a POI to capture
                POI closestUnoccupiedPOI = null;
                POI closestEnemyPOI = null;

                float distanceClosestUnoccupiedPOI = float.PositiveInfinity;
                float distanceClosestEnemyPOI = float.PositiveInfinity;

                foreach (POI poi in POIManager.Instance.GetPOIs())
                {
                    if(poi.GetMinNumerOfRequiredSoldiersToCapture() <= availableSoldiers.Count)
                    {
                        if(poi.GetTeamOccupation() == 0)
                        {
                            if(closestUnoccupiedPOI == null)
                            {
                                closestUnoccupiedPOI = poi;
                            }
                            else
                            {
                                float distanceToCurrentPOI = Vector3.Distance(groupLeader.transform.position, poi.GetComponent<BoxCollider>().ClosestPointOnBounds(groupLeader.transform.position));
                                if (distanceToCurrentPOI < distanceClosestUnoccupiedPOI)
                                {
                                    distanceClosestUnoccupiedPOI = distanceToCurrentPOI;
                                    closestUnoccupiedPOI = poi;
                                }
                            }
                        }
                        else if(poi.GetTeamOccupation() != m_TeamIndex)
                        {
                            if (closestEnemyPOI == null)
                            {
                                closestEnemyPOI = poi;
                            }
                            else
                            {
                                float distanceToCurrentPOI = Vector3.Distance(groupLeader.transform.position, poi.GetComponent<BoxCollider>().ClosestPointOnBounds(groupLeader.transform.position));
                                if (distanceToCurrentPOI < distanceClosestEnemyPOI)
                                {
                                    distanceClosestEnemyPOI = distanceToCurrentPOI;
                                    closestEnemyPOI = poi;
                                }
                            }
                        }
                    }
                }

                POI poiToCapture = closestUnoccupiedPOI;
                if(poiToCapture == null)
                {

                    poiToCapture = closestEnemyPOI;
                }
                

                if(poiToCapture != null)
                {
                    CapturePOISoldierGroup soldierGroup = new CapturePOISoldierGroup(poiToCapture.name + " capture group", m_TeamIndex, poiToCapture);

                    soldierGroup.RegisterSoldiers(availableSoldiers);

                    Debug.Log("Capture group starting.");

                    soldierGroup.BeginAssembleSoldiers();
                    m_SoldierGroup = soldierGroup;
                }
                else
                {
                    Debug.Log("No poi available.");
                }
            }
        }
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

                                if (!m_LastSeenPositionEnemies.ContainsKey(enemy))
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
                }
            }
        }

        List<Soldier> soldiersToRemove = new List<Soldier>();
        foreach (KeyValuePair<Soldier, EnemyVisionData> lastSeenPositionEnemy in m_LastSeenPositionEnemies)
        {
            if(Time.time - lastSeenPositionEnemy.Value.lastSeenTime > m_IgnoreEnemiesAfter || lastSeenPositionEnemy.Key == null)
            {
                //m_LastSeenPositionEnemies.Remove(lastSeenPositionEnemy.Key);
                soldiersToRemove.Add(lastSeenPositionEnemy.Key);
            }
        }

        foreach(Soldier soldier in soldiersToRemove)
        {
            m_LastSeenPositionEnemies.Remove(soldier);
        }
    }

    private void UpdateAgents()
    {
        foreach (Soldier soldier in m_Team)
        {
            GroupAIManager gAIManager = soldier.GetComponent<GroupAIManager>();

            if(soldier.GetTarget() != null)
            {
                // Soldier already has an enemy as target

                Soldier savedTarget = soldier.GetTarget();

                if (soldier.ReadyToFire() &&
                    Vector3.Distance(soldier.transform.position, savedTarget.transform.position) < soldier.GetMaxAttackDistance()
                    )
                {
                    if (soldier.IsInCover())
                    {
                        // Attack from cover
                        SetCoverAttackState(soldier, savedTarget);
                    }
                    else
                    {
                        // Open ground attack
                        gAIManager.m_AttackState.SetNextState(gAIManager.m_MoveToCoverState);
                        SetAttackState(soldier, savedTarget);
                    }
                }

                if (gAIManager.m_CurrentState != gAIManager.m_MoveToCoverState && gAIManager.m_CurrentState != gAIManager.m_CoverState)
                {
                    if (soldier.ReadyToChangeCover())
                    {
                        //SetMoveToCoverState(soldier, CoverManager.Instance.GetCoverForEnemy(soldier, closestVisibleEnemy.transform.position, soldier.GetMaxAttackDistance(), soldier.GetMinAttackDistance(), soldier.GetCurrentCoverSpot()));
                        SetMoveToCoverState(soldier, CoverManager.Instance.GetCoverForEnemy(soldier, savedTarget.transform.position, soldier.GetMaxAttackDistance(), soldier.GetMinAttackDistance(), null));
                        soldier.ResetCoverChangeCooldown();
                    }
                }
            }
            else if(m_VisibleEnemies.Count > 0)
            {
                // other soldiers see enemies -> evaluate to help them

                Soldier closestVisibleEnemy = null;
                float closestVisibleEnemyDistance = float.PositiveInfinity;

                foreach (Soldier enemy in m_VisibleEnemies)
                {
                    if (enemy != null)
                    {
                        if (closestVisibleEnemy == null)
                        {
                            closestVisibleEnemy = enemy;
                            closestVisibleEnemyDistance = Vector3.Distance(soldier.transform.position, enemy.transform.position);
                        }
                        else
                        {
                            float currentEnemyDistance = Vector3.Distance(soldier.transform.position, enemy.transform.position);
                            if (currentEnemyDistance <= m_MaxHelpDistance && currentEnemyDistance < closestVisibleEnemyDistance)
                            {
                                closestVisibleEnemy = enemy;
                                closestVisibleEnemyDistance = currentEnemyDistance;
                            }
                        }
                    }
                }

                if(closestVisibleEnemy != null)
                {
                    // Someone spotted an enemy close to soldier -> go help
                    SetWalkTarget(soldier, closestVisibleEnemy.transform.position, Priority.job);
                }
            }
            else
            {
                // no enemies in sight select task to do

                if(gAIManager.IsInCover())
                {
                    gAIManager.SetIdleState();
                }
                else
                {
                    bool hasAJob = false;
                    if(m_SoldierGroup != null)
                    {
                        if (m_SoldierGroup.GetSoldiers().Contains(soldier))
                        {
                            hasAJob = true;
                            break;
                        }
                    }

                    if (!hasAJob)
                    {
                        if (gAIManager.DoesNothing())
                        {
                            // check if there are enemies to investigate

                            // --- evaluate lastSeenPositionEnemies
                            KeyValuePair<Soldier, EnemyVisionData> closestLastSeenPositionEnemy = new KeyValuePair<Soldier, EnemyVisionData>();

                            if (m_LastSeenPositionEnemies.Count > 0)
                            {
                                float distanceClosestLastSeenPositionEnenmy = float.PositiveInfinity;

                                foreach (KeyValuePair<Soldier, EnemyVisionData> lastSeenPositionEnemy in m_LastSeenPositionEnemies)
                                {
                                    float currentDistance = Vector3.Distance(soldier.transform.position, lastSeenPositionEnemy.Key.transform.position);
                                    if (currentDistance < m_MaxHelpDistance && currentDistance < distanceClosestLastSeenPositionEnenmy)
                                    {
                                        closestLastSeenPositionEnemy = lastSeenPositionEnemy;
                                        distanceClosestLastSeenPositionEnenmy = currentDistance;
                                    }
                                }
                            }
                            // ---

                            if (!closestLastSeenPositionEnemy.Equals(new KeyValuePair<Soldier, EnemyVisionData>()))
                            {
                                // enemy position to investigate available

                                if (soldier.IsTargetVisible(closestLastSeenPositionEnemy.Key))
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
                                /*
                                // nothing to do -> roam unclaimed or enemy area
                                Vector2 walkTarget = InfluenceMapControl.Instance.GetClosestUnclaimedOrEnemyClaimed(new Vector2(soldier.transform.position.x, soldier.transform.position.z), soldier.GetComponent<Propagator>().Value);
                                NavMeshHit hit;
                                if (NavMesh.SamplePosition(new Vector3(walkTarget.x, 0.0f, walkTarget.y), out hit, 100f, NavMesh.AllAreas))
                                {
                                    SetWalkTarget(soldier, hit.position, Priority.low);
                                }
                                */

                                /*
                                if(gAIManager.GetTimeSinceNonePriority() > Random.Range(5.0f, 30.0f))
                                {
                                    POI closestPOI = POIManager.Instance.GetClosestTeamPOI(soldier);
                                    if(closestPOI != null)
                                    {
                                        Vector3 walkTarget = closestPOI.GetRandomWalkablePosition();
                                        SetWalkTarget(soldier, walkTarget, Priority.low);
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
                                else
                                {
                                    gAIManager.SetIdleState();
                                }
                                */

                                gAIManager.SetIdleState();
                            }
                        }
                    }
                }

            }
        }
    }

    private void UpdateGroups()
    {
        if(m_SoldierGroup != null)
        {
            switch (m_SoldierGroup.GetCapturePOIStatus())
            {
                case CapturePOIStatus.AssembleSoldiers:
                    if (m_SoldierGroup.AllSoldiersAtTarget())
                    {
                        // All soldiers ready to capture POI
                        Debug.Log(m_SoldierGroup.m_Name + " ready to capture poi");
                        m_SoldierGroup.BeginCapture();
                    }
                    break;
                case CapturePOIStatus.Capture:
                    // TODO: check if clear of enemies!
                    //if (soldierGroup.m_TargetPOI.GetTeamOccupation() == m_TeamIndex)
                    if (m_SoldierGroup.Capture())
                    {
                        m_SoldierGroup.BeginGuard(this);
                    }
                    break;
                case CapturePOIStatus.Guard:
                    // Sent all units to their guard positions -> destroy group
                    m_SoldierGroup = null;
                    break;
            }
        }
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
        if(m_SoldierGroup != null)
        {
            m_SoldierGroup.DeregisterSoldier(_soldier);
        }

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

    // Getter & Setter
    public List<Soldier> GetVisibleSoldiers()
    {
        return m_VisibleEnemies;
    }

    public CapturePOISoldierGroup GetCapturePOISoldierGroup()
    {
        return m_SoldierGroup;
    }

    // Debug
    private void OnDrawGizmosSelected()
    {
        if(m_VisibleEnemies != null)
        {
            Gizmos.color = Color.green;
            foreach (Soldier enemy in m_VisibleEnemies)
            {
                if(enemy != null)
                {
                    Gizmos.DrawSphere(enemy.transform.position, 1.0f);
                }
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
