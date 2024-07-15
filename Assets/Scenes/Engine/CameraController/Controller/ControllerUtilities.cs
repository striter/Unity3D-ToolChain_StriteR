using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.CameraController
{
    public static class UController
    {
        private static List<Transform> kTransformHelper = new List<Transform>();
        private static List<Renderer> kRendererHelper = new List<Renderer>();
        public static Transform CollectAnchor(Transform _root,string _name)
        {
            if (string.IsNullOrEmpty(_name))
                return _root;
            _root.GetComponentsInChildren(false,kTransformHelper);
            for (var i = 0; i < kTransformHelper.Count; i++)
            {
                var transform = kTransformHelper[i];
                if (string.Equals(transform.gameObject.name, _name))
                    return transform;
            }
            return _root;
        }
        
        public static bool CollectBoundingBox(Transform anchor,out GBox _boundingBox,int mask = -1)
        {
            _boundingBox = default;
            if (mask == 0)
                return false;
            
            anchor.GetComponentsInChildren(false,kRendererHelper);
            var min = kfloat3.max;
            var max = kfloat3.min;
            var valid = false;
            foreach (var renderer in kRendererHelper)
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