using System;
using System.Collections.Generic;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public partial struct GConeCapped
    {
        public float3 origin;
        [PostNormalize] public float3 normal;
        [Range(0,90f)] public float angle;
        public float height;
        public GConeCapped(float3 _origin, float3 _normal, float _angle, float _height)
        {
            origin = _origin;
            normal =_normal;
            angle = _angle;
            height = _height;
        }
        
        public float Radius => ((GCone)this).GetRadius(height);
        public float3 Bottom => origin + normal * height;
        public float3 Origin => origin + normal * height/2;
    }

    public partial struct GConeCapped : IVolume , IRayIntersection , ISDF
    {
        public float3 GetSupportPoint(float3 _direction)
        {
            return math.dot(_direction,normal) > 0 ? new GDisk(Bottom, -normal, Radius).GetSupportPoint(_direction) : origin;
        }

        public GBox GetBoundingBox()        //https://iquilezles.org/articles/diskbbox/
        {
            var a = normal*height;
            var pa = origin;
            var pb = pa + a;
            var ra = 0; //GetRadius(startHeight)
            var rb = Radius;
            var e = math.sqrt( 1.0f - a*a/math.dot(a,a) );
            return GBox.Minmax( math.min( pa - e*ra, pb - e*rb ),
                                math.max( pa + e*ra, pb + e*rb ) );     
        }
        public GSphere GetBoundingSphere()
        {
            var origin = this.origin;
            var disk = new GDisk(Bottom, -normal, Radius);
            var randomPerpendicular = URandom.RandomPerpendicular(normal);
            IEnumerable<float3> GetBoundingPoints()
            {
                yield return origin;
                yield return disk.GetSupportPoint(randomPerpendicular);
                yield return disk.GetSupportPoint(-randomPerpendicular);
            }

            return UGeometry.GetBoundingSphere(GetBoundingPoints());
        }

        public float SDF(float3 _position)
        {
            var a = origin;
            var b = Bottom;
            var ra = 0;
            var rb = Radius;
            var p = _position;
            var rba  = rb-ra;
            var baba = math.dot(b-a,b-a);
            var papa = math.dot(p-a,p-a);
            var paba = math.dot(p-a,b-a)/baba;
            var x = math.sqrt( papa - paba*paba*baba );
            var cax = math.max(0.0f,x-((paba<0.5)?ra:rb));
            var cay = math.abs(paba-0.5f)-0.5f;
            var k = rba*rba + baba;
            var f = math.clamp( (rba*(x-ra)+paba*baba)/k, 0.0f, 1.0f );
            var cbx = x-ra - f*rba;
            var cby = paba - f;
            var s = (cbx<0.0f && cay<0.0f) ? -1.0f : 1.0f;
            return s*math.sqrt(math.min(cax*cax + cay*cay*baba,
                cbx*cbx + cby*cby*baba) );
        }

        public static readonly GConeCapped kDefault = new GConeCapped(){origin = kfloat3.up*.5f, normal = kfloat3.down, angle = 30f, height = 1f};
        public static implicit operator GCone(GConeCapped _coneCapped)=>new GCone(_coneCapped.origin,_coneCapped.normal,_coneCapped.angle);

        public bool RayIntersection(GRay _ray, out float distance)
        {
            distance = -1;
            var pa = origin;
            var pb = Bottom;
            var ra = 0;
            var rb = Radius;
            var ro = _ray.origin;
            var rd = _ray.direction;
            var  ba = pb - pa;
            var  oa = ro - pa;
            var  ob = ro - pb;
            float m0 = math.dot(ba,ba);
            float m1 = math.dot(oa,ba);
            float m2 = math.dot(rd,ba);
            float m3 = math.dot(rd,oa);
            float m5 = math.dot(oa,oa);
            float m9 = math.dot(ob,ba);

            var t = 0f;
            // caps
            if( m1<0.0 )
            {
                if ((oa * m2 - rd * m1).sqrmagnitude() < ra * ra * m2 * m2) // delayed division
                {
                    distance = -m1 / m2;
                    return true;
                }
            }
            else if( m9>0.0 )
            {
                t = -m9/m2;                     // NOT delayed division
                if ((ob + rd * t).sqrmagnitude() < (rb * rb))
                {
                    distance = t;
                    return true;
                }
            }
    
            // body
            var rr = ra - rb;
            var hy = m0 + rr*rr;
            var k2 = m0*m0    - m2*m2*hy;
            var k1 = m0*m0*m3 - m1*m2*hy + m0*ra*(rr*m2*1.0f        );
            var k0 = m0*m0*m5 - m1*m1*hy + m0*ra*(rr*m1*2.0f - m0*ra);
            var h = k1*k1 - k2*k0;
            if( h<0.0 ) 
                return false; //no intersection
            t = (-k1-math.sqrt(h))/k2;
            var y = m1 + t*m2;
            if( y<0.0 || y>m0 ) 
                return false; //no intersection
            distance = t;
            return true;
        }

        public void DrawGizmos() => UGizmos.DrawCone(origin, normal, Radius, height);

    }
    
}