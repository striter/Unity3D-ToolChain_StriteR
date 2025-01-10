using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Procedural.Tile;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Examples.PhysicsScenes.WaveSimulation
{
    [Serializable]
    public struct Wave
    {
        public float initialTime;
        public float amplitude;
        public float frequency;
        public float speed;
        public static Wave kDefault => new Wave { amplitude = 1, frequency = 0.5f, initialTime = 0, speed = 1 };

        public void Tick(float _deltaTime, float _time) => initialTime += _deltaTime * speed;
    }
    
    
    [ExecuteInEditMode]
    public class WaveSimulation : MonoBehaviour
    {
        private List<Wave> m_Waves = new List<Wave>();

        public ColorPalette m_Colors = new ColorPalette();
        Texture2D m_Texture;
        private FTextureDrawer m_Drawer;
        private RawImage m_Image;
        private void OnEnable()
        {
            var canvas = GetComponentInChildren<Canvas>();
            m_Drawer = new FTextureDrawer(new int2(canvas.pixelRect.size), Color.clear);
            m_Texture = new Texture2D(m_Drawer.SizeX,m_Drawer.SizeY, TextureFormat.ARGB32,false) { name = "Wave Texture",filterMode = FilterMode.Point };
            m_Image = GetComponentInChildren<RawImage>();
            m_Image.texture = m_Texture;
        }
        
        private void OnDestroy()
        {
            if (m_Image == null)
                return;
            
            m_Image.texture = null;
            Destroy(m_Texture);
            m_Texture = null;
        }

        [Range(0,500)]public int sampleInterval = 200;
        private void Update()
        {
            m_Drawer.Clear(Color.clear);
            for (var i = 0; i < sampleInterval + 1; i++)
            {
                var timeInterval = (float)i / sampleInterval;
                var sample = 0f;
                sample += math.sin( timeInterval * kmath.kPI2);

                var normalized = new float2(timeInterval, sample);
                m_Drawer.PixelContinuous((int2)(normalized * m_Drawer.size),m_Colors.Evaluate(timeInterval));
            }
            
            m_Texture.SetPixels(m_Drawer.colors);
            m_Texture.Apply();
        }
    }

}
