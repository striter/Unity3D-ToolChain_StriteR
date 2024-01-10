using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace QuadricErrorsMetric
{
    public enum EContractMode
    {
        MinError,
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
        [MFoldout(nameof(mode), EContractMode.MinError)] public float minError;
        
        public static ContractConfigure kDefault = new ContractConfigure(){mode = EContractMode.MinError,minError = float.Epsilon};
    }
}