using System;
using CameraController.Inputs;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController
{
    public interface ITransformHandle
    {
        public Transform transform { get; }
        public Transform Get(string _name);
    }

    public struct FTransformHandleDefault : ITransformHandle
    {
        public Transform transform { get; }
        public FTransformHandleDefault(Transform _anchor) => transform = _anchor;
        public Transform Get(string _name) => UController.CollectAnchor(transform, _name);
        public static implicit operator FTransformHandleDefault(Transform _anchor) => new FTransformHandleDefault(_anchor);
    }
    
    [Serializable]
    public struct AnchoredControllerInput
    {
        [Header("Bounding Box Affection")]
        public bool useBoundingBox;

        [Header("Anchor")] 
        public float3 anchorOffset;
        public bool anchorBeforeRotation;
        public string childName;
        public bool useTransformPosition;     //In some case its bounding box goes pretty weird
        [Foldout(nameof(useBoundingBox),true)] [Range(-.5f,.5f)] public float anchorY;
        
        [Header("Screen Space Centering")]
        [Range(-.5f, .5f)] public float viewportX;
        [Range(-.5f, .5f)] public float viewportY;
        [Header("Rotation")]
        public float pitch;
        public float yaw;
        public float roll;
        
        [Header("Distance")]
        [Range(5f, 90f)] public float fov;
        [Foldout(nameof(useBoundingBox),true)]  public bool distanceAABBAdaption;
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

        public FCameraControllerOutput Evaluate(ITransformHandle _anchor)
        {
            var root = _anchor.Get(childName);
            var anchor = _anchor.transform;
            var origin = (float3)root.position;
            var finalDistance = this.distance;
            var finalAnchor = origin;
            if (useBoundingBox)
            {
                if (!UController.CollectBoundingBox(root, out var boundingBox))
                    boundingBox = new GBox(origin,1f);
                if(distanceAABBAdaption)
                    finalDistance *= boundingBox.size.y;
                finalAnchor = boundingBox.GetCenteredPoint(kfloat3.up*anchorY);
            }

            if (useTransformPosition)
                finalAnchor.xz = ((float3)anchor.position).xz;
            
            return new FCameraControllerOutput()
            {
                anchor = finalAnchor + (anchorBeforeRotation ? anchorOffset : math.mul(anchor.rotation, anchorOffset)),
                euler = new float3(pitch,yaw,roll),
                fov = fov,
                viewport = new float2(viewportX,viewportY),
                distance = finalDistance,
            };
        }
    }
}