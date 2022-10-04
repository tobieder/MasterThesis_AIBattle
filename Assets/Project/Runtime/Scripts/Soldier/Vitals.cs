using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vitals : MonoBehaviour
{
    [SerializeField] 
    private float m_MaxHealth = 100;
    [SerializeField]
    private float m_Health;

    [SerializeField]
    private ProgressBar m_HealthBar;

    private void Start()
    {
        m_Health = m_MaxHealth;
    }

    public void SetupHealthBar(Canvas _canvas)
    {
        m_HealthBar.transform.SetParent(_canvas.transform);
    }

    public float GetHealth()
    {
        return m_Health;
    }

    public void Hit(float _damage, Transform _enemyPosition)
    {
        m_Health -= _damage;

        m_HealthBar.SetProgress(m_Health / m_MaxHealth, 3f);

        transform.LookAt(_enemyPosition);
    }

    public void Die()
    {
        if(m_HealthBar != null)
        {
            Destroy(m_HealthBar.gameObject);
        }
    }

    public ProgressBar GetHealthBar()
    {
        return m_HealthBar;
    }
}
