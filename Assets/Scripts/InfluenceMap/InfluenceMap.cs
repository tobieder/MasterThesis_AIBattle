using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public struct Vector2I
{
    public int x;
    public int y;
    public float d;

    public Vector2I(int _x, int _y)
    {
        x = _x;
        y = _y;
        d = 1;
    }

    public Vector2I(int _x, int _y, float _d)
    {
        x = _x;
        y= _y;
        d= _d;
    }
}

public class InfluenceMap : GridData
{
    List<IPropagator> m_Propagators = new List<IPropagator>();

    float[,] m_Influences;
    float[,] m_InfluencesBuffer;
    int[,] m_LastInfluence;

    public float Decay { get; set; }
    public float Momentum { get; set; }
    public int Width { get { return m_Influences.GetLength(0); } }
    public int Height { get { return m_Influences.GetLength(1); } }

    public float GetValue(int _x, int _y)
    {
        return m_Influences[_x, _y];
    }

    public float[,] GetInfluenceMap()
    {
        return m_Influences;
    }

    public int GetLastInfluenceValue(int _x, int _y)
    {
        return m_LastInfluence[_x, _y];
    }

    public int[,] GetLastInfluenceMap()
    {
        return m_LastInfluence;
    }

    public InfluenceMap(int _size, float _decay, float _momentum)
    {
        m_Influences = new float[_size, _size];
        m_InfluencesBuffer = new float[_size, _size];
        m_LastInfluence = new int[_size, _size];
        Decay = _decay;
        Momentum = _momentum;
    }

    public InfluenceMap(int _width, int _height, float _decay, float _momentum)
    {
        m_Influences = new float[_width, _height];
        m_InfluencesBuffer = new float[_width, _height];
        m_LastInfluence = new int[_width, _height];
        Decay = _decay;
        Momentum = _momentum;
    }

    public InfluenceMap(float[,] _influences, float _decay, float _momentum)
    {
        m_Influences = _influences;
        m_InfluencesBuffer = _influences;
        m_LastInfluence = new int[_influences.GetLength(0), _influences.GetLength(1)];
        Decay = _decay;
        Momentum = _momentum;
    }

    public InfluenceMap(int[,] _lastInfluences, float _decay, float _momentum)
    {
        m_LastInfluence = _lastInfluences;
        m_Influences = new float[_lastInfluences.GetLength(0), _lastInfluences.GetLength(1)];
        m_InfluencesBuffer = new float[_lastInfluences.GetLength(0), _lastInfluences.GetLength(1)];
        Decay = _decay;
        Momentum = _momentum;
    }

    public void SetInfluence (int _x, int _y, float _value)
    {
        if(_x < Width && _y < Height)
        {
            m_Influences[_x, _y] = _value;
            m_InfluencesBuffer[_x, _y] = _value;
            m_LastInfluence[_x, _y] = (int)Mathf.Sign(_value);
        }
    }

    public void SetInfluence(Vector2I _position, float _value)
    {
        if (_position.x < Width && _position.y < Height)
        {
            m_Influences[_position.x, _position.y] = _value;
            m_InfluencesBuffer[_position.x, _position.y] = _value;
            m_LastInfluence[_position.x, _position.y] = (int)Mathf.Sign(_value);
        }
    }

    public void SetLastInfluence(Vector2I _position, int _value)
    {
        if(_position.x < Width && _position.y < Height)
        {
            m_LastInfluence[_position.x, _position.y] = _value;
        }
    }

    public void SetLastInfluence(int _x, int _y, int _value)
    {
        if (_x < Width && _y < Height)
        {
            m_LastInfluence[_x, _y] = _value;
        }
    }

    public void RegisterPropagator(IPropagator _propagator)
    {
        m_Propagators.Add(_propagator);
    }

    public void DelistPropagator(IPropagator _propagator)
    {
        m_Propagators.Remove(_propagator);
    }

    public void Propagate()
    {
        UpdatePropagators();
        UpdatePropagation();
        UpdateInfluenceBuffer();
    }

    void UpdatePropagators()
    {
        foreach(IPropagator _propagator in m_Propagators)
        {
            SetInfluence(_propagator.GridPosition, _propagator.Value);
        }
    }

    void UpdatePropagation()
    {
        for(int x = 0; x < m_Influences.GetLength(0); ++x)
        {
            for(int y = 0; y < m_Influences.GetLength(1); ++y)
            {
                float maxInfluence = 0.0f;
                float minInfluence = 0.0f;

                Vector2I[] neighbors = GetNeighbors(x, y);
                foreach(Vector2I neighbor in neighbors)
                {
                    float influence = m_InfluencesBuffer[neighbor.x, neighbor.y] * Mathf.Exp(-Decay * neighbor.d);
                    maxInfluence = Mathf.Max(influence, maxInfluence);
                    minInfluence = Mathf.Min(influence, minInfluence);
                }

                if (Mathf.Abs(minInfluence) > maxInfluence)
                {
                    /*
                    if(Mathf.Abs(minInfluence) > Mathf.Abs(m_InfluencesBuffer[x, y]) || minInfluence < m_InfluencesBuffer[x, y])
                    {
                        m_Influences[x, y] = Mathf.Lerp(m_InfluencesBuffer[x, y], minInfluence, Momentum);
                    }
                    */
                    m_Influences[x, y] = Mathf.Lerp(m_InfluencesBuffer[x, y], minInfluence, Momentum);
                }
                else if(Mathf.Abs(minInfluence) < maxInfluence)
                {
                    /*
                    if (Mathf.Abs(maxInfluence) > Mathf.Abs(m_InfluencesBuffer[x, y]) || maxInfluence > m_InfluencesBuffer[x, y])
                    {
                        m_Influences[x, y] = Mathf.Lerp(m_InfluencesBuffer[x, y], maxInfluence, Momentum);
                    }
                    */
                    m_Influences[x, y] = Mathf.Lerp(m_InfluencesBuffer[x, y], maxInfluence, Momentum);
                }
                else
                {
                    m_Influences[x, y] = 0.0f;
                }

                if (Sign(m_Influences[x, y]) != m_LastInfluence[x, y] && Mathf.Abs(m_Influences[x, y]) > 0.1f)
                {
                    m_LastInfluence[x, y] = Sign(m_Influences[x, y]);
                }
            }
        }
    }

    void UpdateInfluenceBuffer()
    {
        for(int x = 0; x < m_Influences.GetLength(0); ++x)
        {
            for(int y = 0; y < m_Influences.GetLength(1); ++y)
            {
                m_InfluencesBuffer[x, y] = m_Influences[x, y];
            }
        }
    }

    Vector2I[] GetNeighbors(int _x, int _y)
    {
        List<Vector2I> retVal = new List<Vector2I>();

        // as long as not in left edge
        if (_x > 0)
        {
            retVal.Add(new Vector2I(_x - 1, _y));
        }

        // as long as not in right edge
        if (_x < m_Influences.GetLength(0) - 1)
        {
            retVal.Add(new Vector2I(_x + 1, _y));
        }

        // as long as not in bottom edge
        if (_y > 0)
        {
            retVal.Add(new Vector2I(_x, _y - 1));
        }

        // as long as not in upper edge
        if (_y < m_Influences.GetLength(1) - 1)
        {
            retVal.Add(new Vector2I(_x, _y + 1));
        }


        // diagonals

        // as long as not in bottom-left
        if (_x > 0 && _y > 0)
        {
            retVal.Add(new Vector2I(_x - 1, _y - 1, 1.4142f));
        }

        // as long as not in upper-right
        if (_x < m_Influences.GetLength(0) - 1 && _y < m_Influences.GetLength(1) - 1)
        {
            retVal.Add(new Vector2I(_x + 1, _y + 1, 1.4142f));
        }

        // as long as not in upper-left
        if (_x > 0 && _y < m_Influences.GetLength(1) - 1)
        {
            retVal.Add(new Vector2I(_x - 1, _y + 1, 1.4142f));
        }

        // as long as not in bottom-right
        if (_x < m_Influences.GetLength(0) - 1 && _y > 0)
        {
            retVal.Add(new Vector2I(_x + 1, _y - 1, 1.4142f));
        }

        return retVal.ToArray();
    }

    int Sign(float _number)
    {
        return _number < 0 ? -1 : (_number > 0 ? 1 : 0);
    }
}
