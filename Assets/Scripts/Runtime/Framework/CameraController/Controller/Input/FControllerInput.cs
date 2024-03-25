using Unity.Mathematics;
using UnityEngine;

namespace CameraController.Inputs
{
    public abstract class AControllerInput
    {
        public bool Available => Camera !=null && Camera.enabled  && Camera.gameObject.activeInHierarchy
                                 && Anchor != null && Anchor.gameObject.activeInHierarchy;
        //Persistent
        public abstract Camera Camera { get; }
        public abstract Transform Anchor { get; }
        public abstract Transform Target { get;}
        public abstract float Pitch { get; set; }
        public abstract float Yaw { get; set; }
        public abstract float Pinch { get; set; }
        
        //Final value
        public abstract float3 AnchorOffset { get;}
        public float3 InputEuler => new float3(Pitch,Yaw,0) + (this is IEulerOffset eulerMixin ? new float3(eulerMixin.OffsetPitch,eulerMixin.OffsetYaw,eulerMixin.OffsetRoll) : float3.zero);
        public float InputFOV  => (this is IFOVOffset fovMixin) ? fovMixin.OffsetFOV : 0;
        public float InputDistance => (this is IDistanceOffset distanceMixin) ? distanceMixin.OffsetDistance : 0;
        public float2 InputViewPort => (this is IViewportOffset fovMixin) ? new float2(fovMixin.OffsetViewPortX,fovMixin.OffsetViewPortY) : float2.zero;

        public virtual void ResetInput()
        {
            Pitch = 0;
            Yaw = 0;
            Pinch = 0;
            ClearOffset();
        }
        public void ClearOffset()
        {
            if(this is IControllerPlayerInput playerInput)
                playerInput.Clear();
            
            if (this is IEulerOffset euler)
            {
                euler.OffsetPitch = 0;
                euler.OffsetYaw = 0;
                euler.OffsetRoll = 0;
            }    
            
            if (this is IViewportOffset viewport)
            {
                viewport.OffsetViewPortX = 0;
                viewport.OffsetViewPortY = 0;
            }
            
            if (this is IDistanceOffset distance)
            {
                distance.OffsetDistance = 0;
            }
            
            if (this is IFOVOffset fov)
            {
                fov.OffsetFOV = 0;
            }
        }

    }

    public static class IControllerInput_Extension
    {
        public static void DrawGizmos(this AControllerInput _input)
        {
            if (!_input.Available) return;
            Gizmos.matrix = _input.Anchor.transform.localToWorldMatrix;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, Vector3.right);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, Vector3.up);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }
    }
}