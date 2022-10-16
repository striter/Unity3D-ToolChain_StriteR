float2 TransformTex(float2 _uv, float4 _st) {return _uv * _st.xy + _st.zw;}
float2 TransformTex_Flow(float2 _uv,float4 _st) {return _uv * _st.xy + _Time.y*_st.zw;}

float2 UVRemap_TRS(float2 uv,float2 offset, float rotateAngle, float2 tilling)
{
    const float2 center = float2(.5, .5);
    uv = uv + offset;
    offset += center;
    float2 centerUV = uv - offset;
    return mul( Rotate2x2(rotateAngle), centerUV) * tilling + offset;
}

//&https://iquilezles.org/articles/biplanar/
half4 TriPlanarMapping(TEXTURE2D_PARAM(_tex,_sampler),float3 _position,half3 _normal,half _sharpness)
{
    half4 x = SAMPLE_TEXTURE2D(_tex,_sampler,_position.yz);
    half4 y = SAMPLE_TEXTURE2D(_tex,_sampler,_position.zx);
    half4 z = SAMPLE_TEXTURE2D(_tex,_sampler,_position.xy);
    half3 weight = pow(abs(_normal),_sharpness);
    return (x * weight.x + y * weight.y + z * weight.z) / sum(weight);
}

half4 BiPlanarMapping(TEXTURE2D_PARAM(_tex,_sampler),float3 _position,half3 _normal,half _sharpness)
{
    float3 dpdx = ddx(_position);
    float3 dpdy = ddy(_position);
    half3 n = abs(_normal);
    float3 p = _position;

    int3 ma = (n.x>n.y && n.x>n.z) ? int3(0,1,2) :
               (n.y>n.z)            ? int3(1,2,0) :
                                      int3(2,0,1) ;
    int3 mi = (n.x<n.y && n.x<n.z) ? int3(0,1,2) :
           (n.y<n.z)            ? int3(1,2,0) :
                                  int3(2,0,1) ;
    int3 me = int3(3,3,3) - mi - ma;
    half4 x = SAMPLE_TEXTURE2D_GRAD(_tex,_sampler,float2(   p[ma.y],   p[ma.z]), 
                               float2(dpdx[ma.y],dpdx[ma.z]), 
                               float2(dpdy[ma.y],dpdy[ma.z]) );
    half4 y = SAMPLE_TEXTURE2D_GRAD(_tex,_sampler,float2(   p[me.y],   p[me.z]), 
                               float2(dpdx[me.y],dpdx[me.z]),
                               float2(dpdy[me.y],dpdy[me.z]) );
    half2 w = half2(n[ma.x],n[me.x]);
    w = saturate((w-0.5773)/(1.0-0.5773));
    w = pow(w,_sharpness/8.0);
    return (x*w.x+y*w.y)/sum(w);
}