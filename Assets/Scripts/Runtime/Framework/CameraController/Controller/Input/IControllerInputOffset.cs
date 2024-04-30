using Unity.Mathematics;

namespace CameraController.Inputs
{
    public interface IAnchorOffset
    {
        public float3 OffsetAnchor { get;}
    }
    
    public interface IFOVOffset
    {
        public float OffsetFOV { get; set; }

        public void Clear()
        {
            OffsetFOV = 0;
        }
    }
    public interface IDistanceOffset
    {
        public float OffsetDistance { get; set; }
        public void Clear()
        {
            OffsetDistance = 0;
        }
    }

    public interface IEulerOffset
    {
        public float OffsetPitch { get; set; }
        public float OffsetYaw { get; set; }
        public float OffsetRoll { get; set; }
        public void Clear()
        {
            OffsetPitch = 0;
            OffsetYaw = 0;
            OffsetRoll = 0;
        }
    }
    
    public interface IViewportOffset
    {
        public float OffsetViewPortX { get; set; }
        public float OffsetViewPortY { get; set; }
        public void Clear()
        {
            OffsetViewPortX = 0;
            OffsetViewPortY = 0;
        }
    }

    public static partial class APlayerInputExtensions
    {
    }
}