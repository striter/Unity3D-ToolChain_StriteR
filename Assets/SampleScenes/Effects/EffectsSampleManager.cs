using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsSampleManager : MonoBehaviour
{
    public Light m_Light;
    public float m_LightRotateSpeed;
    private void Update()
    {
        m_Light.transform.Rotate(0, m_LightRotateSpeed * Time.deltaTime, 0, Space.World);

    }
}
