using System;
using System.Collections;
using System.Collections.Generic;
using Rendering;
using UnityEngine;

public class PostEffect_Blurs : PostEffectBase
{
    [Tooltip("模糊参数")]
    public ImageEffectParams_Blurs m_BlurParam;
    protected override AImageEffectBase OnGenerateRequiredImageEffects() => new ImageEffect_Blurs(()=>m_BlurParam);

}
