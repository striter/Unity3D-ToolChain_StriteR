using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using TPool;
using TObjectPool;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.ThePlanet.Module
{
    using static KPCG.Ocean;
    using static KModuleStatic;
    internal static class KModuleStatic
    {
        public static GTriangle kBoatLeverage = GTriangle.kDefault;
    }
    
    public class ModuleStatic : MonoBehaviour , IModuleControl , IModuleQuadCallback
    {
        [Header("Foliage")]
        [Range(0,1)]public float m_Density;
        [Clamp(0f,float.MaxValue)]public float m_Scale;
        [Header("Boat")] [Range(0, 1)] 
        public float m_BoatDensity = 0.99f;
        public GTriangle m_BoatLeverage = GTriangle.kDefault;
        
        public GridManager m_Grid { get; set; }
        private ObjectPoolClass<GridID, StaticFoliage> m_Foliage;
        private ObjectPoolClass<GridID, StaticBoat> m_Boat;
        
        public void Init()
        {
            m_Foliage = new ObjectPoolClass<GridID, StaticFoliage>(transform.Find("Foliage/Item"));
            m_Boat = new ObjectPoolClass<GridID, StaticBoat>(transform.Find("Boat/Item"));
            kBoatLeverage = m_BoatLeverage;
        }

        public void Setup()
        {
            m_Foliage.Clear();
            m_Boat.Clear();
            foreach (var quad in m_Grid.m_Quads)
            {
                var position = (quad.Value.position/KPCG.kGridSize + kfloat3.one)*m_Scale;
                var randomFoliage = UNoise.Perlin.Unit1f3(position) / 2 + .5f;
                if (m_Density > randomFoliage)
                    m_Foliage.Spawn(quad.Key).Init(quad.Value);
                
                var randomBoat = UNoise.Perlin.Unit1f3(position) / 2 + .5f;
                if (randomBoat > m_BoatDensity)
                {
                    m_Boat.Spawn(quad.Key).Init(quad.Value);
                }
            }
        }

        private bool dirty = false;
        private void OnValidate()
        {
            if (m_Grid == null)
                return;
            dirty = true;
        }

        public void Tick(float _deltaTime)
        {
            if (dirty)
            {
                Setup();
                dirty = false;
            }

            m_Boat.Traversal(p=>p.Tick(_deltaTime));
        }

        public void OnPopulateQuad(IQuad _quad)
        {
            if(m_Foliage.TryGet(_quad.Identity,out var foliage))
                foliage.Disable();
            if(m_Boat.TryGet(_quad.Identity,out var boat))
                boat.Disable();
        }

        public void OnDeconstructQuad(GridID _quadID)
        {
            if(m_Foliage.TryGet(_quadID,out var foliage))
                foliage.Enable();
            if(m_Boat.TryGet(_quadID,out var boat))
                boat.Enable();
        }
        
        public void Clear()
        {
        }

        public void Dispose()
        {
        }

        public abstract class StaticElement:APoolTransform<int>
        {
            protected GameObject m_Model { get; private set; }
            protected StaticElement(Transform _transform) : base(_transform)
            {
                m_Model = _transform.Find("Model").gameObject;
            }
            
            public virtual StaticElement Init(PCGQuad _quad)
            {
                return this;
            }
            
            public void Enable()
            {
                m_Model.SetActive(true);
            }

            public void Disable()
            {
                m_Model.SetActive(false);
            }
        }
        
        public class StaticFoliage: StaticElement
        {
            public StaticFoliage(Transform _transform) : base(_transform)
            {
            }

            public override StaticElement Init(PCGQuad _quad)
            {
                Vector2 randomPos = URandom.Random2DSphere();
                transform.SetPositionAndRotation(_quad.m_ShapeWS.GetPoint(randomPos.x,randomPos.y),_quad.rotation);
                transform.localScale = Vector3.one * (RangeFloat.k01.Random() * .2f + .8f);
                return base.Init(_quad);
            }
        }

        public class StaticBoat : StaticElement
        {
            private float3 m_NPositionWS;
            private Quaternion m_Rotation;
            private Matrix4x4 m_ObjectToWorld;
            public StaticBoat(Transform _transform) : base(_transform)
            {
            }

            public override StaticElement Init(PCGQuad _quad)
            {
                m_NPositionWS = _quad.m_ShapeWS.GetPoint(.5f, .5f).normalized ;
                m_Rotation = _quad.rotation;
                m_Model.transform.SetPositionAndRotation(m_NPositionWS* kOceanRadius,m_Rotation);
                return base.Init(_quad);
            }
            
            public void Tick(float _deltaTime)
            {
                float time = Time.time;
                var objectToWorld = Matrix4x4.TRS(m_NPositionWS * kOceanRadius,m_Rotation,m_Model.transform.lossyScale);

                var positions = objectToWorld * kBoatLeverage;

                GTriangle rotatedTriangle = (GTriangle)positions.Convert(p=>OutputOceanCoordinates(p.normalize(),time));
                m_Model.transform.SetPositionAndRotation(rotatedTriangle.GetPoint(.25f),rotatedTriangle.GetRotation());
            }
        }
    }
}