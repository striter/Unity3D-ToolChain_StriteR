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

        public void Tick(float _deltaTime)
        {
            yaw += _deltaTime * 10f;
            m_Light.transform.rotation = Quaternion.Euler(m_RotationDamper.Tick(_deltaTime,new Vector3(pitch,yaw)));
        }

        public void Rotate(float _pitch,float _yaw)
        {
            pitch += _pitch;
            yaw += _yaw;
        }

        public Vector3 Output() => m_Light.transform.forward;

        public void Clear()
        {
        }

        public void Dispose()
        {
        }
    }

}