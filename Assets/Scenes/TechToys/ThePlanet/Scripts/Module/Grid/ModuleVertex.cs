using System;
using System.Collections.Generic;
using TPool;
using Procedural;
using Procedural.Hexagon;
using TObjectPool;
using UnityEngine;

namespace TechToys.ThePlanet.Module
{
    public class ModuleVertex : PoolBehaviour<GridID>,IVertex
    {
        public PCGVertex m_Vertex { get; private set; }
        public readonly List<Vector3> m_NearbyVertexPositionsLS = new List<Vector3>(6);
        public readonly List<Vector3> m_NearbyVertexSurfaceDirectionLS = new List<Vector3>(6);
        public ModuleVertex Init(PCGVertex _vertex)
        {
            m_Vertex = _vertex;
            transform.localPosition = _vertex.m_Position;
            m_NearbyVertexPositionsLS.Clear();
            m_NearbyVertexSurfaceDirectionLS.Clear();

            TSPoolList<PCGVertex>.Spawn(out var adjacentVertices);
            TSPoolList<PCGVertex>.Spawn(out var intervalVertices);
            m_Vertex.IterateNearbyVertices().FillList(adjacentVertices);
            m_Vertex.IterateIntervalVertices().FillList(intervalVertices);
            
            var count = adjacentVertices.Count;
            for (int i = 0; i < count; i++)
            {
                var center = m_Vertex.m_Position;

                var curVertex = adjacentVertices[i].m_Position - center;
                var preInterval = intervalVertices[(i + count - 1) % count].m_Position;
                var curInterval = intervalVertices[i].m_Position;
                var direction= Vector3.Normalize(curInterval-preInterval);
                var position =   curVertex/2;
                m_NearbyVertexSurfaceDirectionLS.Add(direction);
                m_NearbyVertexPositionsLS.Add(position);
            }
            TSPoolList<PCGVertex>.Recycle(adjacentVertices);
            TSPoolList<PCGVertex>.Recycle(intervalVertices);
            return this;
        }
        
        public override void OnPoolRecycle()
        {
            m_Vertex = null;
        }

        public GridID Identity => m_PoolID;
        public Transform Transform => transform;
        public PCGVertex Vertex => m_Vertex;
        public List<Vector3> NearbyVertexPositionLS => m_NearbyVertexPositionsLS;
        public List<Vector3> NearbyVertexSurfaceDirectionLS => m_NearbyVertexSurfaceDirectionLS;
    }
}