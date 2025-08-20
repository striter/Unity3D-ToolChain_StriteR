using System;
using System.Linq.Extensions;
using Rendering;
using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public struct UGUIDepthSortingData
{
    [DefaultAsset("Hidden/UGUIDepthPrePass")] public Shader m_Shader;
    [Layer] public int m_UILayer;
    [Layer] public int m_RendererLayer;
    public float offset;

    public static UGUIDepthSortingData kDefault => new UGUIDepthSortingData()
    {
        offset = 10,
        m_UILayer = 5,
        m_RendererLayer = 1
    };
    
    [InspectorButtonEditor]
    public void Sort()
    {
        var canvasRoot = GameObject.FindObjectOfType<Canvas>();
        if (canvasRoot == null) 
            return;
        foreach (var renderer in canvasRoot.GetComponentsInChildren<Renderer>())
            renderer.gameObject.layer = m_RendererLayer;

        var rootZ = canvasRoot.transform.position.z;
        foreach (var (index,canvasRenderer) in canvasRoot.GetComponentsInChildren<CanvasRenderer>().WithIndex())
        {
            canvasRenderer.transform.position = canvasRenderer.transform.position.SetZ(rootZ-index *offset);
            canvasRenderer.gameObject.layer = m_UILayer;
        }
    }
}

public class UGUIDepthPrePassFeature : ScriptableRendererFeature
{
    [InspectorExtension] public UGUIDepthSortingData m_Data = UGUIDepthSortingData.kDefault;
    private UGUIDepthSortingPass m_Pass;
    public override void Create()
    {
        m_Pass = new (){renderPassEvent = RenderPassEvent.BeforeRenderingOpaques};
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_Pass);
    }

    public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
    {
        base.OnCameraPreCull(renderer, in cameraData);
        m_Pass.OnPreCull(m_Data,renderer, in cameraData);
    }
}

public class UGUIDepthSortingPass : ScriptableRenderPass
{
    private static readonly string kTitle = "UGUIDepthSortingPass";

    private DrawingSettings m_AdditionalDepthDrawingSettings;
    private FilteringSettings m_AdditionalDepthFilterSettings;
    private ScriptableCullingParameters m_CullParameters;
    public void OnPreCull( UGUIDepthSortingData _data, ScriptableRenderer renderer, in CameraData cameraData)
    {
        var renderData = (UniversalRendererData)URenderDebug.GetScriptableRendererData(cameraData.camera);
        var cullingMask = 1 << _data.m_RendererLayer | 1 << _data.m_UILayer;
        
        if (cameraData.camera.TryGetCullingParameters(out m_CullParameters))
            m_CullParameters.cullingMask = (uint)cullingMask;
        
        m_AdditionalDepthDrawingSettings = UPipeline.CreateDrawingSettings(true, cameraData.camera, SortingCriteria.CommonOpaque);
        m_AdditionalDepthDrawingSettings.perObjectData = PerObjectData.None;
        m_AdditionalDepthDrawingSettings.overrideShader = _data.m_Shader;
        m_AdditionalDepthFilterSettings = new FilteringSettings(RenderQueueRange.transparent){ layerMask = renderData.transparentLayerMask };
        m_AdditionalDepthFilterSettings.layerMask = 1 << _data.m_UILayer;
        m_AdditionalDepthFilterSettings.renderQueueRange = RenderQueueRange.transparent;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get(kTitle);
        cmd.BeginSample(kTitle);
        context.ExecuteCommandBuffer(cmd);
        var cullingResult = context.Cull(ref m_CullParameters);
        context.DrawRenderers(cullingResult, ref m_AdditionalDepthDrawingSettings, ref m_AdditionalDepthFilterSettings);
        
        cmd.Clear();
        cmd.EndSample(kTitle);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}