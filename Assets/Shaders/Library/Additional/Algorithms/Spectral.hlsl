//&https://www.alanzucconi.com/2017/07/15/improving-the-rainbow/
float3 spectral_jet(float _waveLength)
{
    float x = saturate((_waveLength-400)/300);
    float3 c;
    if (x<.25)
        c= float3(0,4*x,1);
    else if (x<.5)
        c= float3(0,1,1+4*(.25-x));
    else if(x<.75)
        c= float3(4*(x-.5),1,0);
    else
        c= float3(1,1+4*(.75-x),0);
    return saturate(c);
}

float3 spectral_bruton(float _waveLength)
{
    float w = _waveLength;
    float3 c;
    if(w >=380&&w<440)
        c = float3(-(w-440)/(440-380),0,1);
    else if(w>=440&&w<490)
        c = float3(0,(w-440)/(490-440),1);
    else if(w>=490&&w<510)
        c = float3(0,1,-(w-510)/(510-490));
    else if (w>=510 && w < 580)
        c = float3((w-510)/(580-510) ,1,0);
    else if (w >= 580 && w <645)
        c = float3(1,-(w-645)/(645-580),0);
    else if (w >=645 && w <= 780)
        c = float3(1,0,0);
    else
        c = float3(0,0,0);
    return saturate(c);
}

float3 spectral_gems(float _waveLength)
{
    float x = saturate((_waveLength-400)/300);
    return saturate(bump(float3(4*(x-.75),4*(x-.5),4*(x-.25))));
}

float3 spectral_spektre(float _waveLength)
{
    float l = _waveLength;
    float r=0.0,g=0.0,b=0.0;
    if (l>=400.0&&l<410.0) {  float t=(l-400.0)/(410.0-400.0);  r =+(0.33*t)-(0.20*t*t); }
    else if (l>=410.0&&l<475.0) { float t=(l-410.0)/(475.0-410.0); r=0.14         -(0.13*t*t); }
    else if (l>=545.0&&l<595.0) { float t=(l-545.0)/(595.0-545.0); r=    +(1.98*t)-(     t*t); }
    else if (l>=595.0&&l<650.0) { float t=(l-595.0)/(650.0-595.0); r=0.98+(0.06*t)-(0.40*t*t); }
    else if (l>=650.0&&l<700.0) { float t=(l-650.0)/(700.0-650.0); r=0.65-(0.84*t)+(0.20*t*t); }
    
    if (l>=415.0&&l<475.0) { float t=(l-415.0)/(475.0-415.0); g=             +(0.80*t*t); }
    else if (l>=475.0&&l<590.0) { float t=(l-475.0)/(590.0-475.0); g=0.8 +(0.76*t)-(0.80*t*t); }
    else if (l>=585.0&&l<639.0) { float t=(l-585.0)/(639.0-585.0); g=0.82-(0.80*t)           ; }
    
    if (l>=400.0&&l<475.0) { float t=(l-400.0)/(475.0-400.0); b=    +(2.20*t)-(1.50*t*t); }
    else if (l>=475.0&&l<560.0) { float t=(l-475.0)/(560.0-475.0); b=0.7 -(     t)+(0.30*t*t); }
    
    return saturate(float3(r,g,b));
}

float3 spectral_zucconi(float _waveLength)
{
    float x = saturate((_waveLength-400.)/300.);
    const float3 cs = float3(3.54541723, 2.86670055, 2.29421995);
    const float3 xs = float3(0.69548916, 0.49416934, 0.28269708);
    const float3 ys = float3(0.02320775, 0.15936245, 0.53520021);
    return bumpy(cs*(x-xs),ys);
}

float3 spectral_zucconi6(float _waveLength)
{
    float x = saturate((_waveLength - 400.0)/ 300.0);
    const float3 c1 = float3(3.54585104, 2.93225262, 2.41593945);
    const float3 x1 = float3(0.69549072, 0.49228336, 0.27699880);
    const float3 y1 = float3(0.02312639, 0.15225084, 0.52607955);
    const float3 c2 = float3(3.90307140, 3.21182957, 3.96587128);
    const float3 x2 = float3(0.11748627, 0.86755042, 0.66077860);
    const float3 y2 = float3(0.84897130, 0.88445281, 0.73949448);
    return
        bumpy(c1 * (x - x1), y1) +
        bumpy(c2 * (x - x2), y2) ;
}