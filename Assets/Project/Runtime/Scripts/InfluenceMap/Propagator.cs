using System.Collections;
using UnityEngine;

public interface IPropagator
{
    Vector2I GridPosition { get; }
    float Value { get; }
}

public class Propagator : MonoBehaviour, IPropagator
{
    [SerializeField]
    float m_Value;
    public float Value { get { return m_Value; } }

    public Vector2I GridPosition
    {
        get
        {
            return InfluenceMapControl.Instance.GetGridPosition(transform.position);
        }
    }

    private void Start()
    {
        InfluenceMapControl.Instance.RegisterPropagator(this);
    }

    private void OnDestroy()
    {
        InfluenceMapControl.Instance.DelistPropagator(this);
    }
}
