//#pragma shader_feature_local _ALPHACLIP
//INSTANCE(float,_AlphaClipRange)
void AlphaClip(half alpha)
{
    #ifdef _ALPHACLIP
    clip(alpha-INSTANCE(_AlphaClipRange));
    #endif
}