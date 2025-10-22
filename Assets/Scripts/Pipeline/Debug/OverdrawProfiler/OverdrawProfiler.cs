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
            if (_camera.cameraType is CameraType.Preview or CameraType.Reflection)
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

            OnGUI(_sceneView.camera,new Rect(10, 10, 200, 50));
            
            Handles.EndGUI();
        }
#endif
        static void OnGUI(Camera _camera,Rect _rect)
        {
            GUI.Box(_rect, GUIContent.none);
            GUILayout.BeginArea(_rect.Collapse(Vector2.one * .9f));
            GUILayout.BeginVertical();
            GUILayout.Label( $"{m_RenderPass.m_Data.name}:{m_RenderPass.QueryPixelDrawNormalize(_camera)}");
            if (GUILayout.Button($"Disable"))
                Switch(null);
            GUILayout.EndVertical();
            GUILayout.EndArea();

            if(Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
                Switch(null);
        }
    }
}