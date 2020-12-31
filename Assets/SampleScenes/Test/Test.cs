using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float m_Speed=60;
    public Light m_DirectionalLight;

    private void Update()
    {
        m_DirectionalLight.transform.Rotate(0, m_Speed*Time.deltaTime, 0, Space.World);
    }
}
