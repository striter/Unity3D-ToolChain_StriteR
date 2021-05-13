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
        static void ShowFontsReplacerWindow() => EditorWindow.GetWindow<EUIFontsMissingReplacerWindow>().titleContent=new GUIContent("Missing Fonts Replacer",EditorGUIUtility.IconContent("FilterByLabel").image);
        //Art
        [MenuItem("Work Flow/Art/Plane Mesh Generator", false, 301)]
        static void ShowPlaneGenerator() => EditorWindow.GetWindow(typeof(EWPlaneMeshGenerator)).titleContent=new GUIContent("Plane Generator", EditorGUIUtility.IconContent("CustomTool").image);
        [MenuItem("Work Flow/Art/Mesh Smooth Normal Generator", false, 302)]
        static void ShowSmoothNormalGenerator() => EditorWindow.GetWindow(typeof(EWSmoothNormalGenerator)).titleContent = new GUIContent("Smooth Normal Generator", EditorGUIUtility.IconContent("CustomTool").image);
        [MenuItem("Work Flow/Art/Mesh Editor", false, 303)]
        static void ShowMeshEditor() => EditorWindow.GetWindow(typeof(EMeshEditor)).titleContent = new GUIContent("Mesh Editor",EditorGUIUtility.IconContent("AvatarPivot").image);
        [MenuItem("Work Flow/Art/Noise Texture Generator", false, 304)]
        static void ShowNoiseGenerator() => EditorWindow.GetWindow(typeof(EWNoiseGenerator)).titleContent = new GUIContent("Noise Texture Generator", EditorGUIUtility.IconContent("CustomTool").image);
        [MenuItem("Work Flow/Art/Texture Modifier", false, 305)]
        static void ShowTextureModifier() => EditorWindow.GetWindow(typeof(EWTextureEditor)).titleContent = new GUIContent("Texture Modifier", EditorGUIUtility.IconContent("d_PreTextureMipMapHigh").image);
        [MenuItem("Work Flow/Art/(Optimize)Animation Instance Baker", false, 400)]
        static void ShowOptimizeWindow() => EditorWindow.GetWindow(typeof(EWAnimationInstanceBaker)).titleContent = new GUIContent("GPU Animation Instance Baker", EditorGUIUtility.IconContent("AvatarSelector").image);
        [MenuItem("Work Flow/Art/(Optimize)Animation Clip Optimize", false, 401)]
        static void ShowAssetOptimizeWindow() => EditorWindow.GetWindow(typeof(EWAnimationClipOptimize)).titleContent = new GUIContent("Asset Optimize", EditorGUIUtility.IconContent("Toolbar Plus More").image);
        #endregion
    }

}