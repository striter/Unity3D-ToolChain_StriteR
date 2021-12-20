using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using Procedural;
using Procedural.Hexagon;
using UnityEngine;
using TPool;
using TPoolStatic;

namespace PolyGrid
{
    public class SelectionManager:IPolyGridControl,IPolyGridVertexCallback,IPolyGridCornerCallback
    {
        private static readonly GPlane kZeroPlane = new GPlane(Vector3.up, 0f);
        TObjectPoolMono<PolyID, SelectionContainer> m_SelectionContainer;
        private readonly Dictionary<HexCoord, Mesh> m_CornerMeshes=new Dictionary<HexCoord, Mesh>();
        public void Init(Transform _transform)
        {
            m_SelectionContainer = new TObjectPoolMono<PolyID, SelectionContainer>(_transform.Find("Selections/Item"));
        }
        public void Clear()
        {
            m_SelectionContainer.Clear();
            foreach (var cornerMesh in m_CornerMeshes.Values)
                TSPoolObject<Mesh>.Recycle(cornerMesh);
            m_CornerMeshes.Clear();
        }

        public void OnPopulateVertex(PolyVertex _vertex)
        {
            var cornerMesh = TSPoolObject<Mesh>.Spawn();
            m_CornerMeshes.Add(_vertex.m_Identity,cornerMesh);
            
            cornerMesh.MarkDynamic();
            _vertex.ConstructLocalMesh(cornerMesh,EQuadGeometry.Half,EVoxelGeometry.VoxelTight,true,true);
        }

        public void OnDeconstructVertex(HexCoord _vertexID)
        {
            var cornerMesh = m_CornerMeshes[_vertexID];
            m_CornerMeshes.Remove(_vertexID);
            
            TSPoolObject<Mesh>.Recycle(cornerMesh);
        }
        
        public void OnPopulateCorner(ICorner _corner) => m_SelectionContainer.Spawn(_corner.Identity).Init(_corner,m_CornerMeshes[_corner.Identity.location]);
        public void OnDeconstructCorner(PolyID _cornerID) => m_SelectionContainer.Recycle(_cornerID);

        public bool VerifyConstruction(Ray _ray,IEnumerable<PolyQuad> _quads,out PolyID _selection)
        {
            _selection = default;
            if(Physics.Raycast(_ray, out RaycastHit hit, float.MaxValue, int.MaxValue))
            {
                var selection = hit.collider.GetComponent<SelectionContainer>();
                _selection = selection.ValidateRaycast(ref hit);
                return true;
            }

            var hitPos = _ray.GetPoint(UGeometryIntersect.RayPlaneDistance(kZeroPlane, _ray));
            var hitCoord =  hitPos.ToCoord();
            if (ValidatePlaneSelection(hitCoord, _quads,out HexCoord planeCoord))
            {
                _selection= new PolyID(planeCoord,0);
                return true;
            }
            return false;
        }

        public bool VerifyDeconstruction(Ray _ray,out PolyID _selection)
        {
            _selection = default;
            if (!Physics.Raycast(_ray, out RaycastHit hit, float.MaxValue, int.MaxValue))
                return false;
            var raycast = hit.collider.GetComponent<SelectionContainer>();
            _selection = raycast.Identity;
            return true;
        }
        
        bool ValidatePlaneSelection(Coord _localPos,IEnumerable<PolyQuad> _quads,out HexCoord coord)
        {
            coord=HexCoord.zero;
            var quad= _quads.Find(p =>p.m_CoordQuad.IsPointInside(_localPos),out int quadIndex);
            if (quadIndex != -1)
            {
                var quadVertexIndex = quad.m_CoordQuad.NearestPointIndex(_localPos);
                coord=quad.m_HexQuad[quadVertexIndex];
                return true;
            }
            return false;
        }

        public void Tick(float _deltaTime)
        {
        }

    }

}