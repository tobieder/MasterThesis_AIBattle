using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    // Controller
    SoldierController m_SoldierController;

    // Soldier Properties
    public Team m_Team;
    public Vitals m_Vitals;

    [SerializeField] float m_ViewDistance = 35.0f;
    [SerializeField] float m_FieldOfView = 60.0f;
    [SerializeField] float m_MinAttackDistance = 10.0f, m_MaxAttackDistance = 25.0f;
    [SerializeField] float m_MoveSpeed = 15.0f;

    [SerializeField] float m_DamageDealt = 25.0f;
    [SerializeField] float m_Deviation = 0.75f;
    [SerializeField] float m_FireCooldown = 1.0f;
    private float m_currentFireCooldown = 0.0f;

    [SerializeField] float m_coverChangeCooldown = 5.0f;
    private float m_currentCoverChangeCooldown = 0.0f;

    // Functionality
    public Transform m_Eyes;
    public ParticleSystem m_WeaponEffect;

    // Internal
    private bool m_Dead = false;

    private Vector3 m_PreviousPosition;
    private float m_Velocity;

    private Animator m_Animator;
    private NavMeshAgent m_NavMeshAgent;

    private bool m_WalkTargetSet;
    private Vector3 m_WalkTarget;

    private Soldier m_Target;

    private CoverSpot m_CurrentCover;
    private GuardSpot m_CurrentGuardSpot;


    private void Start()
    {
        m_Vitals = GetComponent<Vitals>();
        m_Team = GetComponent<Team>();
        m_Animator = GetComponent<Animator>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();

        m_SoldierController = GetComponent<SoldierController>();

        m_WalkTargetSet = false;

        m_currentCoverChangeCooldown = m_coverChangeCooldown;
    }


    private void Update()
    {
        if(m_Dead)
        {
            return;
        }

        if(m_Vitals.GetHealth() > 0)
        {
            if(m_Target != null)
            {
                if(Vector3.Distance(transform.position, m_Target.transform.position) > m_ViewDistance)
                {
                    m_Target = null;
                }
            }
            else
            {
                Soldier bestTarget = GetNewTarget();
                if (bestTarget != null)
                {
                    if (Vector3.Distance(bestTarget.transform.position, transform.position) < m_ViewDistance)
                    {
                        // Check if enemy is in fov
                        if(Vector3.Dot(transform.forward, (bestTarget.transform.position - transform.position).normalized) >= Mathf.Cos(GetFieldOfView()))
                        {
                            // Check for obstacles
                            if (Physics.Raycast(m_Eyes.position, (bestTarget.m_Eyes.position - m_Eyes.position).normalized, out RaycastHit hit, GetViewDistance()))
                            {
                                if (hit.collider == bestTarget.GetComponent<Collider>())
                                {
                                    m_Target = bestTarget;
                                }
                            }
                        }
                    }
                }
            }

            // Calculate current velocity
            Vector3 lastFrameMovement = transform.position - m_PreviousPosition;
            m_Velocity = lastFrameMovement.magnitude / Time.deltaTime;
            m_PreviousPosition = transform.position;

            // ----- Update Cooldowns -----
            m_currentFireCooldown -= Time.deltaTime;
            m_currentCoverChangeCooldown -= Time.deltaTime;

            // Run selected Controller
            // ----- Update State Machine -----
            m_SoldierController.RunSoldierController();
        }
        else
        {
            // Die
            TriggerDeathAnimation();
            ResetPathfinding();

            if (m_CurrentCover != null)
            {
                CoverManager.Instance.ExitCover(m_CurrentCover);
            }

            if(m_CurrentGuardSpot != null)
            {
                m_CurrentGuardSpot.SetOccupier(null);
            }

            m_Vitals.Die();

            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<CapsuleCollider>());
            Destroy(GetComponent<ObstacleAgent>());
            Destroy(GetComponent<NavMeshAgent>());
            Destroy(GetComponent<NavMeshObstacle>());
            Destroy(GetComponent<SoldierController>());
            Destroy(GetComponent<Soldier>());
            Destroy(GetComponent<Vitals>());
            Destroy(GetComponent<Team>());
            Destroy(GetComponent<Propagator>());
            Destroy(GetComponent<StateManager>());
            Destroy(GetComponent<GroupAIManager>());

            Destroy(transform.Find("Eyes").gameObject);
            Destroy(transform.Find("FSM").gameObject);
            Destroy(transform.Find("FSM_GroupAI").gameObject);

            Destroy(m_Vitals.GetHealthBar().gameObject);

            //Destroy(gameObject, 2);

            m_Dead = true;
        }
    }


    // ----- HELPER FUNCTIONS -----
    Soldier GetNewTarget()
    {
        Soldier[] allSoldiers = GameObject.FindObjectsOfType<Soldier>();
        Soldier bestTarget = null;
        for (int i = 0; i < allSoldiers.Length; i++)
        {
            Soldier currentSoldier = allSoldiers[i];

            // check if npcs are on the same team
            if (currentSoldier.GetComponent<Team>().GetTeamNumber() != this.m_Team.GetTeamNumber())
            {

                if (IsTargetVisible(currentSoldier))
                {
                    if (bestTarget == null)
                    {
                        bestTarget = currentSoldier;
                    }
                    else
                    {
                        // check distance
                        if (Vector3.Distance(bestTarget.transform.position, transform.position) >
                            Vector3.Distance(currentSoldier.transform.position, transform.position))
                        {
                            bestTarget = currentSoldier;
                        }
                    }
                }
            }
        }

        return bestTarget;
    }

    public Soldier[] GetXClosestTargets(int _numberOfTargets)
    {
        List<Soldier> enemySoldiersOrderedByDistance;
        List<Soldier> closestTargets = new List<Soldier>(_numberOfTargets);

        enemySoldiersOrderedByDistance = GetAllEnemySoliders().OrderBy(
                x => Vector3.Distance(this.transform.position, x.transform.position)
            ).ToList();

        if (_numberOfTargets > enemySoldiersOrderedByDistance.Count)
        {
            _numberOfTargets = enemySoldiersOrderedByDistance.Count;
            closestTargets = new List<Soldier>(_numberOfTargets);
        }

        for (int i = 0; i < enemySoldiersOrderedByDistance.Count; i++)
        {
            Soldier currentSoldier = enemySoldiersOrderedByDistance[i];

            if (IsTargetVisible(currentSoldier))
            {
                closestTargets.Add(currentSoldier);
                if(closestTargets.Count == _numberOfTargets)
                {
                    return closestTargets.ToArray();
                }
            }
        }

        return closestTargets.ToArray();
    }

    public Soldier[] GetAllTargetsInRange()
    {
        List<Soldier> enemySoldiersOrderedByDistance;
        List<Soldier> closestTargets = new List<Soldier>();

        enemySoldiersOrderedByDistance = GetAllEnemySoliders().OrderBy(
                x => Vector3.Distance(this.transform.position, x.transform.position)
            ).ToList();


        for (int i = 0; i < enemySoldiersOrderedByDistance.Count; i++)
        {
            Soldier currentSoldier = enemySoldiersOrderedByDistance[i];

            if (IsTargetVisible(currentSoldier) && Vector3.Distance(transform.position, currentSoldier.transform.position) < m_MaxAttackDistance)
            {
                closestTargets.Add(currentSoldier);
            }
        }

        return closestTargets.ToArray();
    }

    Soldier[] GetAllEnemySoliders()
    {
        Soldier[] allSoldiers = GameObject.FindObjectsOfType<Soldier>();
        List<Soldier> enemySoldiers = new List<Soldier>();

        foreach(Soldier currentSoldier in allSoldiers)
        {
            if(currentSoldier.GetComponent<Team>().GetTeamNumber() != this.m_Team.GetTeamNumber())
            {
                enemySoldiers.Add(currentSoldier);
            }
        }

        return enemySoldiers.ToArray();
    }

    public bool IsTargetVisible()
    {
        return IsTargetVisible(m_Target);
    }

    public bool IsTargetVisible(Soldier _target)
    {
        Vector3 directionToEnemy = _target.m_Eyes.position - m_Eyes.position;
        RaycastHit hit;

        if (Physics.Raycast(m_Eyes.position, directionToEnemy, out hit, m_ViewDistance))
        {
            if (hit.transform == _target.transform)
            {
                return true;
            }
        }

        return false;
    }

    public void ResetPathfinding()
    {
        m_WalkTargetSet = false;
        //GetComponent<ObstacleAgent>().SetDestination(transform.position);

        if (m_NavMeshAgent.enabled)
        {
            m_NavMeshAgent.isStopped = true;
            m_NavMeshAgent.ResetPath();
        }
    }

    public bool WalkTargetReached()
    {
        if(m_WalkTargetSet)
        {
            if (Vector3.Distance(transform.position, m_WalkTarget) < 1.0f)
            {
                return true;
            }
        }

        return false;
    }

    public void ResetCurrentCover()
    {
        if(m_CurrentCover != null)
        {
            CoverManager.Instance.ExitCover(m_CurrentCover);
            m_CurrentCover = null;
        }
    }

    public bool IsInCover()
    {
        if(m_CurrentCover != null)
        {
            if(m_CurrentCover.IsOccupied() && Vector3.Distance(transform.position, m_CurrentCover.transform.position) < 1.0f)
            {
                return true;
            }
        }

        return false;
    }

    // ----- ANIMATION CONTROLLER -----
    public void SetWalkAnimation(bool _state)
    {
        m_Animator.SetBool("move", _state);
    }

    public void SetCrouchAnimation(bool _state)
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        // scale collider to animation
        if(_state == true)
        {
            capsuleCollider.center = new Vector3(0.0f, 0.89f, 0.0f);
            capsuleCollider.height = 1.77f;
        }
        else
        {
            capsuleCollider.center = new Vector3(0.0f, 1.29f, 0.0f);
            capsuleCollider.height = 2.58f;
        }

        m_Animator.SetBool("crouch", _state);
    }

    public void TriggerShotAnimation()
    {
        m_Animator.SetTrigger("fire");
        m_WeaponEffect.Play();
    }

    public void SetGuardAnimation(bool _state)
    {
        m_Animator.SetBool("guard", _state);
    }

    public void TriggerDeathAnimation()
    {
        m_Animator.SetTrigger("death");
    }


    // ----- GETTER/SETTER -----
    public ObstacleAgent GetNavMeshAgent()
    {
        return GetComponent<ObstacleAgent>();
    }

    public bool IsWalkTargetSet()
    {
        return m_WalkTargetSet;
    }

    public Vector3 GetWalkTarget()
    {
        return m_WalkTarget;
    }

    public void SetWalkTarget(Vector3 _walkTarget)
    {
        m_WalkTarget = _walkTarget;
    }

    public float GetViewDistance()
    {
        return m_ViewDistance;
    }

    public float GetFieldOfView()
    {
        return m_FieldOfView;
    }

    public float GetMinAttackDistance()
    {
        return m_MinAttackDistance;
    }

    public float GetMaxAttackDistance()
    {
        return m_MaxAttackDistance;
    }

    public float GetDamage()
    {
        return m_DamageDealt;
    }

    public float GetDeviation()
    {
        return m_Deviation;
    }

    public bool ReadyToFire()
    {
        if(m_currentFireCooldown <= 0.0f)
        {
            return true;
        }

        return false;
    }

    public void ResetFireCooldown()
    {
        m_currentFireCooldown = m_FireCooldown;
    }

    public Transform GetEyes()
    {
        return m_Eyes;
    }

    public Soldier GetTarget()
    {
        return m_Target;
    }

    public void SetTarget(Soldier _target)
    {
        m_Target = _target;
    }

    public CoverSpot GetCurrentCoverSpot()
    {
        return m_CurrentCover;
    }


    public void SetCurrentCoverSpot(CoverSpot _coverSpot)
    {
        m_CurrentCover = _coverSpot;
    }

    public GuardSpot GetCurrentGuardSpot()
    {
        return m_CurrentGuardSpot;
    }

    public void SetCurrentGuardSpot(GuardSpot _guardSpot)
    {
        if(m_CurrentGuardSpot != null)
        {
            m_CurrentGuardSpot.SetOccupier(null);
        }
        m_CurrentGuardSpot = _guardSpot;
        m_CurrentGuardSpot.SetOccupier(this.transform);
    }

    public float GetVelocity()
    {
        return m_Velocity;
    }

    public bool ReadyToChangeCover()
    {
        if(m_currentCoverChangeCooldown <= 0.0f)
        {
            return true;
        }

        return false;
    }

    public void ResetCoverChangeCooldown()
    {
        m_currentCoverChangeCooldown = m_coverChangeCooldown;
    }

}
