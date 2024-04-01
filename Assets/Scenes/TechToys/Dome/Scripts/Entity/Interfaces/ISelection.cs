using System;
using Dome.Model;
using UnityEngine;
using Runtime.Geometry;

namespace Dome.Entity
{
    [Flags]
    public enum ESelections
    {
        Empty = 0, 
        Hover = 1,
        Select = 2,
    }
    public interface ISelection : IExtents
    {
        public new Transform transform { get; }
        public ESelections selectionFlags { get; set; }
        
        public GameObject hoveringObj { get; set; }
        public Animation hoveringAnimation { get; set; }
    }

    public static class ISelection_Extension
    {
        private static readonly string kAssetPath = FAssets.PrecacheAsset("Selection");

        
        public static void OnCreate(this ISelection _entity)
        {
            _entity.selectionFlags =  ESelections.Empty;
        }

        public static void ClearSelectionFlag(this ISelection _entity)
        {
            _entity.selectionFlags = ESelections.Empty;
            _entity.UpdateHoveringState();
            _entity.UpdateHighlightState();
        }
        public static void SetSelectionFlag(this ISelection _entity,ESelections _flag,bool _valid)
        {
            _entity.selectionFlags = _entity.selectionFlags.SetFlag(_flag, _valid);
            _entity.UpdateHoveringState();
            _entity.UpdateHighlightState();
        }

        static void UpdateHoveringState(this ISelection _entity)
        {
            var highlight = _entity.selectionFlags.IsFlagEnable(ESelections.Hover);
            if (!(_entity is IModel model)) return;
            if (highlight)
                model.SetLayer(KDomeLayers.kHighlight);
            else
                model.ResetLayer();
        }

        static void UpdateHighlightState(this ISelection _entity)
        {
            var hovering = _entity.selectionFlags.IsFlagEnable(ESelections.Select);
            if (hovering)
            {
                if (!_entity.hoveringObj)
                {
                    _entity.hoveringObj = FAssets.GetModel(kAssetPath);
                    _entity.hoveringAnimation = _entity.hoveringObj.GetComponent<Animation>();
                    _entity.hoveringObj.transform.localScale = Vector3.one*(_entity.GetBoundingBox().size.maxElement() * 2f);
                    _entity.Tick(0f);
                }
            }
            else
            {
                if (_entity.hoveringObj != null)
                {
                    FAssets.ClearModel(kAssetPath,_entity.hoveringObj);
                    _entity.hoveringObj = null;
                    _entity.hoveringAnimation = null;
                }
            }
        }
        

        public static void Tick(this ISelection _entity, float _deltaTime)
        {
            if (_entity.hoveringObj == null) return;
            _entity.hoveringObj.transform.position = _entity.transform.position + Vector3.up*0.1f;
        }
        
        public static void OnRecycle(this ISelection _entity)
        {
            _entity.ClearSelectionFlag();
        }
    }
}