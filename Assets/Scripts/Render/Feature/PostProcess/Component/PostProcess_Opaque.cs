using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using Rendering.Pipeline;
using Rendering.Pipeline.Mask;
using Runtime.Random;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

namespace Rendering.PostProcess
{
    public class PostProcess_Opaque:PostProcessBehaviour<FOpaqueCore, DOpaque>
    {
        public bool m_Opaque = true;
        public override bool m_OpaqueProcess => m_Opaque;
        public override EPostProcess Event => EPostProcess.Opaque;

        #region HelperFunc
        SingleCoroutine m_ScanCoroutine;
        public void StartDepthScanCircle(Vector3 origin,  float radius = 20, float duration = 1.5f)
        {
            if (m_ScanCoroutine==null)
                m_ScanCoroutine =  CoroutineHelper.CreateSingleCoroutine();
            m_ScanCoroutine.Stop();
            m_Data.m_Scan = true;
            m_Data.m_ScanData.m_Origin = origin;
            m_ScanCoroutine.Start(TIEnumerators.ChangeValueTo((float value) => {
                m_Data.m_ScanData.m_Elapse= radius * value; 
                ValidateParameters();
            }, 0, 1, duration, () => { 
                m_Data.m_Scan = false;
                ValidateParameters();
            }));
        }
        #endregion
        
#if UNITY_EDITOR
        public bool m_DrawGizmos = true;
        private void OnDrawGizmos()
        {
            if (!enabled||!m_DrawGizmos)
                return;

            if (m_Data.m_Scan)
            {
                ref var data=ref m_Data. m_ScanData ;
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(data.m_Origin, .2f);
                Gizmos.color = data.m_Color;
                Gizmos.DrawWireSphere(data.m_Origin, data.m_Elapse);
                Gizmos.DrawWireSphere(data.m_Origin, data.m_Elapse + data.m_Width);
            }


            if (m_Data.m_Area)
            {
                ref var data = ref m_Data.m_AreaData;
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(data.m_Origin, .1f);

                Gizmos.color = data.m_FillColor;
                Gizmos.DrawWireSphere(data.m_Origin,data.m_Radius);

                Gizmos.color = data.m_EdgeColor;
                Gizmos.DrawWireSphere(data.m_Origin,data.m_Radius+data.m_Width);
            }
        }
#endif
    }
    public class FOpaqueCore:PostProcessCore<DOpaque>
    {
        private const string kScanKW = "_SCAN";
        private const string kAreaKW = "_AREA";
        private const string kOutlineKW = "_OUTLINE";
        private const string kHighlightKW = "_HIGHLIGHT";
        private const string kAmbientOcclusionKW = "_AO";
        private const string kVolumetricCloudKW = "_VOLUMETRICCLOUD";
        

        private static readonly int kSampleID = Shader.PropertyToID("_Opaque_Sample");
        
        private readonly FBlursCore m_HighlightBlur;
        private static readonly int kHighlightMaskID = Shader.PropertyToID("_OUTLINE_MASK");
        private static readonly RenderTargetIdentifier kHighlightMaskRT = new RenderTargetIdentifier(kHighlightMaskID);
        private static readonly int kHighlightMaskBlurID = Shader.PropertyToID("_OUTLINE_MASK_BLUR");
        private static readonly RenderTargetIdentifier kHighlightMaskBlurRT = new RenderTargetIdentifier(kHighlightMaskBlurID);

        private RenderTextureDescriptor m_VolumetricCloudDescriptor;
        private readonly Material m_RenderFrontDepth;
        private readonly Material m_RenderBackDepth;
        private static readonly int ID_VolumetricCloud_Depth = Shader.PropertyToID("_VOLUMETRIC_DEPTH");
        private static readonly RenderTargetIdentifier RT_ID_VolumetricCloud_Depth=new RenderTargetIdentifier(ID_VolumetricCloud_Depth);

        enum EPassIndex
        {
            Combine=0,
            Sample=1,
        }
        public FOpaqueCore()
        {
            m_HighlightBlur = new FBlursCore();
            m_RenderBackDepth = new Material(RenderResources.FindInclude("Game/Additive/DepthOnly")){hideFlags = HideFlags.HideAndDontSave};
            m_RenderBackDepth.SetInt(KShaderProperties.kColorMask,(int)ColorWriteMask.Red);
            m_RenderBackDepth.SetInt(KShaderProperties.kZTest,(int)CompareFunction.Greater);
            m_RenderBackDepth.SetInt(KShaderProperties.kCull,(int)CullMode.Front);
            m_RenderFrontDepth = new Material(RenderResources.FindInclude("Game/Additive/DepthOnly")) { hideFlags = HideFlags.HideAndDontSave };
            m_RenderFrontDepth.SetInt(KShaderProperties.kColorMask,(int)ColorWriteMask.Green);
            m_RenderFrontDepth.SetInt(KShaderProperties.kZTest,(int)CompareFunction.Less);
            m_RenderFrontDepth.SetInt(KShaderProperties.kCull,(int)CullMode.Back);
        }
        
        public override void OnValidate(ref DOpaque _data)
        {
            base.OnValidate(ref _data);
            if(m_Material.EnableKeyword(kScanKW,_data.m_Scan))
                _data.m_ScanData.Apply(m_Material);
            if(m_Material.EnableKeyword(kAreaKW,_data.m_Area))
                _data.m_AreaData.Apply(m_Material);
            if (m_Material.EnableKeyword(kOutlineKW, _data.m_Outline))
                _data.m_OutlineData.Apply(m_Material);
            if(m_Material.EnableKeyword(kHighlightKW,_data.m_MaskedHighlight))
                _data.m_HighlightData.Apply(m_Material,m_HighlightBlur);
            if (m_Material.EnableKeyword(kAmbientOcclusionKW, _data.m_SSAO))
                _data.m_SSAOData.Apply(m_Material);
            if(m_Material.EnableKeyword(kVolumetricCloudKW,_data.m_VolumetricCloud))
                _data.m_VolumetricCloudData.Apply(m_Material);
        }

        public override void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor,ref DOpaque _data)
        {
            if (_data.m_MaskedHighlight)
            {
                var highlightDescriptor = _descriptor;
                highlightDescriptor.colorFormat = RenderTextureFormat.R8;
                highlightDescriptor.depthBufferBits = 0;
                _buffer.GetTemporaryRT(kHighlightMaskID, highlightDescriptor);
                _buffer.GetTemporaryRT(kHighlightMaskBlurID, _descriptor, FilterMode.Bilinear);
            }

            if (_data.m_VolumetricCloud && _data.m_VolumetricCloudData.m_Shape)
            {
                var depthDescriptor = new RenderTextureDescriptor(_descriptor.width, _descriptor.height, RenderTextureFormat.RGFloat, 32, 0);
                _buffer.GetTemporaryRT(ID_VolumetricCloud_Depth, depthDescriptor, FilterMode.Bilinear);
            }
        }

        public override void Execute(RenderTextureDescriptor _descriptor, ref DOpaque _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderer _renderer,
            ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            
            if (_data.m_VolumetricCloud && _data.m_VolumetricCloudData.m_Shape)
            {
                var volumetricData = _data.m_VolumetricCloudData;
                var buffer = CommandBufferPool.Get("Volumetric Fog Mask");
                buffer.SetRenderTarget(RT_ID_VolumetricCloud_Depth);
                buffer.ClearRenderTarget(RTClearFlags.ColorDepth,Color.clear,0,0);
                _context.ExecuteCommandBuffer(buffer);

                DrawingSettings drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
                if (_renderingData.cameraData.camera.TryGetCullingParameters(out var parameters))
                {
                    parameters.cullingOptions = CullingOptions.None;
                    parameters.cullingMask = (uint)_data.m_VolumetricCloudData.m_CullingMask;
                    
                    
                    FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = volumetricData.m_CullingMask };
                    drawingSettings.overrideMaterialPassIndex=1;
                    drawingSettings.overrideMaterial = m_RenderBackDepth;
                    _context.DrawRenderers(_context.Cull(ref parameters), ref drawingSettings, ref filterSettings);
                    drawingSettings.overrideMaterial = m_RenderFrontDepth;
                    _context.DrawRenderers(_context.Cull(ref parameters), ref drawingSettings, ref filterSettings);
                    
                }

                buffer.Clear();
                buffer.SetRenderTarget(_renderer.cameraColorTargetHandle);
                _context.ExecuteCommandBuffer(buffer);
                CommandBufferPool.Release(buffer);
            }

            if (_data.m_MaskedHighlight)
            {
                MaskTexturePass.DrawMask(kHighlightMaskRT,_context,ref _renderingData,_data.m_HighlightData.m_Mask);
                m_HighlightBlur.Execute(_descriptor,ref _data.m_HighlightData.m_Blur,_buffer, kHighlightMaskRT, kHighlightMaskBlurRT,_renderer,_context,ref _renderingData);
            }

            if (!_data.m_SSAO && !_data.m_VolumetricCloud)
            {
                _buffer.Blit(_src, _dst, m_Material,  (int)EPassIndex.Combine);
                return;
            }
            
            _descriptor.width /= _data.m_DownSample;
            _descriptor.height /= _data.m_DownSample;
            _descriptor.colorFormat = RenderTextureFormat.ARGB32;
            _descriptor.depthBufferBits = 0;
            
            _buffer.GetTemporaryRT(kSampleID, _descriptor,FilterMode.Bilinear);
            _buffer.Blit(_src, kSampleID, m_Material, (int)EPassIndex.Sample);
            _buffer.Blit(_src, _dst, m_Material,  (int)EPassIndex.Combine);
            _buffer.ReleaseTemporaryRT(kSampleID);
        }

        public override void FrameCleanUp(CommandBuffer _buffer,ref DOpaque _data)
        {
            if (_data.m_MaskedHighlight)
            {
                _buffer.ReleaseTemporaryRT(kHighlightMaskID);
                _buffer.ReleaseTemporaryRT(kHighlightMaskBlurID);
            }

            if (_data.m_VolumetricCloud && _data.m_VolumetricCloudData.m_Shape)
                _buffer.ReleaseTemporaryRT(ID_VolumetricCloud_Depth);
        }
    }

    [Serializable]
    public struct DOpaque:IPostProcessParameter
    {
        [Title] public bool m_Scan;
        [Foldout(nameof(m_Scan), true)] public DScan m_ScanData;
        [Title] public bool m_Area;
        [Foldout(nameof(m_Area), true)] public DArea m_AreaData;
        [Title] public bool m_Outline;
        [Foldout(nameof(m_Outline),true)] public DOutline m_OutlineData;
        [Title] public bool m_MaskedHighlight;
        [Foldout(nameof(m_MaskedHighlight), true)] public DHighlight m_HighlightData;
        
        [Header("Multi Sample")]
        [Range(1, 4)] public int m_DownSample;
        [Title] public bool m_SSAO;
        [Foldout(nameof(m_SSAO), true)] public DSSAO m_SSAOData;
        [Title] public bool m_VolumetricCloud;
        [Foldout(nameof(m_VolumetricCloud), true)] public DVolumetricCloud m_VolumetricCloudData;
        public bool Validate() => m_Scan || m_Area || m_Outline || m_MaskedHighlight || m_SSAO || m_VolumetricCloud;
        public static readonly DOpaque kDefault = new DOpaque()
        {
            m_Scan = true,
            m_ScanData = new DScan()
            {
                m_Origin = Vector3.zero,
                m_Color = Color.green,
                m_Elapse = 5f,
                m_Width = 2f,
                m_FadingPow = .8f,
                m_MaskTextureScale = 1f,
            },
            m_Area = true,
            m_AreaData =new DArea()
            {
                m_Origin = Vector3.zero,
                m_Radius = 5f,
                m_Width = 1f,
                m_FillColor=Color.white,
                m_EdgeColor=Color.black,
                m_FillTextureFlow=Vector2.one,
                m_FillTextureScale=1f,
            },
            m_Outline = true,
            m_OutlineData= new DOutline()
            {
                m_Color = Color.white,
                m_Width = 1,
                // m_Convolution = EConvolution.Prewitt,
                // m_DetectType = EDetectType.Depth,
                m_Strength = 2f,
                m_Bias = .5f,
            },
            m_MaskedHighlight = true,
            m_HighlightData=new DHighlight()
            {
                m_Color=Color.blue,
                m_Blur = DBlurs.kDefault,
            },
                
            m_DownSample = 1,
            m_SSAO = true,
            m_SSAOData = new DSSAO()
            {
                m_Color = Color.grey,
                m_Intensity = 1f,
                m_Radius = .5f,
                m_SampleCount = 16,
                m_Dither=true,
                m_RandomVectorKeywords=DateTime.Now.ToShortTimeString(),
            },
            m_VolumetricCloud = true,
            m_VolumetricCloudData  = new DVolumetricCloud()
            {
                m_VerticalStart = 20f,
                m_VerticalLength = 100f,
                m_MainNoise = TResources.EditorDefaultResources.Noise3D,
                m_MainNoiseScale = Vector3.one * 500f,
                m_MainNoiseFlow = Vector3.one * 0.1f,

                m_Density = 50f,
                m_DensityClip = .6f,
                m_DensitySmooth = 0.1f,
                m_Distance = 100f,
                m_MarchTimes = 32,
                m_Opacity = .8f,

                m_ColorRamp = TResources.EditorDefaultResources.Ramp,
                m_LightAbsorption = .2f,
            },
        };
        
        [Serializable]
        public struct DSSAO
        {
            [ColorUsage(false)]public Color m_Color;
            [Range(0.01f,5f)]public float m_Intensity;
            [Range(0.1f,1f)]public float m_Radius;
            [Range(0.01f, 0.5f)] public float m_Bias;
            [Header("Optimize")]
            [IntEnum(8,16,32,64)]public int m_SampleCount;
            public bool m_Dither;
            public string m_RandomVectorKeywords;
            #region Properties
                static readonly int ID_SampleCount = Shader.PropertyToID("_AOSampleCount");
                static readonly int ID_SampleSphere = Shader.PropertyToID("_AOSampleSphere");
                static readonly int ID_Color = Shader.PropertyToID("_AOColor");
                static readonly int ID_Intensity = Shader.PropertyToID("_AOIntensity");
                static readonly int ID_Radius = Shader.PropertyToID("_AORadius");
                static  readonly int ID_Bias=Shader.PropertyToID("_AOBias");
                const string KW_Dither = "_DITHER";
                const int m_MaxArraySize = 64;
                public void Apply(Material _material)
                {
                    var random = new LCGRandom(m_RandomVectorKeywords?.GetHashCode() ?? "AOCodeDefault".GetHashCode());
                    var randomVectors = new Vector4[m_MaxArraySize].Remake(p=> URandom.RandomSphere(random));
                    _material.SetFloat(ID_Bias,m_Radius+m_Bias);
                    _material.SetFloat(ID_Radius, m_Radius);
                    _material.SetInt(ID_SampleCount, m_SampleCount);
                    _material.SetVectorArray(ID_SampleSphere, randomVectors);
                    _material.SetColor(ID_Color, m_Color);
                    _material.SetFloat(ID_Intensity, m_Intensity);
                    _material.EnableKeyword(KW_Dither,m_Dither);
                }
            #endregion
        }
        
        [Serializable]
        public struct DScan
        {
            [Position] public Vector3 m_Origin;
            [ColorUsage(true,true)]public Color m_Color;
            public float m_Elapse;
            [Range(0,20)]public float m_Width;
            [Range(0.01f,2)]public float m_FadingPow;

            public Texture2D m_MaskTexture;
            [MFold(nameof(m_MaskTexture),null),Clamp(0.000001f)] public float m_MaskTextureScale;

            #region ShaderProperties
            static readonly int ID_Origin = Shader.PropertyToID("_ScanOrigin");
            static readonly int ID_Color = Shader.PropertyToID("_ScanColor");
            static readonly int ID_FadingPow = Shader.PropertyToID("_ScanFadingPow");
            const string KW_Mask = "_MASK_TEXTURE";
            static readonly int ID_Texture = Shader.PropertyToID("_ScanMaskTexture");
            static readonly int ID_TexScale = Shader.PropertyToID("_ScanMaskTextureScale");
            static readonly int ID_MinSqrDistance = Shader.PropertyToID("_ScanMinSqrDistance");
            static readonly int ID_MaxSqrDistance = Shader.PropertyToID("_ScanMaxSqrDistance");
            public void Apply(Material _material)
            {
                _material.SetVector(ID_Origin, m_Origin);
                _material.SetColor(ID_Color, m_Color);
                float minDistance = m_Elapse;
                float maxDistance = m_Elapse + m_Width;
                _material.SetFloat(ID_MinSqrDistance, minDistance * minDistance);
                _material.SetFloat(ID_MaxSqrDistance, maxDistance * maxDistance);
                _material.SetFloat(ID_FadingPow, m_FadingPow);

                if( _material.EnableKeyword(KW_Mask,m_MaskTexture != null))
                {
                    _material.SetTexture(ID_Texture, m_MaskTexture);
                    _material.SetFloat(ID_TexScale,1f/m_MaskTextureScale);
                }
            }
            #endregion
        }
        
        [Serializable]
        public struct DArea
        {
            [Position] public Vector3 m_Origin;
            public float m_Radius;
            public float m_Width;
            [ColorUsage(true,true)]public Color m_FillColor;
            [ColorUsage(true,true)]public Color m_EdgeColor;
            public Texture2D m_FillTexture;
            [MFold(nameof(m_FillTexture),null), RangeVector(-5,5)] public Vector2 m_FillTextureFlow;
            [MFold(nameof(m_FillTexture),null),Clamp(0.000001f)] public float m_FillTextureScale;

            #region ShaderProperties
                static readonly int ID_Origin = Shader.PropertyToID("_AreaOrigin");
                static readonly int ID_FillColor = Shader.PropertyToID("_AreaFillColor");
                static readonly int ID_FillTexture = Shader.PropertyToID("_AreaFillTexture");
                static readonly int ID_FillTextureScale = Shader.PropertyToID("_AreaTextureScale");
                static readonly int ID_FillTextureFlow = Shader.PropertyToID("_AreaTextureFlow");
                static readonly int ID_EdgeColor = Shader.PropertyToID("_AreaEdgeColor");
                static readonly int ID_SqrEdgeMin = Shader.PropertyToID("_AreaSqrEdgeMin");
                static readonly int ID_SqrEdgeMax = Shader.PropertyToID("_AreaSqrEdgeMax");
                public void Apply(Material _material)
                {
                    float edgeMin = m_Radius;
                    float edgeMax = m_Radius + m_Width;
                    _material.SetFloat(ID_SqrEdgeMax, edgeMax * edgeMax);
                    _material.SetFloat(ID_SqrEdgeMin, edgeMin * edgeMin);
                    _material.SetVector(ID_Origin, m_Origin);
                    _material.SetColor(ID_FillColor, m_FillColor);
                    _material.SetColor(ID_EdgeColor, m_EdgeColor);
                    _material.SetTexture(ID_FillTexture, m_FillTexture);
                    _material.SetFloat(ID_FillTextureScale, 1f/m_FillTextureScale);
                    _material.SetVector(ID_FillTextureFlow, m_FillTextureFlow);
                }
            #endregion
        }
        
        [Serializable]
        public struct DOutline
        {
            // public enum EConvolution
            // {
            //     Prewitt = 1,
            //     Sobel = 2,
            // }
            //
            // public enum EDetectType
            // {
            //     Depth = 1,
            //     Normal = 2,
            //     Color = 3,
            // }

            [ColorUsage(true)] public Color m_Color;
            [Header("Options")]
            [Range(.1f, 3f)] public float m_Width;
            // public EConvolution m_Convolution;
            // public EDetectType m_DetectType;
            [Range(0, 10f)] public float m_Strength;
            [Range(0.01f, 5f)] public float m_Bias;

            #region ShaderProperties
            static readonly int ID_EdgeColor = Shader.PropertyToID("_OutlineColor");
            static readonly int ID_OutlineWidth = Shader.PropertyToID("_OutlineWidth");
            // static readonly string[] KW_Convolution = new string[2] { "_CONVOLUTION_PREWITT", "_CONVOLUTION_SOBEL" };
            // static readonly string[] KW_DetectType = new string[3] { "_DETECT_DEPTH", "_DETECT_NORMAL", "_DETECT_COLOR" };
            static readonly int ID_Strength = Shader.PropertyToID("_Strength");
            static readonly int ID_Bias = Shader.PropertyToID("_Bias");
            public void Apply(Material m_Material)
            {
                m_Material.SetColor(ID_EdgeColor, m_Color);
                m_Material.SetFloat(ID_OutlineWidth, m_Width);
                // m_Material.EnableKeywords(KW_Convolution, (int)m_Convolution);
                // m_Material.EnableKeywords(KW_DetectType, (int)m_DetectType);
                m_Material.SetFloat(ID_Strength, m_Strength);
                m_Material.SetFloat(ID_Bias, m_Bias);
            }
            #endregion
        }

        [Serializable]
        public struct DHighlight
        {
            [ColorUsage(true,true)] public Color m_Color;
            public MaskTextureData m_Mask;
            public DBlurs m_Blur;
            
            #region Properties
            static readonly int ID_EdgeColor = Shader.PropertyToID("_HighlightColor");
            public void Apply(Material _material,FBlursCore _blur)
            {
                _material.SetColor(ID_EdgeColor,m_Color);
                _blur.OnValidate(ref m_Blur);
            }
            #endregion
            public static readonly DHighlight kDefault = new DHighlight()
            {
                m_Color = Color.white,
                m_Mask = MaskTextureData.kDefault,
                m_Blur = DBlurs.kDefault
            };
        }
        
        [Serializable]
        public struct DVolumetricCloud
        {
            [Title]public bool m_Shape;
            [Foldout(nameof(m_Shape),true)] [CullingMask] public int m_CullingMask;
            [Foldout(nameof(m_Shape),false)] public float m_VerticalStart;
            [Foldout(nameof(m_Shape),false)] public float m_VerticalLength;
            
            [Title] public Texture3D m_MainNoise;
            [MFold(nameof(m_MainNoise)), RangeVector(0f, 1000f)] public Vector3 m_MainNoiseScale;
            [MFold(nameof(m_MainNoise)), RangeVector(0f, 10f)] public Vector3 m_MainNoiseFlow;
            
            [Range(0f, 100f)] public float m_Density;
            [Range(0, 1)] public float m_DensityClip;
            [Range(0, 1)] public float m_DensitySmooth;
            public float m_Distance;
            [IntEnum(16,32,64,128)]public int m_MarchTimes ;
            [Range(0, 1)] public float m_Opacity;

            [Header("Light Setting")] 
            public Texture2D m_ColorRamp;
            [Range(0, 1)] public float m_LightAbsorption;
            #region ShaderProperties

            private const string KW_Shape = "_SHAPE";
            
            static readonly int ID_VerticalStart = Shader.PropertyToID("_VerticalStart");
            static readonly int ID_VerticalEnd = Shader.PropertyToID("_VerticalEnd");

            static readonly int ID_Opacity = Shader.PropertyToID("_Opacity");
            static readonly int ID_Density = Shader.PropertyToID("_Density");
            static readonly int ID_DensityClip = Shader.PropertyToID("_DensityClip");
            static readonly int ID_DensitySmooth = Shader.PropertyToID("_DensitySmooth");
            static readonly int ID_Distance = Shader.PropertyToID("_Distance");
            static readonly int ID_MarchTimes = Shader.PropertyToID("_RayMarchTimes");
            static readonly int ID_ColorRamp = Shader.PropertyToID("_ColorRamp");

            static readonly int ID_MainNoise = Shader.PropertyToID("_MainNoise");
            static readonly int ID_MainNoiseScale = Shader.PropertyToID("_MainNoiseScale");
            static readonly int ID_MainNoiseFlow = Shader.PropertyToID("_MainNoiseFlow");

            static readonly int ID_LightAbsorption = Shader.PropertyToID("_LightAbsorption");
            public  void Apply(Material _material)
            {
                _material.EnableKeyword(KW_Shape, m_Shape);
                _material.SetFloat(ID_VerticalStart, m_VerticalStart);
                _material.SetFloat(ID_VerticalEnd, m_VerticalStart+m_VerticalLength);
                _material.SetFloat(ID_Opacity, m_Opacity);
                _material.SetFloat(ID_Density, m_Density);
                _material.SetFloat(ID_DensityClip, m_DensityClip);
                _material.SetFloat(ID_DensitySmooth, m_DensitySmooth/2f*m_Distance);
                _material.SetFloat(ID_Distance, m_Distance);
                _material.SetInt(ID_MarchTimes, (int)m_MarchTimes);
                _material.SetTexture(ID_ColorRamp, m_ColorRamp);
                _material.SetTexture(ID_MainNoise, m_MainNoise);
                _material.SetVector(ID_MainNoiseScale, m_MainNoiseScale);
                _material.SetVector(ID_MainNoiseFlow, m_MainNoiseFlow);
                _material.SetFloat(ID_LightAbsorption, m_LightAbsorption);
            }
            #endregion
        }
    }

}