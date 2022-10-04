using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoldierSetup : MonoBehaviour
{
    [SerializeField]
    private Canvas m_HealthBarCanvas;

    private void Awake()
    {
        List<Soldier> soldiers = new List<Soldier>(GameObject.FindObjectsOfType<Soldier>());

        foreach(Soldier soldier in soldiers)
        {
            soldier.GetComponent<Vitals>().SetupHealthBar(m_HealthBarCanvas);
        }
    }
}
