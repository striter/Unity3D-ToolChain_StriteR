using Dome.Entity;
using Dome.Model;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dome.Collision
{
    public struct ECollisionCasterData
    {
        public EGeometryType type;
        [MFoldout(nameof(type),EGeometryType.Sphere)] public float radius;
        [MFoldout(nameof(type),EGeometryType.Box)] public float3 boxBounds;
    }

    public interface ICollisionCaster
    {
        
    }
    
    public static class ICollisionCaster_Extension
    {
        public static bool CastHit(this ICollisionCaster _this,ECollisionCasterData _castData, float3 _start, float3 _end, out RaycastHit _hitInfo,out int _hitEntity, int _layerMask = int.MaxValue)
        {
            _hitEntity = IEntity.kInvalidID;
            var success = false;
            var line = new GLine(_start,_end);
            switch (_castData.type)
            {
                default:
                {
                    success= Physics.Raycast((GRay)line,out _hitInfo,line.length,_layerMask);
                }
                    break;
                case EGeometryType.Sphere:
                {
                    success= Physics.SphereCast(line.start,_castData.radius,line.direction,out _hitInfo,line.length,_layerMask);
                }
                    break;
                case EGeometryType.Box:
                {
                    var rotation = (_this is IMove move) ? move.rotation : quaternion.identity;
                    success= Physics.BoxCast(line.start,_castData.boxBounds,line.direction,out _hitInfo,rotation,line.length,_layerMask);
                }
                    break;
            }

            if(success)
                success = ICollisionReceiver.kColliderIndexes.TryGetValue(_hitInfo.collider.GetInstanceID(), out _hitEntity);

            return success;
        }
    }
}