using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    [Serializable]
    public struct DUVMapping : IPostProcessParameter
    {
        [Title] public bool vortexDistort;
        [Foldout(nameof(vortexDistort), true)][RangeVector(0, 1)] public Vector2 vortexCenter;
        [Foldout(nameof(vortexDistort), true)][Range(-5, 5)] public float vortexStrength;

        [Title] public bool paniniProjection;
        [Foldout(nameof(paniniProjection), true)][Range(0, 1)] public float paniniDistance;
        [Foldout(nameof(paniniProjection), true)][Range(0, 1)] public float paniniCropToFit;
        public bool Validate() => vortexDistort || paniniProjection;

        public static DUVMapping kDefault = new()
        {
            vortexDistort = false,
            vortexCenter = Vector2.one*.5f,
            vortexStrength = .2f,
            
            paniniProjection = true,
            paniniDistance = .5f,
            paniniCropToFit = 1f
        };
    }

    public class FUVMappingCore : PostProcessCore<DUVMapping>
    {
        const string kVortexDistort = "_VORTEXDISTORT";
        static readonly int kVortexStrength = Shader.PropertyToID("_VortexStrength");
        static readonly int kVortexCenter = Shader.PropertyToID("_VortexCenter");
        
        static readonly int kPaniniParams = Shader.PropertyToID("_PaniniParams");
        public enum EPaniniType
        {
            None = 0,
            _PANINI_GENERAIC=1,
            _PANINI_UNITDISTANCE=2,
        }
        
        public override bool Validate(ref RenderingData _renderingData,ref DUVMapping _data)
        {
            if (m_Material.EnableKeyword(kVortexDistort, _data.vortexDistort))
            {
                m_Material.SetVector(kVortexCenter, _data.vortexCenter.ToVector4());
                m_Material.SetFloat(kVortexStrength, _data.vortexStrength);
            }
            DoPaniniProjection(_data.paniniProjection,_data.paniniDistance, _data.paniniCropToFit, _renderingData.cameraData.camera, _renderingData.cameraData.cameraTargetDescriptor);
            return base.Validate(ref _renderingData,ref _data);
        }
        
        // Back-ported & adapted from the work of the Stockholm demo team - thanks Lasse!
        void DoPaniniProjection(bool _enable,float _distance, float _cropToFit, Camera camera, RenderTextureDescriptor _descriptor)
        {
            var paniniType = EPaniniType.None;
            var paniniParams = float4.zero;
            if (_enable)
            {
                var viewExtents = CalcViewExtents(camera,_descriptor);
                var cropExtents = CalcCropExtents(_distance,viewExtents);

                var scaleX = cropExtents.x / viewExtents.x;
                var scaleY = cropExtents.y / viewExtents.y;
                var scaleF = math.min(scaleX, scaleY);

                var paniniD = _distance;
                var paniniS = math.lerp(1f, umath.saturate(scaleF), _cropToFit);
                paniniParams = new float4(viewExtents.x, viewExtents.y, paniniD, paniniS);
                paniniType = 1f - math.abs(paniniD) > float.Epsilon
                    ? EPaniniType._PANINI_GENERAIC
                    : EPaniniType._PANINI_UNITDISTANCE;
            }

            if (m_Material.EnableKeywords(paniniType))
                m_Material.SetVector(kPaniniParams, paniniParams);
        }

        float2 CalcViewExtents(Camera camera, RenderTextureDescriptor _descriptor)
        {
            var fovY = camera.fieldOfView * Mathf.Deg2Rad;
            var aspect = _descriptor.width / (float)_descriptor.height;

            var viewExtY = Mathf.Tan(0.5f * fovY);
            var viewExtX = aspect * viewExtY;

            return new float2(viewExtX, viewExtY);
        }

        float2 CalcCropExtents(float d,float2 projPos)
        {
            var viewDist = 1f + d;

            var projHyp = Mathf.Sqrt(projPos.x * projPos.x + 1f);

            var cylDistMinusD = 1f / projHyp;
            var cylDist = cylDistMinusD + d;
            var cylPos = projPos * cylDistMinusD;

            return cylPos * (viewDist / cylDist);
        }
    }
        
        
    public class PostProcess_UVMapping : APostProcessBehaviour<FUVMappingCore, DUVMapping>
    {
        public override bool OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.UVMapping;
    }
}