using System;
using System.Linq.Extensions;
using Rendering.Pipeline;
using Rendering.Pipeline.Mask;
using Runtime.Random;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_Opaque:APostProcessBehaviour<FOpaqueCore, DOpaque>
    {
        public bool m_Opaque = true;
        public override bool OpaqueProcess => m_Opaque;
        public override EPostProcess Event => EPostProcess.Opaque;

        #region HelperFunc
        SingleCoroutine m_ScanCoroutine;
        public void StartDepthScanCircle(Vector3 origin,  float radius = 20, float duration = 1.5f)
        {
            if (m_ScanCoroutine==null)
                m_ScanCoroutine =  CoroutineHelper.CreateSingleCoroutine();
            m_ScanCoroutine.Stop();
            var data = GetData();
            data.scan = true;
            data.scanData.origin = origin;
            SetEffectData(data);
            var startColor = data.scanData.color;
            m_ScanCoroutine.Start(TIEnumerators.ChangeValueTo(value => {
                var data = GetData();
                data.scanData.elapse = radius * value;
                data.scanData.color = startColor.SetA(1f - value);
                SetEffectData(data);
            }, 0, 1, duration, () => { 
                var data = GetData();
                data.scan = false;
                SetEffectData(data);
            }));
        }
        #endregion
        
#if UNITY_EDITOR
        public bool m_DrawGizmos = true;
        private void OnDrawGizmos()
        {
            if (!enabled||!m_DrawGizmos)
                return;

            if (m_Data.scan)
            {
                ref var data=ref m_Data. scanData ;
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(data.origin, .2f);
                Gizmos.color = data.color;
                Gizmos.DrawWireSphere(data.origin, data.elapse);
                Gizmos.DrawWireSphere(data.origin, data.elapse + data.width);
            }


            if (m_Data.area)
            {
                ref var data = ref m_Data.areaData;
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(data.origin, .1f);

                Gizmos.color = data.fillColor;
                Gizmos.DrawWireSphere(data.origin,data.radius);

                Gizmos.color = data.edgeColor;
                Gizmos.DrawWireSphere(data.origin,data.radius+data.width);
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
        
        public override bool Validate(ref RenderingData _renderingData,ref DOpaque _data)
        {
            if(m_Material.EnableKeyword(kScanKW,_data.scan))
                _data.scanData.Validate(m_Material);
            if(m_Material.EnableKeyword(kAreaKW,_data.area))
                _data.areaData.Validate(m_Material);
            if (m_Material.EnableKeyword(kOutlineKW, _data.outline))
                _data.outlineData.Validate(m_Material);
            if(m_Material.EnableKeyword(kHighlightKW,_data.maskedHighlight))
                _data.highlightData.Validate(ref _renderingData, m_HighlightBlur,m_Material);
            if (m_Material.EnableKeyword(kAmbientOcclusionKW, _data.SSAO))
                _data.SSAOData.Validate(m_Material);
            if(m_Material.EnableKeyword(kVolumetricCloudKW,_data.volumetricCloud))
                _data.volumetricCloudData.Apply(m_Material);
            return base.Validate(ref _renderingData,ref _data);
        }

        public override void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor,ref DOpaque _data)
        {
            if (_data.maskedHighlight)
            {
                var highlightDescriptor = _descriptor;
                highlightDescriptor.colorFormat = RenderTextureFormat.R8;
                highlightDescriptor.depthBufferBits = 0;
                _buffer.GetTemporaryRT(kHighlightMaskID, highlightDescriptor);
                _buffer.GetTemporaryRT(kHighlightMaskBlurID, _descriptor, FilterMode.Bilinear);
            }

            if (_data.volumetricCloud && _data.volumetricCloudData.shape)
            {
                var depthDescriptor = new RenderTextureDescriptor(_descriptor.width, _descriptor.height, RenderTextureFormat.RGFloat, 32, 0);
                _buffer.GetTemporaryRT(ID_VolumetricCloud_Depth, depthDescriptor, FilterMode.Bilinear);
            }
        }

        public override void Execute(RenderTextureDescriptor _descriptor, ref DOpaque _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var renderer = _renderingData.cameraData.renderer;
            if (_data.volumetricCloud && _data.volumetricCloudData.shape)
            {
                var volumetricData = _data.volumetricCloudData;
                var buffer = CommandBufferPool.Get("Volumetric Fog Mask");
                buffer.SetRenderTarget(RT_ID_VolumetricCloud_Depth);
                buffer.ClearRenderTarget(RTClearFlags.ColorDepth,Color.clear,0,0);
                _context.ExecuteCommandBuffer(buffer);

                var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
                if (_renderingData.cameraData.camera.TryGetCullingParameters(out var parameters))
                {
                    parameters.cullingOptions = CullingOptions.None;
                    parameters.cullingMask = (uint)_data.volumetricCloudData.cullingMask;
                    
                    
                    var filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = volumetricData.cullingMask };
                    drawingSettings.overrideMaterialPassIndex=1;
                    drawingSettings.overrideMaterial = m_RenderBackDepth;
                    _context.DrawRenderers(_context.Cull(ref parameters), ref drawingSettings, ref filterSettings);
                    drawingSettings.overrideMaterial = m_RenderFrontDepth;
                    _context.DrawRenderers(_context.Cull(ref parameters), ref drawingSettings, ref filterSettings);
                    
                }

                buffer.Clear();
                buffer.SetRenderTarget(renderer.cameraColorTargetHandle);
                _context.ExecuteCommandBuffer(buffer);
                CommandBufferPool.Release(buffer);
            }

            if (_data.maskedHighlight)
            {
                MaskTexturePass.DrawMask(_buffer,kHighlightMaskRT,_context,ref _renderingData,_data.highlightData.mask);
                m_HighlightBlur.Execute(_descriptor,ref _data.highlightData.blur,_buffer, kHighlightMaskRT, kHighlightMaskBlurRT,_context,ref _renderingData);
            }

            if (!_data.SSAO && !_data.volumetricCloud)
            {
                _buffer.Blit(_src, _dst, m_Material,  (int)EPassIndex.Combine);
                return;
            }
            
            _descriptor.width /= _data.downSample;
            _descriptor.height /= _data.downSample;
            _descriptor.colorFormat = RenderTextureFormat.ARGB32;
            _descriptor.depthBufferBits = 0;
            
            _buffer.GetTemporaryRT(kSampleID, _descriptor,FilterMode.Bilinear);
            _buffer.Blit(_src, kSampleID, m_Material, (int)EPassIndex.Sample);
            _buffer.Blit(_src, _dst, m_Material,  (int)EPassIndex.Combine);
            _buffer.ReleaseTemporaryRT(kSampleID);
        }

        public override void FrameCleanUp(CommandBuffer _buffer,ref DOpaque _data)
        {
            if (_data.maskedHighlight)
            {
                _buffer.ReleaseTemporaryRT(kHighlightMaskID);
                _buffer.ReleaseTemporaryRT(kHighlightMaskBlurID);
            }

            if (_data.volumetricCloud && _data.volumetricCloudData.shape)
                _buffer.ReleaseTemporaryRT(ID_VolumetricCloud_Depth);
        }
    }

    [Serializable]
    public struct DOpaque:IPostProcessParameter
    {
        [Title] public bool scan;
        [Foldout(nameof(scan), true)] public DScan scanData;
        [Title] public bool area;
        [Foldout(nameof(area), true)] public DArea areaData;
        [Title] public bool outline;
        [Foldout(nameof(outline),true)] public DOutline outlineData;
        [Title] public bool maskedHighlight;
        [Foldout(nameof(maskedHighlight), true)] public DHighlight highlightData;
        
        [Header("Multi Sample")]
        [Range(1, 4)] public int downSample;
        [Title] public bool SSAO;
        [Foldout(nameof(SSAO), true)] public DSSAO SSAOData;
        [Title] public bool volumetricCloud;
        [Foldout(nameof(volumetricCloud), true)] public DVolumetricCloud volumetricCloudData;
        public bool Validate() => scan || area || outline || maskedHighlight || SSAO || volumetricCloud;
        public static readonly DOpaque kDefault = new DOpaque()
        {
            scan = true,
            scanData = new DScan()
            {
                origin = Vector3.zero,
                color = Color.green,
                elapse = 5f,
                width = 2f,
                fadingPow = .8f,
                maskTextureScale = 1f,
            },
            area = true,
            areaData =new DArea()
            {
                origin = Vector3.zero,
                radius = 5f,
                width = 1f,
                fillColor=Color.white,
                edgeColor=Color.black,
                fillTextureFlow=Vector2.one,
                fillTextureScale=1f,
            },
            outline = true,
            outlineData= new DOutline()
            {
                color = Color.white,
                width = 1,
                // convolution = EConvolution.Prewitt,
                // detectType = EDetectType.Depth,
                strength = 2f,
                bias = .5f,
            },
            maskedHighlight = true,
            highlightData=new DHighlight()
            {
                color=Color.blue,
                blur = DBlurs.kDefault,
            },
                
            downSample = 1,
            SSAO = true,
            SSAOData = new DSSAO()
            {
                color = Color.grey,
                intensity = 1f,
                radius = .5f,
                sampleCount = 16,
                dither=true,
                randomVectorKeywords=DateTime.Now.ToShortTimeString(),
            },
            volumetricCloud = true,
            volumetricCloudData  = new DVolumetricCloud()
            {
                verticalStart = 20f,
                verticalLength = 100f,
                mainNoise = TResources.EditorDefaultResources.Noise3D,
                mainNoiseScale = Vector3.one * 500f,
                mainNoiseFlow = Vector3.one * 0.1f,

                density = 50f,
                densityClip = .6f,
                densitySmooth = 0.1f,
                distance = 100f,
                marchTimes = 32,
                opacity = .8f,

                colorRamp = TResources.EditorDefaultResources.Ramp,
                lightAbsorption = .2f,
            },
        };
        
        [Serializable]
        public struct DSSAO
        {
            [ColorUsage(false)]public Color color;
            [Range(0.01f,5f)]public float intensity;
            [Range(0.1f,1f)]public float radius;
            [Range(0.01f, 0.5f)] public float bias;
            [Header("Optimize")]
            [IntEnum(8,16,32,64)]public int sampleCount;
            public bool dither;
            public string randomVectorKeywords;
            #region Properties
                static readonly int ID_SampleCount = Shader.PropertyToID("_AOSampleCount");
                static readonly int ID_SampleSphere = Shader.PropertyToID("_AOSampleSphere");
                static readonly int ID_Color = Shader.PropertyToID("_AOColor");
                static readonly int ID_Intensity = Shader.PropertyToID("_AOIntensity");
                static readonly int ID_Radius = Shader.PropertyToID("_AORadius");
                static  readonly int ID_Bias=Shader.PropertyToID("_AOBias");
                const string KW_Dither = "_DITHER";
                const int m_MaxArraySize = 64;
                public void Validate(Material _material)
                {
                    var random = new LCGRandom(randomVectorKeywords?.GetHashCode() ?? "AOCodeDefault".GetHashCode());
                    var randomVectors = new Vector4[m_MaxArraySize].Remake(p=> URandom.RandomSphere(random));
                    _material.SetFloat(ID_Bias,radius+bias);
                    _material.SetFloat(ID_Radius, radius);
                    _material.SetInt(ID_SampleCount, sampleCount);
                    _material.SetVectorArray(ID_SampleSphere, randomVectors);
                    _material.SetColor(ID_Color, color);
                    _material.SetFloat(ID_Intensity, intensity);
                    _material.EnableKeyword(KW_Dither,dither);
                }
            #endregion
        }
        
        [Serializable]
        public struct DScan
        {
            [Position] public Vector3 origin;
            [ColorUsage(true,true)]public Color color;
            public float elapse;
            [Range(0,20)]public float width;
            [Range(0.01f,2)]public float fadingPow;

            public Texture2D m_MaskTexture;
            [Fold(nameof(m_MaskTexture),null),Clamp(0.000001f)] public float maskTextureScale;

            #region ShaderProperties
            static readonly int ID_Origin = Shader.PropertyToID("_ScanOrigin");
            static readonly int ID_Color = Shader.PropertyToID("_ScanColor");
            static readonly int ID_FadingPow = Shader.PropertyToID("_ScanFadingPow");
            const string KW_Mask = "_MASK_TEXTURE";
            static readonly int ID_Texture = Shader.PropertyToID("_ScanMaskTexture");
            static readonly int ID_TexScale = Shader.PropertyToID("_ScanMaskTextureScale");
            static readonly int ID_MinSqrDistance = Shader.PropertyToID("_ScanMinSqrDistance");
            static readonly int ID_MaxSqrDistance = Shader.PropertyToID("_ScanMaxSqrDistance");
            public void Validate(Material _material)
            {
                _material.SetVector(ID_Origin, origin);
                _material.SetColor(ID_Color, color);
                float minDistance = elapse;
                float maxDistance = elapse + width;
                _material.SetFloat(ID_MinSqrDistance, minDistance * minDistance);
                _material.SetFloat(ID_MaxSqrDistance, maxDistance * maxDistance);
                _material.SetFloat(ID_FadingPow, fadingPow);

                if( _material.EnableKeyword(KW_Mask,m_MaskTexture != null))
                {
                    _material.SetTexture(ID_Texture, m_MaskTexture);
                    _material.SetFloat(ID_TexScale,1f/maskTextureScale);
                }
            }
            #endregion
        }
        
        [Serializable]
        public struct DArea
        {
            [Position] public Vector3 origin;
            public float radius;
            public float width;
            [ColorUsage(true,true)]public Color fillColor;
            [ColorUsage(true,true)]public Color edgeColor;
            public Texture2D m_FillTexture;
            [Fold(nameof(m_FillTexture),null), RangeVector(-5,5)] public Vector2 fillTextureFlow;
            [Fold(nameof(m_FillTexture),null),Clamp(0.000001f)] public float fillTextureScale;

            #region ShaderProperties
                static readonly int ID_Origin = Shader.PropertyToID("_AreaOrigin");
                static readonly int ID_FillColor = Shader.PropertyToID("_AreaFillColor");
                static readonly int ID_FillTexture = Shader.PropertyToID("_AreaFillTexture");
                static readonly int ID_FillTextureScale = Shader.PropertyToID("_AreaTextureScale");
                static readonly int ID_FillTextureFlow = Shader.PropertyToID("_AreaTextureFlow");
                static readonly int ID_EdgeColor = Shader.PropertyToID("_AreaEdgeColor");
                static readonly int ID_SqrEdgeMin = Shader.PropertyToID("_AreaSqrEdgeMin");
                static readonly int ID_SqrEdgeMax = Shader.PropertyToID("_AreaSqrEdgeMax");
                public void Validate(Material _material)
                {
                    float edgeMin = radius;
                    float edgeMax = radius + width;
                    _material.SetFloat(ID_SqrEdgeMax, edgeMax * edgeMax);
                    _material.SetFloat(ID_SqrEdgeMin, edgeMin * edgeMin);
                    _material.SetVector(ID_Origin, origin);
                    _material.SetColor(ID_FillColor, fillColor);
                    _material.SetColor(ID_EdgeColor, edgeColor);
                    _material.SetTexture(ID_FillTexture, m_FillTexture);
                    _material.SetFloat(ID_FillTextureScale, 1f/fillTextureScale);
                    _material.SetVector(ID_FillTextureFlow, fillTextureFlow);
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

            [ColorUsage(true)] public Color color;
            [Header("Options")]
            [Range(.1f, 3f)] public float width;
            // public EConvolution convolution;
            // public EDetectType detectType;
            [Range(0, 10f)] public float strength;
            [Range(0.01f, 5f)] public float bias;

            #region ShaderProperties
            static readonly int ID_EdgeColor = Shader.PropertyToID("_OutlineColor");
            static readonly int ID_OutlineWidth = Shader.PropertyToID("_OutlineWidth");
            // static readonly string[] KW_Convolution = new string[2] { "_CONVOLUTION_PREWITT", "_CONVOLUTION_SOBEL" };
            // static readonly string[] KW_DetectType = new string[3] { "_DETECT_DEPTH", "_DETECT_NORMAL", "_DETECT_COLOR" };
            static readonly int ID_Strength = Shader.PropertyToID("_Strength");
            static readonly int ID_Bias = Shader.PropertyToID("_Bias");
            public void Validate(Material m_Material)
            {
                m_Material.SetColor(ID_EdgeColor, color);
                m_Material.SetFloat(ID_OutlineWidth, width);
                // m_Material.EnableKeywords(KW_Convolution, (int)convolution);
                // m_Material.EnableKeywords(KW_DetectType, (int)detectType);
                m_Material.SetFloat(ID_Strength, strength);
                m_Material.SetFloat(ID_Bias, bias);
            }
            #endregion
        }

        [Serializable]
        public struct DHighlight
        {
            [ColorUsage(true,true)] public Color color;
            public MaskTextureData mask;
            public DBlurs blur;
            
            #region Properties
            static readonly int ID_EdgeColor = Shader.PropertyToID("_HighlightColor");
            public void Validate(ref RenderingData _renderingData,FBlursCore _blur,Material _material)
            {
                _material.SetColor(ID_EdgeColor,color);
                _blur.Validate(ref _renderingData,ref blur);
            }
            #endregion
            public static readonly DHighlight kDefault = new()
            {
                color = Color.white,
                mask = MaskTextureData.kDefault,
                blur = DBlurs.kDefault
            };
        }
        
        [Serializable]
        public struct DVolumetricCloud
        {
            [Title]public bool shape;
            [Foldout(nameof(shape),true)] [CullingMask] public int cullingMask;
            [Foldout(nameof(shape),false)] public float verticalStart;
            [Foldout(nameof(shape),false)] public float verticalLength;
            
            [Title] public Texture3D mainNoise;
            [Fold(nameof(mainNoise)), RangeVector(0f, 1000f)] public Vector3 mainNoiseScale;
            [Fold(nameof(mainNoise)), RangeVector(0f, 10f)] public Vector3 mainNoiseFlow;
            
            [Range(0f, 100f)] public float density;
            [Range(0, 1)] public float densityClip;
            [Range(0, 1)] public float densitySmooth;
            public float distance;
            [IntEnum(16,32,64,128)]public int marchTimes ;
            [Range(0, 1)] public float opacity;

            [Header("Light Setting")] 
            public Texture2D colorRamp;
            [Range(0, 1)] public float lightAbsorption;
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
                _material.EnableKeyword(KW_Shape, shape);
                _material.SetFloat(ID_VerticalStart, verticalStart);
                _material.SetFloat(ID_VerticalEnd, verticalStart+verticalLength);
                _material.SetFloat(ID_Opacity, opacity);
                _material.SetFloat(ID_Density, density);
                _material.SetFloat(ID_DensityClip, densityClip);
                _material.SetFloat(ID_DensitySmooth, densitySmooth/2f*distance);
                _material.SetFloat(ID_Distance, distance);
                _material.SetInt(ID_MarchTimes, (int)marchTimes);
                _material.SetTexture(ID_ColorRamp, colorRamp);
                _material.SetTexture(ID_MainNoise, mainNoise);
                _material.SetVector(ID_MainNoiseScale, mainNoiseScale);
                _material.SetVector(ID_MainNoiseFlow, mainNoiseFlow);
                _material.SetFloat(ID_LightAbsorption, lightAbsorption);
            }
            #endregion
        }
    }

}