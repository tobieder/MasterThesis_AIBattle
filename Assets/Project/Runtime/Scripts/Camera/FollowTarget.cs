using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField]
    private Transform m_Target;
    [SerializeField]
    private Vector3 m_Offset;

    [SerializeField]
    public bool m_FaceCamera;

    void Update()
    {
        transform.position = m_Target.position + m_Offset;

        if(m_FaceCamera)
        {
            transform.LookAt(Camera.main.transform, Vector3.up);
        }
    }
}
