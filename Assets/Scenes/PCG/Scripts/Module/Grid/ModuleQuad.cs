using System.Linq;
using Geometry;
using TPool;
using Procedural;
using UnityEngine;

namespace PCG.Module
{
    public class ModuleQuad : PoolBehaviour<GridID>,IQuad
    {
        public GridID Identity => m_PoolID;
        public Transform Transform => transform;
        public Quad<GridID> m_NearbyQuadCW { get; private set; }
        public TrapezoidQuad m_ShapeOS { get; private set; }
        public Quad<float> m_EdgeNormalsCW { get; private set; }
        public Quad<float> m_EdgeDirectionsCW { get; private set; }
        public PCGQuad Quad { get; private set; }
        public ModuleQuad Init(PCGQuad _pcgQuad)
        {
            Quad = _pcgQuad;
            transform.localPosition = Quad.position;
            transform.localRotation = Quad.rotation;

            Quad<float> edgeOrientations = default;
            Quad<GridID> nearbyQuads = default;
            for (int i = 0; i < 4; i++)
            {
                var curVertex = Quad.m_Indexes[i];
                var nextVertex = Quad.m_Indexes[(i+1)%4];
                var quad = Quad.m_Vertices[i].m_NearbyQuads.Find(_p=>_p.m_Indexes.MatchVertexCount(Quad.m_Indexes)==2 && _p.m_Indexes.MatchVertex(curVertex) && _p.m_Indexes.MatchVertex(nextVertex));
                nearbyQuads[i] = quad.m_Identity;
                var edgeDirectionOS = Quad.position - quad.position;
                edgeOrientations[i] = KMath.kRad2Deg * UMath.GetRadClockWise(Vector2.up, edgeDirectionOS);
            }
            m_NearbyQuadCW = nearbyQuads;
            m_EdgeDirectionsCW = edgeOrientations;
            
            m_ShapeOS = Quad.m_ShapeOS;
            Quad<float> centerOrientations = default;
            for (int orientation = 0; orientation < 4; orientation++)
            {
                var orientedNormalOS = (m_ShapeOS.positions[(orientation+3)%4] + m_ShapeOS.positions[orientation%4])/2;
                centerOrientations[orientation] = KMath.kRad2Deg*UMath.GetRadClockWise(Vector2.up, orientedNormalOS);     //Forward To Edge
            }
            m_EdgeNormalsCW = centerOrientations;
            return this;
        }
        public override void OnPoolRecycle()
        {
            Quad = null;
        }
    }
}