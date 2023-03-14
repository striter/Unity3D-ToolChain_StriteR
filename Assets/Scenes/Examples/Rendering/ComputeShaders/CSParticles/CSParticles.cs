using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.ComputeShaders
{
    
    public class CSParticles : MonoBehaviour
    {
        struct FParticle
        {
            public float3 position;
            public float3 velocity;
            public float life;
            public const int kSize = 7 * sizeof(float);
        }

        public int m_ParticleCount = 1000000;
        public Material m_Material;
        public ComputeShader m_Shader;

        private float2 m_CursorPos;
        
        private ComputeBuffer m_ParticleBuffer;
        private int m_GroupSizeX;
        private int kKernelID;
        private bool Available => m_Shader != null && m_Material != null;

        void Init()
        {
            Release();
            
            FParticle[] particles = new FParticle[m_ParticleCount];
            for (int i = 0; i < m_ParticleCount; i++)
                particles[i] = new FParticle() {position = URandom.RandomDirection(),life = URandom.Random01() * 4f};

            m_ParticleBuffer = new ComputeBuffer(m_ParticleCount, FParticle.kSize);
            m_ParticleBuffer.SetData(particles);
            kKernelID = m_Shader.FindKernel("CSParticle");
            m_Shader.GetKernelThreadGroupSizes(kKernelID,out var threadsX,out _,out _);
            m_GroupSizeX = Mathf.CeilToInt((float) m_ParticleCount / threadsX);
            m_Shader.SetBuffer(kKernelID,"_ParticleBuffer",m_ParticleBuffer);
            m_Material.SetBuffer("_ParticleBuffer",m_ParticleBuffer);
        }

        void Release()
        {
            if (m_ParticleBuffer == null)
                return;
            m_ParticleBuffer.Release();
            m_ParticleBuffer = null;
        }

        private void Awake()
        {
            if (!Available)
                return;
            Init();
        }

        private void OnDestroy()
        {
            if (!Available)
                return;
            
            Release();
        }

        private void Update()
        {
            if (!Available)
                return;
            
            m_Shader.SetFloat("_DeltaTime",Time.deltaTime);
            m_Shader.SetVector("_MousePosition",m_CursorPos.to4());
            m_Shader.Dispatch(kKernelID,m_GroupSizeX,1,1);
        }

        private void OnRenderObject()
        {
            m_Material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 6,m_ParticleCount );
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(m_CursorPos.to3xy(),.5f);
        }

        private void OnGUI()
        {
            Camera c = Camera.main;
            Event e = Event.current;
            float2 mousePos = default;
            mousePos = e.mousePosition;
            mousePos.y = c.pixelHeight - mousePos.y;
            
            m_CursorPos = c.ScreenToWorldPoint(new float3(mousePos,c.nearClipPlane+14)).XY();
        }
    }

}