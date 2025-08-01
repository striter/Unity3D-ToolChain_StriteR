using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{
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
        public void DrawGizmos()
        {
            UGizmos.DrawLinesConcat(nearBottomLeft,nearBottomRight,nearTopRight,nearTopLeft);
            UGizmos.DrawLine(farBottomLeft,nearBottomLeft);
            UGizmos.DrawLine(farBottomRight,nearBottomRight);
            UGizmos.DrawLine(farTopLeft,nearTopLeft);
            UGizmos.DrawLine(farTopRight,nearTopRight);
            UGizmos. DrawLinesConcat(farBottomLeft,farBottomRight,farTopRight,farTopLeft);
        }
    }
}