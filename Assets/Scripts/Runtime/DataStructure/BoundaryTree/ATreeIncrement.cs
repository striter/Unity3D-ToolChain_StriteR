using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;

namespace Runtime.DataStructure
{
    public interface ITreeIncrementHelper<Boundary, Element> 
    {
        public Boundary CalculateBoundary(IList<Element> _elements);
    }
    
    public abstract class ATreeIncrement<Helper, Boundary, Element> : ABoundaryTree<Boundary,Element>
        where Helper : struct, ITreeIncrementHelper<Boundary, Element>
        where Boundary : struct
    {
        protected static readonly Helper kHelper = default;
        protected abstract IEnumerable<IList<Element>> Divide(Boundary _src,IList<Element> _elements,int _iteration);
        protected override IEnumerable<(Boundary, IList<Element>)> Split(int _iteration, Boundary _boundary, IList<Element> _elements)
        {
            foreach (var newElements in Divide(_boundary,_elements,_iteration))
            {
                if(newElements.Count > 0)
                    yield return (kHelper.CalculateBoundary(newElements),newElements);
            }
        }

        public void Construct(IList<Element> _elements, int _maxIteration, int _volumeCapacity) => Construct( _elements.ToList(),kHelper.CalculateBoundary(_elements),_maxIteration,_volumeCapacity);
    }
}
