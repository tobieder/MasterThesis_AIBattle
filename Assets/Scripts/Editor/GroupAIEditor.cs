using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using UnityEngine.AI;

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

        if(GUILayout.Button("Spawn Arrow Formation 3 at Random Unit"))
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

        foreach (Soldier soldier in groupAI.m_Team)
        {
            GUILayout.Label(soldier.name, EditorStyles.boldLabel);

            if(GUILayout.Button("Move To Random Position"))
            {
                Vector3 bottomLeft = InfluenceMapControl.Instance.GetBottomLeft();
                Vector3 topRight = InfluenceMapControl.Instance.GetTopRight();

                float randomX = Random.Range(bottomLeft.x, topRight.x);
                float randomZ = Random.Range(bottomLeft.z, topRight.z);

                Vector3 walkTarget = new Vector3(randomX, soldier.transform.position.y, randomZ);

                NavMeshHit hit;
                while(true)
                {
                    if (NavMesh.SamplePosition(walkTarget, out hit, 0.5f, NavMesh.AllAreas))
                    {
                        groupAI.SetWalkTarget(soldier, walkTarget);
                        return;
                    }
                }
            }

            if(GUILayout.Button("Register To Formation."))
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
