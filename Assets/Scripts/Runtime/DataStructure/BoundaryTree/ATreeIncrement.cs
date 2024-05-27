using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;

namespace Runtime.DataStructure
{
    public interface ITreeIncrementHelper<Boundary, Element> 
    {
        public bool Contains(Boundary _bounds, Element _element);
        public Boundary CalculateBoundary(IList<Element> _elements);
    }
    
    public abstract class ATreeIncrement<Helper, Boundary, Element> : ABoundaryTree<Boundary,Element>
        where Helper : struct, ITreeIncrementHelper<Boundary, Element>
        where Boundary : struct
    {
        protected static readonly Helper kHelper = default;
        protected abstract IEnumerable<Boundary> Divide(Boundary _src, int _iteration);
        protected override IEnumerable<(Boundary, IList<Element>)> Split(int _iteration, Boundary _boundary, IList<Element> _elements)
        {
            if (_iteration == 1)
                _boundary = kHelper.CalculateBoundary(_elements);
            
            foreach (var boundary in Divide(_boundary, _iteration))
            {
                var newElements = _elements.Collect(p => kHelper.Contains(boundary, p)).ToList();
                if(newElements.Count > 0)
                    yield return (boundary,newElements);
            }
        }
    }
}
