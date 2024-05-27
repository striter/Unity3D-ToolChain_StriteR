using System;
using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using System.Linq.Extensions;
using TPool;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Dome
{
    public struct FVertex
    {
        public float3 position;
        public float3 normal;
        public half4 color;
        public half2 uv;
    }

    public class DomeCell : APoolTransform<int>
    {
        private FDomeCell m_Cell;
        private FDomeEnvironment m_Environment;
        private Mesh m_Mesh;
        private MeshFilter m_Filter;
        private MeshRenderer m_Renderer;
        public DomeCell(Transform _transform) : base(_transform)
        {
            m_Filter = _transform.GetComponent<MeshFilter>();
            m_Renderer = _transform.GetComponent<MeshRenderer>();
            m_Mesh = new Mesh(){name = "DomeCell",hideFlags = HideFlags.HideAndDontSave};
        }
        
        public override void OnPoolSpawn()
        {
            base.OnPoolSpawn();
            m_Mesh.name = $"DomeCell - {identity}";
            m_Mesh.MarkDynamic();
            m_Filter.sharedMesh = m_Mesh;
        }

        public void Initialize(FDomeEnvironment _environment, FDomeCell _cell)
        {
            m_Environment = _environment;
            m_Cell = _cell;
            PopulateMesh();
        }

        static readonly half4 kUpColor = (half4)new float4 (1, 1, 1, 1); 
        static readonly half4 kDownColor = (half4)new float4 (1, 1, 1, 0);
        
        public void PopulateMesh()
        {
            var outerQuad = m_Cell.positions;
            var center = outerQuad.Center;
            var innerQuad = (GQuad)outerQuad.Shrink(m_Environment.m_Shape.m_Shrink) + kfloat3.up * m_Environment.m_Shape.m_Upward;
            outerQuad -= center;
            innerQuad -= center;
            
            transform.position = center;
            var outerUV = KQuad.kUV;
            var innerUV = KQuad.kUV.Shrink_Dynamic(m_Environment.m_Shape.m_Shrink,.5f);
            
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(4,Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position , VertexAttributeFormat.Float32,3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal , VertexAttributeFormat.Float32 , 3);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Color , VertexAttributeFormat.Float16 , 4);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0 , VertexAttributeFormat.Float16 , 2);

            var vertexCount = 4 + 4;
            var indexCount = 6 * 5;
            meshData.SetVertexBufferParams(vertexCount,vertexAttributes);
            meshData.SetIndexBufferParams(indexCount,IndexFormat.UInt32);
            vertexAttributes.Dispose();
            
            var vertices = meshData.GetVertexData<FVertex>();
            var indexes = meshData.GetIndexData<int>().Reinterpret<int3>(sizeof(int));

            var vertexStart = 0;
            vertices[vertexStart + 0] = new FVertex() {position = outerQuad.B, normal = (center - outerQuad.B).setY(0).normalize(),color = kDownColor,uv = (half2)outerUV[0]};
            vertices[vertexStart + 1] = new FVertex() {position = outerQuad.L, normal = (center - outerQuad.L).setY(0).normalize(),color = kDownColor,uv = (half2)outerUV[1]};
            vertices[vertexStart + 2] = new FVertex() {position = outerQuad.F, normal = (center - outerQuad.F).setY(0).normalize(),color = kDownColor,uv = (half2)outerUV[2]};
            vertices[vertexStart + 3] = new FVertex() {position = outerQuad.R, normal = (center - outerQuad.R).setY(0).normalize(),color = kDownColor,uv = (half2)outerUV[3]};
            
            vertices[vertexStart + 4] = new FVertex() {position = innerQuad.B, normal = kfloat3.up,color = kUpColor,uv = (half2)innerUV[0]};
            vertices[vertexStart + 5] = new FVertex() {position = innerQuad.L, normal = kfloat3.up,color = kUpColor,uv = (half2)innerUV[1]};
            vertices[vertexStart + 6] = new FVertex() {position = innerQuad.F, normal = kfloat3.up,color = kUpColor,uv = (half2)innerUV[2]};
            vertices[vertexStart + 7] = new FVertex() {position = innerQuad.R, normal = kfloat3.up,color = kUpColor,uv = (half2)innerUV[3]};

            int indexStart = 0;        
            UMesh.ApplyQuadIndexes(indexes,indexStart + 0,vertexStart+4,vertexStart+5,vertexStart+6,vertexStart+7);      //Top
            UMesh.ApplyQuadIndexes(indexes,indexStart + 2,vertexStart+0,vertexStart+1,vertexStart+5,vertexStart+4);      //Bottom
            UMesh.ApplyQuadIndexes(indexes,indexStart + 4,vertexStart+1,vertexStart+2,vertexStart+6,vertexStart+5);      //Left
            UMesh.ApplyQuadIndexes(indexes,indexStart + 6,vertexStart+2,vertexStart+3,vertexStart+7,vertexStart+6);      //Forward   
            UMesh.ApplyQuadIndexes(indexes,indexStart + 8,vertexStart+3,vertexStart+0,vertexStart+4,vertexStart+7);      //Right       

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,indexCount){vertexCount = vertexCount});
            m_Mesh.bounds = UGeometry.GetBoundingBox(outerQuad.Concat(innerQuad));
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,m_Mesh,MeshUpdateFlags.DontRecalculateBounds);
        }
        
        public override void OnPoolDispose()
        {
            base.OnPoolDispose();
            m_Filter.sharedMesh = null;
            GameObject.DestroyImmediate(m_Mesh);
        }
    }

    [Serializable]
    public struct EnvironmentShape
    {
        [Header("Cell")]
        [Clamp(0,1f)]public float m_Shrink;
        [Clamp(0,1f)]public float m_Upward;

        public static readonly EnvironmentShape kDefault = new EnvironmentShape()
        {
            m_Upward = 0.1f, m_Shrink = 0.8f,
        };
    }

    public class FDomeEnvironment : ADomeController
    {
        public EnvironmentShape m_Shape = EnvironmentShape.kDefault;
        private ObjectPoolClass<int, DomeCell> m_Cells;

        public override void OnInitialized()
        {
            m_Cells = new ObjectPoolClass<int, DomeCell>(transform.Find("Cell"));
            PopulateMesh();
        }

        void PopulateMesh()
        {
            foreach (var (index,vertex) in Refer<FDomeGrid>().m_Vertices.LoopIndex())
                if(vertex.available)
                    m_Cells.Spawn(index).Initialize(this,vertex);
        }

        public override void Tick(float _deltaTime)
        {
            
        }

        public override void Dispose()
        {
            m_Cells.Dispose();
        }
        

        #if UNITY_EDITOR
        public void OnValidate()
        {
            if (m_Cells == null)
                return;
            m_Cells.Traversal(p=>p.PopulateMesh());
        }
        
        #endif

    }
}