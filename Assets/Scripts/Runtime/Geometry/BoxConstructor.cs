using Unity.Mathematics;

namespace Geometry
{
    public static class BoxConstructor
    {
        public static GBox GetBoundingBox(this GSphere _sphere)
        {
            return GBox.Minmax(_sphere.center + kfloat3.leftDownBack*_sphere.radius,_sphere.center + kfloat3.rightUpForward*_sphere.radius);
        }
    }
    
    public partial struct GBox
    {
        public GBox Move(float3 _deltaPosition)=> new GBox(center + _deltaPosition, extent);
        
        public static GBox Minmax(float3 _min, float3 _max)
        {
            float3 size = _max - _min;
            float3 extend = size / 2;
            return new GBox(_min+extend,extend);
        }
        
        public GBox Split(float3 _anchor, float3 _sizeRatio)
        {
            var min = this.min + size * _anchor;
            return Minmax(min, min + size * _sizeRatio);
        }
        public GBox Encapsulate(GBox _other) => Minmax(math.min(min, _other.min), math.max(max, _other.max));
    }
}