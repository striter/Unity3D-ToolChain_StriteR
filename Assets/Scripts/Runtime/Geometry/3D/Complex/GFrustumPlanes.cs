using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Geometry
{
    public struct GFrustumPlanes:IEnumerable<GPlane>,IIterate<GPlane> 
    {
        public GPlane left;
        public GPlane right;
        public GPlane top;
        public GPlane bottom;
        public GPlane near;
        public GPlane far;

        public GFrustumPlanes(float3 _origin, quaternion _rotation, float fov, float aspect, float zNear, float zFar)
        {
            var origin = _origin;
            var rotation = _rotation;
            var an = fov * .5f  * kmath.kDeg2Rad;
            var s = math.sin(an);
            var c = math.cos(an);
            var aspectC = c / aspect;
            var forward = math.mul( rotation, kfloat3.forward);
            
            var centerDistance = zNear + (zFar-zNear)/2f;
            left = new GPlane(math.mul(rotation, new float3(-aspectC, 0f, -s)), origin + math.mul(rotation, new float3(-s, 0f, aspectC).normalize() * centerDistance));
            right = new GPlane(math.mul(rotation, new float3(aspectC, 0f, -s)), origin + math.mul(rotation, new float3(s, 0f, aspectC).normalize() * centerDistance));
            top = new GPlane(math.mul(rotation, new float3(0f, c, -s)), origin + math.mul(rotation, new float3(0f, s, c).normalize() * centerDistance));
            bottom = new GPlane(math.mul(rotation, new float3(0f, -c, -s)), origin + math.mul(rotation, new float3(0f, -s, c).normalize() * centerDistance));
            near = new GPlane(-forward, -zNear);
            far = new GPlane(forward, zFar);
        }
        
        public IEnumerator<GPlane> GetEnumerator()
        {
            yield return bottom;
            yield return left;
            yield return top;
            yield return right;
            yield return near;
            yield return far;
        }
        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
        public int Length => 6;
        public GPlane this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: throw new IndexOutOfRangeException();
                    case 0: return bottom;
                    case 1: return left;
                    case 2: return top;
                    case 3: return right;
                    case 4: return near;
                    case 5: return far;
                }
            }
        }

    }
}