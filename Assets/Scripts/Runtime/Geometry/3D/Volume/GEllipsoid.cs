﻿using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GEllipsoid: IVolume , IRayVolumeIntersection , ISDF
    {
        public float3 center;
        public float3 radius;
        public GEllipsoid(float3 _center,float3 _radius) {  center = _center; radius = _radius;}
        public static readonly GEllipsoid kDefault = new GEllipsoid(float3.zero, new float3(.5f,1f,0.5f));
        public float3 Origin => center;
        public float3 GetSupportPoint(float3 _direction) => center + _direction.normalize() * radius;
        public float3 GetNormalizedPoint(float3 _uvw) => center + (_uvw-.5f) * radius;
        public GSphere GetBoundingSphere() => new GSphere(center, radius.maxElement());
        public float SDF(float3 _position)
        {
            var p = _position - center;
            var r = radius;
            var k0 = math.length(p/r);
            var k1 = math.length(p/(r*r));
            return k0*(k0-1.0f)/k1;
        }

        public GBox GetBoundingBox() => GBox.Minmax( center-radius, center+radius );


        public bool RayIntersection(GRay _ray, out float2 distances)
        {
            distances = -1;
            var shift = _ray.origin - center;
            var a = _ray.direction.x*_ray.direction.x/(radius.x*radius.x)
                     + _ray.direction.y*_ray.direction.y/(radius.y*radius.y)
                     + _ray.direction.z*_ray.direction.z/(radius.z*radius.z);
            var b = 2*shift.x*_ray.direction.x/(radius.x*radius.x)
                     + 2*shift.y*_ray.direction.y/(radius.y*radius.y)
                     + 2*shift.z*_ray.direction.z/(radius.z*radius.z);
            var c = shift.x*shift.x/(radius.x*radius.x)
                     + shift.y*shift.y/(radius.y*radius.y)
                     + shift.z*shift.z/(radius.z*radius.z) 
                     - 1;
            var discriminant = b*b-4*a*c;
            
            if ( discriminant < 0 ) 
                return false; 
            discriminant = math.sqrt(discriminant);
            var t0 = (-b - discriminant);
            var t1 = (-b + discriminant);
            if (t0 < 0)
                t0 = t1;
            distances =  new float2(t0, t1 - t0)/(2*a);
            return true;
        }
        public void DrawGizmos()
        {
            //Dude
            Matrix4x4 preMatrix = Gizmos.matrix;
            Gizmos.matrix = preMatrix * Matrix4x4.TRS(center,Quaternion.identity, radius);
            Gizmos.DrawWireSphere(Vector3.zero,1f);
            Gizmos.matrix = preMatrix;
        }
        
        public static GEllipsoid FromPoints(IEnumerable<float3> _positions)
        {
            var box = GBox.GetBoundingBox(_positions);
            var m = math.mul(float3x3.identity,box.size);
            var sphere = GSphere.GetBoundingSphere(_positions.Select(p=>m * p));
            return new GEllipsoid(sphere.center,sphere.radius*box.size);
        }
    }
}