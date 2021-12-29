using System;
using UnityEngine;

namespace Boids
{
    public interface IBoidsAnimation
    {
        void Init(Transform _transform);
        void Spawn();
        void SetAnimation(string _animation);
        void Tick(float _deltaTime);
    }

    public class BoidsEmptyAnimation : IBoidsAnimation
    {
        public void Init(Transform _transform)
        {
        }

        public void Spawn()
        {
        }

        public void SetAnimation(string _animation)
        {
        }

        public void Tick(float _deltaTime)
        {
        }
    }
    
    public class BoidsMeshAnimation:IBoidsAnimation
    {
        private readonly Material m_Material;
        private readonly Mesh[] m_AnimationMeshes;
        private readonly Counter m_BlendCounter = new Counter(.25f);
        private MeshFilter m_MainFilter;
        private MeshFilter m_BlendFilter;
        public BoidsMeshAnimation(Material _material,Mesh[] _meshes)
        {
            m_Material = _material;
            m_AnimationMeshes = _meshes;
        }

        public void Init(Transform _transform)
        {
            m_MainFilter = _transform.Find("Main").GetComponent<MeshFilter>();
            m_BlendFilter = _transform.Find("Blend").GetComponent<MeshFilter>();
            m_MainFilter.GetComponent<MeshRenderer>().sharedMaterial = m_Material;
            m_BlendFilter.GetComponent<MeshRenderer>().sharedMaterial = m_Material;
        }
        
        public void Spawn()
        {
            m_BlendFilter.gameObject.SetActive(false);
            m_MainFilter.transform.localScale = Vector3.zero;
        }

        public void SetAnimation(string _animation)
        {
            var mesh = m_AnimationMeshes.Find(p=>p.name==_animation);
            if (mesh == null)
            {
                Debug.LogWarning("Invalid Animation Name:"+_animation);
                return;
            }
            m_BlendFilter.sharedMesh = mesh;
            m_BlendFilter.gameObject.SetActive(true);
            m_BlendFilter.transform.localScale = Vector3.zero;
            m_BlendCounter.Replay();
        }
        
        public void Tick(float _deltaTime)
        {
            if (!m_BlendCounter.m_Counting)
                return;
            m_BlendCounter.Tick(_deltaTime);
            m_MainFilter.transform.localScale = Vector3.one * m_BlendCounter.m_TimeLeftScale;
            m_BlendFilter.transform.localScale = Vector3.one * m_BlendCounter.m_TimeElapsedScale;
            if (m_BlendCounter.m_Counting)
                return;
            m_MainFilter.sharedMesh = m_BlendFilter.sharedMesh;
            m_MainFilter.transform.localScale = Vector3.one;
            m_BlendFilter.gameObject.SetActive(false);
            m_BlendFilter.transform.localScale = Vector3.zero;
        }
    }
}