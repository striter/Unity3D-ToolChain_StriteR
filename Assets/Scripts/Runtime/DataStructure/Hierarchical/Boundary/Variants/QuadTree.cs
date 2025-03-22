using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;

namespace Runtime.DataStructure
{
    public interface IQuadTreeHelper<Boundary,Element> : IBoundaryTreeHelper<Boundary,Element> where Boundary : struct
    {
        public bool QuadTreeValidate(Boundary _boundary, Element _element);
    }

    public class QuadTree2<Element,Helper> : ABoundaryTree<G2Box, Element> 
        where Helper :IQuadTreeHelper<G2Box,Element>
    {
        private static readonly Helper kHelper = default;
        public int m_Division {get; private set;} = 2;
        public QuadTree2(int _nodeCapcity, int __maxIteration,int _division) : base(_nodeCapcity,__maxIteration) { m_Division = _division; }
        public void Construct(IList<Element> _elements) => Construct(kHelper.CalculateBoundary(_elements),_elements);
        protected override void Split(Node _parent, IList<Element> _elements, List<Node> _nodeList)
        {
            var b = _parent.boundary;
            var division = m_Division;
            var size = b.size / division;
            var min = b.min;
            var iteration = _parent.iteration + 1;
            var parentElementIndexes = _parent.elementsIndex;
            
            for (var i = 0; i < division; i++)
            for (var j = 0; j < division; j++)
            {
                var rangeMin = min + size * new float2(i,j);
                var rangeMax = rangeMin + size;

                var newNode = Node.Spawn(iteration,G2Box.Minmax(rangeMin,rangeMax));
                if (i == division - 1 && j == division - 1)
                {
                    newNode.elementsIndex.AddRange(parentElementIndexes);
                    _nodeList.Add(newNode);
                    break;
                }
                
                for (var k = parentElementIndexes.Count - 1; k >= 0; k--)
                {
                    var elementIndex = parentElementIndexes[k];
                    var p =  _elements[elementIndex];
                    if(!kHelper.QuadTreeValidate(newNode.boundary,p))
                        continue;
                    
                    parentElementIndexes.RemoveAt(k);
                    newNode.elementsIndex.Add(elementIndex);
                }
                _nodeList.Add(newNode);
            }
        }
    }

    public class QuadTree3<Element ,Helper> : ABoundaryTree<GBox, Element>
        where Helper :IQuadTreeHelper<GBox,Element>
    {
        private static readonly Helper kHelper = default;
        public int m_Division {get; private set;} = 2;
        public QuadTree3(int _nodeCapcity, int __maxIteration,int _division) : base(_nodeCapcity,__maxIteration) { m_Division = _division; }
        public void Construct(IList<Element> _elements) => Construct(kHelper.CalculateBoundary(_elements),_elements);
        protected override void Split(Node _parent, IList<Element> _elements, List<Node> _nodeList)
        {
            var b = _parent.boundary;
            var division = m_Division;
            var cellSize = b.size / division;
            var min = b.min;
            var iteration = _parent.iteration + 1;
            var parentElementIndexes = _parent.elementsIndex;

            for (var i = 0; i < division; i++)
            for (var j = 0; j < division; j++)
            for (var k = 0; k < division; k++)
            {
                var rangeMin = min + cellSize * new float3(i, j, k);
                var rangeMax = rangeMin + cellSize;

                var newNode = Node.Spawn(iteration, GBox.Minmax(rangeMin, rangeMax));
                if (i == division - 1 && j == division - 1 && k == division - 1)
                {
                    newNode.elementsIndex.AddRange(parentElementIndexes);
                    _nodeList.Add(newNode);
                    break;
                }

                for (var w = parentElementIndexes.Count - 1; w >= 0; w--)
                {
                    var elementIndex = parentElementIndexes[w];
                    var p =  _elements[elementIndex];
                    if(!kHelper.QuadTreeValidate(newNode.boundary,p))
                        continue;

                    parentElementIndexes.RemoveAt(w);
                    newNode.elementsIndex.Add(elementIndex);
                }

                _nodeList.Add(newNode);
            }
        }

    }
}