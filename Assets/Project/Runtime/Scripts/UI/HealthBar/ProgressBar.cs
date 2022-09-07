using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ProgressBar : MonoBehaviour
{
    [SerializeField]
    private Image m_ProgressImage;
    [SerializeField]
    private float m_DefaultSpeed = 1.0f;
    [SerializeField]
    private Gradient m_ColorGradient;
    [SerializeField] 
    UnityEvent<float> m_OnProgress;
    [SerializeField]
    UnityEvent m_OnCompleted;

    private Coroutine m_AnimationCoroutine;

    private void Start()
    {
        if(m_ProgressImage.type != Image.Type.Filled)
        {
            Debug.LogError($"{name}'s ProgressImage is not of type \"Filled\" so it cannot be used.");
            this.enabled = false;
        }

        SetProgress(1f);
    }

    public void SetProgress(float _progress)
    {
        SetProgress(_progress, m_DefaultSpeed);
    }

    public void SetProgress(float _progress, float _speed)
    {
        if(_progress < 0f || _progress > 1f)
        {
            Debug.LogWarning($"Invalid progress passed, excepted value is between 0 and 1. Gro {_progress}. Clamping.");
            _progress = Mathf.Clamp01(_progress);
        }

        if(_progress != m_ProgressImage.fillAmount)
        {
            if(m_AnimationCoroutine != null)
            {
                StopCoroutine(m_AnimationCoroutine);
            }

            m_AnimationCoroutine = StartCoroutine(AnimateProgress(_progress, _speed));
        }
    }

    private IEnumerator AnimateProgress(float _progress, float _speed)
    {
        float time = 0;
        float initialProgress = m_ProgressImage.fillAmount;

        while(time < 1)
        {
            m_ProgressImage.fillAmount = Mathf.Lerp(initialProgress, _progress, time);
            time += Time.deltaTime * _speed;

            m_ProgressImage.color = m_ColorGradient.Evaluate(1 - m_ProgressImage.fillAmount);

            m_OnProgress?.Invoke(m_ProgressImage.fillAmount);
            yield return null;
        }

        m_ProgressImage.fillAmount = _progress;
        m_ProgressImage.color = m_ColorGradient.Evaluate(1 - m_ProgressImage.fillAmount);

        m_OnProgress?.Invoke(_progress);
        m_OnCompleted?.Invoke();
    }
}
