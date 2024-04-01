using System;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dome
{
    public enum EGeometryType
    {
        None,
        Sphere,
        Box,
    }
    
    [Serializable]
    public struct EntityExtentData
    {
        public EGeometryType type;
        [MFoldout(nameof(type),EGeometryType.Sphere)] public GSphere sphericalBounds;
        [MFoldout(nameof(type),EGeometryType.Box)] public GBox boxBounds;

        public static EntityExtentData Sphere(GSphere _sphere) => new() {type = EGeometryType.Sphere, sphericalBounds = _sphere};
        public static EntityExtentData Box(GBox _boxBounds) => new() {type = EGeometryType.Box, boxBounds = _boxBounds};

        public static readonly EntityExtentData kDefault = Box(new GBox(0,1));
    }
    
    public interface IExtents : IMove
    {
        public EntityExtentData kExtentData { get; }
    }

    public static class IExtends_Extension
    {
        public static GBox GetBoundingBox(this IExtents _entity)
        {
            switch (_entity.kExtentData.type)
            {
                default: return default;
                case EGeometryType.Box: return _entity.kExtentData.boxBounds + _entity.position;
                case EGeometryType.Sphere: {
                    var sphere = _entity.kExtentData.sphericalBounds + _entity.position;
                    return new GBox(sphere.center, sphere.radius);
                }
            }
        }
        
        public static bool RayIntersect(this IExtents _entity, GRay _ray)
        {
            var matrix = _entity.worldToLocalMatrix();      //Use Local Space
            var localRay = matrix * _ray;
            switch (_entity.kExtentData.type)
            {
                default: return false;
                case EGeometryType.Box: return localRay.Intersect(_entity.kExtentData.boxBounds);
                case EGeometryType.Sphere: return localRay.Intersect(_entity.kExtentData.sphericalBounds);
            }
        }

        
        public static void DrawGizmos(this IExtents _entity)
        {
            Gizmos.matrix = _entity.localToWorldMatrix();
            switch (_entity.kExtentData.type)
            {
                case EGeometryType.Box: (_entity.kExtentData.boxBounds).DrawGizmos(); break;
                case EGeometryType.Sphere: (_entity.kExtentData.sphericalBounds).DrawGizmos(); break;
            }
        }

    }
}