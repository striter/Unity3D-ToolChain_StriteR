using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using TechToys.ThePlanet.Module.BOIDS;
using TechToys.ThePlanet.Module.BOIDS.Butterfly;
using TPool;
using TObjectPool;
using UnityEngine;

namespace TechToys.ThePlanet.Module.Prop
{
    public class ModulePropContainer : APoolBehaviour<PCGID>,IModuleStructureElement,IButterflyAttractions
    {
        public IVoxel m_Voxel { get; private set; }
        public float m_Random { get; private set;}
        public readonly Dictionary<int, ModulePropCollapseData> m_PropIndexes = new();
        public readonly List<ModulePropElement> m_Props = new();
        public int Identity => identity.GetIdentity(DModule.kIDProp);
        public Action<int> SetDirty { get; set; }
        public List<FBoidsVertex> m_ButterflyPositions { get; } = new();
        public void Init(IVoxel _voxel)
        {
            m_Voxel = _voxel;
            transform.SyncPositionRotation(_voxel.transform);
            m_Random = Noise.Value.Unit1f1(m_Voxel.Identity.GetHashCode());//float)m_Voxel.Identity.location.x/ int.MaxValue,(float)m_Voxel.Identity.location.y/int.MaxValue,(float)m_Voxel.Identity.height/byte.MaxValue);
            
        }
        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Voxel = null;
            m_PropIndexes.Clear();
            foreach (var prop in m_Props)
                prop.Recycle();
            m_Props.Clear();
        }

        public void BeginCollapse(GameObjectPool<ModulePropElement> _propPool)
        {
            TSPoolList<int>.Spawn(out var emptyTypes);
            foreach (var type in m_PropIndexes.Keys)
            {
                if(!m_Voxel.m_TypedCluster.TryGetValue(type,out var clusterByte) || clusterByte==byte.MinValue)
                    emptyTypes.Add(type);
            }

            if (emptyTypes.Count > 0)
            {
                foreach (var remove in emptyTypes)
                    m_PropIndexes[remove]= ModulePropCollapseData.Invalid;
                RefreshProps(_propPool);
            }
            
            TSPoolList<int>.Recycle(emptyTypes);
        }
        
        public void Finalize(int _type,ModulePropCollapseData _propCollapse,GameObjectPool<ModulePropElement> _elementPool)
        {
            if(!m_PropIndexes.ContainsKey(_type))
                m_PropIndexes.Add(_type,ModulePropCollapseData.Invalid);

            if (m_PropIndexes[_type] == _propCollapse)
                return;
            m_PropIndexes[_type] = _propCollapse;
            RefreshProps(_elementPool);
        }

        void RefreshProps(GameObjectPool<ModulePropElement> _propPool)
        {
            foreach (var prop in m_Props)
                prop.TryRecycle();
            m_Props.Clear();

            foreach (var propType in m_PropIndexes.Keys)
            {
                var propData = m_PropIndexes[propType];
                if(!propData.Available)
                    continue;

                var moduleSet = DModule.Collection.m_ModuleLibrary[propType];
                if (!moduleSet.m_Decorations.decorationSets[propData.propIndex].GetOrientedProp(moduleSet.m_ClusterType,propData.propByte, out var decorationData, out var orientation))
                    continue;

                var propSet = decorationData.possibilities[(int)(m_Random*decorationData.possibilities.Length)];
                foreach (var prop in propSet.props)
                {
                    var propElement= _propPool.Spawn().Init(m_Voxel,prop,orientation,DModule.Collection.m_MeshLibrary,DModule.Collection.m_MaterialLibrary,DModule.EmissionColors.RandomElement());
                    propElement.transform.SetParent(transform);
                    m_Props.Add(propElement);
                }
            }
            
            m_Props.Collect(p => p.m_Type == EModulePropType.Flower).Select(
                p =>new FBoidsVertex() {
                    position = p.transform.position,rotation = p.transform.rotation
                }).FillList(m_ButterflyPositions);
            SetDirty(Identity);
        }
        

        public void TickLighting(float _deltaTime,Vector3 _lightDir)
        {
            float ndl = Vector3.Dot(transform.up, _lightDir);
            foreach (var prop in m_Props)
                prop.TickLighting(_deltaTime,ndl);
        }
    }

}