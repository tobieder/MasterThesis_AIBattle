using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    Vector3[] m_PathNodes;
    public int m_CurrentPathIndex = 0;

    public Path(Vector3[] _pathNodes)
    {
        m_PathNodes = _pathNodes;
    }

    public Vector3[] GetPathNodes()
    {
        return m_PathNodes;
    }

    public Vector3 GetNextNode()
    {
        if (m_CurrentPathIndex < m_PathNodes.Length)
        {
            return m_PathNodes[m_CurrentPathIndex];
        }

        return Vector3.negativeInfinity;
    }

    public bool EndNodeReached()
    {
        return (m_CurrentPathIndex >= m_PathNodes.Length);
    }
}
