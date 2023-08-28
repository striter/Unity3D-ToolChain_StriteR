using System;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Dome
{
    public class FDomeCamera : ADomeController
    {
        private Camera m_Camera;

        [Readonly] public float3 position;
        [Readonly] public quaternion rotation;
        public override void OnInitialized()
        {
            m_Camera = transform.GetComponentInChildren<Camera>();
        }

        public void SetPositionRotation(float3 _position,quaternion _rotation)
        {
            position = _position;
            rotation = _rotation;
        }

        public override void Tick(float _deltaTime)
        {
            m_Camera.transform.SetPositionAndRotation(position,rotation);
        }

        public GRay ScreenPointToRay(float2 _position) => m_Camera.ScreenPointToRay(_position.to3xy());
        
        public override void Dispose()
        {
        }
        
    }

}