using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TEditor
{
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
        public static Dictionary<int,string> GetAllLayers(bool emptyInclusive)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!emptyInclusive&&layerName == string.Empty)
                    continue;
                dic.Add(i, layerName);
            }
            return dic;
        }

        #region Assets
        public static T CreateOrReplaceMainAsset<T>(T asset, string path) where T : UnityEngine.Object
        {
            UnityEngine.Object previousAsset = AssetDatabase.LoadMainAssetAtPath(path);
            T replacedAsset = null;

            if (previousAsset != null)
            {
                replacedAsset = previousAsset as T;
                if (replacedAsset != null)
                {
                    if (TCommon.CopyPropertyTo<T>(asset, previousAsset))
                        AssetDatabase.SaveAssets();
                    else
                        EditorUtility.CopySerialized(asset,previousAsset);
                }
                else
                    AssetDatabase.DeleteAsset(path);
            }

            if (!replacedAsset)
            {
                AssetDatabase.CreateAsset(asset, path);
                replacedAsset = AssetDatabase.LoadMainAssetAtPath(path) as T;
            }
            return replacedAsset;
        }
        public static void CreateOrReplaceSubAsset<T>(T asset, string mainPath, string assetName) where T : UnityEngine.Object
        {
            UnityEngine.Object mainAsset = AssetDatabase.LoadMainAssetAtPath(mainPath);
            if (!mainAsset)
                throw new System.Exception("Invalid Main Assets:" + mainPath);
            asset.name = assetName;
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(mainPath);
            UnityEngine.Object subAsset = System.Array.Find(assets, p => p.name == asset.name);

            if (subAsset && (subAsset as T != null))
                if (TCommon.CopyPropertyTo<T>(asset, subAsset))
                    AssetDatabase.SaveAssets();
                else
                    EditorUtility.CopySerialized(asset, subAsset);
            else
                AssetDatabase.AddObjectToAsset(asset, mainAsset);
        }
        public static void CreateAssetCombination(KeyValuePair<UnityEngine.Object, string> _mainAsset,params KeyValuePair<UnityEngine.Object, string>[] _subValues)
        {
            UnityEngine.Object mainAsset = CreateOrReplaceMainAsset(_mainAsset.Key, _mainAsset.Value);
            foreach (KeyValuePair<UnityEngine.Object, string> subValue in _subValues)
                CreateOrReplaceSubAsset(subValue.Key, _mainAsset.Value, subValue.Value);
            AssetDatabase.SaveAssets();
            Debug.Log("Asset Combination Generate Successful:" + _mainAsset.Key);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_mainAsset.Value));
        }
        public static bool SelectPath(UnityEngine.Object _srcAsset, out string savePath, out string objName)
        {
            savePath = "";
            objName = "";
            string assetPath = _srcAsset==null?"Assets/":AssetDatabase.GetAssetPath(_srcAsset);
            string fbxDirectory = (Application.dataPath.Remove(Application.dataPath.Length - 6, 6) + assetPath.Remove(assetPath.LastIndexOf('/'))).Replace("/", @"\");
            string folderPath = EditorUtility.OpenFolderPanel("Select Data Save Folder", fbxDirectory, "");
            if (folderPath.Length == 0)
                return false;
            savePath = GetAssetPath(folderPath);
            objName = GetPathName(assetPath);
            return true;
        }
        public static string GetAssetPath(string path)
        {
            int assetIndex = path.IndexOf("/Assets")+1;
            if (assetIndex != 0)
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
        #endregion
    }
}
