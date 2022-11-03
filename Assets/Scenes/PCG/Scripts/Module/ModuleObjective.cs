using System.Collections.Generic;
using Geometry;
using Procedural;
using TPoolStatic;
using UnityEngine;

namespace PCG.Module
{
    using static PCGDefines<int>;

    public enum EModuleCollapseStatus
    {
        Awaiting,
        Collapsing,
        Finalizing,
    }

    public class ModuleCollapsePropagandaChain: IPoolClass
    {
        public int chainType=-1;
        public HashSet<PCGID> corners=new HashSet<PCGID>();
        public HashSet<PCGID> voxels=new HashSet<PCGID>();
        
        public void OnPoolCreate()
        {
        }

        public void OnPoolInitialize()
        {
            corners.Clear();
            voxels.Clear();
        }

        public void OnPoolRecycle()
        {
        }
    }

    #region Bridge
    public interface IVertex
    {
        SurfaceID Identity { get; }
        Transform Transform { get; }
        PolyVertex VertexData { get; }
        List<Coord> NearbyVertexPositionLS { get; }
        List<Coord> NearbyVertexSurfaceDirectionLS { get; }
    }

    public interface IQuad
    {
        SurfaceID Identity { get; }
        Transform Transform { get; }
        PolyQuad m_Quad { get; }
        Quad<Coord> m_ShapeOS { get; }
        Quad<float> m_EdgeNormalsCW { get; }
        Quad<float> m_EdgeDirectionsCW { get; }
        Quad<Coord>[] m_QubeQuads { get; }
        Quad<float>[] m_QubeQuadsOrientation { get; }
        Quad<SurfaceID> m_NearbyQuadCW { get;}
    }

    public interface IVoxel
    { 
        PCGID Identity { get; }
        Transform Transform { get; }
        IQuad m_Quad { get; }
        Qube<ICorner> m_Corners { get; }
        Dictionary<int,byte> m_TypedCluster { get; }
        Qube<byte> m_ClusterUnitBaseBytes  { get; }
        Qube<byte> m_ClusterUnitAvailableBytes { get; }
        CubeFacing<PCGID> m_CubeSides { get; }
        ECubeFacing m_CubeSidesExists { get; }
    }

    public interface ICorner
    {
        PCGID Identity { get; }
        int m_Type { get; }
        Transform Transform { get; }
        IVertex m_Vertex { get; }
        List<PCGID> m_AdjacentConnectedCorners { get; }
        // List<PolyID> m_IntervalConnectedCorners { get; }
        List<PCGID> m_RelativeVoxels { get; }
        // List<PolyID> m_ClusterRelativeVoxels { get; }
        
        // bool m_UpperCornerRelation { get; }
        // bool m_LowerCornerRelation { get; }
    }
    #endregion
    
    #region Control
    public interface IModuleControl
    {
        public GridManager m_Grid { get; set; }
        void Init();
        void Setup();
        void Clear();
        void Tick(float _deltaTime);
        void Dispose();
    }

    public interface IModuleStructure
    {
        IModuleStructureElement CollectStructure(PCGID _voxelID);
    }

    
    public interface IModuleCollapse
    {
        void Propaganda(float _deltaTime,Stack<ModuleCollapsePropagandaChain> _propagandaChains);
        bool Collapse(float _deltaTime);
        bool Finalize(float _deltaTime);
    }

    public interface IModuleVertexCallback
    {
        void OnPopulateVertex(IVertex _vertex);
        void OnDeconstructVertex(SurfaceID _vertexID);
    }

    public interface IModuleQuadCallback
    {
        void OnPopulateQuad(IQuad _quad);
        void OnDeconstructQuad(SurfaceID _quadID);
    }
        
    public interface IModuleCornerCallback
    {
        void OnCornerConstruct(ICorner _corner);
        void OnCornerDeconstruct(PCGID _cornerID);
    }

    public interface IModuleVoxelCallback
    {
        void OnVoxelConstruct(IVoxel _voxel);
        void OnVoxelDeconstruct(PCGID _voxelID);
    }

    public interface IModuleStructureElement
    {
        public Transform Transform { get; }
        public bool LightEnabled { get; set; }
        public void RefreshLighting();
    }
    #endregion
}