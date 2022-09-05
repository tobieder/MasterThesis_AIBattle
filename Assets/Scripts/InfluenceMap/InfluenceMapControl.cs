using System.Collections;
using UnityEngine;

public class InfluenceMapControl : MonoBehaviour
{
    private static InfluenceMapControl _instance;
    public static InfluenceMapControl Instance { get { return _instance; } }

    [SerializeField]
    Transform m_BottomLeft;
    [SerializeField]
    Transform m_TopRight;

    [SerializeField]
    float m_GridSize;

    [SerializeField]
    float m_Decay = 0.3f;
    [SerializeField]
    float m_Momentum = 0.8f;

    [SerializeField]
    float m_UpdateFrequency = 3.0f;

    InfluenceMap m_InfluenceMap;

    [SerializeField]
    GridDisplay m_GridDisplay;

    public string m_InfluenceMapFileName;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Debug.LogError("Influence Map Control destroyed.");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        CreateMap();

        InvokeRepeating("PropagationUpdate", 0.001f, 1.0f / m_UpdateFrequency);
    }

    private void Update()
    {
        m_InfluenceMap.Decay = m_Decay;
        m_InfluenceMap.Momentum = m_Momentum;

        // DEBUG: Allow to change Influence by User Input
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit mouseHit;
        if (Physics.Raycast(mouseRay, out mouseHit) && Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            // is it within the grid
            // if so, call SetInfluence in that grid position to 1.0
            Vector3 hit = mouseHit.point;
            if (hit.x > m_BottomLeft.position.x && hit.x < m_TopRight.position.x && hit.z > m_BottomLeft.position.z && hit.z < m_TopRight.position.z)
            {
                Vector2I gridPos = GetGridPosition(hit);

                //Debug.Log("hit " + x + " " + y);
                if (gridPos.x < m_InfluenceMap.Width && gridPos.y < m_InfluenceMap.Height)
                {
                    SetInfluence(gridPos, (Input.GetMouseButton(0) ? 1.0f : -1.0f));
                }
            }
        }
    }

    void CreateMap()
    {
        int width = (int)(Mathf.Abs(m_TopRight.position.x - m_BottomLeft.position.x) / m_GridSize);
        int height = (int)(Mathf.Abs(m_TopRight.position.z - m_BottomLeft.position.z) / m_GridSize);

        //Debug.Log(width + " x " + height);

        if(System.IO.File.Exists("./Assets/InfluenceMapData/" + m_InfluenceMapFileName))
        {
            Debug.Log("File found.");
            SaveLoadArray sla = new SaveLoadArray();
            m_InfluenceMap = new InfluenceMap(sla.LoadLastInfluenceMap("./Assets/InfluenceMapData/" + m_InfluenceMapFileName), m_Decay, m_Momentum);
        } else
        {
            Debug.Log("No file found.");
            m_InfluenceMap = new InfluenceMap(width, height, m_Decay, m_Momentum);
        }

        m_GridDisplay.SetGridData(m_InfluenceMap);
        m_GridDisplay.CreateMesh(m_BottomLeft.position, m_GridSize);
    }

    public void RegisterPropagator(IPropagator _propagator)
    {
        m_InfluenceMap.RegisterPropagator(_propagator);
    }

    public void DelistPropagator(IPropagator _propagator)
    {
        m_InfluenceMap.DelistPropagator(_propagator);
    }

    public Vector2I GetGridPosition(Vector3 _position)
    {
        int x = (int)((_position.x - m_BottomLeft.position.x) / m_GridSize);
        int y = (int)((_position.z - m_BottomLeft.position.z) / m_GridSize);

        return new Vector2I(x, y);
    }

    public void GetMovementLimits(out Vector3 _bottomLeft, out Vector3 _topRight)
    {
        _bottomLeft = m_BottomLeft.position;
        _topRight = m_TopRight.position;
    }

    void PropagationUpdate()
    {
        m_InfluenceMap.Propagate();
    }

    void SetInfluence(int _x, int _y, float _value)
    {
        m_InfluenceMap.SetInfluence(_x, _y, _value);
    }

    void SetInfluence(Vector2I _position, float _value)
    {
        m_InfluenceMap.SetInfluence(_position, _value);
    }

    // ----- GETTER & SETTER -----
    public Vector3 GetBottomLeft()
    {
        return m_BottomLeft.position;
    }

    public Vector3 GetTopRight()
    {
        return m_TopRight.position;
    }

    public float[,] GetInfluenceMap()
    {
        return m_InfluenceMap.GetInfluenceMap();
    }

    public float GetInfluence(int _x, int _y)
    {
        return m_InfluenceMap.GetValue(_x, _y);
    }
    
    public int[,] GetLastInfluenceMap()
    {
        return m_InfluenceMap.GetLastInfluenceMap();
    }

    public float GetLastInfluence(int _x, int _y)
    {
        return m_InfluenceMap.GetLastInfluenceValue(_x, _y);
    }

    // Returns average last influence value of specified area
    public float GetLastInfluenceInArea(int _x, int _y, Vector2 size)
    {
        int counter = 0;
        float value = 0;

        for(int x = _x; x < _x + size.x; x++)
        {
            for(int y = _y; y < _y + size.y; y++)
            {
                /*
                float lastInfluente = GetLastInfluence(x, y);
                if(lastInfluente > 0)
                {
                    posValues++;
                }
                else if(lastInfluente < 0)
                {
                    negValues++;
                }
                */

                //m_InfluenceMap.SetLastInfluence(x, y, 1);

                value += GetLastInfluence(x, y);
                counter++;
            }

        }

        /*
        if(posValues > counter/2.0f)
        {
            return (float)posValues / (float)counter;
        }
        else if(negValues > counter / 2.0f)
        {
            return (float)negValues/ (float)counter;
        }
        else
        {
            return 0;
        }
        */
        return value / (float)counter;
    }

    public bool AreAllValuesTheSame()
    {
        int value = m_InfluenceMap.GetLastInfluenceValue(0, 0);
        for (int x = 0; x < m_InfluenceMap.Width; ++x)
        {
            for (int y = 0; y < m_InfluenceMap.Height; ++y)
            {
                if(m_InfluenceMap.GetLastInfluenceValue(x, y) != value)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public Vector2 GetClosestUnclaimedOrEnemyClaimed(Vector2 _position, float _value)
    {
        float currentClosestDistance = float.PositiveInfinity;
        Vector2 currentClosestPosition = _position;

        if(_value > 0.0)
        {
            for (int x = 0; x < m_InfluenceMap.Width; ++x)
            {
                for (int y = 0; y < m_InfluenceMap.Height; ++y)
                {
                    if (GetLastInfluence(x, y) <= 0f)
                    {
                        float currentDistance = Vector2.Distance(_position, new Vector2(transform.position.x, transform.position.z) + new Vector2(x, y));
                        if (currentDistance < currentClosestDistance)
                        {
                            currentClosestPosition = new Vector2(transform.position.x, transform.position.z) + new Vector2(x, y);
                            currentClosestDistance = currentDistance;
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = 0; x < m_InfluenceMap.Width; ++x)
            {
                for (int y = 0; y < m_InfluenceMap.Height; ++y)
                {
                    if (GetLastInfluence(x, y) >= 0f)
                    {
                        float currentDistance = Vector2.Distance(_position, new Vector2(transform.position.x, transform.position.z) + new Vector2(x, y));
                        if (currentDistance < currentClosestDistance)
                        {
                            currentClosestPosition = new Vector2(transform.position.x, transform.position.z) + new Vector2(x, y);
                            currentClosestDistance = currentDistance;
                        }
                    }
                }
            }
        }

        return currentClosestPosition;
    }
}
