using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TEditor
{
    public static class TMenuItem
    {

        [MenuItem("Work Flow/Take Screen Shot",false, 102)]
        static void TakeScreenShot()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath+"/ScreenShots");
            string path = Path.Combine(directory.Parent.FullName, string.Format("Screenshot_{0}.png", DateTime.Now.ToString("yyyyMMdd_Hmmss")));
            Debug.Log("Sceen Shots At " + path);
            ScreenCapture.CaptureScreenshot(path);
        }

        [MenuItem("Work Flow/AssetBundles/Test Asset Bundle Constructing", false, 110)]
        static void BuildAllAssetBundlesAndroid()
        {
            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None, BuildTarget.Android);
        }

        [MenuItem("Work Flow/UI Tools/Missing Fonts Replacer", false, 203)]
         static void ShowWindow()=> EditorWindow.GetWindow<EUIFontsMissingReplacerWindow>().Show();

        [MenuItem("Work Flow/Optimize/Animation Instance Baker", false, 303)]
         static void ShowOptimizeWindow() => EditorWindow.GetWindow(typeof(EAnimationInstanceBakerWindow));

    }
    public static class EEditorAudioHelper
    {
        static AudioClip curClip;
        //Reflection Target  UnityEditor.AudioUtil;
        public static void AttachClipTo(AudioClip clip)
        {
            curClip = clip;
        }
        public static bool IsAudioPlaying()
        {
            if (curClip != null)
                return (bool)GetClipMethod("IsClipPlaying").Invoke(null, new object[] { curClip });
            return false;
        }
        public static int GetSampleDuration()
        {
            if(curClip!=null)
              return(int)GetClipMethod("GetSampleCount").Invoke(null, new object[] { curClip });
            return -1;
        }
        public static int GetCurSample()
        {
            if (curClip != null)
                return (int)GetClipMethod("GetClipSamplePosition").Invoke(null, new object[] { curClip });
            return -1;
        }
        public static float GetCurTime()
        {
            if (curClip != null)
                return (float)GetClipMethod("GetClipPosition").Invoke(null, new object[] { curClip});
            return -1;
        }
        public static void PlayClip()
        {
            if (curClip != null)
                GetClipMethod("PlayClip").Invoke(null, new object[] { curClip });
        }
        public static void PauseClip()
        {
            if (curClip != null)
                GetClipMethod("PauseClip").Invoke( null,  new object[] { curClip } );
        }
        public static void StopClip()
        {
            if(curClip!=null)
            GetClipMethod("StopClip").Invoke(null,  new object[] { curClip } );
        }
        public static void ResumeClip()
        {
            if (curClip != null)
                GetClipMethod("ResumeClip").Invoke(null, new object[] { curClip });
        }
        public static void SetSamplePosition(int startSample)
        {
            GetMethod<AudioClip, int>("SetClipSamplePosition").Invoke(null, new object[] { curClip, startSample });
        }
        static MethodInfo GetClipMethod(string methodName)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
           return  audioUtilClass.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
        }
        static MethodInfo GetMethod<T, U>(string methodName)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            return audioUtilClass.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(T), typeof(U) }, null);

        }
    }
    public static class TEditor
    {
        public static T CreateOrReplaceAsset<T>(T asset, string path) where T : UnityEngine.Object
        {
            UnityEngine.Object previousAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            T replacedAsset = null;
            if (previousAsset != null)
            {
                replacedAsset = previousAsset as T;
                if (replacedAsset != null)
                    EditorUtility.CopySerialized(asset, previousAsset);
                else
                    AssetDatabase.DeleteAsset(path);
            }

            if(!replacedAsset)
            {
                AssetDatabase.CreateAsset(asset, path);
                replacedAsset= AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return replacedAsset;
        }
        public static bool SelectPath(UnityEngine.Object _srcAsset, out string savePath, out string objName)
        {
            savePath = "";
            objName = "";
            string assetPath = AssetDatabase.GetAssetPath(_srcAsset);
            string fbxDirectory = (Application.dataPath.Remove(Application.dataPath.Length - 6, 6) + assetPath.Remove(assetPath.LastIndexOf('/'))).Replace("/", @"\");
            Debug.Log(fbxDirectory);
            string folderPath = EditorUtility.OpenFolderPanel("Select Data Save Folder", fbxDirectory, "");
            if (folderPath.Length == 0)
                return false;
            savePath = GetAssetPath(folderPath);
            objName = GetPathName(assetPath);
            return true;
        }
        public static string GetAssetPath(string path)
        {
            int assetIndex = path.IndexOf("Assets/");
            if (assetIndex != -1)
                path = path.Substring(assetIndex, path.Length - assetIndex);
            if (path[path.Length - 1] != '/')
                path += '/';
            return path;
        }
        public static string RemoveExtension(string path)
        {
            int extensionIndex = path.LastIndexOf('.');
            if (extensionIndex >= 0)
                return path.Remove(extensionIndex);
            return path;
        }
        public static string GetPathName(string path)
        {
            path = RemoveExtension(path);
            int folderIndex = path.LastIndexOf('/');
            if (folderIndex >= 0)
                path = path.Substring(folderIndex + 1, path.Length - folderIndex - 1);
            return path;
        }
    }
}
