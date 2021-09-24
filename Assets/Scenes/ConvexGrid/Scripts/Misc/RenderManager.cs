using System;
using System.Collections;
using System.Collections.Generic;
using Procedural;
using UnityEngine;

namespace  ConvexGrid
{
    public class RenderManager : MonoBehaviour,IConvexGridControl
    {
        public AnimationCurve m_PopCurve;
        public float m_Range=5f;
        private Timer m_PopTimer;

        private static readonly string kPopKeyword = "_POP";
        private static readonly int kPopPosition = Shader.PropertyToID("_PopPosition");
        private static readonly int kPopStrength = Shader.PropertyToID("_PopStrength");
        private static readonly int kPopRadiusSqr = Shader.PropertyToID("_PopRadiusSqr");
        private Transform m_Light;
        public void Init(Transform _transform)
        {
            m_PopTimer = new Timer(m_PopCurve.length, true);
            m_Light = transform.Find("Render/Directional Light");
        }

        private void OnDisable()
        {
            EndPop();
        }

        public void Tick(float _deltaTime)
        {
            TickPop(_deltaTime);
            m_Light.Rotate(0f,_deltaTime*5f,0f,Space.World);
        }

        void BeginPop(Vector3 _position)
        {
            m_PopTimer.Replay();
            
            Shader.SetGlobalVector(kPopPosition,_position);
            Shader.SetGlobalFloat(kPopRadiusSqr,m_Range*m_Range);
            Shader.SetGlobalFloat(kPopStrength,1f);
            URender.EnableGlobalKeyword(kPopKeyword,true);
        }

        void EndPop()
        {
            Shader.SetGlobalFloat(kPopStrength,1f);
            URender.EnableGlobalKeyword(kPopKeyword,false);
        }

        void TickPop(float _deltaTime)
        {
            if (!m_PopTimer.m_Timing)
                return;
            m_PopTimer.Tick(_deltaTime);
            Shader.SetGlobalFloat(kPopStrength,m_PopCurve.Evaluate(m_PopTimer.m_TimeElapsed));
            if (m_PopTimer.m_Timing)
                return;
            EndPop();
        }

        public void OnSelectVertex(ConvexVertex _vertex, byte _height)
        {
            BeginPop(_vertex.GetCornerPositionWS(_height));
        }

        public void OnAreaConstruct(ConvexArea _area)
        {
        }

        public void Clear()
        {
        }
    }

}