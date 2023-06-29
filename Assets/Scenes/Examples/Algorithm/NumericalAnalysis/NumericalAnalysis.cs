using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.NumericalAnalysis
{
    public class NumericalAnalysis : MonoBehaviour
    {
        [Range(-kNumericRange,kNumericRange)] public float m_Guess = 1.3f;
        private const float kGraphSize = 20f;
        private const float kNumericRange = 2f;
        private const int kGraphResolution = 50;
        private readonly Func<float, float> kPolynomial = x => math.pow(x,5) + math.pow(x,2) - x - .2f;
        private readonly Func<float, float> kDerivative = x => 5 * math.pow(x, 4) + 2 * x - 1;

        private readonly Func<float2, float2> kFractalPolynomial = x => ucomplex.pow(x,5) + ucomplex.pow(x,2) - x + new float2(1f,0f);
        private readonly Func<float2, float2> kFractalDerivative = x => 5 * ucomplex.pow(x, 4) + 2 * x - new float2(1,0f);

        private const float kSphereSize = 1f / kGraphResolution;
        private void OnDrawGizmos()
        {
            //Newton's method
            Gizmos.matrix = Matrix4x4.Scale(Vector3.one*kGraphSize);
            Gizmos.color = Color.red.SetA(.5f);
            Gizmos.DrawLine(Vector3.left,Vector3.right);
            Gizmos.color = Color.blue.SetA(.5f);
            Gizmos.DrawLine(Vector3.back,Vector3.forward);

            Vector3[] lines = new Vector3[kGraphResolution];
            for (int i = 0; i < kGraphResolution; i++)
            {
                float x = math.lerp(-1f,1f,i/(float)kGraphResolution);
                float y = kPolynomial(x*kNumericRange)/kNumericRange;
                lines[i] = new Vector3(x,0,y);
            }

            Gizmos.color = Color.green;
            UGizmos.DrawLines(lines);

            Gizmos.color = KColor.kOrange;
            Gizmos.DrawSphere(new Vector3(m_Guess,0,0)/kNumericRange,0.01f);
            Gizmos.color = Color.yellow;
            var root1 = UNumericalAnalysis.NewtonsMethod(kPolynomial,kDerivative,m_Guess);
            Gizmos.DrawSphere(new Vector3(root1,0,0)/kNumericRange,0.01f);
            
            //Newton's fractal
            
            
            const float kSqrApproximation = .01f*.01f;
            List<float2> roots = new List<float2>();
            Gizmos.matrix = Matrix4x4.Translate(kGraphSize*Vector3.right * 2)*Matrix4x4.Scale(Vector3.one*kGraphSize);
            for (int i = 0; i < kGraphResolution; i++)
            {
                float x = math.lerp(-1f,1f,i/(float)kGraphResolution);
                for (int j = 0; j < kGraphResolution; j++)
                {
                    float y = math.lerp(-1f,1f,j/(float)kGraphResolution);
            
                    var guess = new float2(x, y) * kNumericRange;
                    var root2 = UNumericalAnalysis.NewtonsFractal(kFractalPolynomial,kFractalDerivative,guess,kSqrApproximation);
                    if (!roots.Any(p => (root2 - p).sqrmagnitude() < kSqrApproximation))
                        roots.Add(root2);
                    var index = roots.FindIndex(p=>(root2-p).sqrmagnitude()<kSqrApproximation);
                    Gizmos.color = UColor.IndexToColor(index);
                    
                    Gizmos.DrawWireSphere(new Vector3(x,0,y),kSphereSize);
                }
            }
            // var root2 = UNumericalAnalysis.NewtonsFractal(kFractalPolynomial,kFractalDerivative,new float2(-.5f,.5f),kSqrApproximation);
        }
    }

}
