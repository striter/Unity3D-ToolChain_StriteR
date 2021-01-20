using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsOverview : MonoBehaviour
{
    public Light m_Light;
    public float m_LightRotateSpeed;
    public Transform m_LitParent;
    public float m_LitParentRotateSpeed;
    private void Update()
    {
        m_Light.transform.Rotate(0, m_LightRotateSpeed * Time.deltaTime, 0, Space.World);
        m_LitParent.Rotate(m_LitParentRotateSpeed * Time.deltaTime, 0, 0, Space.World);
    }
}
