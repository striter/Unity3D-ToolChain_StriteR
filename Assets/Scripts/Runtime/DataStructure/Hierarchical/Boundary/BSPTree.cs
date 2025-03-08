using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.DataStructure
{
    public class BSPTree : ABoundaryTree<G2Plane, float2>
    {
        protected override IEnumerable<(G2Plane, IList<float2>)> Split(int _iteration, G2Plane _boundary, IList<float2> _elements)
        {
            PCA2.Evaluate(_elements, out var centre, out var right, out var up);
            var _plane = new G2Plane(up, centre);
            var _front = new List<float2>();
            var _back = new List<float2>();
            foreach (var point in _elements)
            {
                if (_plane.IsPointFront(point))
                    _front.Add(point);
                else
                    _back.Add(point);
            }

            yield return (_plane , _front);
            yield return (_plane.Flip() , _back);
        }
        public void Construct(IList<float2> _elements, int _maxIteration, int _volumeCapacity) => Construct(_elements,G2Plane.kDefault, _maxIteration,_volumeCapacity);

    }
}