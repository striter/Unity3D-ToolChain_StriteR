using System;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.PhysicsScenes.WaveSimulation
{
    [ExecuteAlways]
    public abstract class ADrawerSimulation : MonoBehaviour
    {
        Texture2D m_Texture;
        protected abstract void TickDrawer(FTextureDrawer _drawer, float _deltaTime);
        private FTextureDrawer m_Drawer;
        private RawImage m_Image;
        private void OnEnable()
        {
            var canvas = GetComponentInChildren<Canvas>();
            m_Drawer = new FTextureDrawer(new int2(canvas.pixelRect.size), Color.clear);
            m_Texture = new Texture2D(m_Drawer.SizeX,m_Drawer.SizeY, TextureFormat.ARGB32,false) { name = "Wave Texture",filterMode = FilterMode.Point,wrapMode = TextureWrapMode.Clamp};
            m_Image = GetComponentInChildren<RawImage>();
            m_Image.texture = m_Texture;
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
            m_Drawer.Clear(Color.clear);
            TickDrawer(m_Drawer,deltaTime);
            
            m_Texture.SetPixels(m_Drawer.colors);
            m_Texture.Apply();
        }
    }
}