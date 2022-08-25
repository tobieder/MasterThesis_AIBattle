using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupAI : MonoBehaviour
{

    public List<Soldier> m_Team = new List<Soldier>();
    public List<Soldier> m_VisibleEnemies = new List<Soldier>();

    public LayerMask m_EnemyLayer;

    public GameObject m_ArrowFormation3;
    public Formation m_CurrentFormation;

    private void Awake()
    {
        InvokeRepeating("UpdateVisibleEnemies", 0.001f, 1.0f / 10.0f);
    }

    private void Update()
    {
    }

    private void UpdateVisibleEnemies()
    {
        m_VisibleEnemies.Clear();

        foreach (Soldier soldier in m_Team)
        {
            foreach (Collider enemyCollider in Physics.OverlapSphere(soldier.transform.position, soldier.GetMaxAttackDistance(), m_EnemyLayer))
            {
                Soldier enemy = enemyCollider.GetComponent<Soldier>();
                if (!m_VisibleEnemies.Contains(enemy))
                {
                    m_VisibleEnemies.Add(enemy);
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
