using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.DataStructure
{
    public class BSPTree : ABoundaryTree<G2Plane, float2>
    {
        public BSPTree(int _nodeCapcity, int _maxIteration) : base(_nodeCapcity, _maxIteration) { }
        public void Construct(IList<float2> _elements) => Construct(G2Plane.kDefault,_elements);
        protected override IEnumerable<Node> Split(Node _parent, IList<float2> _elements)
        {
            PCA2.Evaluate(_parent.elementsIndex.Select(p=>_elements[p]), out var centre, out var right, out var up);
            var plane = new G2Plane(up, centre);
            var frontNode = Node.Spawn(_parent.iteration + 1,plane);
            var backNode = Node.Spawn(_parent.iteration + 1, plane.Flip());
            foreach (var elementIndex in _parent.elementsIndex)
            {
                if (plane.IsPointFront(_elements[elementIndex]))
                    frontNode.elementsIndex.Add(elementIndex);
                else
                    backNode.elementsIndex.Add(elementIndex);
            }

            yield return frontNode;
            yield return backNode;
        }
    }
}