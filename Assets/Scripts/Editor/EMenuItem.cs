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
        [MenuItem("Work Flow/Hotkeys/Selected Object Sync Scene View &F", false, 101)]
        public static void SyncObjectToSceneView() => EHotkeys.SyncObjectToSceneView();
        [MenuItem("Work Flow/Hotkeys/Scene View Camera Sync To Selected _F11", false, 102)]
        public static void SceneViewCameraSyncSelected() => EHotkeys.SceneViewCameraSyncSelected();
        [MenuItem("Work Flow/Hotkeys/Take Screen Shot _F12", false, 103)]
        static void TakeScreenShot() => EHotkeys.TakeScreenShot();
        #endregion
        #region Window
        //BuiltIn Texture Ref:https://unitylist.com/p/5c3/Unity-editor-icons
        //UI
        [MenuItem("Work Flow/UI/Missing Fonts Replacer", false, 203)]
        static void ShowFontsReplacerWindow() => EditorWindow.GetWindow<EUIFontsMissingReplacerWindow>().titleContent=new GUIContent("Missing Fonts Replacer",EditorGUIUtility.FindTexture("FilterByLabel"));
        //Art
        [MenuItem("Work Flow/Art/Plane Mesh Generator", false, 301)]
        static void ShowPlaneGenerator() => EditorWindow.GetWindow(typeof(EWPlaneMeshGenerator)).titleContent=new GUIContent("Plane Generator", EditorGUIUtility.FindTexture("CustomTool"));
        [MenuItem("Work Flow/Art/Noise Texture Generator", false, 302)]
        static void ShowNoiseGenerator() => EditorWindow.GetWindow(typeof(EWNoiseGenerator)).titleContent=new GUIContent("Noise Texture Generator",EditorGUIUtility.FindTexture("CustomTool"));
        [MenuItem("Work Flow/Art/Mesh Smooth Normal Generator", false, 303)]
        static void ShowSmoothNormalGenerator() => EditorWindow.GetWindow(typeof(EWSmoothNormalGenerator)).titleContent = new GUIContent("Smooth Normal Generator", EditorGUIUtility.FindTexture("CustomTool"));
        [MenuItem("Work Flow/Art/Mesh Editor", false, 304)]
        static void ShowMeshVertexEditor() => EditorWindow.GetWindow(typeof(EMeshEditor)).titleContent = new GUIContent("Mesh Editor",EditorGUIUtility.FindTexture("AvatarPivot"));

        [MenuItem("Work Flow/Art/(Optimize)Animation Instance Baker", false, 400)]
        static void ShowOptimizeWindow() => EditorWindow.GetWindow(typeof(EWAnimationInstanceBaker)).titleContent = new GUIContent("GPU Animation Instance Baker", EditorGUIUtility.FindTexture("AvatarSelector"));
        [MenuItem("Work Flow/Art/(Optimize)Animation Clip Optimize", false, 401)]
        static void ShowAssetOptimizeWindow() => EditorWindow.GetWindow(typeof(EWAnimationClipOptimize)).titleContent = new GUIContent("Asset Optimize", EditorGUIUtility.FindTexture("Toolbar Plus More"));
        #endregion
    }

}