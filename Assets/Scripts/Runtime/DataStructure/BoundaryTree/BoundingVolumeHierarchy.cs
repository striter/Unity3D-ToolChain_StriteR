using System.Collections.Generic;
using System.Linq.Extensions;

namespace Runtime.DataStructure
{
    public interface IBVHHelper<Boundary, Element> where Boundary : struct
    {
        public void SortElements(int _median,Boundary _boundary, IList<Element> _elements);
        public Boundary CalculateBoundary(IList<Element> _elements);
    }

    public class BoundingVolumeHierarchy<Helper, Boundary, Element> : ABoundaryTree<Boundary,Element> where Helper : struct, IBVHHelper<Boundary, Element> where Boundary : struct
    {
        private static readonly Helper kHelper = default;
        protected override IEnumerable<(Boundary, IList<Element>)> Split(int _iteration, Boundary _boundary, IList<Element> _elements)
        {
            var last = _elements.Count;
            var median = _elements.Count / 2;
            kHelper.SortElements(median,_boundary, _elements);
            var elements1 = new List<Element>(_elements.Iterate(0, median));
            yield return (kHelper.CalculateBoundary(elements1), elements1);
            
            var elements2 = new List<Element>(_elements.Iterate(median, last));
            yield return (kHelper.CalculateBoundary(elements2),elements2);
        }
        public void Construct(IList<Element> _elements, int _maxIteration, int _volumeCapacity) => Construct(_elements,kHelper.CalculateBoundary(_elements),_maxIteration,_volumeCapacity);
    }
}

