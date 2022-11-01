using System;
using System.Collections.Generic;
using Geometry;
using MeshFragment;
using PCG.Module;
using PCG.Module.Cluster;
using TPoolStatic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PCG.Simplex
{
    using static PCGDefines<int>;
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class SimplexManager : MonoBehaviour,IPolyGridControl
    {
        private SimplexCollection m_Collection;
        private MeshFilter m_Filter;
        private MeshRenderer m_Renderer;
        private Mesh m_Mesh;
        private GridManager m_Grid;

        private Dictionary<PCGID, Corner> m_Corners;
        private Dictionary<PCGID, Voxel> m_Voxels;
        private Dictionary<string, int> m_Indexer;

        private MaterialPropertyBlock kBlock;
        private static readonly int kStartTime = Shader.PropertyToID("_StartTime");
        private static readonly int kCenter = Shader.PropertyToID("_Center");
        private static readonly int kForward = Shader.PropertyToID("_Forward");
        private Counter m_FadeCounter = new Counter(1f,true);
        
        public void Init()
        {
            m_Mesh = new Mesh() {name = "Simplex", hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            m_Filter = GetComponent<MeshFilter>();
            m_Renderer = GetComponent<MeshRenderer>();
            m_Filter.sharedMesh = m_Mesh;
            m_Corners = new Dictionary<PCGID, Corner>();
            m_Voxels = new Dictionary<PCGID, Voxel>();
            m_Indexer = new Dictionary<string, int>();
            kBlock = new MaterialPropertyBlock();
        }

        public SimplexManager Setup(SimplexCollection _collection, GridManager _grid)
        {
            m_Grid = _grid;
            m_Collection = _collection;
            m_Indexer.Clear();
            foreach (var simplexData in _collection.m_SimplexData)
                m_Indexer.Add(simplexData.m_Name,m_Indexer.Count);
            return this;
        }

        public void Clear()
        {
            m_Corners.Clear();
            m_Voxels.Clear();
            m_Mesh.Clear();
        }
    
        public void Dispose()
        {
            DestroyImmediate(m_Mesh);
            m_Collection = null;
            m_Corners = null;
            m_Voxels = null;
            m_Indexer = null;
        }

        private static readonly EVertexData kOutputVertexData = UEnum.CreateFlags(EVertexData.UV0,EVertexData.Color);
        public void Construct(params (string _res, IList<PCGID> _corners)[] _cornersRes)
        {
            m_Renderer.enabled = true;
            m_Corners.Clear();
            m_Voxels.Clear();
            foreach (var (res,_corners) in _cornersRes)
            {
                var index = m_Indexer[res];
                foreach (var cornerID in _corners)
                {
                    var vertex = m_Grid.m_Vertices[cornerID.location];
                    m_Corners.Add(cornerID,new Corner(cornerID,vertex ,index));
                }
            }
            
            foreach (var cornerID in m_Corners.Keys)
            {
                var vertex = m_Grid.m_Vertices[cornerID.location];
                foreach (var voxelID in vertex.IterateRelativeVoxels(cornerID.height))
                {
                    if(m_Voxels.ContainsKey(voxelID))
                        continue;
                    var quad = m_Grid.m_Quads[voxelID.location];
                    Qube<int> voxelBytes = new Qube<int>();
                    var corners = quad.Corners(voxelID.height);
                    for (int i = 0; i < 8; i++)
                        voxelBytes[i] = m_Corners.TryGetValue(corners[i],out var corner)?corner.type:-1;
                    m_Voxels.Add(voxelID,new Voxel(voxelID,quad,voxelBytes));
                }
            }

            // Populate Mesh
            TSPoolList<IMeshFragment>.Spawn(out var orientedFragments);
            foreach (var voxel in m_Voxels.Values)
            {
                var outputBytes = voxel.OutputBytes();
                foreach (var simplexIndex in outputBytes.Keys)
                {
                    var quadIndexer = DSimplex.kIndexes[outputBytes[simplexIndex]];
                    if (quadIndexer.index == -1)
                        continue;

                    var simplexData = m_Collection.m_SimplexData[simplexIndex];
                    Vector3 offset = KPCG.GetCornerHeight(voxel.height-1);
                    var fragmentInputs = simplexData.m_ModuleData[quadIndexer.index];
                    var orientation = quadIndexer.orientation;
                    var quad = voxel.quad.m_CoordWS;
                    for (int i = 0; i < fragmentInputs.m_MeshFragments.Length; i++)
                    {
                        var fragmentInput = fragmentInputs.m_MeshFragments[i];
                        var fragmentOutput = TSPool<FMeshFragmentObject>.Spawn().Initialize(fragmentInput.embedMaterial);
                
                        var verticesOS = fragmentInput.vertices;
                        var vertexCount = verticesOS.Length;
                        for (int j = 0; j < vertexCount; j++)
                        {
                            Vector3 positionWS = DModuleCluster.ModuleToObjectVertex(4, orientation,fragmentInput.vertices[j], quad,KPCG.kPolyHeight) + offset;
                            fragmentOutput.vertices.Add(positionWS);
                            fragmentOutput.uvs.Add(fragmentInput.uvs[j]);
                        }
                        fragmentOutput.indexes.AddRange(fragmentInput.indexes);
                        fragmentOutput.colors.AddRange(fragmentInput.colors);
                        orientedFragments.Add(fragmentOutput);
                    }
                }
            }
            UMeshFragment.Combine(orientedFragments,m_Mesh,m_Collection.m_MaterialLibrary,out var materials,kOutputVertexData,IndexFormat.UInt32 );
            for(int i=0;i<orientedFragments.Count;i++)
                TSPool<FMeshFragmentObject>.Recycle(orientedFragments[i] as FMeshFragmentObject);
            TSPoolList<IMeshFragment>.Recycle(orientedFragments);
            
            m_Renderer.materials = materials;
            m_FadeCounter.Stop();
            kBlock.SetVector(kCenter,m_Mesh.bounds.center);
            kBlock.SetFloat(kStartTime,Time.time);
            kBlock.SetInt(kForward,1);
            m_Renderer.SetPropertyBlock(kBlock);
        }
        
        public void Fade()
        {
            m_FadeCounter.Replay();
            kBlock.SetFloat(kStartTime,Time.time);
            kBlock.SetInt(kForward,0);
            m_Renderer.SetPropertyBlock(kBlock);
        }

        public void Tick(float _deltaTime)
        {
            if (m_FadeCounter.Tick(_deltaTime))
            {
                Clear();
                m_Renderer.enabled = false;
            }
        }
    #if UNITY_EDITOR
        public bool m_DrawCorners;
        public bool m_DrawVoxels;
        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            
            if(m_DrawCorners)
                foreach (var corner in m_Corners.Values)
                {
                    Gizmos.color = UColor.IndexToColor(corner.type);
                    Gizmos.DrawWireSphere(corner.position,.1f);
                }
            
            if(m_DrawVoxels)
                foreach (var voxel in m_Voxels.Values)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(voxel.position,Vector3.one*.1f);
                    var outputBytes = voxel.OutputBytes();
                    string output = "";
                    foreach (var index in outputBytes.Keys)
                    {
                        var indexer = DSimplex.kIndexes[outputBytes[index]];
                        output += $"B:{indexer.srcByte}\nO:{indexer.orientation}\n";
                    }
                    Gizmos_Extend.DrawString(voxel.position,output,0f);
                }
        }
    #endif
    }

    public struct Corner
    {
        public Vector3 position;
        public int type;

        public Corner(PCGID _identity,PolyVertex _vertex,int _type)
        {
            position = _vertex.GetCornerPosition(_identity.height);
            type = _type;
        }
    }

    public struct Voxel
    {
        public byte height;
        public PolyQuad quad;
        public Qube<int> corners;
        public Vector3 position;

        public Voxel(PCGID _identity, PolyQuad _quad,Qube<int> _cornerses)
        {
            height = _identity.height;
            quad = _quad;
            position = _quad.GetVoxelPosition(_identity.height);
            corners = _cornerses;
        }
        private static readonly Dictionary<int, byte> kVoxelHelper = new Dictionary<int, byte>();

        public Dictionary<int, byte> OutputBytes()
        {
            var srcCorners = corners;
            kVoxelHelper.Clear();
            for (int i = 0; i < 8; i++)
            {
                var curIndex = srcCorners[i];
                if(curIndex==-1||kVoxelHelper.ContainsKey(curIndex))
                    continue;
                kVoxelHelper.Add(curIndex,Qube<bool>.Convert(srcCorners,p=>p==curIndex).ToByte());
            }

            return kVoxelHelper;
        }
    }
}