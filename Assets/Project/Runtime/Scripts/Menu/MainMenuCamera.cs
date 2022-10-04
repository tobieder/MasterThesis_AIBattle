using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    [SerializeField]
    float m_MinAngle, m_MaxAngle;

    private float m_AbsoluteAngle;

    [SerializeField]
    float m_Speed;

    private void Start()
    {
        m_AbsoluteAngle = Mathf.Abs(m_MinAngle) + Mathf.Abs(m_MaxAngle);
    }

    void Update()
    {
        float currentAngle = Mathf.PingPong(Time.time * m_Speed, m_AbsoluteAngle) + m_MinAngle;

        transform.rotation = Quaternion.Euler(0.0f, currentAngle, 0.0f);
    }
}
