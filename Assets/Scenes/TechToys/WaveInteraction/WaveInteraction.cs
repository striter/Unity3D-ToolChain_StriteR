using System;
using System.Linq;
using Geometry;
using Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.WaveInteraction
{
    public class WaveInteraction : MonoBehaviour
    {
        float3 GerstnerWave(float2 _uv,float4 _waveST,float _amplitude,float _spikeAmplitude,float3 _biTangent,float3 _normal,float3 _tangent,float _time)
        {
            float2 flowUV=_uv+_time*_waveST.xy*_waveST.zw;
            float flowSin=flowUV.x*_waveST.x+flowUV.y*_waveST.y;
            math.sincos(flowSin*math.PI,out var sinFlow,out var cosFlow);
            float spike=_spikeAmplitude*cosFlow;
            return _normal*sinFlow*_amplitude + _biTangent*spike*_waveST.x + _tangent * spike*_waveST.y;
        }
        
        public MeshRenderer m_WaterMesh;
        public GameObject m_WaveObj;
        public Damper m_MoveDamper = new Damper();
        public GTriangle m_BoatFloatPoint = GTriangle.kDefault;

        public GameObject[] m_WaveElements;
        private float3[] m_WavePositions;
        
        private float3 m_Position;
        private float3 m_Destination;
        private float3 m_Forward;

        private void Awake()
        {
            m_Position = m_WaterMesh.transform.position;
            m_Destination = m_Position;
            m_Forward = Vector3.forward;
            m_MoveDamper.Initialize(m_Position);
            m_WavePositions = m_WaveElements.Select(p => (float3) p.transform.position.SetY(m_WaterMesh.transform.position.y)).ToArray();
        }

        void Update()
        {
            var material = m_WaterMesh.sharedMaterial;
            var waveST1 = material.GetVector("_WaveST1");
            var amp1 = material.GetFloat("_WaveAmplitude1");
            var spikeAmp1 = material.GetFloat("_WaveSpikeAmplitude1");
            var waveST2 = material.GetVector("_WaveST2");
            var amp2 = material.GetFloat("_WaveAmplitude2");
            var spikeAmp2 = material.GetFloat("_WaveSpikeAmplitude2");
            float time = Time.time;
            
            for(int i = 0; i<m_WavePositions.Length;i++)
            {
                float3 position = m_WavePositions[i];
                var localWave = GerstnerWave(position.xz, waveST1, amp1, spikeAmp1, Vector3.forward, Vector3.up, Vector3.forward, time);
                localWave += GerstnerWave(position.xz, waveST2, amp2, spikeAmp2, Vector3.forward, Vector3.up, Vector3.forward, time);
                m_WaveElements[i].transform.position = position + localWave;
                m_WaveElements[i].transform.rotation = Quaternion.LookRotation(localWave.normalize(),Vector3.up);
            }
            
            
            float deltaTime = Time.deltaTime;
            if (Input.GetMouseButton(0) && UGeometry.Intersect( Camera.main.ScreenPointToRay(Input.mousePosition),
                    new GPlane(Vector3.up, m_WaterMesh.transform.position), out var hitPoint))
            {
                m_Destination = hitPoint;
                if((m_Destination - m_Position).sqrmagnitude() > 0)
                    m_Forward = (m_Destination - m_Position).normalize();
            }
                
            m_Position = m_MoveDamper.Tick(deltaTime,m_Destination);
            
            Quaternion localRotation = Quaternion.LookRotation(m_Forward,Vector3.up);
            
            float3[] waveAffections = new float3[3];
            float3[] wavePositions = new float3[3];
            var localToWorld = Matrix4x4.Rotate(localRotation) * Matrix4x4.Scale(m_WaveObj.transform.lossyScale);
            for (int i = 0; i < m_BoatFloatPoint.Length; i++)
            {
                var position =   m_Position + (float3)localToWorld.MultiplyPoint( m_BoatFloatPoint[i]);
                var localWave = GerstnerWave(position.xz, waveST1, amp1, spikeAmp1, Vector3.forward, Vector3.up, Vector3.forward, time);
                localWave += GerstnerWave(position.xz, waveST2, amp2, spikeAmp2, Vector3.forward, Vector3.up, Vector3.forward, time);
                waveAffections[i] = localWave;
                wavePositions[i] = position + localWave;
            }

            GTriangle waveTriangle = new GTriangle(wavePositions[0], wavePositions[1], wavePositions[2]);
            m_WaveObj.transform.rotation = waveTriangle.GetRotation();

            float3 waveAffection = 0;
            foreach (var affection in waveAffections)
                waveAffection += affection;
            waveAffection /= waveAffections.Length;

            waveAffection.xz = 0;
            m_WaveObj.transform.position = m_Position + waveAffection;

        }
    }

}
