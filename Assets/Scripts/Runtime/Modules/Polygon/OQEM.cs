using System;
using UnityEngine;

namespace QuadricErrorsMetric
{
    public enum EContractMode
    {
        VertexCount,
        Percentage,
        DecreaseAmount,
    }

    [Serializable]
    public struct ContractConfigure
    {
        public EContractMode mode;
        [MFoldout(nameof(mode),EContractMode.Percentage)][Range(1f,100f)] public float percent;
        [MFoldout(nameof(mode),EContractMode.VertexCount , EContractMode.DecreaseAmount)]public int count;
    }
}