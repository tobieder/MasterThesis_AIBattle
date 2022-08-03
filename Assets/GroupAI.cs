using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupAI : MonoBehaviour
{

    public List<Soldier> m_Team = new List<Soldier>();
    public HashSet<Soldier> m_VisibleEnemies;

    public LayerMask m_EnemyLayer;

    private void Start()
    {
        m_VisibleEnemies = new HashSet<Soldier>();
    }

    private void Update()
    {
        m_VisibleEnemies.Clear();

        foreach (Soldier soldier in m_Team)
        {
            foreach (Collider enemyCollider in Physics.OverlapSphere(soldier.transform.position, soldier.GetMaxAttackDistance(), m_EnemyLayer))
            {
                m_VisibleEnemies.Add(enemyCollider.GetComponent<Soldier>());
            }
        }
    }

    public void RegisterSoldier(Soldier _soldier)
    {
        m_Team.Add(_soldier);

        Debug.Log(m_Team.Count);
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
