﻿using System.Collections.Generic;
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
        protected override void Split(Node _parent, IList<float2> _elements, List<Node> _nodeList)
        {
            var coordinates = G2Coordinates.PrincipleComponentAnalysis(_parent.elementsIndex.Select(p=>_elements[p]));
            var plane = coordinates.RightPlane();
            var frontNode = Node.Spawn(_parent.iteration + 1,plane);
            var backNode = Node.Spawn(_parent.iteration + 1, plane.Flip());
            foreach (var elementIndex in _parent.elementsIndex)
            {
                if (plane.IsPointFront(_elements[elementIndex]))
                    frontNode.elementsIndex.Add(elementIndex);
                else
                    backNode.elementsIndex.Add(elementIndex);
            }

            _nodeList.Add(frontNode);
            _nodeList.Add(backNode);
        }
    }
}