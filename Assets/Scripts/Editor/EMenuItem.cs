using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public static class EMenuItem
    {
        [MenuItem("Work Flow/Take Screen Shot _F12", false, 102)]
        static void TakeScreenShot()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath + "/ScreenShots");
            string path = Path.Combine(directory.Parent.FullName, string.Format("Screenshot_{0}.png", DateTime.Now.ToString("yyyyMMdd_Hmmss")));
            Debug.LogFormat("ScreenShot Successful:\n<Color=#F1F635FF>{0}</Color>",path);
            ScreenCapture.CaptureScreenshot(path);
        }

        [MenuItem("Work Flow/UI Tools/Missing Fonts Replacer", false, 203)]
        static void ShowWindow() => EditorWindow.GetWindow<EUIFontsMissingReplacerWindow>().titleContent=new GUIContent("Missing Fonts Replacer",EditorGUIUtility.FindTexture("FilterByLabel"));
        [MenuItem("Work Flow/Art/(Optimize)Animation Instance Baker", false, 300)]
        static void ShowOptimizeWindow() => EditorWindow.GetWindow(typeof(EAnimationInstanceBakerWindow)).titleContent = new GUIContent("GPU Animation Instance Baker", EditorGUIUtility.FindTexture("AvatarSelector"));
        [MenuItem("Work Flow/Art/Plane Mesh Generator", false, 301)]
        static void ShowPlaneGenerator() => EditorWindow.GetWindow(typeof(EPlaneMeshGeneratorWindow)).titleContent=new GUIContent("Plane Generator", EditorGUIUtility.FindTexture("CustomTool"));
        [MenuItem("Work Flow/Art/Noise Texture Generator", false, 302)]
        static void ShowNoiseGenerator() => EditorWindow.GetWindow(typeof(ENoiseGeneratorWindow)).titleContent=new GUIContent("Noise Texture Generator",EditorGUIUtility.FindTexture("CustomTool"));
        [MenuItem("Work Flow/Art/Mesh Smooth Normal Generator", false, 303)]
        static void ShowSmoothNormalGenerator() => EditorWindow.GetWindow(typeof(ESmoothNormalGeneratorWindow)).titleContent = new GUIContent("Smooth Normal Generator", EditorGUIUtility.FindTexture("CustomTool"));
    }

}