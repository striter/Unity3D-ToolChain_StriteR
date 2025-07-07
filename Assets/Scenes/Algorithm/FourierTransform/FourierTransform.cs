using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Pool;
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

    public enum EFourierType
    {
        DiscreteFourierTransform,
        ColeyTukeyFastFourierTransform,
        StockhamFastFourierTransform
    }
    
    public class FourierTransform : MonoBehaviour
    {
        public EFourierType m_FourierType = EFourierType.DiscreteFourierTransform;
        public EResolution m_TimeResolution = EResolution._128;
        public EResolution m_FrequencyResolution = EResolution._64;
        public EResolution m_InverseResolution = EResolution._256;
        [Readonly] public cfloat2[] m_Inputs;

        public void OnValidate()
        {
            var random = new LCGRandom("FourierTransform".GetHashCode());
            m_Inputs = new cfloat2[(int)m_TimeResolution];
            for (var i = 0; i < m_Inputs.Length; i++)
                m_Inputs[i] = URandom.Random01(random) * cfloat2.rOne;
        }

        private void OnDrawGizmos()
        {
            if (m_Inputs == null ) return;

            Gizmos.matrix = transform.localToWorldMatrix;
            
            Gizmos.color = Color.white;
            var bounds = GBox.kDefault;
            bounds.DrawGizmos();
            
            Gizmos.color = Color.red;
            UGizmos.DrawLines(m_Inputs.Select((c, i) => bounds.GetPoint((float)i / (m_Inputs.Length - 1),m_Inputs[i].x,0)));

            Gizmos.color = Color.blue;
            var frequencies = (IList<cfloat2>)PoolList<cfloat2>.Empty(nameof(FourierTransform) + "_frequencies");
            frequencies.Resize((int)m_TimeResolution);
            switch (m_FourierType)
            {
                case EFourierType.DiscreteFourierTransform: UFourier.DiscreteFourier.Transform(m_Inputs,frequencies); break;
                case EFourierType.ColeyTukeyFastFourierTransform: UFourier.CooleyTukeyFastFourier.Transform(m_Inputs, frequencies); break;
                case EFourierType.StockhamFastFourierTransform: UFourier.StockhamFastFourier.Transform(m_Inputs, frequencies); break;
            }
            var downsizeFactor = (float)m_FrequencyResolution / (float)m_TimeResolution;
            frequencies = frequencies.Resize((int)m_FrequencyResolution).Remake(p => p * downsizeFactor);

            var inversedResult = (IList<cfloat2>)PoolList<cfloat2>.Empty(nameof(FourierTransform) + "_inversedResult");
            inversedResult.Resize((int)m_InverseResolution);
            switch (m_FourierType)
            {
                default:
                case EFourierType.DiscreteFourierTransform: inversedResult = UFourier.DiscreteFourier.Inverse(frequencies, inversedResult); break;
                case EFourierType.ColeyTukeyFastFourierTransform: inversedResult = UFourier.CooleyTukeyFastFourier.Inverse(frequencies, inversedResult); break;
                case EFourierType.StockhamFastFourierTransform: inversedResult = UFourier.StockhamFastFourier.Inverse(frequencies, inversedResult); break;
            }
            var inverseResolution = (int)m_InverseResolution;
            UGizmos.DrawLines(inversedResult.Select((c, i) => bounds.GetPoint((float)i / (inverseResolution - 1),c.x,1)));
        }
    }

}