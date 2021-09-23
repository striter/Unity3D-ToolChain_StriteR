using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace TEditor
{

    public static class UEAsset
    {
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
                    if (CopyPropertyTo(asset, previousAsset))
                        AssetDatabase.SaveAssets();
                    else
                        EditorUtility.CopySerialized(asset, previousAsset);
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
                UnityEngine.Object subAsset = Array.Find(assets, p => p.name == subValue.Key && p.GetType() == subValue.Value.GetType());

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
        public static T CreateAssetCombination<T>(string _path, T _mainAsset, params KeyValuePair<string, UnityEngine.Object>[] _subAssets) where T : UnityEngine.Object
        {
            T mainAsset = CreateOrReplaceMainAsset(_mainAsset, _path);
            CreateOrReplaceSubAsset(_path, _subAssets);
            Debug.Log("Asset Combination Generate Successful:" + _path);
            return mainAsset;
        }

        public static string GetCurrentProjectWindowPath() => (string)(typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null));
        public static bool SelectFilePath(out string filePath, string extensiton = "", string startDirectory = null)
        {
            filePath = EditorUtility.OpenFilePanel("Select File Path", startDirectory == null ? GetCurrentProjectWindowPath() : startDirectory, extensiton);
            if (filePath.Length == 0)
                return false;
            return true;
        }

        public static bool SaveFilePath(out string filePath, string extension = "", string defaultName = "", string startDirectory = null)
        {
            filePath = EditorUtility.SaveFilePanel("Select Save File Path", startDirectory == null ? GetCurrentProjectWindowPath() : startDirectory, defaultName, extension);
            if (filePath.Length == 0)
                return false;
            return true;
        }

        public static bool SelectDirectory(UnityEngine.Object _srcAsset, out string directoryPath, out string objName)
        {
            directoryPath = "";
            objName = "";
            string assetPath = AssetDatabase.GetAssetPath(_srcAsset);
            string fbxDirectory = (Application.dataPath.Remove(Application.dataPath.Length - 6, 6) + assetPath.Remove(assetPath.LastIndexOf('/'))).Replace("/", @"\");
            string folderPath = EditorUtility.OpenFolderPanel("Select Directory", fbxDirectory, "");
            if (folderPath.Length == 0)
                return false;
            directoryPath = UEPath.FileToAssetPath(folderPath) + "/";
            objName = UEPath.GetPathName(assetPath);
            return true;
        }

        public static bool CreateOrReplaceFile(string path, byte[] bytes)
        {
            try
            {
                FileStream fileStream = File.Open(path, FileMode.OpenOrCreate);
                BinaryWriter writer = new BinaryWriter(fileStream);
                writer.Write(bytes);
                writer.Close();
                fileStream.Close();
                AssetDatabase.Refresh();
                return true;

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        #endregion
        #region Serialize Helper
        static readonly Dictionary<Type, Action<UnityEngine.Object, UnityEngine.Object>> m_CopyHelper = new Dictionary<Type, Action<UnityEngine.Object, UnityEngine.Object>>() {
            { typeof(Mesh),(src, dst) => CopyMesh((Mesh)src, (Mesh)dst)},
            {typeof(AnimationClip),(src,dst)=>CopyAnimationClip((AnimationClip)src,(AnimationClip)dst) }
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

        public static void CopyMesh(Mesh _src, Mesh _tar)
        {
            _tar.Clear();
            Vector3[] verticies = _src.vertices;
            _tar.vertices = verticies;
            _tar.normals = _src.normals;
            _tar.tangents = _src.tangents;
            _tar.name = _src.name;
            _tar.bounds = _src.bounds;
            _tar.bindposes = _src.bindposes;
            _tar.colors = _src.colors;
            _tar.boneWeights = _src.boneWeights;
            _tar.triangles = _src.triangles;
            List<Vector4> uvs = new List<Vector4>();
            for (int i = 0; i < 8; i++)
            {
                _src.GetUVs(i, uvs);
                _tar.SetUVsResize(i, uvs);
            }

            _tar.subMeshCount = _src.subMeshCount;
            for (int i = 0; i < _src.subMeshCount; i++)
                _tar.SetSubMesh(i, _src.GetSubMesh(i));

            _tar.ClearBlendShapes();
            _src.TraversalBlendShapes(verticies.Length, (name, index, frame, weight, deltaVerticies, deltaNormals, deltaTangents) => _tar.AddBlendShapeFrame(name, weight, deltaVerticies, deltaNormals, deltaTangents));
        }

        public static void CopyAnimationClip(AnimationClip _src, AnimationClip _dstClip)
        {
            AnimationUtility.SetAnimationEvents(_dstClip, _src.events);
            _dstClip.frameRate = _src.frameRate;
            _dstClip.wrapMode = _src.wrapMode;
            _dstClip.legacy = _src.legacy;
            _dstClip.localBounds = _src.localBounds;
            _dstClip.ClearCurves();
            foreach (var curveBinding in AnimationUtility.GetCurveBindings(_src))
                _dstClip.SetCurve(curveBinding.path, curveBinding.type, curveBinding.propertyName, AnimationUtility.GetEditorCurve(_src, curveBinding));
        }

        public static Mesh Copy(this Mesh _srcMesh)
        {
            Mesh copy = new Mesh();
            CopyMesh(_srcMesh, copy);
            return copy;
        }
        public static AnimationClip Copy(this AnimationClip _srcClip)
        {
            AnimationClip copy = new AnimationClip();
            CopyAnimationClip(_srcClip, copy);
            return copy;
        }
        #endregion
    }
}
