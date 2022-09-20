using System;
using System.Collections;
using System.Collections.Generic;
using Procedural;
using UnityEngine;

namespace  PolyGrid
{
    public class RenderManager : MonoBehaviour,IPolyGridControl,IPolyGridModifyCallback
    {
        public AnimationCurve m_PopCurve;
        public float m_RangeMultiplier=5f;
        private Counter m_PopCounter;

        private static readonly string kPopKeyword = "_POP";
        private static readonly int kPopPosition = Shader.PropertyToID("_PopPosition");
        private static readonly int kPopStrength = Shader.PropertyToID("_PopStrength");
        private static readonly int kPopRadiusSqr = Shader.PropertyToID("_PopRadiusSqr");
        private Transform m_Light;
        public void Init(Transform _transform)
        {
            m_PopCounter = new Counter(m_PopCurve.length, true);
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
            m_PopCounter.Replay();
            
            Shader.SetGlobalVector(kPopPosition,_position);
            var radius = KPolyGrid.tileSize * m_RangeMultiplier;
            Shader.SetGlobalFloat(kPopRadiusSqr,radius*radius);
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
            if (!m_PopCounter.m_Playing)
                return;
            m_PopCounter.Tick(_deltaTime);
            Shader.SetGlobalFloat(kPopStrength,m_PopCurve.Evaluate(m_PopCounter.m_TimeElapsed));
            if (m_PopCounter.m_Playing)
                return;
            EndPop();
        }

        public void OnVertexModify(PolyVertex _vertex, byte _height, bool _construct)
        {
            BeginPop(_vertex.ToCornerPosition(_height));
        }
        
        public void Clear()
        {
        }

    }

}