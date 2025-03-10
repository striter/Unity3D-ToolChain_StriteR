using System.Collections.Generic;

namespace Runtime.DataStructure
{
    public interface IBoundaryTreeHelper<Boundary, Element> where Boundary : struct
    {
        public Boundary CalculateBoundary(IEnumerable<Element> _elements);
    }
}