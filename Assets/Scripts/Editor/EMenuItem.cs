using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TEditor
{
    public static class EMenuItem
    {
        #region Hotkeys
        [MenuItem("Work Flow/Hotkeys/Selected Camera Sync Scene View _F10", false, 101)]
        static void SelectedCameraSyncSceneView()
        {
            if (Application.isPlaying)
                return;
            if (m_SceneCameraSync)
            {
                m_SceneCameraSync = null;
                EditorApplication.update -= SyncCameraPositions;
                return;
            }
            if (!Selection.activeGameObject)
                return;
            m_SceneCameraSync = Selection.activeGameObject.GetComponent<Camera>();
            if (!m_SceneCameraSync)
                return;
            EditorApplication.update += SyncCameraPositions;
            Undo.RecordObject(m_SceneCameraSync.transform, "Camera Position Sync");
        }
        static Camera m_SceneCameraSync;
        static void SyncCameraPositions()
        {
            if (!m_SceneCameraSync || Selection.activeObject != m_SceneCameraSync.gameObject)
            {
                EditorApplication.update -= SyncCameraPositions;
                m_SceneViewSyncObject = null;
                return;
            }
            SceneView targetView = SceneView.sceneViews[0] as SceneView;
            m_SceneCameraSync.transform.position = targetView.camera.transform.position;
            m_SceneCameraSync.transform.rotation = targetView.camera.transform.rotation;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Work Flow/Hotkeys/Scene View Camera Sync To Selected _F11", false, 102)]
        static void SceneViewCameraSyncSelected()
        {
            if(m_SceneViewSyncObject)
            {
                EditorApplication.update -= SceneViewSyncObject;
                m_SceneViewSyncObject = null;
                return;
            }
            if (!Selection.activeGameObject)
                return;
            m_SceneViewSyncObject= Selection.activeGameObject;
            EditorApplication.update += SceneViewSyncObject;
        }
        static GameObject m_SceneViewSyncObject;
        static void SceneViewSyncObject()
        {
            if (!m_SceneViewSyncObject || Selection.activeObject != m_SceneViewSyncObject)
            {
                EditorApplication.update -= SceneViewSyncObject;
                m_SceneViewSyncObject = null;
                return;
            }
            SceneView targetView = SceneView.sceneViews[0] as SceneView;
            targetView.pivot = m_SceneViewSyncObject.transform.position;
        }

        [MenuItem("Work Flow/Hotkeys/Take Screen Shot _F12", false, 103)]
        static void TakeScreenShot()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath + "/ScreenShots");
            string path = Path.Combine(directory.Parent.FullName, string.Format("Screenshot_{0}.png", DateTime.Now.ToString("yyyyMMdd_Hmmss")));
            Debug.LogFormat("ScreenShot Successful:\n<Color=#F1F635FF>{0}</Color>", path);
            ScreenCapture.CaptureScreenshot(path);
        }
        #endregion
        #region Window
        //BuiltIn Texture Ref:https://unitylist.com/p/5c3/Unity-editor-icons
        [MenuItem("Work Flow/UI Tools/Missing Fonts Replacer", false, 203)]
        static void ShowFontsReplacerWindow() => EditorWindow.GetWindow<EUIFontsMissingReplacerWindow>().titleContent=new GUIContent("Missing Fonts Replacer",EditorGUIUtility.FindTexture("FilterByLabel"));
        [MenuItem("Work Flow/Art/Plane Mesh Generator", false, 301)]
        static void ShowPlaneGenerator() => EditorWindow.GetWindow(typeof(EWPlaneMeshGenerator)).titleContent=new GUIContent("Plane Generator", EditorGUIUtility.FindTexture("CustomTool"));
        [MenuItem("Work Flow/Art/Noise Texture Generator", false, 302)]
        static void ShowNoiseGenerator() => EditorWindow.GetWindow(typeof(EWNoiseGenerator)).titleContent=new GUIContent("Noise Texture Generator",EditorGUIUtility.FindTexture("CustomTool"));
        [MenuItem("Work Flow/Art/Mesh Smooth Normal Generator", false, 303)]
        static void ShowSmoothNormalGenerator() => EditorWindow.GetWindow(typeof(EWSmoothNormalGenerator)).titleContent = new GUIContent("Smooth Normal Generator", EditorGUIUtility.FindTexture("CustomTool"));
        [MenuItem("Work Flow/Art/Mesh Vertex Editor", false, 304)]
        static void ShowMeshVertexEditor() => EditorWindow.GetWindow(typeof(EWMeshVertexEditor)).titleContent = new GUIContent("Vertex Editor",EditorGUIUtility.FindTexture("CustomTool"));

        [MenuItem("Work Flow/Art/(Optimize)Animation Instance Baker", false, 400)]
        static void ShowOptimizeWindow() => EditorWindow.GetWindow(typeof(EWAnimationInstanceBaker)).titleContent = new GUIContent("GPU Animation Instance Baker", EditorGUIUtility.FindTexture("AvatarSelector"));
        #endregion
    }

}