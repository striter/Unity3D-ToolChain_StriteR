using System;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Geometry.Extension.Sphere;
using Runtime.Random;

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
        BCCLattice,
        BCCLattice3D,
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
        [Foldout(nameof(patternType), ESamplePattern.PoissonDisk)] public Texture2D m_Texture;
        [Foldout(nameof(patternType), ESamplePattern.PoissonDisk)] public string m_Seed = "Test";
        [Readonly] public float3[] patterns;
        [Range(0, 1f)] public float gizmosRadius = 0.03f;
        
        [InspectorButton(true)]
        void Generate()
        {
            var randomGenerator = !string.IsNullOrEmpty(m_Seed) ? new LCGRandom(m_Seed.GetHashCode()) : null;
            patterns = patternType switch
            {
                ESamplePattern.Grid => ULowDiscrepancySequences.Grid2D(patternWidth, patternHeight).Select(p=>(p-.5f).to3xz()).ToArray(),
                ESamplePattern.Stratified => ULowDiscrepancySequences.Stratified2D(patternWidth, patternHeight, true).Select(p=>(p-.5f).to3xz()).ToArray(),
                ESamplePattern.Halton => new float3[patternWidth * patternHeight].Remake((i, p) => (ULowDiscrepancySequences.Halton2D((uint)i) - .5f).to3xz()),
                ESamplePattern.HammersLey => new float3[patternWidth * patternHeight].Remake((i, p) => (ULowDiscrepancySequences.Hammersley2D((uint)i, (uint)(patternWidth * patternHeight))-.5f).to3xz()),
                ESamplePattern.Sobol => ULowDiscrepancySequences.Sobol2D((uint)(patternWidth * patternHeight)).Select(p=>(p-.5f).to3xz()).ToArray(),
                ESamplePattern.PoissonDisk => m_Texture != null
                    ? ULowDiscrepancySequences.PoissonDisk2D((int)math.sqrt(patternWidth*patternHeight), 30, randomGenerator, val =>
                        math.lerp(3f, 1f, UColor.RGBtoLuminance(m_Texture.GetPixel((int)(val.x * m_Texture.width), (int)(val.y * m_Texture.height)).to3())))
                        .Select(p=>(p-.5f).to3xz()).ToArray()
                    : ULowDiscrepancySequences.PoissonDisk2D((int)math.sqrt(patternWidth * patternHeight),30,randomGenerator).Select(p=>(p-.5f).to3xz()).ToArray(),
                ESamplePattern.BCCLattice => ULowDiscrepancySequences.BCCLattice2D(1f / math.sqrt(patternWidth * patternHeight)).Select(p=>(p-.5f).to3xz()).ToArray(),
                ESamplePattern.BCCLattice3D => ULowDiscrepancySequences.BCCLattice3D(1f/math.sqrt(patternWidth * patternHeight)).Remake(p=>p-.5f),
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
                    for (var k = 0; k < size; k++)
                    {
                        var pattern = patterns[k];
                        Gizmos.color = Color.Lerp(Color.red,Color.green,k/(float)size);
                        Gizmos.DrawSphere(pattern,gizmosRadius);
                    }
                    Gizmos.color = Color.white;
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
                        var pattern = patterns[k].xy;
                        var uv = pattern + .5f;
                        Gizmos.color = Color.red * uv.x + Color.green * uv.y;
                        Gizmos.DrawSphere(ESphereMapping.ConcentricOctahedral.UVToSphere( uv) * .5f,gizmosRadius);
                    }
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(Vector3.zero,.5f);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
}
