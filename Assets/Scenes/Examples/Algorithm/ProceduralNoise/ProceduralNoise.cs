using System;
using Noise;
using Rendering.GI.SphericalHarmonics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
namespace ExampleScenes.Algorithm.Procedural
{
    public enum EShape
    {
        Plane,
        Sphere,
        Torus,
    }
    
    [ExecuteInEditMode]
    public class ProceduralNoise : MonoBehaviour
    {
        public EShape m_Shape = EShape.Torus;
        private static int kHashID = Shader.PropertyToID("_Hashes"),
                            kPositions = Shader.PropertyToID("_Positions"),
                            kNormals = Shader.PropertyToID("_Normals"),
                            kConfigID = Shader.PropertyToID("_Config");

        public TransformData m_HashTransform = TransformData.kDefault;
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
                    new ShapeJob<Plane>()
                    {
                        positions = m_Positions,
                        normals = m_Normals,
                        resolution = m_Resolution,
                        invResolution = 1f / m_Resolution,
                        transform = (float4x4)transform.localToWorldMatrix
                    }.ScheduleParallel(length,m_Resolution,default).Complete();
                    break;
                case EShape.Sphere:
                    new ShapeJob<Sphere>()
                    {
                        positions = m_Positions,
                        normals = m_Normals,
                        resolution = m_Resolution,
                        invResolution = 1f / m_Resolution,
                        transform = (float4x4)transform.localToWorldMatrix
                    }.ScheduleParallel(length,m_Resolution,default).Complete();
                    break;
                case EShape.Torus:
                    new ShapeJob<Torus>()
                    {
                        positions = m_Positions,
                        normals = m_Normals,
                        resolution = m_Resolution,
                        invResolution = 1f / m_Resolution,
                        transform = (float4x4)transform.localToWorldMatrix
                    }.ScheduleParallel(length,m_Resolution,default).Complete();
                    break;
            }
            
            new HashJob()
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
            l2.OutputSH(out var shAr,out var shAg,out var shAb,out var shBr,out var shBg,out var shBb,out var shc);
            m_MaterialPropertyBlock.SetVector("unity_SHAr",shAr);
            m_MaterialPropertyBlock.SetVector("unity_SHAg",shAg);
            m_MaterialPropertyBlock.SetVector("unity_SHAb",shAb);
            m_MaterialPropertyBlock.SetVector("unity_SHBr",shBr);
            m_MaterialPropertyBlock.SetVector("unity_SHBg",shBg);
            m_MaterialPropertyBlock.SetVector("unity_SHBb",shBb);
            m_MaterialPropertyBlock.SetVector("unity_SHC",shc);
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

    struct Point4
    {
        public float4x3 positions, normals;
    }

    interface IShape
    {
        Point4 GetPoint(int _i, float _resolution, float _invResolution);

        static float4x2 IndexTo4UV(int _i, float _resolution, float _invResolution)
        {
            float4x2 uv;
            float4 i4 = 4f * _i + new float4(0, 1, 2, 3);
            uv.c1 = math.floor(_invResolution * i4 + 0.00001f);
            uv.c0 = _invResolution * (i4 - _resolution * uv.c1 + 0.5f) ;
            uv.c1 = _invResolution * (uv.c1 + 0.5f);
            return uv;
        }
    }

    struct Plane :IShape
    {
        public Point4 GetPoint(int _i, float _resolution, float _invResolution)
        {
            float4x2 uv = IShape.IndexTo4UV(_i,_resolution,_invResolution);
            return new Point4() {positions = new float4x3(uv.c0-.5f, 0, uv.c1-.5f),normals = new float4x3(0,1,0)};
        }
    }

    struct Sphere : IShape
    {
        public Point4 GetPoint(int _i, float _resolution, float _invResolution)
        {
            float4x2 uv = IShape.IndexTo4UV(_i,_resolution,_invResolution);
            float4x3 p;
            p.c0 = uv.c0 - 0.5f;
            p.c1 = uv.c1 - 0.5f;
            p.c2 = 0.5f - math.abs(p.c0) - math.abs(p.c1);
            float4 offset = math.max(-p.c2, 0f);
            p.c0 += math.@select(-offset,offset,p.c0<0f);
            p.c1 += math.@select(-offset,offset,p.c1<0f);
            float4 scale = 0.5f * math.rsqrt(p.c0 * p.c0 + p.c1 * p.c1 + p.c2 * p.c2);
            p.c0 *= scale;
            p.c1 *= scale;
            p.c2 *= scale;
            return new Point4(){positions = p,normals = p};
        }
    }

    struct Torus : IShape
    {
        public Point4 GetPoint(int _i, float _resolution, float _invResolution)
        {            
            float4x2 uv = IShape.IndexTo4UV(_i,_resolution,_invResolution);
            float r1 = 0.375f;
            float r2 = 0.125f;
            float4 s = r1 + r2 * math.cos(KMath.kPI2 * uv.c1);
            float4x3 p;
            p.c0 = s * math.sin(KMath.kPI2 * uv.c0);
            p.c1 = r2 * math.sin(KMath.kPI2 * uv.c1);
            p.c2 = s * math.cos(KMath.kPI2 * uv.c0);

            float4x3 n = p;
            n.c0 -= r1 * math.sin(KMath.kPI2 * uv.c0);
            n.c2 -= r1 * math.cos(KMath.kPI2 * uv.c0);
            return new Point4(){positions = p,normals = n};
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct ShapeJob<T> : IJobFor where T:struct,IShape
    {
        [WriteOnly] public NativeArray<float3x4> positions;
        [WriteOnly] public NativeArray<float3x4> normals;
        public int resolution;
        public float invResolution;
        public HomogeneousCoordinateTransformMatrix transform;

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
    struct HashJob : IJobFor
    {
        public SmallXXHash4 hash;
        [ReadOnly] public NativeArray<float3x4> positions;
        public HomogeneousCoordinateTransformMatrix transform;
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