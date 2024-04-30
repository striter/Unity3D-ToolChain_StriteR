using System;
using CameraController.Inputs;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController
{
    [Serializable]
    public struct AnchoredControllerInput
    {
        [Header("Bounding Box Affection")]
        public bool useBoundingBox;

        [Header("Anchor")] 
        public float3 anchorOffset;
        public string childName;
        public bool useTransformPosition;     //In some case its bounding box goes pretty weird
        [MFoldout(nameof(useBoundingBox),true)] [Range(-.5f,.5f)] public float anchorY;
        
        [Header("Screen Space Centering")]
        [Range(-.5f, .5f)] public float viewportX;
        [Range(-.5f, .5f)] public float viewportY;
        [Header("Rotation")]
        public float pitch;
        public float yaw;
        public float roll;
        
        [Header("Distance")]
        [Range(5f, 90f)] public float fov;
        [MFoldout(nameof(useBoundingBox),true)]  public bool distanceAABBAdaption;
        public float distance;
        
        public static readonly AnchoredControllerInput kDefault = new()
        {
            pitch = 15,
            yaw = 180f,
            viewportX = 0,
            viewportY = 0,
            fov = 50f,
            anchorY = 0f,
            distance = 1f,
        };
        
        public AnchoredControllerParameters Evaluate(Transform _anchor)
        {
            var root = UController.CollectAnchor(_anchor, childName);
            var origin = (float3)root.position;
            var distance = this.distance;
            var finalAnchor = origin;
            if (useBoundingBox)
            {
                if (!UController.CollectBoundingBox(root, out var boundingBox))
                    boundingBox = new GBox(origin,1f);
                if(distanceAABBAdaption)
                    distance *= boundingBox.size.y;
                finalAnchor = boundingBox.GetPoint(kfloat3.up*anchorY);
            }

            if (useTransformPosition)
                finalAnchor.xz = ((float3)_anchor.position).xz;
            
            return new AnchoredControllerParameters()
            {
                anchor = finalAnchor + math.mul(_anchor.rotation, anchorOffset),
                euler = new float3(pitch,yaw,roll),
                fov = fov,
                viewport = new float2(viewportX,viewportY),
                distance = distance,
            };
        }
    }
    
    [Serializable]
    public struct AnchoredControllerParameters
    {
        public float3 anchor;
        public float3 euler;
        public float2 viewport;
        public float distance;
        public float fov;
        
        public static readonly AnchoredControllerParameters kDefault = default;

        public static AnchoredControllerParameters operator +(AnchoredControllerParameters _a, AnchoredControllerParameters _b) => new() {
            anchor = _a.anchor + _b.anchor,
            euler = _a.euler + _b.euler,
            viewport = _a.viewport + _b.viewport,
            distance = _a.distance + _b.distance,
            fov = _a.fov + _b.fov,
        };
        
        public static AnchoredControllerParameters operator -(AnchoredControllerParameters _a, AnchoredControllerParameters _b) => new() {
            anchor = _a.anchor - _b.anchor,
            euler = _a.euler - _b.euler,
            viewport = _a.viewport - _b.viewport,
            distance = _a.distance - _b.distance,
            fov = _a.fov - _b.fov,
        };
        
        public static AnchoredControllerParameters operator *(AnchoredControllerParameters _a, float _b) => new() {
            anchor = _a.anchor * _b,
            euler = _a.euler * _b,
            viewport = _a.viewport * _b,
            distance = _a.distance * _b,
            fov = _a.fov * _b,
        };
        
        public static AnchoredControllerParameters operator /(AnchoredControllerParameters _a, float _b) => new() {
            anchor = _a.anchor / _b,
            euler = _a.euler / _b,
            viewport = _a.viewport / _b,
            distance = _a.distance / _b,
            fov = _a.fov / _b,
        };
        
        public static AnchoredControllerParameters FormatDelta(AControllerInput _input) => new AnchoredControllerParameters()
        {
            anchor = math.mul(_input.Anchor.rotation,_input.InputAnchorOffset),
            euler = _input.InputEuler,
            distance = _input.InputDistance,
            viewport = _input.InputViewPort,
            fov = _input.InputFOV,
        };
        
        public static AnchoredControllerParameters Lerp( AnchoredControllerParameters _a, AnchoredControllerParameters _b, float _t)=>_a + (_b - _a) * _t;
    }
}