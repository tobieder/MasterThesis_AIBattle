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

    [SerializeField] float m_MinAttackDistance = 10.0f, m_MaxAttackDistance = 25.0f;
    [SerializeField] float m_MoveSpeed = 15.0f;

    [SerializeField] float m_DamageDealt = 25.0f;
    [SerializeField] float m_FireCooldown = 1.0f;
    private float m_currentFireCooldown = 0.0f;

    [SerializeField] float m_coverChangeCooldown = 5.0f;
    private float m_currentCoverChangeCooldown = 0.0f;

    // Functionality
    public Transform m_Eyes;

    // Internal
    private Animator m_Animator;
    private NavMeshAgent m_NavMeshAgent;

    private bool m_WalkTargetSet;
    private Vector3 m_WalkTarget;

    private Soldier m_Target;

    private CoverSpot m_CurrentCover;


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
        if(m_Vitals.GetHealth() > 0)
        {
            if(m_Target != null)
            {
                if(Vector3.Distance(transform.position, m_Target.transform.position) > m_MaxAttackDistance)
                {
                    m_Target = null;
                }
            }
            else
            {
                Soldier bestTarget = GetNewTarget();
                if (bestTarget != null)
                {
                    if (Vector3.Distance(bestTarget.transform.position, transform.position) < m_MaxAttackDistance)
                    {
                        m_Target = bestTarget;
                    }
                }
            }

            // Run selected Controller

            // ----- Update State Machine -----
            m_SoldierController.RunSoldierController();

            // ----- Update Cooldowns -----
            m_currentFireCooldown -= Time.deltaTime;
            m_currentCoverChangeCooldown -= Time.deltaTime;
        }
        else
        {
            // Die
            m_Animator.SetBool("move", false);
            ResetPathfinding();

            if (GetComponent<CapsuleCollider>() != null)
            {
                Destroy(GetComponent<CapsuleCollider>());
            }

            if (m_CurrentCover != null)
            {
                CoverManager.Instance.ExitCover(m_CurrentCover);
            }

            Quaternion deathRotation = Quaternion.Euler(-90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            if (transform.rotation != deathRotation)
            {
                transform.rotation = deathRotation;
            }

            m_Vitals.Die();

            Destroy(gameObject, 2);
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

    bool IsTargetVisible(Soldier _target)
    {
        Vector3 directionToEnemy = _target.m_Eyes.position - m_Eyes.position;
        RaycastHit hit;

        if (Physics.Raycast(m_Eyes.position, directionToEnemy, out hit, Mathf.Infinity))
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
        m_NavMeshAgent.isStopped = true;
        m_NavMeshAgent.ResetPath();
    }

    public void ResetCurrentCover()
    {
        if(m_CurrentCover != null)
        {
            CoverManager.Instance.ExitCover(m_CurrentCover);
            m_CurrentCover = null;
        }
    }

    // ----- ANIMATION CONTROLLER -----
    public void SetWalkAnimation(bool _state)
    {
        m_Animator.SetBool("move", _state);
    }

    public void TriggerShotAnimation()
    {
        m_Animator.SetTrigger("fire");
    }

    public bool HasAnimationFinished(string _animationTag)
    {
        if(m_Animator.GetCurrentAnimatorStateInfo(0).IsTag(_animationTag))
        {
            return false;
        }

        return true;
    }

    // ----- GETTER/SETTER -----
    public NavMeshAgent GetNavMeshAgent()
    {
        return m_NavMeshAgent;
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

    public CoverSpot GetCurrentCoverSpot()
    {
        return m_CurrentCover;
    }


    public void SetCurrentCoverSpot(CoverSpot _coverSpot)
    {
        m_CurrentCover = _coverSpot;
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
