using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;

namespace Runtime.DataStructure
{
    public class KDTree_float2 : ATreeIncrement<TreeNode_float2, G2Box, float2>
    {

        protected override IEnumerable<IList<float2>> Divide(G2Box _src, IList<float2> _elements, int _iteration)
        {
            var b = _src;
            var newElements1 = new List<float2>();
            var newElements2 = new List<float2>();
            var axis = _iteration % 2;
            foreach (var element in _elements)
            {
                if (element[axis] < b.center[axis])
                    newElements1.Add(element);
                else
                    newElements2.Add(element);
            }

            yield return newElements1;
            yield return newElements2;
        }
    }
    public class KDTree_float3 : ATreeIncrement<TreeNode_float3, GBox, float3>
    {
        protected override IEnumerable<IList<float3>> Divide(GBox _src, IList<float3> _elements, int _iteration)
        {
            var b = _src;
            var axis = _iteration % 3;
            var newElements1 = new List<float3>();
            var newElements2 = new List<float3>();
            foreach (var element in _elements)
            {
                if (element[axis] < b.center[axis])
                    newElements1.Add(element);
                else
                    newElements2.Add(element);
            }

            yield return newElements1;
            yield return newElements2;
            
        }
    }
}