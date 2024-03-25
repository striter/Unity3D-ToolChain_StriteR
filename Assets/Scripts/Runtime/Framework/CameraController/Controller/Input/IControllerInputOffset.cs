using Unity.Mathematics;

namespace CameraController.Inputs
{
    public interface IFOVOffset
    {
        public float OffsetFOV { get; set; }
    }
    public interface IDistanceOffset
    {
        public float OffsetDistance { get; set; }
    }

    public interface IEulerOffset
    {
        public float OffsetPitch { get; set; }
        public float OffsetYaw { get; set; }
        public float OffsetRoll { get; set; }
    }
    
    public interface IViewportOffset
    {
        public float OffsetViewPortX { get; set; }
        public float OffsetViewPortY { get; set; }
    }
}