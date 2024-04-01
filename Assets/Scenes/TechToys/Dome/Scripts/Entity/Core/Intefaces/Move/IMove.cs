using System;
using Dome.Entity.AI;
using Unity.Mathematics;
using UnityEngine;

namespace Dome
{
    public interface IMove      //Using transform currently
    {
        public Transform transform { get; }
        public float3 position { get; set; }
        public quaternion rotation { get; set; }
    }

    public static class IMove_Extension
    {
        public static float4x4 localToWorldMatrix(this IMove _move) => _move.transform.localToWorldMatrix;
        public static float4x4 worldToLocalMatrix(this IMove _move) => _move.transform.worldToLocalMatrix;

        
        public static void OnInitialize(this IMove _move,EntityInitializeParameters _parameters) {
            _move.SetPositionAndRotation(_parameters.transformTR.position,_parameters.transformTR.rotation);
        }
        
        public static void SetPositionAndRotation(this IMove _move, float3 _position, quaternion _rotation)
        {
            _move.position = _position;
            _move.rotation = _rotation;
            _move.transform.SetPositionAndRotation(_move.position,_move.rotation);
        }
    }
}