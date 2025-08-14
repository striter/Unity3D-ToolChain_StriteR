using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{

    [Serializable]
    public partial struct G2Box : IConvex2 , IArea2,IRayArea2Intersection,ISerializationCallbackReceiver
    {
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => Ctor();

        public bool Contains(float2 _point,float _tolerence = float.Epsilon)
        {
            var absOffset = math.abs(center-_point) - _tolerence;
            return absOffset.x < extent.x && absOffset.y < extent.y;
        }

      public float2 Clamp(float2 _point) => math.clamp(_point, min, max);
        public bool Clamp(float2 _point, out float2 _clamped)
        {
            _clamped = Clamp(_point);
            return !Contains(_point);
        }
        
        public float2 GetUV(float2 _pos) => (_pos - min) / size;
        public float2 GetPoint(float2 _uv) => min + _uv * size;
        public float2 GetPoint(float _u, float _v) => GetPoint(new float2(_u, _v));

        public float2 GetSupportPoint(float2 _direction) => this.MaxElement(_p => math.dot(_direction, _p));
        public float GetArea() => extent.x * extent.y;

        public float2 Origin => center;

        public GBox To3XZ() => new(center.to3xz(),extent.to3xz());
        public GBox To3XY() => new(center.to3xy(),extent.to3xy());

        public IEnumerator<float2> GetEnumerator()
        {
            yield return GetPoint(new float2(0, 0));
            yield return GetPoint(new float2(1, 0));
            yield return GetPoint(new float2(1, 1));
            yield return GetPoint(new float2(0, 1));
        }

        public IEnumerable<G2Plane> GetClipPlanes()
        {
            yield return new G2Plane(kfloat2.left,GetPoint(1f,.5f));
            yield return new G2Plane(kfloat2.up,GetPoint(.5f,0f));
            yield return new G2Plane(kfloat2.right,GetPoint(0f,.5f));
            yield return new G2Plane(kfloat2.down,GetPoint(.5f,1f));
        }
        
        public IEnumerable<G2Line> GetEdges()
        {
            yield return new G2Line(GetPoint(new float2(0, 0)), GetPoint(new float2(1, 0)));
            yield return new G2Line(GetPoint(new float2(1, 0)), GetPoint(new float2(1, 1)));
            yield return new G2Line(GetPoint(new float2(1, 1)), GetPoint(new float2(0, 1)));
            yield return new G2Line(GetPoint(new float2(0, 1)), GetPoint(new float2(0, 0)));
        }

        public static G2Box operator +(G2Box _src, float2 _dst) => new G2Box(_src.center+_dst,_src.extent);
        public static G2Box operator -(G2Box _src, float2 _dst) => new G2Box(_src.center-_dst,_src.extent);
        public static G2Box operator /(G2Box _bounds,float2 _div) => new G2Box(_bounds.center/_div,_bounds.extent/_div);
        public static G2Box operator *(G2Box _bounds,float2 _div) => new G2Box(_bounds.center*_div,_bounds.extent*_div);
        public static G2Box operator /(G2Box _src,G2Box _dst) => new G2Box(_src.center / _dst.center, _src.extent / _dst.extent);
        public static G2Box operator *(G2Box _src,G2Box _dst) => new G2Box(_src.center * _dst.center, _src.extent * _dst.extent);
        
        public float4 ToTexelSize() => new( size.x, size.y,min.x, min.y);
        
        public static bool operator ==(G2Box _a, G2Box _b) => ((float4)_a == _b).all();
        public static bool operator !=(G2Box _a, G2Box _b) => ((float4)_a != _b).any();
        
        public bool Equals(G2Box other) => center.Equals(other.center) && extent.Equals(other.extent) && size.Equals(other.size) && min.Equals(other.min) && max.Equals(other.max);

        public override bool Equals(object obj) => obj is G2Box other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(center, extent, size, min, max);
        
        public override string ToString() => $"G2Box {center} {extent}";
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void DrawGizmos() => Gizmos.DrawWireCube(center.to3xz(),size.to3xz());
        public void DrawGizmosXY() => Gizmos.DrawWireCube(center.to3xy(), size.to3xy());
        
        public bool RayIntersection(G2Ray _ray, out float2 distances)
        {
            distances = -1;
            var invRayDir = 1f/(_ray.direction);
            var t0 = (min - _ray.origin)*(invRayDir);
            var t1 = (max - _ray.origin)*(invRayDir);
            var tmin = math.min(t0, t1);
            var tmax = math.max(t0, t1);
            if (tmin.maxElement() > tmax.minElement())
                return false;
            
            var dstA = math.max(tmin.x, tmin.y);
            var dstB = math.min(tmax.x, tmax.y);
            var dstToBox = math.max(0, dstA);
            var dstInsideBox = math.max(0, dstB - dstToBox);
            distances = new float2(dstToBox, dstInsideBox);
            return true;
        }
        
        //https://www.skytopia.com/project/articles/compsci/clipping.html -Liang Barskey
        public bool Clip(G2Line _line,out G2Line _clippedLine)
        {
            _clippedLine = G2Line.kZero;
            var x0src = _line.start.x; var y0src = _line.start.y; var x1src = _line.end.x; var y1src = _line.end.y;
            var edgeLeft = min.x; var edgeRight = max.x; var edgeBottom = min.y; var edgeTop = max.y;
            var t0 = 0f;    var t1 = 1f;
            var xdelta = x1src-x0src;
            var ydelta = y1src-y0src;
            float p,q,r;

            for(int edge=0; edge<4; edge++) {   // Traverse through left, right, bottom, top edges.
                switch(edge) {
                    default:
                    case 0: p = -xdelta;    q = -(edgeLeft-x0src);  break;
                    case 1: p = xdelta;     q =  (edgeRight-x0src); break;
                    case 2: p = -ydelta;    q = -(edgeBottom-y0src);break;
                    case 3: p = ydelta;     q =  (edgeTop-y0src);   break;
                }
                r = q/p;
                
                if(p==0 && q<0) 
                    return false;   // Don't draw line at all. (parallel line outside)

                if(p<0) {
                    if(r>t1) return false;         // Don't draw line at all.
                    else if(r>t0) t0=r;            // Line is clipped!
                } else if(p>0) {
                    if(r<t0) return false;      // Don't draw line at all.
                    else if(r<t1) t1=r;         // Line is clipped!
                }
            }

            var x0clip = x0src + t0*xdelta;
            var y0clip = y0src + t0*ydelta;
            var x1clip = x0src + t1*xdelta;
            var y1clip = y0src + t1*ydelta;

            _clippedLine = new G2Line(new float2(x0clip,y0clip),new float2(x1clip,y1clip));
            return true;        // (clipped) line is drawn
        }

    }

}