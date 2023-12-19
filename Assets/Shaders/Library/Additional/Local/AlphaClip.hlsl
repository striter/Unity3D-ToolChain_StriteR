//#pragma shader_feature_local _ALPHACLIP
//INSTANCE(float,_AlphaCutoff)
void AlphaClip(half _alpha)
{
    #ifdef _ALPHACLIP
        clip(_alpha-INSTANCE(_AlphaCutoff));
    #endif
}