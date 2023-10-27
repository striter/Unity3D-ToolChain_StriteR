using Noise;
using Rendering.GI.SphericalHarmonics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Geometry.Explicit.Shape;

namespace Examples.Algorithm.Procedural
{
    public enum EShape
    {
        Plane,
        Sphere,
        Torus,
    }
    
    [ExecuteInEditMode]
    public class ProceduralNoiseDisplayer : MonoBehaviour
    {
        public EShape m_Shape = EShape.Torus;
        private static int kHashID = Shader.PropertyToID("_Hashes"),
                            kPositions = Shader.PropertyToID("_Positions"),
                            kNormals = Shader.PropertyToID("_Normals"),
                            kConfigID = Shader.PropertyToID("_Config");

        public TRS m_HashTransform = TRS.kDefault;
        public float elementSize = 1f;
        [Range(-2f,2f)]public float displacement;
        public int seed;
        public Mesh m_InstanceMesh;
        public Material m_InstanceMaterial;
        [Clamp(0,int.MaxValue)]public int m_Resolution = 64;
        private NativeArray<uint4> m_Hashes;
        private NativeArray<float3x4> m_Positions;
        private NativeArray<float3x4> m_Normals;
        private ComputeBuffer m_HashBuffer,m_PositionBuffer,m_NormalBuffer;
        private MaterialPropertyBlock m_MaterialPropertyBlock;
        
        private void OnEnable()
        {
            m_MaterialPropertyBlock ??= new MaterialPropertyBlock();
            int length = m_Resolution * m_Resolution;
            length = length / 4 + (length & 1);
            
            m_Hashes = new NativeArray<uint4>(length, Allocator.Persistent);
            m_Positions = new NativeArray<float3x4>(length, Allocator.Persistent);
            m_Normals = new NativeArray<float3x4>(length, Allocator.Persistent);
            switch (m_Shape)
            {
                case EShape.Plane:
                    new ShapeJob<SPlane>()
                    {
                        positions = m_Positions,
                        normals = m_Normals,
                        resolution = m_Resolution,
                        invResolution = 1f / m_Resolution,
                        transform = (float4x4)transform.localToWorldMatrix
                    }.ScheduleParallel(length,m_Resolution,default).Complete();
                    break;
                case EShape.Sphere:
                    new ShapeJob<SSphere>()
                    {
                        positions = m_Positions,
                        normals = m_Normals,
                        resolution = m_Resolution,
                        invResolution = 1f / m_Resolution,
                        transform = (float4x4)transform.localToWorldMatrix
                    }.ScheduleParallel(length,m_Resolution,default).Complete();
                    break;
                case EShape.Torus:
                    new ShapeJob<STorus>()
                    {
                        positions = m_Positions,
                        normals = m_Normals,
                        resolution = m_Resolution,
                        invResolution = 1f / m_Resolution,
                        transform = (float4x4)transform.localToWorldMatrix
                    }.ScheduleParallel(length,m_Resolution,default).Complete();
                    break;
            }
            
            new NoiseJob()
            {
                hashes = m_Hashes,
                positions = m_Positions,
                hash = SmallXXHash.Seed(seed),
                transform = m_HashTransform.transformMatrix,
            }.ScheduleParallel(length,m_Resolution,default).Complete();

            length *= 4;
            m_HashBuffer = new ComputeBuffer(length, 4);
            m_HashBuffer.SetData(m_Hashes.Reinterpret<uint>(4 * 4));
            m_PositionBuffer = new ComputeBuffer(length, 3 * 4);
            m_PositionBuffer.SetData(m_Positions.Reinterpret<float3>(  3 * 4 * 4));
            m_NormalBuffer = new ComputeBuffer(length, 3 * 4);
            m_NormalBuffer.SetData(m_Normals.Reinterpret<float3>( 3 * 4 * 4));
            m_MaterialPropertyBlock.SetBuffer(kHashID,m_HashBuffer);
            m_MaterialPropertyBlock.SetBuffer(kPositions,m_PositionBuffer);
            m_MaterialPropertyBlock.SetBuffer(kNormals,m_NormalBuffer);
            m_MaterialPropertyBlock.SetVector(kConfigID,new Vector4(m_Resolution,elementSize/m_Resolution,displacement));
            SHL2Data l2 = SphericalHarmonicsExport.ExportL2Gradient(512,RenderSettings.ambientSkyColor,RenderSettings.ambientEquatorColor,RenderSettings.ambientGroundColor);
            l2.Output().Apply(m_MaterialPropertyBlock,SHShaderProperties.kUnity);
        }

        private void OnDisable()
        {
            m_Positions.Dispose();
            m_Normals.Dispose();
            m_PositionBuffer.Release();
            m_PositionBuffer = null;
            m_NormalBuffer.Release();
            m_NormalBuffer = null;
            
            m_Hashes.Dispose();
            m_HashBuffer.Release();
            m_HashBuffer = null;
        }

        private void Update()
        {
            if(transform.hasChanged)
                OnValidate();
            
            if (!m_InstanceMaterial || !m_InstanceMesh)
                return;
            var bounds = new Bounds(transform.position,new float3( math.cmax(math.abs(displacement)*2+math.abs(transform.lossyScale))));    //Temporary
            Graphics.DrawMeshInstancedProcedural(m_InstanceMesh,0,m_InstanceMaterial, bounds,m_Hashes.Length*4,m_MaterialPropertyBlock);
        }

        private void OnValidate()
        {
            if (m_HashBuffer != null && enabled)
            {
                OnDisable();
                OnEnable();
            }
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct ShapeJob<T> : IJobFor where T:struct,IShapeExplicit
    {
        [WriteOnly] public NativeArray<float3x4> positions;
        [WriteOnly] public NativeArray<float3x4> normals;
        public int resolution;
        public float invResolution;
        public float3x4_homogeneous transform;

        public void Execute(int _i)
        {
            var points = default(T).GetPoint(_i, resolution, invResolution);
            positions[_i] = math.transpose(TransformVectors(transform,points.positions ,1));
            float3x4 n =  math.transpose(TransformVectors(transform, points.normals,0));
            normals[_i] = new float3x4(math.normalize(n.c0),math.normalize(n.c1),math.normalize(n.c2),math.normalize(n.c3));
        }
        
        float4x3 TransformVectors (float3x4 trs, float4x3 p, float w = 1f) => new float4x3(
            trs.c0.x * p.c0 + trs.c1.x * p.c1 + trs.c2.x * p.c2 + trs.c3.x * w,
            trs.c0.y * p.c0 + trs.c1.y * p.c1 + trs.c2.y * p.c2 + trs.c3.y * w,
            trs.c0.z * p.c0 + trs.c1.z * p.c1 + trs.c2.z * p.c2 + trs.c3.z * w
        );
    }
    
    [BurstCompile(FloatPrecision.Standard,FloatMode.Fast,CompileSynchronously = true)]
    struct NoiseJob : IJobFor
    {
        public SmallXXHash4 hash;
        [ReadOnly] public NativeArray<float3x4> positions;
        public float3x4_homogeneous transform;
        [WriteOnly] public NativeArray<uint4> hashes;
        public void Execute(int _i)
        {
            float4x3 p = math.transpose(positions[_i]);
            p = TransformPositions(transform,p);

;           int4 u = (int4) math.floor(p.c0);
            int4 v = (int4) math.floor(p.c1);
            int4 w = (int4) math.floor(p.c2);
            
            hashes[_i] = hash.Eat(u).Eat(v).Eat(w);
        }

        float4x3 TransformPositions(float3x4 trs,float4x3 hgtm)
        {
            return new float4x3(
                trs.c0.x * hgtm.c0 + trs.c1.x * hgtm.c1 + trs.c2.x * hgtm.c2 + trs.c3.x,
                trs.c0.y * hgtm.c0 + trs.c1.y * hgtm.c1 + trs.c2.y * hgtm.c2 + trs.c3.y,
                trs.c0.z * hgtm.c0 + trs.c1.z * hgtm.c1 + trs.c2.z * hgtm.c2 + trs.c3.z
                );
        }
    }
}