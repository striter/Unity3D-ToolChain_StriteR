using System;
using System.IO;
using UnityEditor.Extensions.EditorPath;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Extensions
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
            m_ObjectSyncToSceneView = _object;
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
        
        public static void TakeScreenShot()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath + "/ScreenShots");
            string path = Path.Combine(directory.Parent.FullName, $"Screenshot_{DateTime.Now:yyyyMMdd_Hmmss}.png");
            Debug.LogFormat("ScreenShot Successful:\n<Color=#F1F635FF>{0}</Color>", path);
            ScreenCapture.CaptureScreenshot(path);
        }
        
        public static void OutputActiveWindowDirectory()=> Debug.Log(  UEPath.GetCurrentProjectWindowDirectory());
        public static void OutputAssetDirectory()=> Debug.Log(  AssetDatabase.GetAssetPath(Selection.activeObject));
        
        public static void SortTransformBySize()
        {
            if (!Selection.activeGameObject)
                return;

            var selectObject = (GameObject) Selection.activeObject;
            var childCount = selectObject.transform.childCount;
            var position = Vector3.zero;
            for (int i = 0; i < selectObject.transform.childCount; i++)
            {
                var child = selectObject.transform.GetChild(i);
                UBoundsIncrement.Begin();
                foreach (var filter in child.GetComponentsInChildren<MeshFilter>())
                    UBoundsIncrement.Iterate(filter.sharedMesh.bounds);
                var bounds = UBoundsIncrement.End();
                
                child.transform.localPosition = position;
                position += (bounds.size.x + 1f) * Vector3.right;
            }
            
        }
    }
}