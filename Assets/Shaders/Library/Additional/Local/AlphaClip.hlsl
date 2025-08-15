//#pragma shader_feature_local _ALPHATEST_ON
//INSTANCE(float,_AlphaCutoff)
void AlphaClip(half _alpha)
{
    #ifdef _ALPHATEST_ON
        clip(_alpha-INSTANCE(_AlphaCutoff));
    #endif
}