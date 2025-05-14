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
        public float3 position;
        public float3 velocity;
        public float3 force;
        public float mass;
        public class BVHHelper : IBVHHelper<GBox,ParticleData>
        {
            private static IList<ParticleData> kElements;
            Comparison<int> CompareX = (_a, _b) => kElements[_a].position.x >= kElements[_b].position.x ? 1 : -1;
            Comparison<int> CompareY = (_a, _b) => kElements[_a].position.y >= kElements[_b].position.y ? 1 : -1;
            public void SortElements(int _median, GBox _boundary,IList<int> _elementIndexes, IList<ParticleData> _elements)
            {
                kElements = _elements;
                switch (_boundary.size.maxAxis())
                {
                    case EAxis.X: _elementIndexes.Divide(_median,CompareX); break;
                    case EAxis.Y: _elementIndexes.Divide(_median,CompareY); break;
                }
            }

            public GBox CalculateBoundary(IList<ParticleData> _elements) => UGeometry.GetBoundingBox(_elements,p=>p.position);
        }
    }

    [Serializable]
    public struct FieldData
    {
        public float density;
        public float2 gradient;
        public static readonly FieldData kDefault = new();
    }
    
    public class ParticleBVH : BoundingVolumeHierarchy<GBox, ParticleData,ParticleData.BVHHelper>
    {
        public ParticleBVH(int _nodeCapcity, int _maxIteration) : base(_nodeCapcity, _maxIteration) { }
    }
    
    public struct ParticleDensityQuery  : IBoundaryTreeQuery<GBox,ParticleData>
    {
        public GSphere circle;
        public int index;

        public ParticleDensityQuery(ParticleData _data,float _radius)
        {
            circle = new GSphere(_data.position, _radius);
            index = _data.index;
        }

        public ParticleDensityQuery(float3 _origin, float _radius)
        {
            circle = new GSphere(_origin, _radius);
            index = -1;
        }
        public bool Query(GBox _boundary) => _boundary.Intersect(circle);
        public bool Query(int _index,ParticleData _element) => _index != index && circle.Intersect(_element.position);
    }
}