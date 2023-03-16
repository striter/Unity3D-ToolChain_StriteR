using System;
using System.Collections;
using System.Collections.Generic;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace TechToys.ThePlanet.Module
{
    public class ModuleStatic : MonoBehaviour , IModuleControl , IModuleQuadCallback
    {
        [Header("Foliage")]
        [Range(0,1)]public float m_Density;
        [Clamp(0f,float.MaxValue)]public float m_Scale;
        [Header("Boat")] [Range(0, 1)] 
        public float m_BoatDensity = 0.99f;
        
        public GridManager m_Grid { get; set; }
        private ObjectPoolClass<GridID, StaticElement> m_Foliage,m_Boat;
        
        public void Init()
        {
            m_Foliage = new ObjectPoolClass<GridID, StaticElement>(transform.Find("Foliage/Item"));
            m_Boat = new ObjectPoolClass<GridID, StaticElement>(transform.Find("Boat/Item"));
        }

        public void Setup()
        {
            m_Foliage.Clear();
            m_Boat.Clear();
            foreach (var quad in m_Grid.m_Quads)
            {
                var position = (quad.Value.position/DPCG.kGridSize + Vector3.one)*m_Scale;
                var randomFoliage = UNoise.Perlin.Unit1f3(position) / 2 + .5f;
                if (m_Density > randomFoliage)
                    m_Foliage.Spawn(quad.Key).Init(quad.Value);
                
                var randomBoat = UNoise.Perlin.Unit1f3(position) / 2 + .5f;
                if (UNoise.Perlin.Unit1f3(position) > m_BoatDensity)
                    m_Boat.Spawn(quad.Key).Init(quad.Value);
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

        public class StaticElement: APoolTransform<int>
        {
            private GameObject m_Model;
            public StaticElement(Transform _transform) : base(_transform)
            {
                m_Model = _transform.Find("Model").gameObject;
            }

            public StaticElement Init(PCGQuad _quad)
            {
                Vector2 randomPos = URandom.Random2DSphere();
                Transform.SetPositionAndRotation(_quad.m_ShapeWS.GetPoint(randomPos.x,randomPos.y),_quad.rotation);
                Transform.localScale = Vector3.one * (RangeFloat.k01.Random() * .2f + .8f);
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

    }
}