using System.Linq;
using Dome.Model;
using UnityEngine;

namespace Dome.Entity
{
    public interface ICaster : IModel,IEntity
    {
        public Transform[] castNodes { get; set; }
        public int castIndex { get; set; }
    }

    public static class ICaster_Extension
    {
        
        public static void OnModelSet(this ICaster _this,IModel _)
        {
            _this.castIndex = 0;
            _this.castNodes = _this.modelRoot.transform.IterateInAllChild(p => p.name.Contains("FX_Cast")).ToArray();
        }

        public static void OnModelClear(this ICaster _this)
        {
            _this.castNodes = null;
        }

        public static Transform GetCasterTransform(this ICaster _caster)
        {
            if (!_caster.isModelAvailable()) return _caster.transform;
            
            _caster.castIndex++;
            return _caster.castNodes.Length>0?_caster.castNodes[_caster.castIndex % _caster.castNodes.Length] : _caster.transform;
        }
    }
}