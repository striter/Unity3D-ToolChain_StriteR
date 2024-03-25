using Unity.Mathematics;
using UnityEngine;

namespace CameraController
{
    public struct FCameraOutput
    {
        public float3 position;
        public quaternion rotation;
        public float fov;

        public FCameraOutput(Camera _camera)
        {
            position = _camera.transform.position;
            rotation = _camera.transform.rotation;
            fov = _camera.fieldOfView;
        }
        
        public void Apply(Camera _camera)
        {
            _camera.transform.SetPositionAndRotation( position,rotation);
            _camera.fieldOfView = fov;
        }
    }
}