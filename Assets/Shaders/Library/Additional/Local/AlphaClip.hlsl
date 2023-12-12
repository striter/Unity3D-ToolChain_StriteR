//#pragma shader_feature_local _ALPHACLIP
//INSTANCE(float,_AlphaCutoff)
void AlphaClip(half alpha)
{
    #ifdef _ALPHACLIP
        clip(alpha-INSTANCE(_AlphaCutoff));
    #endif
}