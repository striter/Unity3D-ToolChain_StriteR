using System.Linq;
using Runtime.Geometry;
using TPool;
using Procedural;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace TechToys.ThePlanet.Module
{
    public class ModuleQuad : PoolBehaviour<GridID>,IQuad
    {
        public GridID Identity => identity;
        public Quad<GridID> m_NearbyQuadCW { get; private set; }
        public TrapezoidQuad m_ShapeOS { get; private set; }
        public PCGQuad Quad { get; private set; }
        public ModuleQuad Init(PCGQuad _quad)
        {
            Quad = _quad;
            m_ShapeOS = Quad.m_ShapeOS;
            transform.localPosition = Quad.position;
            transform.localRotation = Quad.rotation;

            Quad<GridID> nearbyQuads = default;
            for (int i = 0; i < 4; i++)
            {
                var curVertex = Quad.m_Indexes[i];
                var nextVertex = Quad.m_Indexes[(i+1)%4];
                var quad = Quad.m_Vertices[i].m_NearbyQuads.Find(_p=>_p.m_Indexes.MatchVertexCount(Quad.m_Indexes)==2 && _p.m_Indexes.MatchVertex(curVertex) && _p.m_Indexes.MatchVertex(nextVertex));
                nearbyQuads[i] = quad.m_Identity;
            }
            m_NearbyQuadCW = nearbyQuads;
            return this;
        }
        public override void OnPoolRecycle()
        {
            Quad = null;
        }
    }
}