using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering
{
    public struct GHorizonBend
    {
        public Camera m_BendCamera;
        public float m_BendBegin;
        public float m_BendWidth;
        public Vector3 m_BendDirection;

        public static readonly GHorizonBend kDefault = new GHorizonBend()
        {
            m_BendCamera=null,
            m_BendBegin=10f, 
            m_BendWidth=5f,
            m_BendDirection=new Vector3(0,-1,0)
        };
        
        private static readonly string kKeyword = "_HORIZONBEND";
        private static readonly int ID_HorizonBendPosition =  Shader.PropertyToID("_HorizonBendPosition");
        private static readonly int ID_HorizonBendDistances = Shader.PropertyToID("_HorizonBendDistances");
        private static readonly int ID_HorizonBendDirection = Shader.PropertyToID("_HorizonBendDirection");
        public void OnKeywordSet()
        {
            Shader.EnableKeyword(kKeyword);
            if (!m_BendCamera)
                return;
            
            Shader.SetGlobalVector(ID_HorizonBendPosition,m_BendCamera.transform.position);
            Shader.SetGlobalVector(ID_HorizonBendDirection,m_BendDirection);
            Shader.SetGlobalVector(ID_HorizonBendDistances,new Vector4(m_BendBegin,m_BendBegin+m_BendWidth));
        }

        public void OnKeywordDispose()
        {
            Shader.DisableKeyword(kKeyword);
        }

        public void Tick()
        {
            if (!m_BendCamera)
                return;
            Shader.SetGlobalVector(ID_HorizonBendPosition,m_BendCamera.transform.position);
        }
    }

}