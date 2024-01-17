using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;

namespace Runtime.DataStructure
{
    public abstract class AQuadTree<Node, Boundary, Element> : ATreeIncrement<Node, Boundary, Element> 
        where Node : struct, ITreeNode<Boundary, Element>
        where Boundary : struct
        where Element : struct
    {
        public int m_QuadDivision { get; private set; }

        public AQuadTree(int _quadDivision)
        {
            m_QuadDivision = _quadDivision;
        }

    }

    public class QuadTree_float2 : AQuadTree<TreeNode_float2, G2Box, float2>
    {
        protected override IEnumerable<G2Box> Divide(G2Box _src, int _iteration)
        {
            var b = _src;
            var division = m_QuadDivision;
            var extent = b.extent / division;
            var size = b.size / division;
            var min = b.min;
            for (int i = 0; i < division; i++)
            for (int j = 0; j < division; j++)
                yield return new G2Box(min + size * new float2((.5f + i), (.5f + j)), extent);
        }

        public QuadTree_float2(int _quadDivision) : base(_quadDivision)
        {
        }
    }

    public class QuadTree_float3 : AQuadTree<TreeNode_float3, GBox, float3>
    {

        protected override IEnumerable<GBox> Divide(GBox _src, int _iteration)
        {
            var division = m_QuadDivision;
            var extent = _src.extent / division;
            var size = _src.size / division;
            var min = _src.min;
            for (var i = 0; i < division; i++)
            for (var j = 0; j < division; j++)
            for (var k = 0; k < division; k++)
                yield return new GBox(min + size * new float3((.5f + i), (.5f + j), (.5f + k)), extent);
        }

        public QuadTree_float3(int _quadDivision) : base(_quadDivision)
        {
        }
    }

    public class QuadTree_triangle3 : AQuadTree<TreeNode_triangle3, GBox, GTriangle>
    {
        protected override IEnumerable<GBox> Divide(GBox _src, int _iteration)
        {
            var division = m_QuadDivision;
            var extent = _src.extent / division;
            var size = _src.size / division;
            var min = _src.min;
            for (var i = 0; i < division; i++)
            for (var j = 0; j < division; j++)
            for (var k = 0; k < division; k++)
                yield return new GBox(min + size * new float3((.5f + i), (.5f + j), (.5f + k)), extent);
        }
        
        public QuadTree_triangle3(int _quadDivision) : base(_quadDivision)
        {
        }
    }
}