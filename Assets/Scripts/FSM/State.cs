using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public Soldier m_SoldierData;

    public abstract State RunCurrentState();
}
