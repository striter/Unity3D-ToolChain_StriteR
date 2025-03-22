using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Scripting;

namespace Runtime.DataStructure
{

    public interface IBVHHelper<Boundary, Element> : IBoundaryTreeHelper<Boundary,Element> where Boundary : struct
    {
        public void SortElements(int _median,Boundary _boundary,IList<int> _elementIndexes, IList<Element> _elements);
    }
    public class BoundingVolumeHierarchy<Boundary, Element, Helper> : ABoundaryTree<Boundary, Element> 
        where Helper : class,IBVHHelper<Boundary, Element>,new() 
        where Boundary : struct
    {
        private static readonly Helper kHelper = new ();
        public BoundingVolumeHierarchy(int _nodeCapcity, int _maxIteration) : base(_nodeCapcity, _maxIteration) { }
        public void Construct(IList<Element> _elements) => Construct(kHelper.CalculateBoundary(_elements), _elements);

        private static List<Element> kElementHelper = new();
        protected override void Split(Node _parent, IList<Element> _elements, List<Node> _nodeList)
        {
            var last = _parent.elementsIndex.Count;
            var median = _parent.elementsIndex.Count / 2;
            kHelper.SortElements(median,_parent.boundary,_parent.elementsIndex, _elements);
            var newNode1 = Node.Spawn();
            newNode1.iteration = -1;
            for(var i=0;i<median;i++)
                newNode1.elementsIndex.Add(_parent.elementsIndex[i]);
            newNode1.boundary = kHelper.CalculateBoundary(newNode1.FillList(_elements,kElementHelper));
            _nodeList.Add(newNode1);
            
            var newNode2 = Node.Spawn();
            newNode2.iteration = -1; 
            for(var i=median;i<last;i++)
                newNode2.elementsIndex.Add(_parent.elementsIndex[i]);
            newNode2.boundary = kHelper.CalculateBoundary(newNode2.FillList(_elements,kElementHelper));
            _nodeList.Add(newNode2);
        }

    }
}

