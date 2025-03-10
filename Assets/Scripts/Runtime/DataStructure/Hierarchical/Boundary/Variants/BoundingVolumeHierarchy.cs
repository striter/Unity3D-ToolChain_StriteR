using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;

namespace Runtime.DataStructure
{

    public interface IBVHHelper<Boundary, Element> : IBoundaryTreeHelper<Boundary,Element> where Boundary : struct
    {
        public void SortElements(int _median,Boundary _boundary,IList<int> _elementIndexes, IList<Element> _elements);
    }
    public class BoundingVolumeHierarchy<Boundary, Element, Helper> : ABoundaryTree<Boundary, Element> where Helper : struct, IBVHHelper<Boundary, Element> where Boundary : struct
    {
        private static readonly Helper kHelper = default;
        public BoundingVolumeHierarchy(int _nodeCapcity, int _maxIteration) : base(_nodeCapcity, _maxIteration) { }
        public void Construct(IList<Element> _elements) => Construct(kHelper.CalculateBoundary(_elements), _elements);
        
        protected override IEnumerable<Node> Split(Node _parent, IList<Element> _elements)
        {
            var last = _parent.elementsIndex.Count;
            var median = _parent.elementsIndex.Count / 2;
            kHelper.SortElements(median,_parent.boundary,_parent.elementsIndex, _elements);
            var newNode1 = Node.Spawn();
            newNode1.iteration = -1;
            _parent.elementsIndex.Iterate(0, median).FillList(newNode1.elementsIndex);
            newNode1.boundary = kHelper.CalculateBoundary(newNode1.elementsIndex.Select(p=>_elements[p]));
            yield return newNode1;
            
            var newNode2 = Node.Spawn();
            newNode2.iteration = -1; 
            _parent.elementsIndex.Iterate(median, last).FillList(newNode2.elementsIndex);
            newNode2.boundary = kHelper.CalculateBoundary(newNode2.elementsIndex.Select(p=>_elements[p]));
            yield return newNode2;
        }

    }
}

