using System;
using UnityEngine;
using Unity.Mathematics;
using Rendering.Pipeline;

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
    
    public class SamplePatternVisualize : MonoBehaviour
    {
        public ESamplePattern patternType = ESamplePattern.Grid;
        public int patternWidth=4,patternHeight=4;
        public bool m_Repeat = false;
        [MFoldout(nameof(patternType), ESamplePattern.PoissonDisk)] public Texture2D m_Texture;
        [Readonly] public float2[] patterns;
        [Range(0, 1f)] public float gizmosRadius = 0.03f;
        [Button]
        void Generate()
        {
            switch (patternType)
            {
                case ESamplePattern.Grid:
                    patterns= ULowDiscrepancySequences.Grid2D(patternWidth,patternHeight);
                    break;
                case ESamplePattern.Stratified:
                    patterns= ULowDiscrepancySequences.Stratified2D(patternWidth,patternHeight,true);
                    break;
                case ESamplePattern.Halton:
                    patterns = new float2[patternWidth * patternHeight].Remake((i, p) => ULowDiscrepancySequences.Halton2D((uint)i) - .5f);
                    break;
                case ESamplePattern.HammersLey:
                    patterns = new float2[patternWidth * patternHeight].Remake((i, p) => ULowDiscrepancySequences.Hammersley2D((uint)i,(uint)(patternWidth * patternHeight)) - .5f);
                    break;
                case ESamplePattern.Sobol:
                    patterns = ULowDiscrepancySequences.Sobol2D((uint) (patternWidth * patternHeight));
                    break;
                case ESamplePattern.PoissonDisk:
                    patterns = m_Texture !=null ? ULowDiscrepancySequences.PoissonDisk2D(patternWidth,patternHeight,30,null,val=>math.lerp(3f,1f,UColorTransform.RGBtoLuminance(m_Texture.GetPixel((int)(val.x*m_Texture.width),(int)(val.y * m_Texture.height)).to3()))) 
                        : ULowDiscrepancySequences.PoissonDisk2D(patternWidth,patternHeight);
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            int size = patterns.Length;

            if (m_Repeat)
            {
                for (int i = -2; i < 2; i++)
                {
                    for (int j = -2; j < 2; j++)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.matrix = Matrix4x4.Translate(new Vector3(i+.5f,0,j+.5f));
                        Gizmos.DrawWireCube(Vector3.zero,Vector3.one.SetY(0f));
                        Gizmos.color = Color.red;
                        for (int k = 0; k < size; k++)
                        {
                            var pattern = patterns[k];
                            Gizmos.DrawSphere(new Vector3(pattern.x,0,pattern.y),gizmosRadius);
                        }
                    }
                }
            }
            else
            {
                Gizmos.color = Color.white;
                for (int k = 0; k < size; k++)
                {
                    Gizmos.DrawSphere(patterns[k].to3xz(),gizmosRadius);
                }
            }
        }
    }
}
