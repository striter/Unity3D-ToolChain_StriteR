using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TEditor
{
    public static class UECommon
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
                    if (CopyPropertyTo( asset, previousAsset))
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
            EditorGUIUtility.PingObject(replacedAsset);
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
                    if (CopyPropertyTo(subValue.Value, subAsset))
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
            directoryPath =UEPath.FilePathToAssetPath(folderPath)+"/";
            objName = UEPath.GetPathName(assetPath);
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

        #endregion

        #region SceneView
        public static Vector2 GetScreenPoint(this SceneView _sceneView)
        {
            Vector2 screenPoint = Event.current.mousePosition;
            screenPoint.y = _sceneView.camera.pixelHeight - screenPoint.y;
            return screenPoint;
        }
        #endregion

        #region Serialize Helper
        static readonly Dictionary<Type, Action<UnityEngine.Object, UnityEngine.Object>> m_CopyHelper = new Dictionary<Type, Action<UnityEngine.Object, UnityEngine.Object>>() {
            { typeof(Mesh),(src, dst) => CopyMesh((Mesh)src, (Mesh)dst)}
        };

        public static bool CopyPropertyTo(UnityEngine.Object _src, UnityEngine.Object _tar)
        {
            Type type = _src.GetType();
            if (type != _tar.GetType())
            {
                Debug.LogError("Assets Type Not Match:" + _src.GetType() + "," + _tar.GetType());
                return false;
            }

            if (m_CopyHelper.ContainsKey(_src.GetType()))
            {
                m_CopyHelper[type](_src, _tar);
                return true;
            }
            return false;
        }

        public static void CopyMesh(Mesh source, Mesh target)
        {
            target.Clear();
            target.vertices = source.vertices;
            target.normals = source.normals;
            target.tangents = source.tangents;
            target.name = source.name;
            target.bounds = source.bounds;
            target.bindposes = source.bindposes;
            target.colors = source.colors;
            target.boneWeights = source.boneWeights;
            target.triangles = source.triangles;
            List<Vector4> uvs = new List<Vector4>();
            for (int i = 0; i < 8; i++)
            {
                source.GetUVs(i, uvs);
                if (uvs.Count <= 0)
                    continue;
                bool third = false;
                bool fourth = false;
                for (int j = 0; j < uvs.Count; j++)
                {
                    Vector4 check = uvs[j];
                    third |= check.z != 0;
                    fourth |= check.w != 0;
                }

                if (fourth)
                    target.SetUVs(i, uvs);
                else if (third)
                    target.SetUVs(i, uvs.ToList(vec4 => new Vector3(vec4.x, vec4.y, vec4.z)));
                else
                    target.SetUVs(i, uvs.ToList(vec4 => new Vector2(vec4.x, vec4.y)));
            }
            for (int i = 0; i < source.subMeshCount; i++)
                target.SetIndices(source.GetIndices(i), MeshTopology.Triangles, i);
        }

        public static Mesh Copy(this Mesh _srcMesh)
        {
            Mesh copy = new Mesh();
            CopyMesh(_srcMesh, copy);
            return copy;
        }

        #endregion
    }
    public static class UEPath
    {
        public static string FilePathToAssetPath(string path)
        {
            int assetIndex = path.IndexOf("/Assets") + 1;
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
    }
}
