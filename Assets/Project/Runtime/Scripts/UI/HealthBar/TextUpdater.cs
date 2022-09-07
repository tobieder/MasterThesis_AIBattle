using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextUpdater : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_Text;

    public void SetText(float _progress)
    {
        m_Text.SetText($"{(_progress * 100).ToString("N2")}%");
    }
}
