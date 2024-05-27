using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.ArtScenes.Pool
{
    public class Pool : MonoBehaviour
    {
        static float3 GerstnerWave(float2 _uv,float4 _waveST,float _amplitude,float _spikeAmplitude,float3 _biTangent,float3 _normal,float3 _tangent,float _time)
        {
            float2 flowUV=_uv+_time*_waveST.xy*_waveST.zw;
            float flowSin=flowUV.x*_waveST.x+flowUV.y*_waveST.y;
            math.sincos(flowSin*math.PI,out var sinFlow,out var cosFlow);
            float spike=_spikeAmplitude*cosFlow;
            return _normal*sinFlow*_amplitude + _biTangent*spike*_waveST.x + _tangent * spike*_waveST.y;
        }
        
        public MeshRenderer m_WaterMesh;
        private Transform[] m_WaveElements;
        private float3[] m_WavePositions;

        private ComplexElements[] m_WaveComplex;
        public G2Polygon[] m_ComplexPolygons;
        
        private GPlane WaterPlane => new GPlane( m_WaterMesh.transform.up, m_WaterMesh.bounds.center);
        public class ComplexElements
        {
            public Transform transform;
            public G2Triangle[] triangles;
            public float4x4 initialMatrix;
        }
        private void Awake()
        {
            m_WaveElements = transform.Find("Elements").GetSubChildren().ToArray();
            m_WavePositions = m_WaveElements.Select(p => (float3) p.transform.position.SetY(WaterPlane.position.y)).ToArray();

            List<PTriangle> triangleIndexes = new List<PTriangle>();
            
            
            m_WaveComplex = transform.Find("Complexes").GetSubChildren().Select((p,i)=>
            {
                var initialPosition = p.transform.position;
                initialPosition.y = WaterPlane.position.y;
                var initialRotation = p.transform.rotation;
                UTriangulation.BowyerWatson(m_ComplexPolygons[i].positions, ref triangleIndexes);
                return new ComplexElements() { triangles = m_ComplexPolygons[i].GetTriangles(triangleIndexes).ToArray(), transform = p,initialMatrix = Matrix4x4.TRS(initialPosition,initialRotation,transform.localScale) };
            }).ToArray();
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
            var time = Time.time;
            
            for(var i = 0; i<m_WavePositions.Length;i++)
            {
                float3 position = m_WavePositions[i];
                var localWave = GerstnerWave(position.xz, waveST1, amp1, spikeAmp1, Vector3.forward, Vector3.up, Vector3.forward, time);
                localWave += GerstnerWave(position.xz, waveST2, amp2, spikeAmp2, Vector3.forward, Vector3.up, Vector3.forward, time);
                m_WaveElements[i].transform.position = position + localWave;
                m_WaveElements[i].transform.rotation = Quaternion.LookRotation(localWave.normalize(),Vector3.up);
            }

            foreach (var complex in m_WaveComplex)
            {
                var positionAffection =  float3.zero;
                var rotationAffection = float4.zero;
                foreach (var triangle in complex.triangles)
                {
                    var triangle3 = complex.initialMatrix * triangle.to3xz();
                    float3 waveAffection = 0;
                    for (var i = 0; i < triangle3.Length; i++)
                    {
                        var position = triangle3[i];
                        var localWave = GerstnerWave(position.xz, waveST1, amp1, spikeAmp1, Vector3.forward, Vector3.up, Vector3.forward, time);
                        localWave += GerstnerWave(position.xz, waveST2, amp2, spikeAmp2, Vector3.forward, Vector3.up, Vector3.forward, time);
                        waveAffection += localWave;
                        triangle3[i] += localWave;
                    }

                    rotationAffection += triangle3.GetRotation().value;
                    waveAffection /= triangle3.Length;
                    waveAffection.xz = 0;
                    positionAffection += waveAffection;
                }
                
                positionAffection /= complex.triangles.Length;
                rotationAffection /= complex.triangles.Length;
            
                
                complex.transform.SetPositionAndRotation(math.mul(complex.initialMatrix , positionAffection.to4(1)).xyz,new quaternion( rotationAffection)  );
            }
        }

        private void OnDrawGizmos()
        {
            if (m_WaveComplex == null)
                return;
            
            foreach (var complex in m_WaveComplex)
            {
                Gizmos.matrix = complex.initialMatrix;
                foreach (var triangle in complex.triangles)
                    triangle.DrawGizmos();
                
            }
            
        }
    }

}
