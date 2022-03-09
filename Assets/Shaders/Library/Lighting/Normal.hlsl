//Normals
half3 DecodeNormalMap(float4 _normal)
{
    #if defined(UNITY_NO_DXT5nm)
        return _normal.xyz*2.h-1.h;
    #else
        half3 normal=half3(_normal.ag,0)*2.h-1.h;
        return half3(normal.xy,max(1.0e-16, sqrt(1.0 - saturate(dot(normal.xy,normal.xy)))));
    #endif
}

//Refer: @https://blog.selfshadow.com/publications/blending-in-detail/
half3 BlendNormal(half3 _normal1, half3 _normal2, uint _blendMode)
{
    half3 blendNormal=half3(0.h,0.h,1.h);
    [branch]switch (_blendMode)
    {
        default:blendNormal=0.h;break;
        case 0u://Linear
            {
                blendNormal= _normal1 + _normal2;         
            }
        break;
        case 1u://Overlay
            {
                blendNormal =Blend_Overlay(_normal1*.5h+.5h,_normal2*.5h+.5h);
                blendNormal=blendNormal*2.h-1.h;
            }
        break;
        case 2u://Partial Derivative
            {
                half2 pd=_normal1.xy*_normal2.z+_normal2.xy*_normal1.z;
                blendNormal=half3(pd,_normal1.z*_normal2.z);
            }
        break;
        case 3u://Unreal Developer Network
            {
                //blendNormal=half3(_normal1.xy+_normal2.xy,_normal1.z*_normal2.z); //Whiteout
                blendNormal=half3(_normal1.xy+_normal2.xy,_normal1.z);
            }
        break;
        case 4u://Reoriented
            {
                half3 t=_normal1*half3(1.h,1.h,1.h)+half3(0.h,0.h,1.h);
                half3 u=_normal2*half3(-1.h,-1.h,1.h);
                blendNormal=t*dot(t,u)-u*t.z;
            }
        break;
    }
    return normalize(blendNormal);
}
