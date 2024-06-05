using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController
{
    public static class UController
    {
        public static Transform CollectAnchor(Transform _root,string _name)
        {
            if (string.IsNullOrEmpty(_name))
                return _root;
            
            var root = _root.GetComponentsInChildren<Transform>().Find(p => p.name == _name);
            return root ? root : _root;
        }
        
        public static bool CollectBoundingBox(Transform anchor,out GBox _boundingBox,int mask = -1)
        {
            _boundingBox = default;
            if (mask == 0)
                return false;
            
            var renderers = anchor.GetComponentsInChildren<Renderer>(false);
            var min = kfloat3.max;
            var max = kfloat3.min;
            var valid = false;
            foreach (var renderer in renderers)
            {
                if ((mask & (1 << renderer.gameObject.layer)) == 0)
                    continue;
                    
                var bound = renderer.bounds;
                min = math.min(min, bound.min);
                max = math.max(max, bound.max);
                valid = true;
            }

            if (!valid)
                return false;

            _boundingBox = GBox.Minmax(min, max);
            
            return true;
        }

        public static float GetYaw(float3 forward) =>  math.atan2(forward.x, forward.z) * kmath.kRad2Deg;
        public static float GetPitch(float3 forward) => - math.asin(forward.y) * kmath.kRad2Deg;
    }
}