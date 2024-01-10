using System;
using UnityEngine;

namespace TechToys.ThePlanet
{
    using static KPCG.Ocean;
    
    public class PCGEnvironment : MonoBehaviour,IPCGControl
    {
        private Material m_OceanMaterial;
        public SpringDamper m_RotationDamper ;
        private Light m_Light;
        private float pitch,yaw;
        private Transform m_Ocean;
        
        public void Init()
        {
            m_Ocean = transform.Find("Ocean");
            m_OceanMaterial = m_Ocean.GetComponent<MeshRenderer>().sharedMaterial;
            
            m_Light = transform.GetComponentInChildren<Light>();
            pitch = 30;
            yaw = 180;
            m_RotationDamper.Initialize(new Vector3(0f,0f,0f));
        }


        public void Tick(float _deltaTime)
        {
            kOceanRadius = m_OceanMaterial.GetFloat("_Radius");
            kOceanST1 = m_OceanMaterial.GetVector("_WaveST1");
            kOceanST2 = m_OceanMaterial.GetVector("_WaveST2");
            kOceanAmplitude1 = m_OceanMaterial.GetFloat("_WaveAmplitude1");
            kOceanAmplitude2 = m_OceanMaterial.GetFloat("_WaveAmplitude2");
            
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