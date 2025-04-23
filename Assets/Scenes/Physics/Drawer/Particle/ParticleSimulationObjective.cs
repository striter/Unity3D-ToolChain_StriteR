using System;
using System.Collections.Generic;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Examples.PhysicsScenes.Particle
{
    public struct ParticleData
    {
        [NonSerialized] public int index;
        public float2 position;
        public float2 velocity;
        public float2 force;
        public float mass;
        public class BVHHelper : IBVHHelper<G2Box,ParticleData>
        {
            private static IList<ParticleData> kElements;
            Comparison<int> CompareX = (_a, _b) => kElements[_a].position.x >= kElements[_b].position.x ? 1 : -1;
            Comparison<int> CompareY = (_a, _b) => kElements[_a].position.y >= kElements[_b].position.y ? 1 : -1;
            public void SortElements(int _median, G2Box _boundary,IList<int> _elementIndexes, IList<ParticleData> _elements)
            {
                kElements = _elements;
                switch (_boundary.size.maxAxis())
                {
                    case EAxis.X: _elementIndexes.Divide(_median,CompareX); break;
                    case EAxis.Y: _elementIndexes.Divide(_median,CompareY); break;
                }
            }

            public G2Box CalculateBoundary(IList<ParticleData> _elements) => UGeometry.GetBoundingBox(_elements,p=>p.position);
        }
    }

    [Serializable]
    public struct FieldData
    {
        public float density;
        public float2 gradient;
        public static readonly FieldData kDefault = new();
    }
    
    public class ParticleBVH : BoundingVolumeHierarchy<G2Box, ParticleData,ParticleData.BVHHelper>
    {
        public ParticleBVH(int _nodeCapcity, int _maxIteration) : base(_nodeCapcity, _maxIteration) { }
        public override void DrawGizmos(IList<ParticleData> _elements, bool _parentMode = false)
        {
            // base.DrawGizmos(_elements, _parentMode);
            foreach (var leaf in GetLeafs())
                leaf.boundary.DrawGizmosXY();
        }
    }
    
    public struct ParticleDensityQuery  : IBoundaryTreeQuery<G2Box,ParticleData>
    {
        public G2Circle circle;
        public bool Query(G2Box _boundary) => _boundary.Intersect(circle);
        public bool Query(ParticleData _element) => circle.Intersect(_element.position);
    }
}