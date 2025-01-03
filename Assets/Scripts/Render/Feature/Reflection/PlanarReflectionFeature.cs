using System;
using Rendering.PostProcess;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    [Serializable]
    public struct ReflectionPassData
    {
        [Range(1, 8)] public int maxPlanarReflectionTexture;
        
        public static ReflectionPassData kDefault = new ReflectionPassData()
        {
            maxPlanarReflectionTexture = 4,
        };
    }
    
    public class PlanarReflectionFeature : ScriptableRendererFeature
    {
        public ReflectionPassData m_Data = ReflectionPassData.kDefault;
        private FBlursCore m_Blurs;
        public override void Create()
        {
            m_Blurs = new FBlursCore();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_Blurs.Destroy();
        }

        public override void AddRenderPasses(ScriptableRenderer _renderer, ref RenderingData _renderingData)
        {
            if (!RenderResources.Enabled)
                return;

            if (_renderingData.cameraData.isPreviewCamera)
                return;
            if (PlanarReflectionProvider.m_Reflections.Count == 0)
                return;
            
            var propertyBlock = new MaterialPropertyBlock();
            int index = 0; 
            foreach (var reflectionComponent in PlanarReflectionProvider.m_Reflections)
            {
                if (index >= m_Data.maxPlanarReflectionTexture)
                {
                    Debug.LogWarning("Reflection Plane Outta Limit!");
                    break;
                }
                reflectionComponent.SetPropertyBlock(propertyBlock,index);

                var data = reflectionComponent.m_Data;
                APlanarReflectionBase reflectionPass=null;
                switch (data.m_Type)
                {
                    case EPlanarReflectionMode.ScreenSpaceGeometry: reflectionPass = new FGeometryReflectionScreenSpace(data,m_Blurs, reflectionComponent,_renderer,index){renderPassEvent = RenderPassEvent.BeforeRenderingSkybox + 2}; break;
                    case EPlanarReflectionMode.Render: reflectionPass = new FGeometryReflectionMirrorSpace(data,m_Blurs, reflectionComponent,_renderer,index){renderPassEvent = RenderPassEvent.BeforeRenderingSkybox + 2}; break;
                }
                _renderer.EnqueuePass(reflectionPass);
                index++;
            }
            
        }
    }
}