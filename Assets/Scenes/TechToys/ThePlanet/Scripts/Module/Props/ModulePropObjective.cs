using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using TechToys.ThePlanet.Module.Cluster;
using TPool;
using TObjectPool;
using UnityEngine;

namespace TechToys.ThePlanet.Module.Prop
{
    public class ModulePathCollapse
    {
        public bool m_Collapsed { get; private set; }
        public Quad<bool> m_Result { get; private set; }
        public int m_Priority { get; private set; }
        public float m_Random { get; private set; }
        public readonly List<Quad<bool>> m_Possibilities = new List<Quad<bool>>();
        public IVoxel m_Voxel { get; private set; }
        public ModulePathCollapse Init(IVoxel _voxel)
        {
            m_Voxel = _voxel;
            m_Collapsed = false;
            m_Result = KQuad.kFalse;

            m_Priority = _voxel.Identity.GetHashCode(); 
            m_Random = Noise.Value.Unit1f1((float)m_Priority/ int.MaxValue);
            return this;
        }

        public void Fill(Dictionary<PCGID,ModulePathCollapse> _voxels)
        {
            m_Possibilities.Clear();
            foreach (var fillPossibility in DModuleProp.kAllPossibilities)
            {
                bool skip = false;
                for (int i = 0; i < 4; i++)
                {
                    if(!fillPossibility[i])
                        continue;
                    
                    if (!m_Voxel.m_CubeSidesExists.IsFlagEnable(i)||!_voxels.ContainsKey(m_Voxel.m_CubeSides[i]))
                    {
                        skip = true;
                        break;
                    }
                }

                if (skip)
                    continue;
                m_Possibilities.Add(fillPossibility);
            }
        }

        public void Collapse()
        {
            // var index = m_Possibilities.MaxIndex(p=>p.ToPossibilityPriority());
            
            m_Result = m_Possibilities.SelectPossibility(m_Random);
            m_Possibilities.Clear();
            m_Collapsed = true;
        }

        public bool Propaganda(Dictionary<PCGID,ModulePathCollapse> _voxels)
        {
            if (m_Collapsed)
                return false;
            TSPoolStack<Quad<bool>>.Spawn(out var invalidPossibilities);
            for (int facing = 0; facing < 4; facing++)
            {
                if(!_voxels.TryGetValue(m_Voxel.m_CubeSides[facing],out var sideCollapse))
                    continue;
                int opposite = sideCollapse.m_Voxel.m_CubeSides.FindIndex(p=>p==m_Voxel.Identity);

                if (sideCollapse.m_Collapsed)
                {
                    bool oppositeConnection = sideCollapse.m_Result[opposite];
                    foreach (var possibility in m_Possibilities)
                    {
                        if(oppositeConnection==possibility[facing])
                            continue;
                        invalidPossibilities.TryPush(possibility);
                    }
                }
                else
                {
                    var oppositeConnection = sideCollapse.m_Possibilities.Any(p => p[opposite]);
                    var oppositeDeConnection = sideCollapse.m_Possibilities.Any(p => !p[opposite]);
                    foreach (var possibility in m_Possibilities)
                    {
                        var facingConnection = possibility[facing];
                        
                        if (!oppositeConnection && facingConnection)
                            invalidPossibilities.TryPush(possibility);
                        
                        if(!oppositeDeConnection && !facingConnection)
                            invalidPossibilities.TryPush(possibility);
                    }
                }
            }

            var propagandaValidate = invalidPossibilities.Count > 0;
            m_Possibilities.RemoveRange(invalidPossibilities);
            TSPoolStack<Quad<bool>>.Recycle(invalidPossibilities);
            return propagandaValidate;
        }
    }

    public struct ModulePropCollapseData
    {
        public int propIndex;
        public byte propByte;
        public bool Available => propIndex >= 0;
        public static readonly ModulePropCollapseData Invalid = new ModulePropCollapseData(){propIndex = -1,propByte = byte.MinValue};

        public static bool operator ==(ModulePropCollapseData _src, ModulePropCollapseData _dst) => _src.propByte == _dst.propByte && _src.propIndex == _dst.propIndex;
        public static bool operator !=(ModulePropCollapseData _src, ModulePropCollapseData _dst) => _src.propByte != _dst.propByte && _src.propIndex != _dst.propIndex;
        public bool Equals(ModulePropCollapseData other) => propIndex == other.propIndex && propByte == other.propByte;
        public override bool Equals(object obj) => obj is ModulePropCollapseData other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(propIndex, propByte);

    }
    public class VoxelPropCollapse
    {
        public int m_Type { get; private set; }
        public IVoxel m_Voxel { get; private set; }
        public bool m_Collapsed { get; private set; }
        public ModulePropCollapseData m_Result { get; private set; }
        public int m_Priority { get; private set; }
        public byte m_MaskByte { get; private set; } 
        public byte m_BaseByte => m_Voxel.m_TypedCluster[m_Type];
        public byte VoxelByte=> (byte)(m_BaseByte & m_MaskByte);
        public VoxelPropCollapse Init(int _type,IVoxel _voxel)
        {
            m_Type = _type;
            m_Voxel = _voxel;
            m_Priority = m_Voxel.Identity.GetHashCode();
            m_Collapsed = false;
            m_Result = ModulePropCollapseData.Invalid;
            m_MaskByte = byte.MaxValue;
            return this;
        }
        public bool Available(EClusterType _clusterType,uint[] _propMasks)
        {
            if (m_Collapsed)
                return false;

            var index = UModulePropByte.GetOrientedPropIndex(_clusterType,VoxelByte).index;
            if (index == -1)
                return false;
            var maskIndex = index / 32;
            var readMask = 1 << (index - maskIndex*32);
            return (_propMasks[maskIndex] & readMask) == readMask;
        }

        public void Collapse(int _index)
        {
            m_Result = new ModulePropCollapseData()
            {
                propIndex = _index,
                propByte = VoxelByte,
            };
            m_Collapsed = true;
        }

        private static readonly byte[] kPropMasks = {
            new Qube<bool>(
                true,false,true,true,
                true,false,true,true).ToByte(),            
            new Qube<bool>(
                true,true,false,true,
                true,true,false,true).ToByte(),            
            new Qube<bool>(
                true,true,true,false,
                true,true,true,false).ToByte(),            
            new Qube<bool>(
                false,true,true,true,
                false,true,true,true).ToByte(),
        };  //Da f*ck
        public void AppendCollapseMask(PCGID _maskVoxel)
        {
            var facing =m_Voxel.m_CubeSides.FindIndex(p => p == _maskVoxel);
            m_MaskByte = (byte) ( m_MaskByte & kPropMasks[facing]);
        }
    }
    
    public class ModulePropElement:APoolTransform<int>
    {
        public EModulePropType m_Type { get; private set; }
        private readonly MeshRenderer m_MeshRenderer;
        private readonly MeshFilter m_MeshFilter;
        
        private bool m_Show;
        private Vector3 m_Scale;
        private Counter m_AnimationCounter = new Counter(.25f,true);

        private readonly MaterialPropertyBlock m_PropertyBlock;
        private bool m_Emissive;
        private Color m_EmissionColor;
        private Counter m_EmissionCounter = new Counter(.25f,true);
        
        public ModulePropElement(Transform _transform):base(_transform)
        {
            m_MeshFilter = _transform.gameObject.AddComponent<MeshFilter>();
            m_MeshRenderer = _transform.gameObject.AddComponent<MeshRenderer>();
            m_PropertyBlock = new MaterialPropertyBlock();
        }

        public ModulePropElement Init(IVoxel _voxel, ModulePropData _propData,int _orientation, IList<Mesh> _meshLibrary, IList<Material> _materialLibrary,Color _color)
        {
            m_Type = _propData.type;
            m_Scale = _propData.scale;
            transform.gameObject.name = _voxel.Identity.ToString();
            m_MeshFilter.sharedMesh = _meshLibrary[_propData.meshIndex];
            m_MeshRenderer.sharedMaterials = _propData.embedMaterialIndex.Select(p=>_materialLibrary[p]).ToArray();

            DModuleProp.OrientedToObjectVertex(_orientation,_propData.position,_voxel.m_ShapeOS,out var objectPosition,  _propData.rotation, out var objectRotation);
            transform.position = _voxel.transform.localToWorldMatrix.MultiplyPoint(objectPosition);
            transform.rotation = _voxel.transform.rotation * objectRotation;
            transform.localScale = Vector3.zero;
            
            m_Show = true;
            m_AnimationCounter.Replay();
            
            m_EmissionCounter.Replay();
            m_Emissive = false;
            m_EmissionColor = _color;
            m_PropertyBlock.SetColor(KShaderProperties.kEmissionColor,Color.black);
            m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
            return this;
        }

        public void TryRecycle()
        {
            m_Show = false;
            m_AnimationCounter.Replay();
        }
        
        public bool TickRecycle(float _deltaTime)
        {
            if (!m_AnimationCounter.Playing)
                return false;
            m_AnimationCounter.Tick(_deltaTime);
            transform.localScale = (m_Show?m_AnimationCounter.TimeElapsedScale:m_AnimationCounter.TimeLeftScale)*m_Scale*KPCG.kUnitSize*2;
            if (!m_AnimationCounter.Playing&&!m_Show)
                return true;
            return false;
        }

        private static readonly RangeFloat kRandomEmission = new RangeFloat(.2f, 1.8f);
        public void TickLighting(float _deltaTime,float _ndl)
        {
            float lightParam = _ndl;
            bool enable = lightParam > .3f;
            if (m_Emissive != enable)
            {
                m_Emissive = enable;
                m_EmissionCounter.Set(kRandomEmission.Random());
            }

            if (!m_EmissionCounter.TickTrigger(_deltaTime))
                return;
            
            if (m_Type == EModulePropType.Light)
                m_MeshRenderer.enabled = enable;

            m_PropertyBlock.SetColor(KShaderProperties.kEmissionColor,m_Emissive?m_EmissionColor:Color.black);
            m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
        }

    }
}