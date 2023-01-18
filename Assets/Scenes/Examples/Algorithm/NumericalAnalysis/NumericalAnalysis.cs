using System;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.NumericalAnalysis
{
    public class NumericalAnalysis : MonoBehaviour
    {
        [Range(-kNumericRange,kNumericRange)] public float m_Guess = 1.3f;
        private const float kGraphSize = 20f;
        private const float kNumericRange = 2f;
        private const int kGraphResolution = 100;
        private readonly Func<float, float> kPolynomial = p => math.pow(p,5) + math.pow(p,2) - p - .2f;
        private readonly Func<float, float> kTangentDerivative = p => 5 * math.pow(p, 4) + 2 * p - 1;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.Scale(Vector3.one*kGraphSize);
            Gizmos.color = Color.red.SetAlpha(.5f);
            Gizmos.DrawLine(Vector3.left,Vector3.right);
            Gizmos.color = Color.blue.SetAlpha(.5f);
            Gizmos.DrawLine(Vector3.back,Vector3.forward);

            Vector3[] lines = new Vector3[kGraphResolution];
            for (int i = 0; i < kGraphResolution; i++)
            {
                float x = math.lerp(-1f,1f,i/(float)kGraphResolution);
                float y = kPolynomial(x*kNumericRange)/kNumericRange;
                lines[i] = new Vector3(x,0,y);
            }

            Gizmos.color = Color.green;
            Gizmos_Extend.DrawLines(lines);

            Gizmos.color = KColor.kOrange;
            Gizmos.DrawSphere(new Vector3(m_Guess,0,0)/kNumericRange,0.01f);
            Gizmos.color = Color.yellow;
            var root = UNumericalAnalysis.NewtonsMethod(kPolynomial,kTangentDerivative,m_Guess);
            Gizmos.DrawSphere(new Vector3(root,0,0)/kNumericRange,0.01f);
        }
    }

}
