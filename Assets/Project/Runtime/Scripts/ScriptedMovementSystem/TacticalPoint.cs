using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalPoint : MonoBehaviour
{
    public enum SoldierPosition {First, Second, Third, Fourth};
    public SoldierPosition m_SoldierPosition;

    public bool m_WaitForOthers;

    private void OnDrawGizmos()
    {
        switch (m_SoldierPosition)
        {
            case SoldierPosition.First: Gizmos.color = Color.red; break;
            case SoldierPosition.Second: Gizmos.color = Color.blue; break;
            case SoldierPosition.Third: Gizmos.color = Color.green; break;
            case SoldierPosition.Fourth: Gizmos.color= Color.yellow; break;
        }

        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
