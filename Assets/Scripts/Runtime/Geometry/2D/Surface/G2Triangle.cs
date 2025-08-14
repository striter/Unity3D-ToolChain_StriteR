﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct G2Triangle 
    {
        public Triangle<float2> triangle;

        [NonSerialized] public float2 baryCentre;
        [NonSerialized] public float2 uOffset;
        [NonSerialized] public float2 vOffset;

        public G2Triangle(float2 _vertex0, float2 _vertex1, float2 _vertex2)
        {
            this = default;
            triangle = new Triangle<float2>(_vertex0, _vertex1, _vertex2);
            Ctor();
        }

        public G2Triangle(IList<float2> _position, PTriangle _index)
        {
            this = default;
            triangle = new Triangle<float2>(_position[_index.V0], _position[_index.V1], _position[_index.V2]);
            Ctor();
        }
        void Ctor()
        {
            baryCentre = GetBaryCentre();
            uOffset = V1 - V0;
            vOffset = V2 - V0;
        }
        public float2 GetBaryCentre() => GetPoint(.25f);
        public float2 GetPoint(float2 _uv) => GetPoint(_uv.x, _uv.y);
        public float2 GetPoint(float _u,float _v) => V0 + _u * uOffset + _v * vOffset;
        public static implicit operator G2Polygon(G2Triangle _triangle) => new G2Polygon(_triangle.V0,_triangle.V1,_triangle.V2);
        public static readonly G2Triangle kDefault = new G2Triangle(new float2(0,1),new float2(-.5f,-1),new float2(.5f,-1));
    }

    [Serializable]
    public partial struct G2Triangle : IGeometry2, IConvex2, IArea2, ITriangle<float2>, ISerializationCallbackReceiver
    {
        public float2 V0 => triangle.v0;
        public float2 V1 => triangle.v1;
        public float2 V2 => triangle.v2;
        public float2 this[int _index] => triangle[_index];
        public IEnumerator<float2> GetEnumerator() => triangle.GetEnumerator();

        public IEnumerable<G2Line> GetEdges()
        {
            yield return new G2Line(V0, V1);
            yield return new G2Line(V1, V2);
            yield return new G2Line(V2, V0);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public float2 GetSupportPoint(float2 _direction) => this.MaxElement(_p => math.dot(_p, _direction));

        public bool Contains(float2 _position)
        {
            var AB = V1 - V0;
            var AC = V2 - V0;
            var AO = _position - V0;
            var ABperp = umath.tripleProduct(AC, AB, AB);
            var ACperp = umath.tripleProduct(AB, AC, AC);
            return math.dot(ABperp, AO) > 0 || math.dot(ACperp, AO) > 0;
        }

        public static G2Triangle operator +(G2Triangle _triangle, float2 _offset) =>
            new G2Triangle(_triangle.V0 + _offset, _triangle.V1 + _offset, _triangle.V2 + _offset);

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize() => Ctor();
        public float2 Origin => baryCentre;
        public void DrawGizmos() => UGizmos.DrawLinesConcat(this.Select(p => p.to3xz()));

        //0.5 * |x1(y2 - y3) + x2(y3 - y1) + x3(y1 - y2)|
        public float GetArea() => 0.5f * math.abs(V0.x * (V1.y - V2.y) + V1.x * (V2.y - V0.y) + V2.x * (V0.y - V1.y));

        public float GetCircumscribeRadius()
        {
            var ab = (V1 - V0).magnitude();
            var bc = (V2 - V1).magnitude();
            var ca = (V0 - V2).magnitude();
            var area = GetArea();
            if (area < float.Epsilon) return 0f;
            return (ab * bc * ca) / (4 * area);
        }

        public G2Triangle Resize(float _scaleNormalized)
        {
            var center = GetBaryCentre();
            return new G2Triangle(
                center + (V0 - center) * _scaleNormalized,
                center + (V1 - center) * _scaleNormalized,
                center + (V2 - center) * _scaleNormalized
            );
        }


        public static G2Triangle GetSuperTriangle(params float2[] _positions) //always includes,but not minimum
        {
            UGeometry.Minmax(_positions, out var min, out var max);
            var delta = (max - min);
            return new G2Triangle(
                new float2(min.x - delta.x, min.y - delta.y * 3f),
                new float2(min.x - delta.x, max.y + delta.y),
                new float2(max.x + delta.x * 3f, max.y + delta.y)
            );
        }

        public static G2Triangle GetCircumscribedTriangle(G2Circle _circle)
        {
            var r = _circle.radius;
            return new G2Triangle(
                _circle.center + new float2(0f, r / kmath.kSin30d),
                _circle.center + new float2(r * kmath.kTan60d, -r),
                _circle.center + new float2(-r * kmath.kTan60d, -r));
        }
    }
}