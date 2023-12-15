using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{

    public partial struct G2Quad
    {
        public Quad<float2> quad;
        [NonSerialized] public float2 center;
        public G2Quad(Quad<float2> _quad) { 
            quad = _quad;
            center = default;
            Ctor();
        }
        void Ctor()
        {
            center = quad.Average();
        }
        public G2Quad(float2 _index0, float2 _index1, float2 _index2, float2 _index3):this(new Quad<float2>(_index0,_index1,_index2,_index3)){}
        public static readonly G2Quad kDefaultUV = new G2Quad(new float2(0,0),new float2(0,1),new float2(1,1),new float2(1,0));
    }

    [Serializable]
    public partial struct G2Quad : IQuad<float2>, IEnumerable<float2>, IIterate<float2>, I2Shape, ISerializationCallbackReceiver
    {
        public static implicit operator G2Quad(Quad<float2> _src) => new G2Quad(_src);
        
        public int Length => 4;
        
        public IEnumerator<float2> GetEnumerator() => quad.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

        public float2 this[int _index] => quad[_index];
        public float2 this[EQuadCorner _corner] => quad[_corner];

        public float2 B => quad.B;
        public float2 L => quad.L;
        public float2 F => quad.F;
        public float2 R => quad.R;

        public float2 GetSupportPoint(float2 _direction) => quad.Max(p => math.dot(_direction, p));
        public float2 Center => center;
        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();
    }
}