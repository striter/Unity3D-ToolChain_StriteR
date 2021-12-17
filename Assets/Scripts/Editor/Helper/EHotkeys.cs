using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TEditor
{
    public static class Hotkeys
    {
        #region SyncObjectToCamera
        public static void SyncSelectedToSceneViewCamera()
        {
            if (Application.isPlaying)
                return;
            if (EndSync())
                return;
            if (!Selection.activeGameObject)
                return;
            BeginSync(Selection.activeGameObject);
        }

        static GameObject m_ObjectSyncToSceneView;
        static void BeginSync(GameObject _object)
        {
            m_ObjectSyncToSceneView = Selection.activeGameObject;
            EditorApplication.update += SyncObjectPositionToSceneView;
            Undo.undoRedoPerformed += SyncUndo;
            Undo.RecordObject(m_ObjectSyncToSceneView.transform, "Camera Position Sync");
        }
        static void SyncUndo()=> EndSync();
        static bool EndSync()
        {
            if (!m_ObjectSyncToSceneView)
                return false;
            m_ObjectSyncToSceneView = null;
            EditorApplication.update -= SyncObjectPositionToSceneView;
            Undo.undoRedoPerformed -= SyncUndo;
            return true;
        }
        static void SyncObjectPositionToSceneView()
        {
            if (!m_ObjectSyncToSceneView || Selection.activeObject != m_ObjectSyncToSceneView)
            {
                EditorApplication.update -= SyncObjectPositionToSceneView;
                m_ObjectSyncToSceneView = null;
                return;
            }
            SceneView targetView = SceneView.sceneViews[0] as SceneView;
            var targetViewCamera = targetView.camera.transform;
            m_ObjectSyncToSceneView.transform.position = targetViewCamera.position;
            m_ObjectSyncToSceneView.transform.rotation = targetViewCamera.rotation;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        #endregion
        #region SyncCameraToSelection
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
        #endregion

        public static void SwitchPause()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }
        public static void TakeScreenShot()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath + "/ScreenShots");
            string path = Path.Combine(directory.Parent.FullName, $"Screenshot_{DateTime.Now:yyyyMMdd_Hmmss}.png");
            Debug.LogFormat("ScreenShot Successful:\n<Color=#F1F635FF>{0}</Color>", path);
            ScreenCapture.CaptureScreenshot(path);
        }
        public static void OutputActiveWindowDirectory()=> Debug.Log(  UEAsset.GetCurrentProjectWindowDirectory());
    }
}