using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Rendering.Pipeline
{
    using PostProcess;

    [Serializable]
    public struct ReflectionPassData
    {
        public bool screenSpaceReflection;
        [MFoldout(nameof(screenSpaceReflection), EPlanarReflectionMode.ScreenSpaceGeometry)] [Range(1, 8)] public int maxPlanarReflectionTexture;

        public static ReflectionPassData kDefault = new ReflectionPassData()
        {
            screenSpaceReflection = false,
            maxPlanarReflectionTexture = 4,
        };
    }
    
    public class ReflectionTexturePass: ISRPBase
    {
        private readonly FBlursCore m_Blurs;
        private readonly ReflectionPassData m_Data;
        private readonly RenderPassEvent m_Event;
        public ReflectionTexturePass(ReflectionPassData _data,RenderPassEvent _event)
        {
            m_Event = _event;
            m_Data = _data;
            m_Blurs = new FBlursCore();
        }

        public void EnqueuePass(ScriptableRenderer _renderer)
        {
            if (m_Data.screenSpaceReflection)
            {
                _renderer.EnqueuePass(new ScreenSpaceReflectionPass(m_Blurs));
                return;
            }
            
            if (PlanarReflection.m_Reflections.Count == 0)
                return;
            
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            int index = 0; 
            foreach (var reflectionComponent in PlanarReflection.m_Reflections)
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
                    case EPlanarReflectionMode.ScreenSpaceGeometry: reflectionPass = new FGeometryReflectionScreenSpace(); break;
                    case EPlanarReflectionMode.Render: reflectionPass = new FGeometryReflectionMirrorSpace(); break;
                }
                _renderer.EnqueuePass(reflectionPass.Setup(data,m_Blurs, reflectionComponent,_renderer,index,m_Event));
                index++;
            }
        }

        public void Dispose()
        {
            m_Blurs.Destroy();
        }
    }
}