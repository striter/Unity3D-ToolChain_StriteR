using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Random;
using Runtime.SignalProcessing;
using UnityEngine;

namespace Examples.Algorithm.FourierTransform
{
    public enum EResolution
    {
        _4 = 4,
        _8 = 8,
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
    }
    public class FourierTransform : MonoBehaviour
    {
        public EResolution m_InputResolution;
        public EResolution m_OutputResolution;
        public float[] m_Inputs;

        public void OnValidate()
        {
            var random = new LCGRandom("FourierTransform".GetHashCode());
            m_Inputs = new float[(int)m_InputResolution];
            for (var i = 0; i < m_Inputs.Length; i++)
                m_Inputs[i] = URandom.Random01(random);
        }

        public EResolution kResolution = EResolution._256;
        private void OnDrawGizmos()
        {
            if (m_Inputs == null ) return;

            Gizmos.matrix = transform.localToWorldMatrix;
            var bounds = GBox.kDefault;
            bounds.DrawGizmos();
            for (var i = 0; i < m_Inputs.Length; i++)
            {
                var color = Color.red;
                Gizmos.color = color;
                // Gizmos.DrawSphere(bounds.GetPoint((float)i / m_Inputs.Length,m_Inputs[i],0), .5f / m_Inputs.Length);
            }
            UGizmos.DrawLines(m_Inputs.Select((c, i) => bounds.GetPoint((float)i / (m_Inputs.Length - 1),m_Inputs[i],0)));

            Gizmos.color = Color.blue;
            var fourierCoefficients = new cfloat2[(int)m_OutputResolution];
            UFourier.DiscreteFourier.Transform(m_Inputs,fourierCoefficients);
            // var fftList = m_Inputs.Select(p=>p*cfloat2.rOne).ToList();
            // UFourier.CooleyTukeyFastFourier.Transform(fftList);
            // fftList.FillArray(m_FourierCoefficients);
            var resolution = (int)kResolution;
            var inversedResult = UFourier.DiscreteFourier.Inverse(fourierCoefficients, resolution);
            UGizmos.DrawLines(inversedResult.Select((c, i) => bounds.GetPoint((float)i / (resolution - 1),c.x,1)));
        }
    }

}