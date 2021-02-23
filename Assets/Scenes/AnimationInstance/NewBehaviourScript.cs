using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public int m_Test;

    private void Awake()
    {
        Debug.Log(m_Test);
    }
    private void Start()
    {
        Debug.LogWarning(m_Test);
    }
    private void OnDestroy()
    {
        Debug.LogError(m_Test);
    }
}
