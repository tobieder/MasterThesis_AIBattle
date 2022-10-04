using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;
using UnityEngine.PlayerLoop;
using static UnityEngine.GraphicsBuffer;

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

public class CoverDebugData
{
    public CoverDebugData(Vector3 _position, float _value)
    {
        position = _position;
        value = _value;
    }

    public Vector3 position;
    public float value;
}

public class GroupAI : MonoBehaviour
{
    public List<Soldier> m_Team = new List<Soldier>();
    private List<Soldier> m_VisibleEnemies = new List<Soldier>();
    public Dictionary<Soldier, EnemyVisionData> m_LastSeenPositionEnemies = new Dictionary<Soldier, EnemyVisionData>();

    private CapturePOISoldierGroup m_SoldierGroup;

    public SoldierSpawner m_SoldierSpawner;


    // --- SETTINGS ---
    public float m_StartDelay = 0.0f;
    public int m_TeamIndex;
    public int m_MaxSoldiers = 50;
    public int m_MaxConcrurrentSoldiers = 10;
    public LayerMask m_EnemyLayer;
    public LayerMask m_CoverLayer;
    [Range(-1.0f, 1.0f)]
    public float m_HideSensitivity = 0.0f;
    public float m_IgnoreEnemiesAfter = 60.0f;
    public float m_MaxHelpDistance = 60.0f;
    // --- SETTINGS ---

    // --- FEATURES SELECTION ---
    [SerializeField]
    private bool m_EnableSpawnSoldiers;
    [SerializeField]
    private bool m_EnableUpdateGoals;
    [SerializeField]
    private bool m_EnableUpdateVisibleEnemies;
    [SerializeField]
    private bool m_EnableUpdateAgents;
    [SerializeField]
    private bool m_EnableUpdateGroups;
    // --- FEATURES SELECTION ---

    // --- FUNCTIONALITY ---
    private int m_SpawnedSoldiers;
    private Soldier m_CurrentSoldier;
    public List<CoverDebugData> m_CoverDebugData = new List<CoverDebugData>();
    private Dictionary<Soldier, List<Soldier>> m_VisibleEnemiesFromSoldier = new Dictionary<Soldier, List<Soldier>>();
    // --- FUNCTIONALITY ---

    public GameObject m_ArrowFormation3;

    private void Awake()
    {
        m_SpawnedSoldiers = 0;
        m_SoldierGroup = null;

        if(m_EnableSpawnSoldiers)
            InvokeRepeating("UpdateSoldierSpawn", m_StartDelay + 2.0f, 1.0f);

        if (m_EnableUpdateGoals)
            InvokeRepeating("UpdateGoals", m_StartDelay + 10.0f, 2.5f);

        if (m_EnableUpdateVisibleEnemies)
            InvokeRepeating("UpdateVisibleEnemies", m_StartDelay + 0.5f, 1.0f / 10.0f);
        if (m_EnableUpdateAgents)
            InvokeRepeating("UpdateAgents", m_StartDelay + 1f, 1.0f / 10.0f);
        if (m_EnableUpdateGroups)
            InvokeRepeating("UpdateGroups", m_StartDelay + 1.5f, 5.0f / 10.0f);

    }

    private void UpdateSoldierSpawn()
    {
        if(m_SpawnedSoldiers < m_MaxSoldiers)
        {
            if(m_Team.Count < m_MaxConcrurrentSoldiers)
            {
                SpawnUnit();
            }
        }
    }

    private void UpdateGoals()
    {
        if(m_TeamIndex == -1)
        {
            foreach(POI poi in POIManager.Instance.GetPOIsTeam2())
            {
                List<GuardSpot> freeGuardSpots = poi.GetFreeGuardSpots();

                foreach (GuardSpot guardSpot in freeGuardSpots)
                {
                    Soldier soldier = GetClosestFreeSoldier(guardSpot.transform.position);
                    if (soldier != null)
                    {
                        SetMoveToGuardSpotState(soldier, guardSpot);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
        if (m_TeamIndex == 1)
        {
            foreach (POI poi in POIManager.Instance.GetPOIsTeam1())
            {
                List<GuardSpot> freeGuardSpots = poi.GetFreeGuardSpots();

                foreach (GuardSpot guardSpot in freeGuardSpots)
                {
                    Soldier soldier = GetClosestFreeSoldier(guardSpot.transform.position);
                    if (soldier != null)
                    {
                        SetMoveToGuardSpotState(soldier, guardSpot);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        if (m_SoldierGroup == null)
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
                        BoxCollider poiBC = poi.GetComponent<BoxCollider>();
                        Collider[] colliders = new Collider[25];
                        int enemyCount = Physics.OverlapBoxNonAlloc(poi.transform.position + poiBC.center, poiBC.size / 2.0f, colliders, poiBC.transform.rotation, m_EnemyLayer);

                        if (poi.GetTeamOccupation() == 0)
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

                    soldierGroup.BeginAssembleSoldiers(this);
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
            List<Soldier> visibleEnemiesFromSoldierList = new List<Soldier>();

            foreach (Collider enemyCollider in Physics.OverlapSphere(soldier.transform.position, soldier.GetViewDistance(), m_EnemyLayer))
            {
                Soldier enemy = enemyCollider.GetComponent<Soldier>();

                // Check if enemy is in fov
                Vector3 directionToEnemy = (enemy.transform.position - soldier.transform.position).normalized;
                if (Vector3.Dot(soldier.transform.forward, directionToEnemy) >= Mathf.Cos(soldier.GetFieldOfView()))
                {
                    // Check for obstacles
                    if(Physics.Raycast(soldier.m_Eyes.position, (enemy.m_Eyes.position - soldier.m_Eyes.position).normalized, out RaycastHit hit, soldier.GetViewDistance()))
                    {
                        if(hit.collider == enemyCollider)
                        {
                            Debug.DrawLine(soldier.m_Eyes.position, hit.point, Color.magenta, 1f/10f);

                            visibleEnemiesFromSoldierList.Add(enemy);

                            if (!m_VisibleEnemies.Contains(enemy))
                            {
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

            m_VisibleEnemiesFromSoldier[soldier] = visibleEnemiesFromSoldierList;
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
            if(gAIManager == null)
            {
                continue;
            }

            if(soldier.GetTarget() != null)
            {
                // Soldier already has an enemy as target

                Soldier savedTarget = soldier.GetTarget();

                if (soldier.ReadyToFire() &&
                    Vector3.Distance(soldier.transform.position, savedTarget.transform.position) < soldier.GetMaxAttackDistance()
                    )
                {
                    if (gAIManager.m_CurrentState == gAIManager.m_CoverState)
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
                else if (gAIManager.m_CurrentState != gAIManager.m_MoveToCoverState && 
                    gAIManager.m_CurrentState != gAIManager.m_WaitState &&
                    gAIManager.m_CurrentState != gAIManager.m_AttackState &&
                    gAIManager.m_CurrentState != gAIManager.m_CoverAttackState
                    )
                {
                    if (soldier.ReadyToChangeCover())
                    {
                        Vector3 bestPossibleCoverPosition = GetBestPossibleCoverPosition(soldier);
                        if(bestPossibleCoverPosition != Vector3.zero)
                        {
                            SetMoveToCoverState(soldier, CoverManager.Instance.GetClosestCoverToPosition(soldier, bestPossibleCoverPosition));
                            soldier.ResetCoverChangeCooldown();
                        }
                        else
                        {
                            soldier.ResetCurrentCover();
                            Vector3 directionEnemyToSoldier = (soldier.transform.position - soldier.GetTarget().transform.position).normalized;
                            Vector3 targetPosition = soldier.GetTarget().transform.position + (directionEnemyToSoldier * soldier.GetMinAttackDistance());
                            if(NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
                            {
                                SetWalkToTargetState(soldier, hit.position, Priority.low);
                            }
                        }
                    }
                }
            }
            else if(m_VisibleEnemies.Count > 0)
            {
                // other soldiers see enemies -> evaluate to help them

                bool freeToHelp = true;
                if(soldier.GetCurrentGuardSpot() != null)
                {
                    if(soldier.GetCurrentGuardSpot().GetStayOnDuty())
                    {
                        freeToHelp = false;
                    }
                }

                if(freeToHelp)
                {
                    Soldier closestVisibleEnemy = null;
                    float closestVisibleEnemyDistance = float.PositiveInfinity;

                    foreach (Soldier enemy in m_VisibleEnemies)
                    {
                        if (enemy != null)
                        {
                            if (closestVisibleEnemy == null)
                            {
                                float currentEnemyDistance = Vector3.Distance(soldier.transform.position, enemy.transform.position);
                                if (currentEnemyDistance <= m_MaxHelpDistance)
                                {
                                    closestVisibleEnemy = enemy;
                                    closestVisibleEnemyDistance = currentEnemyDistance;
                                }
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

                    if (closestVisibleEnemy != null)
                    {
                        // Someone spotted an enemy close to soldier -> go help
                        SetWalkToTargetState(soldier, closestVisibleEnemy.transform.position, Priority.job);
                    }
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
                                    SetWalkToTargetState(soldier, closestLastSeenPositionEnemy.Key.transform.position, Priority.low);
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

                                if(gAIManager.GetTimeSinceNonePriority() > Random.Range(2.5f, 20.0f))
                                {
                                    POI closestPOI = POIManager.Instance.GetClosestTeamPOI(soldier);
                                    if(closestPOI != null)
                                    {
                                        Vector3 walkTarget = closestPOI.GetRandomWalkablePosition();
                                        SetWalkToTargetState(soldier, walkTarget, Priority.low);
                                    }
                                    else
                                    {
                                        Vector2 walkTarget = InfluenceMapControl.Instance.GetClosestUnclaimedOrEnemyClaimed(new Vector2(soldier.transform.position.x, soldier.transform.position.z), soldier.GetComponent<Propagator>().Value);
                                        NavMeshHit hit;
                                        if (NavMesh.SamplePosition(new Vector3(walkTarget.x, 0.0f, walkTarget.y), out hit, 100f, NavMesh.AllAreas))
                                        {
                                            SetWalkToTargetState(soldier, hit.position, Priority.low);
                                        }
                                    }
                                }
                                else
                                {
                                    gAIManager.SetIdleState();
                                }

                                //gAIManager.SetIdleState();
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
                    m_SoldierGroup.AssembleSoldiers();
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
                        m_MaxConcrurrentSoldiers += m_SoldierGroup.m_TargetPOI.GetNumberOfRequiredGuards();
                    }
                    break;
                case CapturePOIStatus.Guard:
                    // Sent all units to their guard positions -> destroy group
                    m_SoldierGroup = null;
                    break;
                case CapturePOIStatus.EOL:
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

    public void SpawnUnit()
    {
        m_SoldierSpawner.SpawnUnit();
        m_SpawnedSoldiers++;
    }

    private int ColliderArraySortComparer(Collider A, Collider B)
    {
        if(A == null && B != null)
        {
            return 1;
        }
        else if(A != null && B == null)
        {
            return -1;
        }
        else if(A == null && B == null)
        {
            return 0;
        }
        else
        {
            return Vector3.Distance(m_CurrentSoldier.transform.position, A.transform.position).CompareTo(Vector3.Distance(m_CurrentSoldier.transform.position, B.transform.position));
        }
    }

    private Vector3 GetBestPossibleCoverPosition(Soldier _soldier)
    {
        if (m_VisibleEnemiesFromSoldier[_soldier].Count > 0)
        {
            m_CoverDebugData.Clear();

            Collider[] colliders = new Collider[10];
            int hits = Physics.OverlapSphereNonAlloc(_soldier.transform.position, 10.0f, colliders, m_CoverLayer);

            m_CurrentSoldier = _soldier;
            System.Array.Sort(colliders, ColliderArraySortComparer);

            for (int i = 0; i < hits; i++)
            {
                if (Vector3.Distance(colliders[i].transform.position, _soldier.transform.position) >= _soldier.GetMinAttackDistance())
                {
                    if (NavMesh.SamplePosition(colliders[i].transform.position, out NavMeshHit navMeshHit, 2f, NavMesh.AllAreas))
                    {
                        if (!NavMesh.FindClosestEdge(navMeshHit.position, out navMeshHit, NavMesh.AllAreas))
                        {
                            Debug.LogError($"Unable to find edge close to {navMeshHit.position}");
                        }

                        float sum1 = 0.0f;
                        foreach (Soldier enemy in m_VisibleEnemiesFromSoldier[_soldier])
                        {
                            if(enemy != null)
                            {
                                sum1 += Vector3.Dot(navMeshHit.normal, (enemy.transform.position - navMeshHit.position).normalized);
                            }
                        }
                        float dot1 = sum1 / (float)m_VisibleEnemiesFromSoldier[_soldier].Count;
                        if (dot1 < m_HideSensitivity)
                        {
                            //Debug.DrawLine(soldier.transform.position, navMeshHit.position, Color.magenta, 1.0f / 10.0f);
                            m_CoverDebugData.Add(new CoverDebugData(navMeshHit.position, dot1));
                        }
                        else
                        {
                            if (NavMesh.SamplePosition(colliders[i].transform.position - (m_VisibleEnemiesFromSoldier[_soldier][Random.Range(0, m_VisibleEnemiesFromSoldier[_soldier].Count - 1)].transform.position - navMeshHit.position).normalized * 2, out NavMeshHit navMeshHit2, 2f, NavMesh.AllAreas))
                            {
                                if (!NavMesh.FindClosestEdge(navMeshHit2.position, out navMeshHit2, NavMesh.AllAreas))
                                {
                                    Debug.LogError($"Unable to find edge close to {navMeshHit2.position}");
                                }

                                float sum2 = 0.0f;
                                foreach (Soldier enemy in m_VisibleEnemiesFromSoldier[_soldier])
                                {
                                    sum2 += Vector3.Dot(navMeshHit2.normal, (enemy.transform.position - navMeshHit2.position).normalized);
                                }
                                float dot2 = sum2 / (float)m_VisibleEnemiesFromSoldier[_soldier].Count;
                                if (dot2 < m_HideSensitivity)
                                {
                                    //Debug.DrawLine(soldier.transform.position, navMeshHit2.position, Color.yellow, 1.0f / 10.0f);
                                    m_CoverDebugData.Add(new CoverDebugData(navMeshHit2.position, dot2));
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"Unable to find NavMesh near object {colliders[i].name} at {colliders[i].transform.position}");
                    }
                }
            }

            CoverDebugData bestCover = null;
            foreach (CoverDebugData cdb in m_CoverDebugData)
            {
                if(Vector3.Distance(cdb.position, _soldier.GetTarget().transform.position) <= _soldier.GetMaxAttackDistance())
                {
                    if (bestCover == null)
                    {
                        bestCover = cdb;
                    }
                    else
                    {
                        if (cdb.value < bestCover.value)
                        {
                            bestCover = cdb;
                        }
                    }
                }
            }
            if(bestCover != null)
            {
                return bestCover.position;
            }
        }

        return Vector3.zero;
    }

    public Formation CreateFormation(List<Soldier> _soldiers, Vector3 _target)
    {
        GameObject formationGO = Instantiate(m_ArrowFormation3, _soldiers[0].transform.position, _soldiers[0].transform.rotation, m_SoldierSpawner.transform);

        Formation formation = formationGO.GetComponent<Formation>();

        formation.SetTarget(_target);

        foreach(Soldier soldier in _soldiers)
        {
            SetFormationState(soldier, formation);
        }

        return formation;
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
    public void SetIdleState(Soldier _soldier)
    {
        _soldier.GetComponent<GroupAIManager>().SetIdleState();
    }

    public void SetWalkToTargetState(Soldier _soldier, Vector3 _target, Priority _priority)
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
                    //Handles.Label(keyValuePair.Value.position, (Time.time - keyValuePair.Value.lastSeenTime).ToString("F2"));
                }
            }
        }

        foreach(CoverDebugData cdd in m_CoverDebugData)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cdd.position, 0.1f);
            //Handles.Label(cdd.position, cdd.value.ToString());
        }

        if (m_Team != null)
        {
            Gizmos.color = Color.blue;
            foreach (Soldier soldier in m_Team)
            {
                Gizmos.DrawSphere(soldier.transform.position, 1.0f);
            }
        }
    }
}
