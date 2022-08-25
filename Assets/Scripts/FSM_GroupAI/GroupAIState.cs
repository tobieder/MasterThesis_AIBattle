using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GroupAIState : MonoBehaviour
{
    public Soldier m_SoldierData;

    public abstract GroupAIState RunCurrentState();
}
