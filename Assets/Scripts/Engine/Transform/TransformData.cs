using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct TR
{
    public float3 position;
    public quaternion rotation;
    public static TR kDefault = new(){position = 0,rotation = quaternion.identity};

    public TR(float3 _position, quaternion _rotation)
    {
        position = _position;
        rotation = _rotation;
    }

    public TR(Transform _transform)
    {
        position = _transform.position;
        rotation = _transform.rotation;
    }
    
    public void SyncTransformWS(Transform _transform) => _transform.SetPositionAndRotation(position, rotation);
}

[Serializable]
public struct TRS
{
    public float3 position;
    public float3 rotation;
    public float3 scale;
    
    public static TRS kDefault = new TRS(){position = Vector3.zero,rotation = Vector3.zero,scale = Vector3.one};
    
    public float4x3_homogenous transformMatrix=>float4x3_homogenous.TRS(position,quaternion.EulerZXY(rotation),scale);
}