using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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
        
        public GFrustum(float3 _position, quaternion _rotation, float _fov, float _aspect, float _zNear, float _zFar)
        {
            origin = _position;
            rotation = _rotation;
            fov = _fov;
            aspect = _aspect;
            zNear = _zNear;
            zFar = _zFar;
        }
        public GFrustumPlanes GetFrustumPlanes()
        {
            var origin = (Vector3)this.origin;
            var rotation = (Quaternion)this.rotation;
            float an = fov * .5f  * kmath.kDeg2Rad;
            float s = Mathf.Sin(an);
            float c = Mathf.Cos(an);
            float aspectC = c / aspect;
            Vector3 forward = rotation*Vector3.forward;
            
            float centerDistance = zNear + (zFar-zNear)/2f;
            return new GFrustumPlanes
            {
                left = new GPlane( rotation*new Vector3(-aspectC , 0f,-s  ), origin + rotation*new Vector3(-s,0f,aspectC).normalized*centerDistance),
                right = new GPlane( rotation*new Vector3(aspectC, 0f, -s ), origin + rotation*new Vector3(s,0f,aspectC).normalized*centerDistance),
                top = new GPlane( rotation*new Vector3(0f, c, -s), origin+rotation*new Vector3(0f,s,c).normalized*centerDistance),
                bottom = new GPlane( rotation*new Vector3(0f, -c, -s), origin+rotation*new Vector3(0f,-s,c).normalized*centerDistance),
                near = new GPlane(-forward, -zNear),
                far = new GPlane(forward, zFar),
            };
        }
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

        public GFrustumRays(Vector3 origin, Quaternion rotation, float fov, float aspect, float zNear, float zFar)
        {
            float halfHeight = zNear * Mathf.Tan(fov * .5f * Mathf.Deg2Rad);
            Vector3 forward = rotation*Vector3.forward;
            Vector3 toRight = rotation*Vector3.right * halfHeight * aspect;
            Vector3 toTop = rotation*Vector3.up * halfHeight ;

            Vector3 tl = forward * zNear + toTop - toRight;
            float scale = tl.magnitude / zNear;
            tl.Normalize();
            tl *= scale;
            Vector3 tr = forward * zNear + toTop + toRight;
            tr.Normalize();
            tr *= scale;
            Vector3 bl = forward * zNear - toTop - toRight;
            bl.Normalize();
            bl *= scale;
            Vector3 br = forward * zNear - toTop + toRight;
            br.Normalize();
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
                bounding = PointSet.UBounds.GetBoundingBox(new []{bottomLeft.origin,bottomRight.origin,topRight.origin,topLeft.origin,farBottomLeft,farBottomRight,farTopRight,farTopLeft}),
            };
        }
    }
    
    public struct GFrustumPoints : IEnumerable<Vector3>, IIterate<Vector3>
    {
        public Vector3 nearBottomLeft;
        public Vector3 nearBottomRight;
        public Vector3 nearTopRight;
        public Vector3 nearTopLeft;
        public Vector3 farBottomLeft;
        public Vector3 farBottomRight;
        public Vector3 farTopRight;
        public Vector3 farTopLeft;
        public GBox bounding;

        public int Length => 8;

        public Vector3 this[int _index]
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

        public IEnumerator<Vector3> GetEnumerator()
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