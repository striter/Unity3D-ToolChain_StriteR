using System;
using UnityEngine;
using Unity.Mathematics;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Geometry.Extension.Sphere;

namespace Examples.Algorithm.SamplePatternVisualize
{
    public enum ESamplePattern
    {
        Grid,
        Stratified,
        Halton,
        HammersLey,
        Sobol,
        PoissonDisk,
    }

    public enum EVisualMode
    {
        Flat,
        Repeated,
        Sphere,
    }
    
    public class LowDiscrepancySequenceVisualize : MonoBehaviour
    {
        public ESamplePattern patternType = ESamplePattern.Grid;
        public int patternWidth=4,patternHeight=4;
        public EVisualMode m_Mode = EVisualMode.Flat;
        [MFoldout(nameof(patternType), ESamplePattern.PoissonDisk)] public Texture2D m_Texture;
        [Readonly] public float2[] patterns;
        [Range(0, 1f)] public float gizmosRadius = 0.03f;
        
        [InspectorButton]
        void Generate()
        {
            patterns = patternType switch
            {
                ESamplePattern.Grid => ULowDiscrepancySequences.Grid2D(patternWidth, patternHeight),
                ESamplePattern.Stratified => ULowDiscrepancySequences.Stratified2D(patternWidth, patternHeight, true),
                ESamplePattern.Halton => new float2[patternWidth * patternHeight].Remake((i, p) =>
                    ULowDiscrepancySequences.Halton2D((uint)i) - .5f),
                ESamplePattern.HammersLey => new float2[patternWidth * patternHeight].Remake((i, p) =>
                    ULowDiscrepancySequences.Hammersley2D((uint)i, (uint)(patternWidth * patternHeight)) - .5f),
                ESamplePattern.Sobol => ULowDiscrepancySequences.Sobol2D((uint)(patternWidth * patternHeight)),
                ESamplePattern.PoissonDisk => m_Texture != null
                    ? ULowDiscrepancySequences.PoissonDisk2D(patternWidth*patternHeight, 30, null,
                        val => math.lerp(3f, 1f,
                            UColor.RGBtoLuminance(m_Texture.GetPixel((int)(val.x * m_Texture.width),
                                    (int)(val.y * m_Texture.height))
                                .to3())))
                    : ULowDiscrepancySequences.PoissonDisk2D(patternWidth, patternHeight),
                _ => patterns
            };
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            var size = patterns.Length;

            switch (m_Mode)
            {
                case EVisualMode.Flat:
                {
                    Gizmos.color = Color.white;
                    for (var k = 0; k < size; k++)
                        Gizmos.DrawSphere(patterns[k].to3xz(),gizmosRadius);
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(Vector3.zero,Vector3.one.SetY(0f));
                }
                    break;
                case EVisualMode.Repeated:
                {
                    for (var i = -2; i < 2; i++)
                    {
                        for (var j = -2; j < 2; j++)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.matrix = Matrix4x4.Translate(new Vector3(i+.5f,0,j+.5f));
                            Gizmos.DrawWireCube(Vector3.zero,Vector3.one.SetY(0f));
                            Gizmos.color = Color.red;
                            for (var k = 0; k < size; k++)
                            {
                                var pattern = patterns[k];
                                Gizmos.DrawSphere(new Vector3(pattern.x,0,pattern.y),gizmosRadius);
                            }
                        }
                    }
                }
                    break;
                case EVisualMode.Sphere:
                {
                    for (var k = 0; k < size; k++)
                    {
                        var uv = patterns[k] + .5f;
                        Gizmos.color = Color.red * uv.x + Color.green * uv.y;
                        Gizmos.DrawSphere(ESphereMapping.ConcentricOctahedral.UVToSphere( uv) * .5f,gizmosRadius);
                    }
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(Vector3.zero,.5f);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
