using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Render.Debug
{
    public static class OverdrawProfiler
    {
        private static OverdrawProfilerPass m_RenderPass;

        public static void Switch(OverdrawProfilerData _data)
        {
            if (m_RenderPass != null)
            {
                m_RenderPass = null;
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
#if UNITY_EDITOR
            SceneView.duringSceneGui -= SceneGUI;
#endif
                return;
            }

            if (_data == null)
                return;
            
            m_RenderPass = new OverdrawProfilerPass(_data);
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
#if UNITY_EDITOR
            SceneView.duringSceneGui += SceneGUI;
#endif
        }
        
        static void OnBeginCameraRendering(ScriptableRenderContext _context, Camera _camera)
        {
            var cameraData = _camera.GetUniversalAdditionalCameraData();
            if (_camera.cameraType == CameraType.Preview || _camera.cameraType == CameraType.Reflection)
                return;
            
            cameraData.scriptableRenderer.EnqueuePass(m_RenderPass);
        }
        
#if UNITY_EDITOR
        static void SceneGUI(SceneView _sceneView)
        {
            if (_sceneView.camera == null)
                return;

            if (m_RenderPass == null)
                return;
            
            Handles.BeginGUI();
            GUI.Label(new Rect(10, _sceneView.camera.pixelHeight - 50, 200, 20), $"{m_RenderPass.m_Data.name}");
            if (GUI.Button(new Rect(10, _sceneView.camera.pixelHeight - 30, 200, 20), $"Disable"))
                Switch(null);

            Handles.EndGUI();
        }
#endif
    }
}