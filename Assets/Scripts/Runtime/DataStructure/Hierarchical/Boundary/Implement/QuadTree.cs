using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using Enumerable = System.Linq.Enumerable;

namespace Runtime.DataStructure
{
    public abstract class AQuadTree<Helper, Boundary, Element> : ATreeIncrement<Helper, Boundary, Element> 
        where Helper : struct, ITreeIncrementHelper<Boundary,Element>
        where Boundary : struct
    {
        public int m_QuadDivision { get; private set; }

        public AQuadTree(int _quadDivision)
        {
            m_QuadDivision = _quadDivision;
        }

    }

    public class QuadTree_float2 : AQuadTree<TreeNode_float2, G2Box, float2>
    {
        public QuadTree_float2(int _quadDivision) : base(_quadDivision)
        {
        }

        protected override IEnumerable<IList<float2>> Divide(G2Box _src, IList<float2> _elements,int _iteration)
        {
            var b = _src;
            var division = m_QuadDivision;
            var size = b.size / division;
            var min = b.min;
            
            for (var i = 0; i < division; i++)
            for (var j = 0; j < division; j++)
            {
                var rangeMin = min + size * new float2(i,j);
                var rangeMax = rangeMin + size;
                
                var newElements = new List<float2>();
                if (i == division - 1 && j == division - 1)
                {
                    newElements.AddRange(_elements);
                    yield return newElements;
                    break;
                }
                
                for (var index = _elements.Count - 1; index >= 0; index--)
                {
                    var p = _elements[index];
                    if(p.x > rangeMax.x || p.y > rangeMax.y)
                        continue;
                    
                    _elements.RemoveAt(index);
                    newElements.Add(p);
                }
                yield return newElements;
            }
        }
    }

    public class QuadTree_float3 : AQuadTree<TreeNode_float3, GBox, float3>
    {
        public QuadTree_float3(int _quadDivision) : base(_quadDivision)
        {
        }

        protected override IEnumerable<IList<float3>> Divide(GBox _src, IList<float3> _elements,int _iteration)
        {
            var division = m_QuadDivision;
            var cellSize = _src.size / division;
            var min = _src.min;
            for (var i = 0; i < division; i++)
            for (var j = 0; j < division; j++)
            for (var k = 0; k < division; k++)
            {
                var rangeMin = min + cellSize * new float3(i,j,k);
                var rangeMax = rangeMin + cellSize;
                
                var newElements = new List<float3>();
                if (i == division - 1 && j == division - 1 && k == division - 1)
                {
                    newElements.AddRange(_elements);
                    yield return newElements;
                    break;
                }
                
                for (var index = _elements.Count - 1; index >= 0; index--)
                {
                    var p = _elements[index];
                    if(p.x > rangeMax.x || p.y > rangeMax.y || p.z > rangeMax.z)
                        continue;
                    
                    _elements.RemoveAt(index);
                    newElements.Add(p);
                }
                yield return newElements;
            }
        }
    }

    public class QuadTree_triangle3 : AQuadTree<TreeHelper_Box_Triangle, GBox, GTriangle>
    {
        public QuadTree_triangle3(int _quadDivision) : base(_quadDivision)
        {
        }

        protected override IEnumerable<IList<GTriangle>> Divide(GBox _src, IList<GTriangle> _elements,int _iteration)
        {
            var division = m_QuadDivision;
            var size = _src.size / division;
            var min = _src.min;
            for (var i = 0; i < division; i++)
            for (var j = 0; j < division; j++)
            for (var k = 0; k < division; k++)
            {
                var rangeMin = min + size * new float3(i,j,k);
                var rangeMax = rangeMin + size;
                
                var newElements = new List<GTriangle>();
                
                if (i == division - 1 && j == division - 1 && k == division - 1)
                {
                    newElements.AddRange(_elements);
                    yield return newElements;
                    break;
                }
                
                for (var index = _elements.Count - 1; index >= 0; index--)
                {
                    var triangle = _elements[index];
                    if(triangle.Any(p=>p.x > rangeMax.x || p.y > rangeMax.y || p.z > rangeMax.z))
                        continue;
                    
                    _elements.RemoveAt(index);
                    newElements.Add(triangle);
                }
                yield return newElements;
            }
        }
    }
}