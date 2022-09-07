using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    [SerializeField] int m_teamNumber;

    public int GetTeamNumber()
    {
        return m_teamNumber;
    }
}
