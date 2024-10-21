using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension.BoundingSphere;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        static void Minmax(IEnumerable<float3> _positions,out float3 _min,out float3 _max)
        {
            _min = float.MaxValue;
            _max = float.MinValue;
            foreach (var position in _positions)
            {
                _min = math.min(position, _min);
                _max = math.max(position, _max);
            }
        }
        
        public static GBox GetBoundingBox(IEnumerable<float3> _positions)
        {
            Minmax(_positions,out var min,out var max);
            return GBox.Minmax(min,max);
        }
        
        public static GBox GetBoundingBoxOriented(float3 _right,float3 _up,float3 _forward,IEnumerable<float3> _points)
        {
            float3 min = float.MaxValue;
            float3 max = float.MinValue;
            foreach (var point in _points)
            {
                var cur = new float3(math.dot(point,_right),math.dot(point,_up),math.dot(point,_forward));
                min = math.min(cur, min);
                max = math.max(cur, max);
            }

            return GBox.Minmax(min, max);
        }

        public static GBox GetBoundingBox(IEnumerable<GBox> _boxes)
        {
            var min = kfloat3.max;
            var max = kfloat3.min;
            foreach (var box in _boxes)
            {
                min = math.min(box.min, min);
                max = math.max(box.max, max);
            }
            return GBox.Minmax(min, max);
        }
        
        public static GSphere GetBoundingSphere(IEnumerable<GSphere> _spheres)
        {
            var boundingSphere = _spheres.First();
            foreach (var sphere in _spheres)
                boundingSphere = GSphere.Minmax(boundingSphere, sphere);
            return boundingSphere;
        }

        public static GSphere GetBoundingSphere(IEnumerable<float3> _positions) => EPOS._3D.Evaluate(_positions,EPOS._3D.EMode.EPOS26,Welzl<GSphere,float3>.Evaluate);
        public static GEllipsoid GetBoundingEllipsoid(IEnumerable<float3> _positions)
        {
            var box = GetBoundingBox(_positions);
            var m = math.mul(float3x3.identity,box.size);
            var sphere = GetBoundingSphere(_positions.Select(p=>m * p));
            return new GEllipsoid(sphere.center,sphere.radius*2*box.size);
        }
        
        public static GSphere GetSuperSphere(params float3[] _points) => GetSuperSphere(_points.AsEnumerable());
        public static GSphere GetSuperSphere(IEnumerable<float3> _points) => GetBoundingBox(_points).GetBoundingSphere();
        
    }
}