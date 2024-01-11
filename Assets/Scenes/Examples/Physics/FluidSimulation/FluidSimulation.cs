using System;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;
using static umath;
using float3 = Unity.Mathematics.float3;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.PhysicsScenes.FluidSimulation
{
    [Serializable]
    public struct FluidSystemInput : ISerializationCallbackReceiver
    {
        [Header("Constant")] 
        public float c;
        public float mu;
        public float t;
        
        [Header("Shape")]
        public int width;
        public int height;
        public float size;

        [Header("Constants")] 
        [Readonly] public float k1;
        [Readonly] public float k2;
        [Readonly] public float k3;
        public FluidSystemInput Ctor()
        {
            var f1 = pow2(c) * pow2(t) / pow2(size);
            var f2 = 1f / (mu * t + 2);
            k1 = (4f - 8f * f1) * f2;
            k2 = (mu * t - 2) * f2;
            k3 = 2f * f1 * f2;
            return this;
        }
        
        public static readonly FluidSystemInput kDefault = new FluidSystemInput()
        {
            c = 1f,
            mu = 0.01f,
            t = 0.05f,
            
            width = 10,
            height = 10,
            size = 1f,
        }.Ctor();

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize() => Ctor();
    }
    
    public struct FluidVertex
    {
        public float3 position;
        public float3 normal;
        public float4 tangent;
        public float2 uv;

        public static readonly VertexAttributeDescriptor[] Layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };
    }

    public enum EFluidLife
    {
        Invalid,
        Created,
        Initialized,
        UnInitialized,
        Disposed,
    }
    
    public class FluidSystem
    {
        public EFluidLife m_Life { get; private set; } = EFluidLife.Invalid;
        private static int sInstanceCount = 0;
        
        private Mesh m_Mesh;
        public Mesh Create()
        {
            m_Life = EFluidLife.Created;
            m_Mesh = new Mesh(){name = $"Fluid Simulation {sInstanceCount++}",hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            return m_Mesh;
        }

        private FluidSystemInput m_Data;
        private NativeArray<float3>[] vertexBuffers;
        private NativeArray<float3> normalBuffers,tangentBuffers;
        private NativeArray<float2> uvBuffers;
        private int vertexBufferIndex = 0;
        private Ticker m_Ticker = new Ticker(.5f);
        public void Init(FluidSystemInput _input)
        {
            UnInit();
            m_Life = EFluidLife.Initialized;
            m_Data = _input;

            
            m_Ticker.Set(m_Data.t);
            int vertexCount = m_Data.width * m_Data.height;
            vertexBuffers = new[] {
                new NativeArray<float3>(vertexCount,Allocator.Persistent),
                new NativeArray<float3>(vertexCount,Allocator.Persistent)
            };
            normalBuffers = new NativeArray<float3>(vertexCount, Allocator.Persistent);
            tangentBuffers = new NativeArray<float3>(vertexCount, Allocator.Persistent);
            uvBuffers = new NativeArray<float2>(vertexCount,Allocator.Persistent);
            for(int j=0;j<m_Data.height;j++)
                for (int i = 0; i < m_Data.width; i++)
                {
                    var position = new float3(i * m_Data.size, 0, j * m_Data.size);
                    var index = Index(i, j);
                    vertexBuffers[0][index] = position;
                    vertexBuffers[1][index] = position;
                    normalBuffers[index] = kfloat3.up;
                    tangentBuffers[index] = kfloat3.right;
                }
            WriteMesh();
        }

        void UnInit()
        {
            m_Life = EFluidLife.UnInitialized;
            if (vertexBuffers != null)
            {
                vertexBuffers[0].Dispose();
                vertexBuffers[1].Dispose();
            }
            vertexBuffers = null;
            normalBuffers.Dispose();
            tangentBuffers.Dispose();
            uvBuffers.Dispose();
        }

        public void Dispose()
        {
            UnInit();
            m_Life = EFluidLife.Disposed;
            GameObject.DestroyImmediate(m_Mesh);
            m_Mesh = null;
        }

        int Index(int i, int j) => j * m_Data.height + i;

        void WriteMesh()
        { 
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            var vertexCount = vertexBuffers[0].Length;
            meshData.SetVertexBufferParams(vertexCount,FluidVertex.Layout);
            var vertices = meshData.GetVertexData<FluidVertex>();

            var curVertices = vertexBuffers[vertexBufferIndex];
            for(int j=0;j<m_Data.height;j++)
                for (int i = 0; i < m_Data.width; i++)
                {
                    int index = j * m_Data.height + i;
                    vertices[index] = new FluidVertex()
                    {
                        position = curVertices[index],
                        normal = normalBuffers[index],
                        tangent = tangentBuffers[index].to4(1),
                        uv = uvBuffers[index],
                    };
                }

            int quadCount = (m_Data.width - 1) * (m_Data.height - 1);
            int indexCount = quadCount * 6;
            meshData.SetIndexBufferParams(indexCount,IndexFormat.UInt16);
            var fluidIndexes = meshData.GetIndexData<ushort>();
            for(int i = 0 ;i < m_Data.width - 1;i++)
            for (int j = 0; j < m_Data.height - 1; j++)
            {
                var lb = (ushort)(i * m_Data.height + j);
                var lu =  (ushort)(i * m_Data.height + j + 1);
                var ru =  (ushort)((i + 1) * m_Data.height + j + 1);
                var rb =  (ushort)((i + 1) * m_Data.height + j);

                int startIndex = i * (m_Data.height - 1) * 6 + j * 6;

                fluidIndexes[startIndex] = lb;
                fluidIndexes[startIndex + 1] = rb;
                fluidIndexes[startIndex + 2] = ru;

                fluidIndexes[startIndex + 3] = lb;
                fluidIndexes[startIndex + 4] = ru;
                fluidIndexes[startIndex + 5] = lu; 
            }
        
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,indexCount)
            {
                vertexCount = vertexCount,
                topology = MeshTopology.Triangles,
            });

            m_Mesh.bounds = meshData.GetSubMesh(0).bounds;
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,m_Mesh);
        }

        void Execute()
        {
            var k1 = m_Data.k1;
            var k2 = m_Data.k2;
            var k3 = m_Data.k3;
            
            var crnt = vertexBuffers[vertexBufferIndex];
            var prev = vertexBuffers[1 - vertexBufferIndex];
            for(int j=1;j<m_Data.height - 1;j++)
                for (int i = 1; i < m_Data.width - 1; i++)
                {
                    prev[Index(i, j)] = k1 * crnt[Index(i, j)] + k2 * prev[Index(i, j)] +
                                        k3 * (crnt[Index(i + 1, j)] + crnt[Index(i - 1, j)] +
                                              crnt[Index(i , j + 1)] + crnt[Index(i, j - 1)]);
                }

            vertexBufferIndex = 1 - vertexBufferIndex;

            crnt = vertexBuffers[vertexBufferIndex];
            
            var d2 = m_Data.size * 2f;
            for(int j=1;j<m_Data.height - 1;j++)
            for (int i = 1; i < m_Data.width - 1; i++)
            {
                var index = Index(i, j);
                var xOffset = crnt[Index(i + 1, j)].y - crnt[Index(i - 1, j)].y;
                var yOffset = crnt[Index(i,j+1)].y - crnt[Index(i,j-1)].y;
                normalBuffers[index] = normalize(new float3(xOffset,d2,yOffset)); 
                tangentBuffers[index] = normalize(new float3(d2,0,yOffset));
                uvBuffers[index ] = new float2(i / (m_Data.width - 1f),j / (m_Data.height - 1f));
            }
        }
        
        public void Tick(float _deltaTime)
        {
            if (m_Life != EFluidLife.Initialized) return;

            if (!m_Ticker.Tick(_deltaTime)) return;
            Execute();
            WriteMesh();
        }

        public void Pop(GSphere _blastSphere,float _strength,bool _fallOff)
        {
            if (m_Life != EFluidLife.Initialized) return;
            
            var crnt = vertexBuffers[vertexBufferIndex];
            for(int j=1;j<m_Data.height - 1;j++)
            for (int i = 1; i < m_Data.width - 1; i++)
            {
                var position =  crnt[Index(i, j)];
                if (!_blastSphere.Contains(position))
                    continue;
                
                var delta = position - _blastSphere.center;
                var direction = delta.normalize();
                var distance = delta.magnitude();
                if (_fallOff)
                {
                    distance *= 1f - distance / _blastSphere.radius;
                }
                
                crnt[Index(i, j)] = position + direction * _strength * distance;
            }
        }
    }
    
    
    [ExecuteInEditMode]
    public class FluidSimulation : MonoBehaviour
    {
        public FluidSystemInput m_Input = FluidSystemInput.kDefault;
        public Transform m_Ping;
        public float m_Radius;
        private Camera m_Camera;
        private FluidSystem m_FluidSystem = new FluidSystem();
        
        public void Awake()
        {
            m_Camera = GetComponentInChildren<Camera>();
            GetComponent<MeshFilter>().sharedMesh = m_FluidSystem.Create();
            m_FluidSystem.Init(m_Input);
        }

        private void OnValidate()
        {
            if (m_FluidSystem.m_Life != EFluidLife.Initialized) return;
            m_FluidSystem.Init(m_Input);
        }

        private void Update()
        {
            var deltaTime = UTime.deltaTime;
            m_FluidSystem.Tick(deltaTime);

            if (m_Ping != null)
                m_FluidSystem.Pop(new GSphere(m_Ping.transform.position, m_Radius),10f * deltaTime,true);
            
            if (Input.GetMouseButton(0) && UGeometry.Intersect(m_Camera.ScreenPointToRay(Input.mousePosition), GPlane.kDefault, out float3 _hitPoint))
                m_FluidSystem.Pop(new GSphere(_hitPoint + kfloat3.up*0.01f, 0.75f),10f * deltaTime,true);
        }

        private void OnDestroy()
        {
            m_FluidSystem.Dispose();
        }

        private void OnDrawGizmos()
        {
            if(m_Ping!=null)
                Gizmos.DrawWireSphere(m_Ping.transform.position,m_Radius);
        }
    }

}