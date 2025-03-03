using System;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.PhysicsScenes
{
    [ExecuteAlways]
    public abstract class ADrawerSimulation : MonoBehaviour
    {
        [Readonly] public Ticker m_Ticker = new Ticker(1f/60f);
        Texture2D m_Texture;
        private FTextureDrawer m_Drawer;
        private RawImage m_Image;
        private void OnEnable()
        {
            var canvas = GetComponentInChildren<Canvas>();
            m_Drawer = new FTextureDrawer(new int2(canvas.pixelRect.size), Color.clear);
            m_Texture = new Texture2D(m_Drawer.SizeX,m_Drawer.SizeY, TextureFormat.ARGB32,false) { name = "Wave Texture",filterMode = FilterMode.Point,wrapMode = TextureWrapMode.Clamp};
            m_Image = GetComponentInChildren<RawImage>();
            m_Image.texture = m_Texture;
            Update();
        }
        
        private void OnDisable()
        {
            if (m_Image == null)
                return;
            
            m_Image.texture = null;
            DestroyImmediate(m_Texture);
            m_Texture = null;
        }

        private void Update()
        {
            var deltaTime = UTime.deltaTime;

            var maxSimulatePerTick = 5;
            while (maxSimulatePerTick-- > 0 && m_Ticker.Tick(deltaTime))
            {
                deltaTime = 0f;
                FixedTick(m_Ticker.duration);
            }
            m_Drawer.Clear(Color.clear);
            Draw(m_Drawer);
            m_Texture.SetPixels(m_Drawer.colors);
            m_Texture.Apply();
        }

        protected abstract void FixedTick(float _fixedDeltaTime);
        protected abstract void Draw(FTextureDrawer _drawer);
    }
}