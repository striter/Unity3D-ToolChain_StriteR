using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Extensions;
using Procedural.Tile;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Examples.PhysicsScenes.WaveSimulation
{
    [Serializable]
    public class Wave
    {
        [Range(0,1)]public float position;
        [Range(-1, 1)] public float offset;
        public float amplitude;
        public float frequency;
        public float speed;
        public int speedSign = 1;
        public static Wave kDefault => new Wave { amplitude = 1, speedSign = 1, frequency = 0.5f,  speed = 1 };

        public void Tick(float _deltaTime,RangeFloat _edge)
        {
            position += _deltaTime * speed * speedSign;

            if (!_edge.Contains(position))
                speedSign *= -1;
            position = _edge.Clamp(position);
        }

        public float Evaluate(float _x)
        {
            
            var distance = math.min(math.abs(position - _x) * frequency,.5f) ;
            var height = amplitude * .5f * (math.cos(distance * kmath.kPI2)) + offset;
            return height;
        } 
    }
    
    
    [ExecuteAlways]
    public class WaveSimulation : MonoBehaviour
    {
        public List<Wave> m_Waves = new List<Wave>();

        public ColorPalette m_Colors = new ColorPalette();
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
        public RangeFloat m_Edge = new RangeFloat(0.05f,0.95f);
        private void Update()
        {
            var deltaTime = UTime.deltaTime;
            m_Waves.Traversal(p=>p.Tick(deltaTime,m_Edge));
            
            m_Drawer.Clear(Color.clear);
            for (var i = 0; i < sampleInterval ; i++)
            {
                var timeInterval = (float)i / sampleInterval;
                var sample = 0f;

                foreach (var wave in m_Waves)
                    sample += wave.Evaluate(timeInterval);

                var normalized = new float2(timeInterval, sample);
                var pixel = (int2)(normalized * m_Drawer.size);
                if(i == 0)
                    m_Drawer.PixelContinuousStart(pixel);
                m_Drawer.PixelContinuous(pixel,m_Colors.Evaluate(timeInterval));
            }

            foreach (var wave in m_Waves)
            {
                var centre = (int2)(new float2(wave.position,wave.Evaluate(wave.position)) * m_Drawer.size);
                m_Drawer.Circle(centre,5, m_Colors.Evaluate(wave.position));
            }
            
            m_Texture.SetPixels(m_Drawer.colors);
            m_Texture.Apply();
        }
    }

}
