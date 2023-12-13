using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Extensions
{
    public static class UEAsset
    {
        #region Assets
        public static T CreateOrReplaceMainAsset<T>(T asset, string path,bool ping = true) where T : UnityEngine.Object
        {
            asset.name = UEPath.RemoveExtension(UEPath.GetFileName(path));
            UnityEngine.Object previousAsset = AssetDatabase.LoadMainAssetAtPath(path);
            T replacedAsset = null;

            if (previousAsset != null)
            {
                replacedAsset = previousAsset as T;
                if (replacedAsset != null)
                    CopyPropertyTo(asset, previousAsset);
                else
                    AssetDatabase.DeleteAsset(path);
            }

            if (!replacedAsset)
            {
                AssetDatabase.CreateAsset(asset, path);
                replacedAsset = AssetDatabase.LoadMainAssetAtPath(path) as T;
            }
            if(ping)
                EditorGUIUtility.PingObject(replacedAsset);
            return replacedAsset;
        }
        public static void CreateOrReplaceSubAsset(string _mainAssetPath, IEnumerable<UnityEngine.Object> _subValues)
        {
            UnityEngine.Object mainAsset = AssetDatabase.LoadMainAssetAtPath(_mainAssetPath);
            if (!mainAsset)
                throw new Exception("Invalid Main Assets:" + _mainAssetPath);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(_mainAssetPath);
            foreach (var dstAsset in _subValues)
            {
                UnityEngine.Object srcAsset = Array.Find(assets, p => AssetDatabase.IsSubAsset(p)&& p.name == dstAsset.name && p.GetType() == dstAsset.GetType());
                if (srcAsset)
                    CopyPropertyTo(dstAsset, srcAsset);
                else
                    AssetDatabase.AddObjectToAsset(dstAsset, mainAsset);
            }
            AssetDatabase.SaveAssets();
        }

        public static void ClearSubAssets(string _mainAssetPath)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(_mainAssetPath))
            {
                if(!AssetDatabase.IsSubAsset(asset))
                    continue;
                AssetDatabase.RemoveObjectFromAsset(asset);
            }
        }
        public static T CreateAssetCombination<T>(string _path, T _mainAsset, IEnumerable<UnityEngine.Object> _subAssets) where T : UnityEngine.Object
        {
            T mainAsset = CreateOrReplaceMainAsset(_mainAsset, _path);
            CreateOrReplaceSubAsset(_path, _subAssets);
            Debug.Log("Asset Combination Generate Successful:" + _path);
            return mainAsset;
        }

        public static string GetCurrentProjectWindowDirectory() => (string)(typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null));
        public static bool SelectFilePath(out string filePath, string extension = "", string startDirectory = null)
        {
            filePath = EditorUtility.OpenFilePanel("Select File To Open", startDirectory ?? GetCurrentProjectWindowDirectory(), extension);
            if (filePath.Length == 0)
                return false;
            return true;
        }

        public static bool SaveFilePath(out string filePath, string extension = "", string defaultName = "", string startDirectory = null)
        {
            filePath = EditorUtility.SaveFilePanel("Select File To Save", startDirectory ?? GetCurrentProjectWindowDirectory(), defaultName, extension);
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

        public static bool SelectDirectory(out string directoryPath)
        {
            directoryPath = "";
            string folderPath = EditorUtility.OpenFolderPanel("Select Directory",GetCurrentProjectWindowDirectory() , "");
            if (folderPath.Length == 0)
                return false;
            directoryPath = UEPath.FileToAssetPath(folderPath) + "/";
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

        public static IEnumerable<string> GetAllAssetsPathAtDirectory(string _assetDirectory) => Directory
            .GetFiles(UEPath.AssetToFilePath(_assetDirectory)).Select(UEPath.FileToAssetPath)
            .Collect(p => UEPath.GetExtension(p) != ".meta");

        
        public static IEnumerable<string> GetDepthPathByExtension(string path, List<string> FileList, string extension = "")
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] fil = dir.GetFiles();
            DirectoryInfo[] dii = dir.GetDirectories();
            foreach (FileInfo f in fil)
            {
                if (extension != "" && !f.Name.EndsWith(extension))
                {
                    continue;
                }
                FileList.Add(f.FullName.Replace("\\","/").Replace(Application.dataPath, "Assets/"));
            }
            foreach (DirectoryInfo d in dii)
            {
                GetDepthPathByExtension(d.FullName, FileList,extension);
            }
            return FileList;
        }
        #endregion

        #region Serialize Helper

        public static void CopyPropertyTo(UnityEngine.Object _src, UnityEngine.Object _tar)
        {
            Type type = _src.GetType();
            if (type != _tar.GetType())
                throw new Exception("Assets Type Not Match:" + _src.GetType() + "," + _tar.GetType());

            if (_tar is Mesh) (_tar as Mesh).Clear();
            EditorUtility.CopySerialized(_src,_tar);
            EditorUtility.SetDirty(_tar);
        }

        public static void CopyMesh(Mesh _src, Mesh _tar)
        {
            _tar.Clear();
            Vector3[] vertices = _src.vertices;
            _tar.vertices = vertices;
            _tar.normals = _src.normals;
            _tar.tangents = _src.tangents;
            _tar.name = _src.name;
            _tar.bounds = _src.bounds;
            _tar.bindposes = _src.bindposes;
            _tar.colors = _src.colors;
            _tar.boneWeights = _src.boneWeights;
            List<Vector4> uvs = new List<Vector4>();
            for (int i = 0; i < 8; i++)
            {
                _src.GetUVs(i, uvs);
                _tar.SetUVsResize(i, uvs);
            }

            _tar.subMeshCount = _src.subMeshCount;
            for (int i = 0; i < _src.subMeshCount; i++)
            {
                _tar.SetIndices(_src.GetIndices(i),MeshTopology.Triangles,i,false);
                _tar.SetSubMesh(i,_src.GetSubMesh(i),MeshUpdateFlags.DontRecalculateBounds);
            }

            _tar.ClearBlendShapes();
            _src.TraversalBlendShapes(vertices.Length, (name, index, frame, weight, deltaVertices, deltaNormals, deltaTangents) => _tar.AddBlendShapeFrame(name, weight, deltaVertices, deltaNormals, deltaTangents));
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
