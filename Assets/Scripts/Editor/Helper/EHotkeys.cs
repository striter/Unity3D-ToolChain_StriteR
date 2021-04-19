using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TEditor
{
    public static class EHotkeys
    {
        public static void SyncObjectToSceneView()
        {
            if (Application.isPlaying)
                return;
            if (m_ObjectSyncToSceneView)
            {
                m_ObjectSyncToSceneView = null;
                EditorApplication.update -= SyncObjectPositionToSceneView;
                return;
            }
            if (!Selection.activeGameObject)
                return;
            m_ObjectSyncToSceneView = Selection.activeGameObject;
            EditorApplication.update += SyncObjectPositionToSceneView;
            Undo.RecordObject(m_ObjectSyncToSceneView.transform, "Camera Position Sync");
        }
        static GameObject m_ObjectSyncToSceneView;
        static void SyncObjectPositionToSceneView()
        {
            if (!m_ObjectSyncToSceneView || Selection.activeObject != m_ObjectSyncToSceneView)
            {
                EditorApplication.update -= SyncObjectPositionToSceneView;
                m_ObjectSyncToSceneView = null;
                return;
            }
            SceneView targetView = SceneView.sceneViews[0] as SceneView;
            m_ObjectSyncToSceneView.transform.position = targetView.camera.transform.position;
            m_ObjectSyncToSceneView.transform.rotation = targetView.camera.transform.rotation;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        public static void SceneViewCameraSyncSelected()
        {
            if (m_SceneViewSyncObject)
            {
                EditorApplication.update -= SceneViewSyncObject;
                m_SceneViewSyncObject = null;
                return;
            }
            if (!Selection.activeGameObject)
                return;
            m_SceneViewSyncObject = Selection.activeGameObject;
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


        public static void TakeScreenShot()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath + "/ScreenShots");
            string path = Path.Combine(directory.Parent.FullName, string.Format("Screenshot_{0}.png", DateTime.Now.ToString("yyyyMMdd_Hmmss")));
            Debug.LogFormat("ScreenShot Successful:\n<Color=#F1F635FF>{0}</Color>", path);
            ScreenCapture.CaptureScreenshot(path);
        }
    }
}