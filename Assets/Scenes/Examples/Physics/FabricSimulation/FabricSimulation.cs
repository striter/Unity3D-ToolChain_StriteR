using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Examples.PhysicsScenes.FabricSimulation
{
    [Serializable]
    public struct FabricInput
    {
        public float t;
        [Header("Shape")]
        public int width;
        public int height;
        public float size;

        [Header("Damper")] 
        public float stiffness;
        public float damping;
        
        public static FabricInput kDefault = new FabricInput()
        {
            t = 0.05f,
            width = 10,
            height = 10,
            size = 1f,
            
            stiffness = 20f,
            damping = 5f,
        };
    }
    
    public struct FabricVertex
    {
        public float3 position;
        public float2 uv;

        public static readonly VertexAttributeDescriptor[] Layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };
    }

    public struct FabricConnection
    {
        public int start;
        public int end;
        public float initDistance;
    }

    public class FabricInstance
    {
        private static int sInstanceCount = 0;
        private Mesh m_Mesh;
        public Mesh Create()
        {
            m_Mesh = new Mesh(){name = $"Fluid Simulation {sInstanceCount++}",hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            return m_Mesh;
        }

        public FabricInput m_Data;
        private Ticker m_Ticker = new Ticker(.5f);

        private NativeArray<float3> particles;
        private NativeArray<float3> velocities;
        private NativeArray<float2> uvs;

        private NativeArray<float3> forces;
        
        private NativeArray<FabricConnection> connections;
        int Index(int i, int j) => j * m_Data.height + i;
        public void Initialize(FabricInput _input)
        {
            if (m_Mesh == null)
                return;
            
            UnInit();
            m_Data = _input;
            m_Ticker.Set(_input.t);
            
            var vertexCount = m_Data.width * m_Data.height;
            particles = new NativeArray<float3>(vertexCount,Allocator.Persistent);
            velocities = new NativeArray<float3>(vertexCount,Allocator.Persistent);
            uvs = new NativeArray<float2>(vertexCount,Allocator.Persistent);
            forces = new NativeArray<float3>(vertexCount,Allocator.Persistent);
            Reset();
            
            var connectionCount = m_Data.width * (m_Data.height - 1) +  (m_Data.width-1) * m_Data.height;       //Horizontal + Vertical
            connections = new NativeArray<FabricConnection>(connectionCount,Allocator.Persistent);
            RebuildConnection();

            WriteMesh();
        }

        public void Reset()
        {
            for(int j=0;j<m_Data.height;j++)
            for (int i = 0; i < m_Data.width; i++)
            {
                var index = Index(i, j);
                var position = new float3(i * m_Data.size, 0, j * m_Data.size);
                particles[index] = position;
                velocities[index] = float3.zero;
                uvs[index] = new float2(i / (m_Data.width - 1f), j / (m_Data.height - 1f));
            }
        }

        public void RebuildConnection()
        {
            int connectionIndex = 0;
            void PopConnection(int2 _start,int2 _end)
            {
                var start = Index(_start.x, _start.y);
                var end = Index(_end.x, _end.y);
                var startPosition = particles[start];
                var endPosition = particles[end];
                connections[connectionIndex++] = new FabricConnection()
                {
                    start = start,
                    end = end,
                    initDistance = math.length(startPosition - endPosition),
                };
            }
            
            for (int i = 0; i <  m_Data.width - 1; i++)
            for (int j = 0; j < m_Data.height; j++)
                PopConnection(new int2(i,j),new int2(i + 1,j));

            
            for (int i = 0; i <  m_Data.width; i++)
                for (int j = 0; j < m_Data.height - 1; j++)
                    PopConnection(new int2(i,j),new int2(i,j+1));
        }
        
        void UnInit()
        {
            particles.Dispose();
            connections.Dispose();
            velocities.Dispose();
            uvs.Dispose();
            forces.Dispose();
        }
        
        public void Dispose()
        {
            UnInit();
            GameObject.DestroyImmediate(m_Mesh);
            m_Mesh = null;
        }

        public void Tick(float _deltaTime)
        {
            if (!m_Ticker.Tick(_deltaTime))
                return;

            var stiffness = m_Data.stiffness;
            var damping = m_Data.damping;
            var deltaTime = m_Ticker.m_Duration;
            var particleCount = particles.Length;

            for(int i=0;i<particleCount;i++)
                forces[i] = kfloat3.down * .98f;
            
            foreach (var connection in connections)
            {
                var delta = particles[connection.start] -  particles[connection.end];
                var distance = math.length(delta);
                var springForce = stiffness * (delta/distance) * (distance - connection.initDistance);
                var dampingForce = damping * (velocities[connection.start] - velocities[connection.end]);

                var force = springForce + dampingForce;
                
                forces[connection.start] += -force;
                forces[connection.end] += force;
            }

            for(int i=0;i<particleCount;i++)
                if(i>m_Data.width - 1)
                    velocities[i] += forces[i] * _deltaTime;

            for (int i = 0; i < particleCount; i++)
                particles[i] += velocities[i] * deltaTime;

            WriteMesh();
        }
        
        void WriteMesh()
        { 
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            var vertexCount = particles.Length;
            meshData.SetVertexBufferParams(vertexCount,FabricVertex.Layout);
            var vertices = meshData.GetVertexData<FabricVertex>();

            for(int j=0;j<m_Data.height;j++)
                for (int i = 0; i < m_Data.width; i++)
                {
                    var index = Index(i, j);
                    vertices[index] = new FabricVertex()
                    {
                        position = particles[index],
                        uv = uvs[index],
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

        public void DrawGizmos()
        {
            Gizmos.color = Color.white;
            
            for (int i = 0; i < m_Data.width; i++)
            for (int j = 0; j < m_Data.height; j++)
            {
                var index = j * m_Data.height + i;
                Gizmos.DrawSphere(particles[index],0.1f);
            }

            foreach (var connection in connections)
            {
                var start = particles[connection.start];
                var end = particles[connection.end];
                Gizmos.DrawLine(start,end);
            }

        }
    }
    

    [ExecuteInEditMode]
    public class FabricSimulation : MonoBehaviour
    {
        public MeshFilter m_FabricFilter;
        public FabricInput m_FabricInput = FabricInput.kDefault;
        private FabricInstance m_FabricInstance = new FabricInstance();
        public bool simulate;
        public bool m_DrawGizmos;
        
        private void Awake()
        {
            m_FabricFilter = GetComponent<MeshFilter>();
            m_FabricFilter.sharedMesh = m_FabricInstance.Create();
            m_FabricInstance.Initialize(m_FabricInput);
        }

        private void OnValidate()
        {
            m_FabricInstance.Initialize(m_FabricInput);
        }

        private void OnDestroy()
        {
            m_FabricInstance.Dispose();
        }

        private void Update()
        {
            if (!simulate)
                return;
            
            var deltaTime = UTime.deltaTime;
            m_FabricInstance.Tick(deltaTime);
        }

        private void OnDrawGizmos()
        {
            if (!m_DrawGizmos)
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            m_FabricInstance.DrawGizmos();
        }

        [Button]
        public void Reset()
        {
            m_FabricInstance.Reset();
        }
    }

    
}
