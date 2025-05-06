using System;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.PhysicsScenes.Particle
{
        public interface ISPH
        {
            public float Radius { get; }
            double this[float _distance] { get; }
            double FirstDerivative(float _distance);
            double SecondDerivative(float _distance);
        }


        public static class ISPH_Extension
        {
            public static float3 Gradient(this ISPH _sph,float _distance, float3 _directionToCenter) => -(float)_sph.FirstDerivative(_distance) * _directionToCenter;
        }
        
        public struct SPHStdKernel3 : ISPH
        {
            public float h;
            public float Radius => h;
            private double h2;
            private double h3;
            private double h5;
            public SPHStdKernel3(float _radius)
            {
                h = _radius;
                h2 = h * h;
                h3 = h * h * h;
                h5 = h * h * h * h * h;
            }

            public double this[float _distance]
            {
                get
                {
                    var x = 1f - _distance * _distance / h2;
                    return 315 / (64f * kmath.kPI * h3) * x * x * x;
                }
            }

            public double FirstDerivative(float _distance)
            {
                if (_distance >= h)
                    return 0f;
                var x = 1.0f - _distance * _distance / h2;
                return -945.0f / (32f * kmath.kPI * h5) * _distance * x * x;
            }

            public double SecondDerivative(float _distance)
            {
                if (_distance > h)
                    return 0f;
                var x = _distance * _distance / h2;
                return 945f / (32f * kmath.kPI * h5) * (1 - x) * (3 * x - 1);
            }
        }
        
        [Serializable]
        public struct SPHSpikyKernel : ISPH
        {
            public float h;
            public float Radius => h;
            private double h2;
            private double h3;
            private double h4;
            private double h5;
            public SPHSpikyKernel(float _radius)
            {
                h = _radius;
                h2 = h * h;
                h3 = h * h * h;
                h4 = h * h * h * h;
                h5 = h * h * h * h * h;
            }

            public double this[float _distance]
            {
                get
                {
                    if (_distance >= h)
                        return 0f;
                    var x = 1.0 - _distance / h;
                    return (15 / (kmath.kPI * h3) * x * x * x);
                }
            }

            public double FirstDerivative(float _distance)
            {
                if (_distance >= h)
                    return 0f;
                var x = 1.0 - _distance / h;
                return (float)(-45f / (kmath.kPI * h4) * x * x);
            }

            public double SecondDerivative(float _distance)
            {
                if (_distance >= h)
                    return 0f;
                
                var x = 1.0 - _distance / h;
                return (float)(90f / (kmath.kPI * h5) * x);
            }
        }
}