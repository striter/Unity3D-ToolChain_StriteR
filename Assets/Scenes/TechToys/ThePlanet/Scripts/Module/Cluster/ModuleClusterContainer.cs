using System;
using System.Collections.Generic;
using Runtime.Geometry;
using MeshFragment;
using TechToys.ThePlanet.Module.BOIDS;
using TechToys.ThePlanet.Module.BOIDS.Bird;
using TPool;
using System.Linq.Extensions;
using TObjectPool;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace TechToys.ThePlanet.Module.Cluster
{
    public class ModuleClusterContainer : PoolBehaviour<PCGID> ,IModuleStructureElement,IBirdPerchingRoot
    {
        public IVoxel m_Voxel { get; private set; }
        public Qube<ModuleClusterInputData> m_Input;
        public Qube<ModuleClusterCornerData> m_Data;
        public Qube<bool> m_Modified;
        private bool m_Dirty;
        private Mesh m_ClusterMesh;
        private MeshRenderer m_Renderer;

        public int Identity => identity.GetIdentity(DModule.kIDCluster);
        public Action<int> SetDirty { get; set; }
        public Vector3 CenterWS => transform.position;
        public List<FBoidsVertex> m_BirdLandings { get; } = new List<FBoidsVertex>();

        private Counter m_AnimationCounter = new Counter(.25f);
        private MaterialPropertyBlock m_PropertyBlock ;
        private static readonly int kProgressID = Shader.PropertyToID("_Progress");

        public override void OnPoolCreate()
        {
            base.OnPoolCreate();
            m_PropertyBlock = new MaterialPropertyBlock();
            m_ClusterMesh = new Mesh();
            m_ClusterMesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = m_ClusterMesh;
            m_Renderer = GetComponent<MeshRenderer>();
        }

        public ModuleClusterContainer Init(IVoxel _voxel)
        {
            m_Voxel = _voxel;
            transform.SyncPositionRotation(_voxel.transform);
            m_Input = new Qube<ModuleClusterInputData>(ModuleClusterInputData.kInvalid);
            m_Data = new Qube<ModuleClusterCornerData>(ModuleClusterCornerData.kInvalid);
            m_ClusterMesh.name = _voxel.Identity.ToString();
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Voxel = null;
            m_ClusterMesh.Clear();
        }
        
        public void Prepare(Dictionary<PCGID,ModuleClusterCorner> _corners)
        {
            m_Modified = KQube.kFalse;
            var targetInput = new Qube<ModuleClusterInputData>(ModuleClusterInputData.kInvalid);
            var unitStatus = DModuleCluster.CollectClusterStatus(this,_corners);
            var unitType = Qube<int>.Convert(m_Voxel.m_Corners, _p => _p?.m_Type ?? -1);
            
            for (int i = 0; i < 8; i++)
            {
                if (unitType[i] == -1)
                {
                    targetInput[i]=ModuleClusterInputData.kInvalid;
                    continue;
                }
                
                targetInput[i] = new ModuleClusterInputData()
                {
                    valid = true,
                    type = unitType[i],
                    anchorByte = m_Voxel.m_ClusterUnitBaseBytes[i],
                    relationByte =  m_Voxel.m_ClusterUnitAvailableBytes[i],
                    corner = m_Voxel.m_Corners[i],
                    status = unitStatus[i],
                };
            }
            if(!m_Input.Equals(targetInput))
                m_Dirty = true;
            m_Input = targetInput;
        }
        
        static readonly EVertexAttributeFlags kOutputs = EVertexAttributeFlags.Normal | EVertexAttributeFlags.Tangent | EVertexAttributeFlags.UV0 | EVertexAttributeFlags.Color; 
        public void Collapse()
        {
            for (int i = 0; i < 8; i++)
            {
                var data = DModule.Collection.GetOrientedClusterUnitIndex(m_Input[i]);
                if (!m_Data[i].Equals(data))
                {
                    m_Modified[i] = m_Data[i].Equals(ModuleClusterCornerData.kInvalid);
                    m_Dirty = true;
                }
                m_Data[i] = data;
            }
            if(m_Dirty)
                PopulateMesh();
            m_Dirty = false;
        }

        void PopulateMesh()
        {
            var orientedToWorld = transform.localToWorldMatrix;
            var worldToObject = transform.worldToLocalMatrix;
            TSPoolList<IMeshFragment>.Spawn(out var orientedMeshes);

            for (int i = 0; i < 8; i++)
            {
                var input = m_Input[i];
                var output = m_Data[i];
                if(!input.valid||output.index<0)
                    continue;
                
                var moduleMesh = DModule.Collection.GetClusterData(input,output);
                var moduleOrientation = output.orientation;
                
                ref var orientedShape = ref m_Voxel.m_ClusterQuads[i%4];
                var orientedRotation = Quaternion.Euler(0f, moduleOrientation * 90, 0f);

                bool birdLandingAvailable = input.status == EClusterStatus.Rooftop;
                var animAvailable = m_Modified[i];
                var corner = m_Voxel.transform.position;
                int colorAlpha = (animAvailable?1:0);
                
                for(int j=0;j<moduleMesh.m_MeshFragments.Length;j++)
                {
                    var fragmentInput = moduleMesh.m_MeshFragments[j];
                    var vertexCount = fragmentInput.vertices.Length;
                    var indexCount = fragmentInput.indexes.Length;
                    bool containsColor = fragmentInput.colors.Length>0;
                    
                    var fragmentOutput = ObjectPool<FMeshFragmentObject>.Spawn().Initialize(fragmentInput.embedMaterial);
                    for (int k = 0; k < vertexCount; k++)
                    {
                        Vector3 positionMS = fragmentInput.vertices[k];
                        
                        Vector3 normalOS = orientedRotation * fragmentInput.normals[k] ;
                        Vector3 positionOS = DModuleCluster.ModuleToObjectVertex(orientedShape, moduleOrientation,  positionMS,i/4-1);
                        Vector3 positionWS = orientedToWorld.MultiplyPoint(positionOS);

                        fragmentOutput.vertices.Add(worldToObject.MultiplyPoint(positionWS));
                        fragmentOutput.normals.Add(normalOS);

                        var srcTangent = fragmentInput.tangents[k];
                        var tangentOS = orientedRotation * srcTangent.XYZ();
                        fragmentOutput.tangents.Add(tangentOS.ToVector4(srcTangent.w));

                        if (birdLandingAvailable)
                        {
                            Vector3 normalWS = orientedToWorld.MultiplyVector(normalOS);
                            if (Vector3.Dot(normalWS, transform.up) > .95f)
                            {
                                Vector3 rightWS = orientedToWorld.MultiplyVector(tangentOS);
                                m_BirdLandings.Add(new FBoidsVertex {position = positionWS,rotation =  Quaternion.LookRotation( rightWS,normalWS)});
                            }
                        }

                        fragmentOutput.colors.Add(containsColor?fragmentInput.colors[k].SetA(colorAlpha):new Color(1,1,1,colorAlpha));
                        fragmentOutput.uvs.Add(fragmentInput.uvs[k]);
                    }

                    for (int k = 0; k < indexCount; k += 3)
                    {
                        var index0 = fragmentInput.indexes[k];
                        var index1 = fragmentInput.indexes[k + 1];
                        var index2 = fragmentInput.indexes[k + 2];
                        fragmentOutput.indexes.Add(index0);
                        fragmentOutput.indexes.Add(index1);
                        fragmentOutput.indexes.Add(index2);
                    }
                    
                    orientedMeshes.Add( fragmentOutput);
                }
            }
                
            m_AnimationCounter.Replay();
            UMeshFragment.Combine(orientedMeshes,m_ClusterMesh,DModule.Collection.m_MaterialLibrary,out var materials,kOutputs);
            m_Renderer.sharedMaterials = materials;
            SetDirty(Identity);
            TSPoolList<IMeshFragment>.Recycle(orientedMeshes);
        }
        
        public void TickLighting(float _deltaTime, Vector3 _lightDir)
        {
            if (!m_AnimationCounter.Playing)
                return;
            m_AnimationCounter.Tick(_deltaTime);
            m_PropertyBlock.SetFloat(kProgressID,m_AnimationCounter.TimeLeftScale);
            m_Renderer.SetPropertyBlock(m_PropertyBlock);
        }
#if UNITY_EDITOR
        private readonly Vector3 kClusterCube = Vector3.one * .3f;
        public void DrawGizmos(bool _quad)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, kClusterCube);

            if (!_quad||m_Voxel == null)
                return;
            m_Voxel.m_ClusterQuads.Traversal(p=>p.DrawGizmos());
        }

        private void OnDrawGizmosSelected()
        {
            if (m_Voxel == null)
                return;

            if (Selection.activeObject != this.gameObject)
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero,.1f);
            int index = 0;
            foreach (var pair in m_Voxel.m_TypedCluster)
            {
                Gizmos.color = UColor.IndexToColor(pair.Key);
                var orientedIndex=UModuleByte.kByteOrientation[pair.Value];
                UGizmos.DrawString($"{pair.Key}|{orientedIndex._byte},{orientedIndex._orientation}", Vector3.up*.15f*(2+index++));
            }
            for (int i = 0; i < 8; i++)
            {
                if (m_Voxel.m_Corners[i] == null)
                    continue;

                var localQuad = m_Voxel.m_ClusterQuads[i % 4];
                var qubeCenterLS = DModuleCluster.ModuleToObjectVertex(localQuad, 0, Vector3.one * .5f);
                Gizmos.color = UColor.IndexToColor(m_Voxel.m_Corners[i].m_Type);
                Gizmos.DrawLine(Vector3.zero,qubeCenterLS);
                Gizmos.DrawSphere(qubeCenterLS,.02f);
                
                //Draw Visualize Cubes
                var input = m_Input[i];
                var anchorByte = input.anchorByte.ToQube();        //Same
                var maskByte = input.relationByte.ToQube();
                // var availableByte = ((byte) (input.baseByte | input.mixableByte)).ToQube();
                var outputByte = UModuleByte.kByteOrientation[input.anchorByte];

                // anchorByte = anchorByte.RotateYawCW((ushort) ((4-outputByte._orientation) % 4));
                // availableByte = availableByte.RotateYawCW((ushort) ((4-outputByte._orientation) % 4));
                UGizmos.DrawString($"Qube Ind:{i}\nType:{input.type},{input.status}\nAnchor:{outputByte._byte},Ort:{outputByte._orientation}",qubeCenterLS, 0f);
                var localQube = new Qube<Vector3>();
                for (int j = 0; j < 8; j++)
                    localQube[j]=qubeCenterLS+(localQuad.positions[j%4] - qubeCenterLS).setY(0f)*.25f + (-.5f+j/4)*kfloat3.up*.5f;   //Da fk
                Gizmos.color = Color.white;
                ((GQube)localQube).DrawGizmos();
                for (int j = 0; j < 8; j++)
                {
                    // if (availableByte[j])
                    // {
                    //     Gizmos.color = Color.white;
                    //     Gizmos.DrawSphere(center,0.03f);
                    // }

                    if (anchorByte[j])
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(localQube[j],.03f);
                    }

                    Gizmos.color = maskByte[j]?KColor.kOrange:Color.red.SetA(.3f);
                    Gizmos.DrawWireSphere(localQube[j] ,.05f);
                }
            }
        }
#endif
    }   
}
