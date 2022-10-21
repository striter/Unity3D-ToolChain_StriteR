using System;
using System.Collections.Generic;
using Geometry.Voxel;
using TPool;
using Procedural;
using Procedural.Hexagon;
using TPoolStatic;
using UnityEngine;

namespace PCG.Module
{
    using static PCGDefines<int>;
    public class ModuleVertex : PoolBehaviour<SurfaceID>,IVertex
    {
        public PolyVertex m_Vertex { get; private set; }
        public readonly List<Coord> m_NearbyVertexPositionsLS = new List<Coord>(6);
        public readonly List<Coord> m_NearbyVertexSurfaceDirectionLS = new List<Coord>(6);
        public ModuleVertex Init(PolyVertex _vertex)
        {
            m_Vertex = _vertex;
            transform.localPosition = _vertex.m_Coord.ToPosition();
            m_NearbyVertexPositionsLS.Clear();
            m_NearbyVertexSurfaceDirectionLS.Clear();

            TSPoolList<PolyVertex>.Spawn(out var adjacentVertices);
            TSPoolList<PolyVertex>.Spawn(out var intervalVertices);
            m_Vertex.IterateNearbyVertices().FillList(adjacentVertices);
            m_Vertex.IterateIntervalVertices().FillList(intervalVertices);
            
            var count = adjacentVertices.Count;
            for (int i = 0; i < count; i++)
            {
                var center = m_Vertex.m_Coord;

                var curVertex = adjacentVertices[i].m_Coord - center;
                var preInterval = intervalVertices[(i + count - 1) % count].m_Coord;
                var curInterval = intervalVertices[i].m_Coord;
                var direction= Coord.Normalize(curInterval-preInterval);
                var position =   curVertex/2;
                m_NearbyVertexSurfaceDirectionLS.Add(direction);
                m_NearbyVertexPositionsLS.Add(position);
            }
            TSPoolList<PolyVertex>.Recycle(adjacentVertices);
            TSPoolList<PolyVertex>.Recycle(intervalVertices);
            return this;
        }
        
        public override void OnPoolRecycle()
        {
            m_Vertex = null;
        }

        public SurfaceID Identity => m_PoolID;
        public Transform Transform => transform;
        public PolyVertex VertexData => m_Vertex;
        public List<Coord> NearbyVertexPositionLS => m_NearbyVertexPositionsLS;
        public List<Coord> NearbyVertexSurfaceDirectionLS => m_NearbyVertexSurfaceDirectionLS;
    }
}