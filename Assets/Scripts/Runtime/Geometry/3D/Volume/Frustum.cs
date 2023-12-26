using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Geometry.Validation;

namespace Geometry
{
    [Serializable]
    public struct GFrustum
    {
        public float3 origin;
        public quaternion rotation;
        [Clamp(0)]public float fov;
        public float aspect;
        [Clamp(0)]public float zNear;
        [Clamp(0)]public float zFar;
        public GFrustum(Camera _camera)
        {
            origin = _camera.transform.position;
            rotation = _camera.transform.rotation;
            fov = _camera.fieldOfView;
            aspect = _camera.aspect;
            zNear = _camera.nearClipPlane;
            zFar = _camera.farClipPlane;
        }
        
        public GFrustum(float3 _origin, quaternion _rotation, float _fov, float _aspect, float _zNear, float _zFar)
        {
            origin = _origin;
            rotation = _rotation;
            fov = _fov;
            aspect = _aspect;
            zNear = _zNear;
            zFar = _zFar;
        }
        public GFrustumPlanes GetFrustumPlanes() => new GFrustumPlanes(origin,rotation ,fov,aspect,zNear,zFar);
        public GFrustumRays GetFrustumRays() => new GFrustumRays(origin,rotation ,fov,aspect,zNear,zFar);
    }
    
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
            left = new GPlane(math.mul(rotation, new float3(-aspectC, 0f, -s)), origin + math.mul(rotation, new Vector3(-s, 0f, aspectC).normalized * centerDistance));
            right = new GPlane(math.mul(rotation, new float3(aspectC, 0f, -s)), origin + math.mul(rotation, new Vector3(s, 0f, aspectC).normalized * centerDistance));
            top = new GPlane(math.mul(rotation, new float3(0f, c, -s)), origin + math.mul(rotation, new Vector3(0f, s, c).normalized * centerDistance));
            bottom = new GPlane(math.mul(rotation, new float3(0f, -c, -s)), origin + math.mul(rotation, new Vector3(0f, -s, c).normalized * centerDistance));
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
    
    public struct GFrustumRays:IEnumerable<GRay>
    {
        public GRay bottomLeft;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
        public GRay bottomRight;
        public GRay topRight;
        public GRay topLeft;
        public float farDistance;
        public GFrustumRays(float3 origin, quaternion rotation, float fov, float aspect, float zNear, float zFar)
        {
            var halfHeight = zNear * Mathf.Tan(fov * .5f * Mathf.Deg2Rad);
            var forward = math.mul(rotation , kfloat3.forward);
            var toRight = math.mul(rotation , kfloat3.right * halfHeight * aspect);
            var toTop = math.mul(rotation , kfloat3.up * halfHeight);

            var tl = forward * zNear + toTop - toRight;
            float scale = tl.magnitude() / zNear;
            tl = tl.normalize();
            tl *= scale;
            var tr = forward * zNear + toTop + toRight;
            tr = tr.normalize();
            tr *= scale;
            var bl = forward * zNear - toTop - toRight;
            bl = bl.normalize();
            bl *= scale;
            var br = forward * zNear - toTop + toRight;
            br = br.normalize();
            br *= scale;

            topLeft = new GRay(origin + tl * zNear, tl);
            topRight = new GRay(origin + tr * zNear, tr);
            bottomLeft = new GRay(origin + bl * zNear, bl);
            bottomRight = new GRay(origin + br * zNear, br);
            farDistance = zFar - zNear;
        }

        public GRay GetRay(float2 _viewportPoint)
        {
            return new GRay()
            {
                origin = umath.bilinearLerp(bottomLeft.origin, bottomRight.origin, topRight.origin, topLeft.origin,
                    _viewportPoint.x, _viewportPoint.y),
                direction = umath.bilinearLerp(bottomLeft.direction, bottomRight.direction, topRight.direction,
                    topLeft.direction, _viewportPoint)
            };

        }
        
        public IEnumerator<GRay> GetEnumerator()
        {
            yield return bottomLeft;
            yield return bottomRight;
            yield return topRight;
            yield return topLeft;
        }
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();

        public GFrustumPoints GetFrustumPoints()
        {
            var farBottomLeft = bottomLeft.GetPoint(farDistance);
            var farBottomRight = bottomRight.GetPoint(farDistance);
            var farTopRight = topRight.GetPoint(farDistance);
            var farTopLeft = topLeft.GetPoint(farDistance);
            return new GFrustumPoints()
            {
                nearBottomLeft = bottomLeft.origin,
                nearBottomRight = bottomRight.origin,
                nearTopRight = topRight.origin,
                nearTopLeft = topLeft.origin,
                farBottomLeft = farBottomLeft,
                farBottomRight = farBottomRight,
                farTopRight = farTopRight,
                farTopLeft = farTopLeft,
                bounding = UBounds.GetBoundingBox(new []{bottomLeft.origin,bottomRight.origin,topRight.origin,topLeft.origin,farBottomLeft,farBottomRight,farTopRight,farTopLeft}),
            };
        }
    }
    
    public struct GFrustumPoints : IEnumerable<float3>, IIterate<float3>
    {
        public float3 nearBottomLeft;
        public float3 nearBottomRight;
        public float3 nearTopRight;
        public float3 nearTopLeft;
        public float3 farBottomLeft;
        public float3 farBottomRight;
        public float3 farTopRight;
        public float3 farTopLeft;
        public GBox bounding;

        public int Length => 8;

        public float3 this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: throw new IndexOutOfRangeException();
                    case 0: return nearBottomLeft;
                    case 1: return nearBottomRight;
                    case 2: return nearTopRight;
                    case 3: return nearTopLeft;
                    case 4: return farBottomLeft;
                    case 5: return farBottomRight;
                    case 6: return farTopRight;
                    case 7: return farTopLeft;
                }
            }
        }

        public IEnumerator<float3> GetEnumerator()
        {
            yield return nearBottomLeft;
            yield return nearBottomRight;
            yield return nearTopRight;
            yield return nearTopLeft;
            yield return farBottomLeft;
            yield return farBottomRight;
            yield return farTopRight;
            yield return farTopLeft;
        }

        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
    }

}