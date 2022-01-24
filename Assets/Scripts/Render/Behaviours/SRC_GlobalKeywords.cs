using System;
using UnityEngine;

namespace Rendering.Pipeline
{
    [ExecuteInEditMode]
    public class SRC_GlobalKeywords : MonoBehaviour
    {
        public GlobalKeyword_EditGrid m_EditGrid=new GlobalKeyword_EditGrid();
        public GlobalKeyword_CloudShadow m_CloudShadow=new GlobalKeyword_CloudShadow();
        public GlobalKeyword_HorizonBend m_HorizonBend=new GlobalKeyword_HorizonBend();
        public GlobalKeyword_VerticalFog m_VerticalFog = new GlobalKeyword_VerticalFog();
        private AGlobalKeyword[] m_GlobalKeywordsHelper => new AGlobalKeyword[] {m_EditGrid,m_CloudShadow,m_HorizonBend,m_VerticalFog};
        private void OnDidApplyAnimationProperties()=>OnValidate();

        private void OnEnable() => OnValidate();

        private void OnValidate()
        {
            if (!enabled)
                return;
            
            foreach (AGlobalKeyword aGlobalKeyword in m_GlobalKeywordsHelper)
                aGlobalKeyword.SetupKeywords();
        }
        
        private void OnDisable()
        {
            foreach (AGlobalKeyword aGlobalKeyword in m_GlobalKeywordsHelper)
                aGlobalKeyword.DisposeKeywords();
        }

        private void LateUpdate()
        {
            foreach (AGlobalKeyword aGlobalKeyword in m_GlobalKeywordsHelper.Collect(p=>p.m_Enable))
                aGlobalKeyword.TickParameters();
        }
    }

    #region Keywords
    public abstract class AGlobalKeyword
    {
        public bool m_Enable=false;
        protected abstract string CoreKeyword { get; }
        private readonly string KW_Core;
        protected AGlobalKeyword()
        {
            KW_Core = CoreKeyword;
        }

        public void SetupKeywords()
        {
            URender.EnableGlobalKeyword(KW_Core,m_Enable);
            if (!m_Enable)
                return;
            OnKeywordSet();
        }

        public void DisposeKeywords()
        {
            URender.EnableGlobalKeyword(KW_Core,false);
            OnKeywordDispose();
        }

        protected abstract void OnKeywordSet();
        protected abstract void OnKeywordDispose();
        public virtual void TickParameters()
        {
        }
    }
    
    [Serializable]
    public class GlobalKeyword_EditGrid:AGlobalKeyword
    {
        protected override string CoreKeyword=>"_EDITGRID";
        private static readonly int ID_GridSize = Shader.PropertyToID("_GridSize");
        private static readonly int ID_GridColor = Shader.PropertyToID("_GridColor");
        [MFoldout(nameof(m_Enable),true)][ColorUsage(true, true)] public Color m_GridColor = Color.white;
        [MFoldout(nameof(m_Enable),true)][Range(0.01f, .2f)] public float m_GridSize = .015f;
        protected override void OnKeywordSet()
        {
            Shader.SetGlobalColor(ID_GridColor, m_GridColor);
            Shader.SetGlobalVector(ID_GridSize, new Vector4(0f,0f, 2f, m_GridSize));
        }
        protected override void OnKeywordDispose()
        {
        }
    }

    [Serializable]
    public class GlobalKeyword_CloudShadow : AGlobalKeyword
    {
        protected override string CoreKeyword => "_CLOUDSHADOW";
        private static readonly int ID_CloudTexture = Shader.PropertyToID("_CloudShadowTexture");
        private static readonly int ID_CloudShape = Shader.PropertyToID("_CloudParam1");
        private static readonly int ID_CloudFlow = Shader.PropertyToID("_CloudParam2");
        
        public Texture m_ShadowTexture;
        [MFoldout(nameof(m_Enable),true)][Range(0,1)] public float m_ShadowStrength = 1f;
        [MFoldout(nameof(m_Enable),true)][Range(1,200)]public float m_ShadowScale;
        [MFoldout(nameof(m_Enable),true)][Range(0,100)]public float m_ShadowPlaneDistance;
        [MFoldout(nameof(m_Enable),true)][Range(0f,1f)] public float m_StepBegin;
        [MFoldout(nameof(m_Enable),true)][Range(0f,.5f)]public float m_StepWidth;
        public Vector2 m_Flow;

        protected override void OnKeywordSet()
        {
            Shader.SetGlobalTexture(ID_CloudTexture,m_ShadowTexture);
            Shader.SetGlobalVector(ID_CloudShape,new Vector4(1f-m_ShadowStrength,m_ShadowScale,m_ShadowPlaneDistance));
            Shader.SetGlobalVector(ID_CloudFlow,new Vector4(m_StepBegin,m_StepBegin+m_StepWidth,m_Flow.x,m_Flow.y));
        }
        protected override void OnKeywordDispose()
        {
            Shader.SetGlobalTexture(ID_CloudTexture,null);
        }
    }

    [Serializable]
    public class GlobalKeyword_HorizonBend : AGlobalKeyword
    {        
        protected override string CoreKeyword  => "_HORIZONBEND";
        private static readonly int ID_HorizonBendPosition =  Shader.PropertyToID("_HorizonBendPosition");
        private static readonly int ID_HorizonBendDistances = Shader.PropertyToID("_HorizonBendDistances");
        private static readonly int ID_HorizonBendDirection = Shader.PropertyToID("_HorizonBendDirection");
        
        [MFoldout(nameof(m_Enable),true)] public Camera m_BendCamera=null;     //?
        [MFoldout(nameof(m_Enable),true)] public float m_BendBegin=10f;
        [MFoldout(nameof(m_Enable),true)] public float m_BendWidth=5f;
        [MFoldout(nameof(m_Enable),true)] public Vector3 m_BendDirection=new Vector3(0,-1,0);

        protected override void OnKeywordSet()
        {
            if (!m_BendCamera)
                return;
            
            Shader.SetGlobalVector(ID_HorizonBendPosition,m_BendCamera.transform.position);
            Shader.SetGlobalVector(ID_HorizonBendDirection,m_BendDirection);
            Shader.SetGlobalVector(ID_HorizonBendDistances,new Vector4(m_BendBegin,m_BendBegin+m_BendWidth));
        }

        protected override void OnKeywordDispose()
        {
        }

        public override void TickParameters()
        {
            base.TickParameters();
            if (!m_BendCamera)
                return;
            Shader.SetGlobalVector(ID_HorizonBendPosition,m_BendCamera.transform.position);
        }
    }

    [Serializable]
    public class GlobalKeyword_VerticalFog : AGlobalKeyword
    {
        protected override string CoreKeyword=> "_VERTICALFOG";
        private static readonly int ID_VerticalFogBegin=Shader.PropertyToID("_VerticalFogBegin");
        private static readonly int ID_VerticalFogEnd=Shader.PropertyToID("_VerticalFogEnd");
        private static readonly int ID_VerticalFogPow=Shader.PropertyToID("_VerticalFogPow");
        private static readonly int ID_VerticalFogColor=Shader.PropertyToID("_VerticalFogColor");

        public float m_FogBegin = 0f;
        [Clamp(0f)]public float m_FogDistance = 2f;
        [Clamp(0.01f)] public float m_FogPow = 1f;
        [ColorUsage(true, true)] public Color m_FogColor = Color.white;
        protected override void OnKeywordSet()
        {
            Shader.SetGlobalFloat(ID_VerticalFogBegin,m_FogBegin);
            Shader.SetGlobalFloat(ID_VerticalFogEnd,m_FogBegin+m_FogDistance);
            Shader.SetGlobalFloat(ID_VerticalFogPow,m_FogPow);
            Shader.SetGlobalColor(ID_VerticalFogColor,m_FogColor);
        }

        protected override void OnKeywordDispose()
        {
        }
    }
    #endregion
}