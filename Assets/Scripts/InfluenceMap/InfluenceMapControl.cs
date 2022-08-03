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
        /*
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
        */
    }

    void CreateMap()
    {
        int width = (int)(Mathf.Abs(m_TopRight.position.x - m_BottomLeft.position.x) / m_GridSize);
        int height = (int)(Mathf.Abs(m_TopRight.position.z - m_BottomLeft.position.z) / m_GridSize);

        //Debug.Log(width + " x " + height);

        m_InfluenceMap = new InfluenceMap(width, height, m_Decay, m_Momentum);

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
}
