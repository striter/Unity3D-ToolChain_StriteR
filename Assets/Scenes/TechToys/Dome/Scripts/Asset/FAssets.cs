using System.Collections.Generic;
using TPool;
using UnityEngine;

namespace Dome
{
    //Part of the factory
    public static class FAssets
    {
        private static readonly Dictionary<string, Object> kAssets = new Dictionary<string, Object>();

        private static readonly Transform kPrefabsRoot = new GameObject("ModelPool").transform;
        private static Dictionary<int, int> kModelPoolIndexer = new Dictionary<int, int>();
        private static readonly Dictionary<string, ObjectPoolGameObject> kModelPool = new Dictionary<string, ObjectPoolGameObject>();

        static void PrecacheAsset(string _path, Object _asset)
        {
            kAssets.Add(_path,_asset);

            var prefabAsset = (GameObject) _asset;
            if (prefabAsset == null) return;
            if (kModelPool.ContainsKey(_path)) return;
            kModelPool.Add(_path,new ObjectPoolGameObject(((GameObject)Object.Instantiate(_asset,kPrefabsRoot)).transform));
        }
        
        public static void PrecachePrefabsAtPath(string _path)
        {
            var prefabs = Resources.LoadAll(_path);
            foreach (var prefab in prefabs) {
                var path = $"{_path}/{prefab.name}";
                PrecacheAsset(path, prefab);
            }
        }

        public static string PrecacheAsset(string _path)
        {
            var asset = Resources.Load(_path);
            if (!asset) {
                Debug.LogWarning($"Asset: {_path} Found Null");
                return default;
            }
            PrecacheAsset(_path,asset);
            return _path;
        }
        
        public static void Dispose()
        {
            foreach (var asset in kAssets.Values)
            {
                if(asset is GameObject)
                    continue;
                Resources.UnloadAsset(asset);
            }
            
            kAssets.Clear();
            foreach (var pool in kModelPool.Values)
                pool.Dispose();
            kModelPool.Clear();
        }

        public static GameObject GetModel(string _path,Transform _root = null)
        {
            if (!kModelPool.TryGetValue(_path, out var modelPool))
            {
                Debug.LogWarning($"Model: {_path} Found Null");
                return null;
            }
            var poolHandler = modelPool.Spawn(out var _model);
            kModelPoolIndexer.Add(_model.GetInstanceID(),poolHandler);
            if(_root!=null) _model.transform.SetParent(_root);
            _model.transform.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);
            return _model;
        }

        public static bool ClearModel(string _path, GameObject _model)
        {
            if (!kModelPool.TryGetValue(_path, out var modelPool))
            {
                Debug.LogWarning($"Model: {_path} Found Null");
                return false;
            }

            var id = _model.GetInstanceID();
            if (kModelPoolIndexer.ContainsKey(id))
            {
                modelPool.Recycle(kModelPoolIndexer[_model.GetInstanceID()]);
                kModelPoolIndexer.Remove(id);
                return true;
            }
            Debug.LogWarning($"Trying to remove an model without register {id}");
            return false;
        }
    }
}