using UnityEngine;

namespace Boids
{
    public class BoidsAnimation
    {
        private Mesh[] m_Animations;
        private readonly MeshFilter m_MainFilter;
        private readonly MeshFilter m_BlendFilter;
        private readonly MeshRenderer m_MainRenderer;
        private readonly MeshRenderer m_BlendRenderer;
        private readonly Timer m_BlendTimer = new Timer(.35f);
        private float m_Time;
        public BoidsAnimation(Transform _transform)
        {
            m_MainFilter = _transform.Find("Main").GetComponent<MeshFilter>();
            m_BlendFilter = _transform.Find("Blend").GetComponent<MeshFilter>();
            m_MainRenderer=m_MainFilter.GetComponent<MeshRenderer>();
            m_BlendRenderer=m_BlendFilter.GetComponent<MeshRenderer>();
        }

        public void Init(Material _material,Mesh[] _animations)
        {
            m_MainRenderer.sharedMaterial = _material;
            m_BlendRenderer.sharedMaterial = _material;
            m_Animations = _animations;
            m_BlendFilter.gameObject.SetActive(false);
        }

        public void SetAnimation(string _animation)
        {
            var mesh = m_Animations.Find(p=>p.name==_animation);
            if (mesh == null)
            {
                Debug.LogWarning("Invalid Animation Name:"+_animation);
                return;
            }
            m_BlendFilter.sharedMesh = mesh;
            m_BlendFilter.gameObject.SetActive(true);
            m_BlendTimer.Replay();
            m_Time = 0f;
        }
        
        public void Tick(float _deltaTime)
        {
            m_Time += _deltaTime;
            if (!m_BlendTimer.m_Timing)
                return;
            m_BlendTimer.Tick(_deltaTime);
            m_MainFilter.transform.localScale = Vector3.one * m_BlendTimer.m_TimeLeftScale;
            m_BlendFilter.transform.localScale = Vector3.one * m_BlendTimer.m_TimeElapsedScale;
            if (m_BlendTimer.m_Timing)
                return;
            m_MainFilter.sharedMesh = m_BlendFilter.sharedMesh;
            m_MainFilter.transform.localScale = Vector3.one;
            m_BlendFilter.gameObject.SetActive(false);
        }
    }
}