using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;

namespace Runtime.DataStructure
{
    public class KDTree_float2 : ATreeIncrement<TreeNode_float2, G2Box, float2>
    {
        protected override IEnumerable<G2Box> Divide(G2Box _src, int _iteration)
        {
            var b = _src;
            var extent = b.size / 2;
            var min = b.min;

            if (_iteration % 2 == 1)
            {
                var halfExtent = b.extent * new float2(1, 0.5f);
                yield return new G2Box(min + extent * new float2(1f, .5f), halfExtent);
                yield return new G2Box(min + extent * new float2(1f, 1.5f), halfExtent);
            }
            else
            {
                var halfExtent = b.extent * new float2(0.5f, 1f);
                yield return new G2Box(min + extent * new float2(.5f, 1f), halfExtent);
                yield return new G2Box(min + extent * new float2(1.5f, 1f), halfExtent);
            }
        }
    }
    public class KDTree_float3 : ATreeIncrement<TreeNode_float3, GBox, float3>
    {
        protected override IEnumerable<GBox> Divide(GBox _src, int _iteration)
        {
            var b = _src;
            var extent = b.size / 2;
            var min = b.min;

            var axis = _iteration % 3;
            if (axis == 0)
            {
                var halfExtent = b.extent * new float3(0.5f, 1f,1f);
                yield return new GBox(min + extent * new float3(.5f, 1f,1f), halfExtent);
                yield return new GBox(min + extent * new float3(1.5f, 1f,1f), halfExtent);
            }
            else if(axis == 1)
            {
                var halfExtent = b.extent * new float3(1, 0.5f,1f);
                yield return new GBox(min + extent * new float3(1f, .5f,1f), halfExtent);
                yield return new GBox(min + extent * new float3(1f, 1.5f,1f), halfExtent);
            }
            else
            {
                var halfExtent = b.extent * new float3(1f,1f,0.5f);
                yield return new GBox(min + extent * new float3(1f, 1f,.5f), halfExtent);
                yield return new GBox(min + extent * new float3(1f, 1f,1.5f), halfExtent);
            }
        }
    }
}