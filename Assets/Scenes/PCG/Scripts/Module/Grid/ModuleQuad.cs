using System.Linq;
using Geometry;
using TPool;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace PCG.Module
{
    using static PCGDefines<int>;
    public class ModuleQuad : PoolBehaviour<SurfaceID>,IQuad
    {
        public SurfaceID Identity => m_PoolID;
        public Transform Transform => transform;
        public Quad<SurfaceID> m_NearbyQuadCW { get; private set; }
        public Quad<Coord> m_ShapeOS { get; private set; }
        public Quad<float> m_EdgeNormalsCW { get; private set; }
        public Quad<float> m_EdgeDirectionsCW { get; private set; }
        public Quad<Coord>[] m_QubeQuads { get; } = new Quad<Coord>[4];
        public Quad<float>[] m_QubeQuadsOrientation { get; }= new Quad<float>[4];
        public PolyQuad m_Quad { get; private set; }
        public ModuleQuad Init(PolyQuad _quad)
        {
            m_Quad = _quad;
            transform.localPosition = m_Quad.m_CenterWS.ToPosition();
            transform.localRotation = Quaternion.Euler(0, m_Quad.m_Orientation, 0);

            var worldToLocal = URotation.Rotate2D(-UMath.kDeg2Rad * m_Quad.m_Orientation, true);
            Quad<float> edgeOrientations = default;
            Quad<SurfaceID> nearbyQuads = default;
            for (int i = 0; i < 4; i++)
            {
                var curVertex = m_Quad.m_Hex[i];
                var nextVertex = m_Quad.m_Hex[(i+1)%4];
                var quad = m_Quad.m_Vertices[i].m_NearbyQuads.Find(_p=>_p.m_Hex.MatchVertexCount(m_Quad.m_Hex)==2 && _p.m_Hex.MatchVertex(curVertex) && _p.m_Hex.MatchVertex(nextVertex));
                nearbyQuads[i] = quad.m_Identity;
                var edgeDirectionWS =  m_Quad.m_CenterWS - quad.m_CenterWS;
                var edgeDirectionOS=(Coord)worldToLocal.MultiplyVector(edgeDirectionWS);
                edgeOrientations[i] = UMath.kRad2Deg * UMath.GetRadClockWise(Vector2.up, edgeDirectionOS);
            }
            m_NearbyQuadCW = nearbyQuads;
            m_EdgeDirectionsCW = edgeOrientations;
            
            m_ShapeOS = m_Quad.m_CoordWS.Convert(p => (Coord)worldToLocal.MultiplyVector(p-m_Quad.m_CenterWS));
            Quad<float> centerOrientations = default;
            for (int orientation = 0; orientation < 4; orientation++)
            {
                var orientedNormalOS = (m_ShapeOS[(orientation+3)%4] + m_ShapeOS[orientation%4])/2;
                centerOrientations[orientation] = UMath.kRad2Deg*UMath.GetRadClockWise(Vector2.up, orientedNormalOS);     //Forward To Edge
            }
            m_EdgeNormalsCW = centerOrientations;
            
            m_ShapeOS.SplitToQuads(false).FillArray(m_QubeQuads);
            m_QubeQuads.Select(_orientedShape => Quad<float>.Convert(_orientedShape, (_orientation,_coord) => {
                    var orientedLeft = _orientedShape[(_orientation+1)%4]-_orientedShape[_orientation];
                    return UMath.kRad2Deg*UMath.GetRadClockWise(Vector2.left, orientedLeft);
                })
            ).FillArray(m_QubeQuadsOrientation);

            return this;
        }
        public override void OnPoolRecycle()
        {
            m_Quad = null;
        }
    }
}