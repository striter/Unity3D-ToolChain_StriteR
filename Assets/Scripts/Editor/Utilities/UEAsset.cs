using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Extensions;
using System.Reflection;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Extensions
{
    public static class UEAsset
    {
        #region Assets
        public static T CreateOrReplaceMainAsset<T>(T asset, string path,bool ping = true) where T : UnityEngine.Object
        {
            asset.name = path.GetFileName().RemoveExtension();
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
        
        public static void CreateOrReplaceSubAsset(ScriptableObject _object, IEnumerable<UnityEngine.Object> _subAssets)=> CreateOrReplaceSubAsset(AssetDatabase.GetAssetPath(_object), _subAssets);
        
        private static List<UnityEngine.Object> kOriginalAssets = new List<UnityEngine.Object>();
        public static void CreateOrReplaceSubAsset(string _mainAssetPath, IEnumerable<UnityEngine.Object> _subAssets)
        {
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(_mainAssetPath);
            if (!mainAsset)
                throw new Exception("Invalid Main Assets:" + _mainAssetPath);
            var assets = AssetDatabase.LoadAllAssetsAtPath(_mainAssetPath).Collect(p=>p!=mainAsset).FillList(kOriginalAssets);
            for(var i=assets.Count-1;i>=0;i--)
            {
                var srcAsset = assets[i];
                var subAsset = _subAssets.Find(p=>p.name == srcAsset.name && p.GetType() == srcAsset.GetType());
                if (subAsset)
                    continue;
                AssetDatabase.RemoveObjectFromAsset(srcAsset);
            }
            
            foreach (var dstAsset in _subAssets)
            {
                var srcAsset = assets.Find( p => AssetDatabase.IsSubAsset(p)&& p.name == dstAsset.name && p.GetType() == dstAsset.GetType());
                if (srcAsset)
                    CopyPropertyTo(dstAsset, srcAsset);
                else
                    AssetDatabase.AddObjectToAsset(dstAsset, mainAsset);
            }
            EditorUtility.SetDirty(mainAsset);
            AssetDatabase.SaveAssetIfDirty(mainAsset);
        }

        public static void ClearSubAssets(ScriptableObject _object)=> ClearSubAssets(AssetDatabase.GetAssetPath(_object));
        public static void ClearSubAssets(string _mainAssetPath)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(_mainAssetPath))
            {
                if(asset == null || !AssetDatabase.IsSubAsset(asset))
                    continue;
                AssetDatabase.RemoveObjectFromAsset(asset);
            }
        }
        
        public static T CreateAssetCombination<T>(string _assetPath, T _mainAsset, IEnumerable<UnityEngine.Object> _subAssets) where T : UnityEngine.Object
        {
            T mainAsset = CreateOrReplaceMainAsset(_mainAsset, _assetPath);
            CreateOrReplaceSubAsset(_assetPath, _subAssets);
            Debug.Log("Asset Combination Generate Successful:" + _assetPath);
            return mainAsset;
        }

        private static List<FieldInfo> kSubAssetFields = new();
        private static List<UnityEngine.Object> kSubAssets = new();
        private static List<string> kSubAssetNames = new();
        public static T CreateAssetCombination<T>(string _assetPath, T _mainAsset) where T : ScriptableObject
        {
            var directory =_assetPath.GetPathDirectory();
            if (!AssetDatabase.IsValidFolder(directory))
            {
                Debug.LogError($"Directory not exists {directory}");
                return null;
            }
            
            var mainAsset = CreateOrReplaceMainAsset(_mainAsset, _assetPath);
            var mainAssetPath = AssetDatabase.GetAssetPath(mainAsset);
            var subAssetFields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Collect(p=>p.FieldType.IsSubclassOf(typeof(UnityEngine.Object))).Where(p=>p!=null).FillList(kSubAssetFields);
            var subAssets = subAssetFields.Select(p => p.GetValue(_mainAsset) as UnityEngine.Object).Where(p => p != null).FillList(kSubAssets);
            var assetNames = subAssets.Select(p=>p.name).FillList(kSubAssetNames);
            
            CreateOrReplaceSubAsset(_assetPath, subAssets);
            var storedAssets = AssetDatabase.LoadAllAssetsAtPath(mainAssetPath).Exclude(mainAsset);
            foreach (var (index,fieldInfo) in subAssetFields.LoopIndex())
            {
                var assetName = assetNames[index];
                var storedAsset = storedAssets.Find(p => p.name == assetName && p.GetType() == fieldInfo.FieldType);
                if (!storedAsset)
                {
                    Debug.LogError($"Invalid asset stored found:{assetName}");
                    continue;
                }
                
                EditorGUIUtility.PingObject(storedAsset);
                fieldInfo.SetValue(_mainAsset, storedAsset);
            }
            mainAsset = CreateOrReplaceMainAsset(_mainAsset, _assetPath);
            Debug.Log("Asset Combination Generate Successful:" + mainAsset);
            return mainAsset;
        }
        
        public static bool SelectFilePath(out string filePath, string extension = "", string startDirectory = null)
        {
            filePath = EditorUtility.OpenFilePanel("Select File To Open", startDirectory ?? UEPath.GetCurrentProjectWindowDirectory(), extension);
            if (filePath.Length == 0)
                return false;
            return true;
        }

        public static bool SaveFilePath(out string filePath, string extension = "", string defaultName = "", string startDirectory = null)
        {
            filePath = EditorUtility.SaveFilePanel("Select File To Save", startDirectory ?? UEPath.GetCurrentProjectWindowDirectory(), defaultName, extension);
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
            directoryPath = folderPath.FileToAssetPath() + "/";
            objName = assetPath.GetPathName();
            return true;
        }

        public static bool SelectDirectory(out string directoryPath)
        {
            directoryPath = "";
            string folderPath = EditorUtility.OpenFolderPanel("Select Directory",UEPath.GetCurrentProjectWindowDirectory() , "");
            if (folderPath.Length == 0)
                return false;
            directoryPath = folderPath.FileToAssetPath() + "/";
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
            .GetFiles(_assetDirectory.AssetToFilePath()).Select(UEPath.FileToAssetPath)
            .Collect(p => p.GetExtension() != ".meta");

        
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

        public static IEnumerable<T> LoadAllAssetsAtDirectory<T>(string _directoryPath) where T:UnityEngine.Object
        {
            
            var assetPaths = AssetDatabase.FindAssets("", new[] { _directoryPath });
            foreach (var assetPath in assetPaths)
            {
                var assetFullPath = AssetDatabase.GUIDToAssetPath(assetPath);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetFullPath);

                if (asset is T template)
                    yield return template;
            }
        }

        public static string MakeSureDirectory(string _filePath)
        {
            if(!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);
            return _filePath;
        }
        
        public static void DeleteAllAssetAtPath(string _assetPath, Predicate<string> _p = null)
        {
            string[] guids = AssetDatabase.FindAssets("", new[] { _assetPath });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (_p != null && _p(assetPath))
                    continue;
                // Debug.LogError(assetPath);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        public static void CreateScriptableInstanceAtCurrentSceneRoot<T>(string _defaultName,bool _ping = true) where T:ScriptableObject
        {
            var path = UEPath.PathRegex($"<#activeScenePath>/{_defaultName}.asset");
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
            AssetDatabase.ImportAsset(path);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<T>(path));
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
            var vertices = _src.vertices;
            _tar.vertices = vertices;
            _tar.normals = _src.normals;
            _tar.tangents = _src.tangents;
            _tar.name = _src.name;
            _tar.bounds = _src.bounds;
            _tar.bindposes = _src.bindposes;
            _tar.colors = _src.colors;
            _tar.boneWeights = _src.boneWeights;
            var uvs = new List<Vector4>();
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
