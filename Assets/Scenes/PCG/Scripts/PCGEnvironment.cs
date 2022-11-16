using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCG
{
    public class PCGEnvironment : MonoBehaviour,IPolyGridControl
    {
        public Damper m_RotationDamper ;
        private Light m_Light;
        private float pitch,yaw;
        
        public void Init()
        {
            m_Light = transform.GetComponentInChildren<Light>();
            pitch = 30;
            yaw = 180;
            m_RotationDamper.Initialize(new Vector3(0f,0f,0f));
        }

        public void Tick(float _deltaTIme)
        {
            m_Light.transform.rotation = Quaternion.Euler(m_RotationDamper.Tick(_deltaTIme,new Vector3(pitch,yaw,0)));
        }

        public void Rotate(float _pitch,float _yaw)
        {
            pitch += _pitch;
            yaw += _yaw;
        }

        public void Clear()
        {
        }

        public void Dispose()
        {
        }
    }

}