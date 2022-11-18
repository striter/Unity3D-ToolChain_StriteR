using System;
using System.Collections;
using System.Collections.Generic;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace PCG.Module
{
    public class ModuleFoliage : MonoBehaviour , IModuleControl , IModuleQuadCallback
    {
        [Range(0,1)]public float m_Density;
        [Clamp(0f,float.MaxValue)]public float m_Scale;
        public GridManager m_Grid { get; set; }
        public TObjectPoolClass<GridID, FoliageElement> m_Foliage;
        public void Init()
        {
            m_Foliage = new TObjectPoolClass<GridID, FoliageElement>(transform.Find("Item"));
        }

        public void Setup()
        {
            m_Foliage.Clear();
            foreach (var quad in m_Grid.m_Quads)
            {
                Vector3 position = (quad.Value.position/DPCG.kGridSize + Vector3.one)*m_Scale;
                var random = Noise.Perlin.Unit1f3(position.x, position.y, position.z) / 2 + .5f; 
                if(m_Density>random)
                    m_Foliage.Spawn(quad.Key).Init(quad.Value);
            }
        }

        private bool dirty = false;
        private void OnValidate()
        {
            if (m_Grid == null)
                return;
            dirty = true;
        }

        public void OnPopulateQuad(IQuad _quad)
        {
            if(m_Foliage.TryGet(_quad.Identity,out var foliage))
                foliage.Disable();
        }

        public void OnDeconstructQuad(GridID _quadID)
        {
            if(m_Foliage.TryGet(_quadID,out var foliage))
                foliage.Enable();
        }
        
        public void Clear()
        {
        }

        public void Tick(float _deltaTime)
        {
            if (dirty)
            {
                Setup();
                dirty = false;
            }
        }

        public void Dispose()
        {
        }

        public class FoliageElement: APoolItem<int>
        {
            private GameObject m_Model;
            public FoliageElement(Transform _transform) : base(_transform)
            {
                m_Model = _transform.Find("Model").gameObject;
            }

            public FoliageElement Init(PCGQuad _quad)
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