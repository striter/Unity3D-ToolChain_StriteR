using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq.Extensions;

namespace TechToys.ThePlanet.Module.BOIDS
{
    public interface IBoidsAnimation
    {
        void Init(Transform _transform);
        void Initialize();
        void SetAnimation(string _animation);
        void Tick(float _deltaTime);
    }

    public class FBoidsAnimationEmpty : IBoidsAnimation
    {
        public void Init(Transform _transform)
        {
        }

        public void Initialize()
        {
        }

        public void SetAnimation(string _animation)
        {
        }

        public void Tick(float _deltaTime)
        {
        }
    }

    public class FBoidsAnimationNormal : IBoidsAnimation
    {
        private Animation m_Animation;
        public void Init(Transform _transform)
        {
            m_Animation = _transform.GetComponent<Animation>();
        }

        public void Initialize()
        {
            m_Animation.Rewind();
            m_Animation.Play();
        }

        public void SetAnimation(string _animation)
        {
            m_Animation.CrossFade(_animation,.2f);
        }

        public void Tick(float _deltaTime)
        {
        }
    }
    
    [Serializable]
    public struct FBoidsMeshAnimationConfig
    {
        public Material material;
        public Mesh[] meshes;
    }

    public class FBoidsMeshAnimation:IBoidsAnimation
    {
        private readonly Material m_Material;
        private readonly Mesh[] m_AnimationMeshes;
        private readonly Counter m_BlendCounter = new Counter(.25f);
        private MeshFilter m_MainFilter;
        private MeshFilter m_BlendFilter;
        private Animation m_MainAnimation;
        private Animation m_BlendAnimation;
        public FBoidsMeshAnimation(FBoidsMeshAnimationConfig _config)
        {
            m_Material = _config.material;
            m_AnimationMeshes = _config.meshes;
        }

        public void Init(Transform _transform)
        {
            m_MainFilter = _transform.Find("Main").GetComponent<MeshFilter>();
            m_BlendFilter = _transform.Find("Blend").GetComponent<MeshFilter>();
            m_MainFilter.GetComponent<MeshRenderer>().sharedMaterial = m_Material;
            m_BlendFilter.GetComponent<MeshRenderer>().sharedMaterial = m_Material;
            m_MainAnimation = m_MainFilter.GetComponent<Animation>();
            m_BlendAnimation = m_MainFilter.GetComponent<Animation>();
        }
        
        public void Initialize()
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
            m_MainAnimation["BoidsAnim"].speed = Random.Range(0.9f,1.1f);
        }
        
        public void Tick(float _deltaTime)
        {
            if (!m_BlendCounter.m_Playing)
                return;
            m_BlendCounter.Tick(_deltaTime);
            m_MainFilter.transform.localScale = Vector3.one * m_BlendCounter.m_TimeLeftScale;
            m_BlendFilter.transform.localScale = Vector3.one * m_BlendCounter.m_TimeElapsedScale;
            if (m_BlendCounter.m_Playing)
                return;
            m_MainFilter.sharedMesh = m_BlendFilter.sharedMesh;
            m_MainFilter.transform.localScale = Vector3.one;
            m_BlendFilter.gameObject.SetActive(false);
            m_BlendFilter.transform.localScale = Vector3.zero;
        }
    }
}