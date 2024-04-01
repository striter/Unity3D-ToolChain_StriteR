using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Dome.Entity;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Model
{
    public interface IModel
    {
        public string modelPath { get; set; }
        
        public GameObject modelRoot { get; set; }
        public MeshRenderer[] meshRenderers { get; set; }
        public Material[] restoreMaterials { get; set; }
        public Dictionary<string,Transform> modelNodes { get; set; }
    }

    public static class IModel_Extension
    {
        //Running parameters
        public static bool isModelAvailable(this IModel _this) => _this.modelRoot;
        public static void SetModel(this IModel _this, string _modelPath,Transform _transform = null)
        {
            if(_this.isModelAvailable()) _this.ClearModel();
            
            _this.modelPath = _modelPath;
            _this.modelRoot = FAssets.GetModel(_this.modelPath, _transform);
            if (!_this.isModelAvailable()) return;
            
            _this.meshRenderers = _this.modelRoot.GetComponentsInChildren<MeshRenderer>();
            _this.modelNodes = new Dictionary<string, Transform>();
            if(_this is IFireEvent _event) _event.FireEvents("OnModelSet",_this);
        }
        
        public static void ClearModel(this IModel _this)
        {
            if (!_this.isModelAvailable()) return;
            _this.ResetLayer();
            _this.ResetMaterial();
            FAssets.ClearModel(_this.modelPath, _this.modelRoot);
            _this.modelPath = null;
            _this.modelRoot = null;
            _this.meshRenderers = null;
            _this.restoreMaterials = null;
            if(_this is IFireEvent _event) _event.FireEvents("OnModelClear");
        }

        public static void SetOverrideMaterial(this IModel _this,Material _material)
        {
            if (!_this.isModelAvailable()) return;
            _this.restoreMaterials = _this.meshRenderers.CollectAs(p => p.material).ToArray();
            foreach (var renderer in _this.meshRenderers)
                renderer.material = _material;
        }

        public static void ResetMaterial(this IModel _this)
        {
            if (!_this.isModelAvailable()) return;
            if (_this.restoreMaterials == null) return;
            foreach (var (index,renderer) in _this.meshRenderers.LoopIndex())
                renderer.material = _this.restoreMaterials[index];
            _this.restoreMaterials = null;
        }
        public static void SetLayer(this IModel _this, int _layer)
        {
            if (!_this.isModelAvailable()) return;
            _this.meshRenderers.Traversal(p=>p.gameObject.layer = _layer);
        }

        public static Transform GetNode(this IModel _this , string _name)
        {
            if (!_this.isModelAvailable()) return null;
            
            if (_this.modelNodes.TryGetValue(_name, out var transform))
                return transform;

            var search = _this.modelRoot.transform.FindInAllChild(p => p.name == _name);
            if (!search)
            {
                Debug.LogWarning($"Invalid Path:{_name} Found From Root:{_this.modelPath}");
                search = _this.modelRoot.transform;
            }
            _this.modelNodes.Add(_name,search);
            return search;
        }
        
        public static void ResetLayer(this IModel _model)
        {
            _model.SetLayer(KDomeLayers.kEntities);
        }
        
        public static void SetModelPositionRotation(this IModel _model,float3 _position, quaternion _rotation)
        {
            if (!_model.isModelAvailable()) return;
            _model.modelRoot.transform.SetLocalPositionAndRotation(_position,_rotation);
        }
        
        //Life cycle
        public static void OnInitialize(this IModel _model,EntityInitializeParameters _parameters)
        {
            if (string.IsNullOrEmpty(_parameters.defines.modelPath)) return;
            _model.modelPath = _parameters.defines.modelPath;
        }

        public static void OnCreate(this IModel _model)
        {
            if (_model.modelPath == null) return;
            Transform attach = _model is IMove move?move.transform:null;
            _model.SetModel(_model.modelPath,attach);
        }

        public static void OnRecycle(this IModel _model)
        {
            ClearModel(_model);
        }
    }
}