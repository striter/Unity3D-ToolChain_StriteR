using System;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public struct GSphere
    {
        public Vector3 center;
        [Clamp(0)] public float radius;
        public GSphere(Vector3 _center,float _radius) { center = _center;radius = _radius; }
        public static readonly GSphere kDefault = new GSphere(Vector3.zero, .5f);
    }

    [Serializable]
    public struct GEllipsoid
    {
        public Vector3 center;
        public Vector3 radius;
        public GEllipsoid(Vector3 _center,Vector3 _radius) {  center = _center; radius = _radius;}
        public static readonly GEllipsoid kDefault = new GEllipsoid(Vector3.zero, new Vector3(.5f,1f,0.5f));
    }

}