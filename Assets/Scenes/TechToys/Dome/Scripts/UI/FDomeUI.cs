using Dome.LocalPlayer;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dome.UI
{
    public class FDomeUI : ADomeController
    {
        [Readonly] public float m_Fade;
        public float m_FadeSpeed = 1;
        public Image m_FadeImg { get; private set; }
        
        public override void OnInitialized()
        {
            m_FadeImg = transform.Find("Fade").GetComponent<Image>();
            m_Fade = 0;
            m_FadeImg.enabled = false;
        }

        public override void Tick(float _deltaTime)
        {
            m_Fade = math.clamp(m_Fade - _deltaTime * m_FadeSpeed,0,1);
            m_FadeImg.color = Color.black.SetA(umath.pow2(m_Fade));
            m_FadeImg.enabled = m_Fade != 0;
        }

        public void OnEntityControlChanged(ADomePlayerControl _controller)
        {
            m_Fade = 1.2f;
            m_FadeImg.enabled = false;
        }

        public override void Dispose()
        {
        }
    }
}