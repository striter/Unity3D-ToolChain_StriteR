using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
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
                    if (TCommon.CopyPropertyTo( asset, previousAsset))
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
        public static void CreateOrReplaceSubAsset(string _path, params KeyValuePair<string, UnityEngine.Object>[] _subValues)
        {
            UnityEngine.Object mainAsset = AssetDatabase.LoadMainAssetAtPath(_path);
            if (!mainAsset)
                throw new Exception("Invalid Main Assets:" + _path);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(_path);
            foreach (KeyValuePair<string, UnityEngine.Object> subValue in _subValues)
            {
                subValue.Value.name = subValue.Key;
                UnityEngine.Object subAsset = Array.Find(assets, p => p.name == subValue.Key&&p.GetType()==subValue.Value.GetType());

                if (subAsset)
                    if (TCommon.CopyPropertyTo(subValue.Value, subAsset))
                        AssetDatabase.SaveAssets();
                    else
                        EditorUtility.CopySerialized(subValue.Value, subAsset);
                else
                    AssetDatabase.AddObjectToAsset(subValue.Value, mainAsset);
            }
            AssetDatabase.SaveAssets();
        }
        public static T CreateAssetCombination<T>(string _path, T _mainAsset,params KeyValuePair< string, UnityEngine.Object>[] _subAssets) where T: UnityEngine.Object
        {
            T mainAsset = CreateOrReplaceMainAsset(_mainAsset, _path);
            CreateOrReplaceSubAsset(_path, _subAssets);
            Debug.Log("Asset Combination Generate Successful:" + _path);
            EditorGUIUtility.PingObject(mainAsset);
            return mainAsset;
        }

        public static string GetCurrentProjectWindowPath()=> (string)(typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,null));
        public static bool SelectFilePath(out string filePath,string extensiton = "", string startDirectory = null)
        { 
            filePath = EditorUtility.OpenFilePanel("Select File Path", startDirectory == null ? GetCurrentProjectWindowPath() : startDirectory, extensiton);
            if (filePath.Length == 0)
                return false;
            return true;
        }

        public static bool SaveFilePath(out string filePath,string extension="",string defaultName="",string startDirectory=null)
        {
            filePath = EditorUtility.SaveFilePanel("Select Save File Path",startDirectory==null?GetCurrentProjectWindowPath():startDirectory,defaultName,extension);
            if (filePath.Length == 0)
                return false;
            return true;
        }

        public static bool SelectDirectory(UnityEngine.Object _srcAsset, out string directoryPath, out string objName)
        {
            directoryPath = "";
            objName = "";
            string assetPath =  AssetDatabase.GetAssetPath(_srcAsset);
            string fbxDirectory = (Application.dataPath.Remove(Application.dataPath.Length - 6, 6) + assetPath.Remove(assetPath.LastIndexOf('/'))).Replace("/", @"\");
            string folderPath = EditorUtility.OpenFolderPanel("Select Directory", fbxDirectory, "");
            if (folderPath.Length == 0)
                return false;
            directoryPath = FilePathToAssetPath(folderPath)+"/";
            objName = GetPathName(assetPath);
            return true;
        }

        public static bool CreateOrReplaceFile(string path,byte[] bytes)
        {
            try
            {
                FileStream fileStream = File.Open(path,FileMode.OpenOrCreate);
                BinaryWriter writer = new BinaryWriter(fileStream);
                writer.Write(bytes);
                writer.Close();
                fileStream.Close();
                AssetDatabase.Refresh();
                return true;

            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }


        public static string FilePathToAssetPath(string path)
        {
            int assetIndex = path.IndexOf("/Assets")+1;
            if (assetIndex != 0)
                path = path.Substring(assetIndex, path.Length - assetIndex);
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
    public static class TEditor_GUIStyle
    {
        public static GUIStyle m_TitleLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
        public static GUIStyle m_ErrorLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.BoldAndItalic, richText = true };
    }
    public static class TEditor_GUIScope_Horizontal
    {
        static Vector2 m_StartPos;
        static Vector2 m_Offset;
        static float m_SizeY;
        public static void Begin(float _startX, float _startY, float _startSizeY)
        {
            m_SizeY = _startSizeY;
            m_StartPos = new Vector2(_startX, _startY);
            m_Offset = Vector2.zero;
        }
        public static Rect NextRect(float _spacingX, float _sizeX)
        {
            Vector2 originOffset = m_Offset;
            m_Offset.x += _sizeX + _spacingX;
            return new Rect(m_StartPos + originOffset, new Vector2(_sizeX, m_SizeY));
        }
        public static void NextLine(float _spacingY, float _sizeY)
        {
            m_Offset.y += m_SizeY + _spacingY;
            m_SizeY = _sizeY;
            m_Offset.x = 0;
        }
    }
}
