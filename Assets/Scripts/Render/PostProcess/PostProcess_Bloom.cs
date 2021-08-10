using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    public class PostProcess_Bloom : PostProcessComponentBase<PPCore_Bloom,PPData_Bloom>
    {
    }

    [Serializable]
    public struct PPData_Bloom 
    {
    }

    public class PPCore_Bloom : PostProcessCore<PPData_Bloom>
    {
        #region ShaderProperties
        #endregion


        public override void OnValidate(ref PPData_Bloom _data)
        {
            base.OnValidate(ref _data);
        }
    }
}