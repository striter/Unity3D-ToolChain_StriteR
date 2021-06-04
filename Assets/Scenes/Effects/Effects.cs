using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Effects : MonoBehaviour
{
    public Vector3 m_RotateSpeed;
    private void Update()
    {
        transform.Rotate(m_RotateSpeed*Time.deltaTime, Space.World);
    }
}
