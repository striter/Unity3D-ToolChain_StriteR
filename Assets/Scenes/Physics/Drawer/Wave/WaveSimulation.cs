using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;

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
    
    public class WaveSimulation : ADrawerSimulation
    {
        public List<Wave> m_Waves = new List<Wave>();

        public ColorPalette m_Colors = ColorPalette.kDefault;
        public RangeFloat m_Edge = new RangeFloat(0.05f,0.95f);
        [Range(0,500)]public int sampleInterval = 200;
        protected override void FixedTick(float _fixedDeltaTime)
        {
            m_Waves.Traversal(p=>p.Tick(_fixedDeltaTime,m_Edge));
            
            for (var i = 0; i < sampleInterval ; i++)
            {
                var timeInterval = (float)i / sampleInterval;
                var sample = 0f;

                foreach (var wave in m_Waves)
                    sample += wave.Evaluate(timeInterval);
            }
        }

        protected override void Draw(FTextureDrawer _drawer)
        {
            for (var i = 0; i < sampleInterval; i++)
            {
                var timeInterval = (float)i / sampleInterval;
                var sample = 0f;
                var normalized = new float2(timeInterval, sample);
                var pixel = (int2)(normalized * _drawer.size);
                if(i == 0)
                    _drawer.PixelContinuousStart(pixel);
                _drawer.PixelContinuous(pixel,m_Colors.Evaluate(timeInterval));
            }
            
            foreach (var wave in m_Waves)
            {
                var centre = (int2)(new float2(wave.position,wave.Evaluate(wave.position)) * _drawer.size);
                _drawer.Circle(centre,5, m_Colors.Evaluate(wave.position));
            }
        }
    }
}
