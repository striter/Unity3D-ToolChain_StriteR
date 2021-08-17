//Properties
// _HueShift("Hue Shift",Range(-180,180))=0
// _Saturation("Saturation",Range(-100,100))=0
// _Brightness("Brightness",Range(-100,100))=0

half _HueShift;
half _Saturation;
half _Brightness;

half3 HSL(half3 col)
{
    half lightness=remap(_Brightness,-100,100,-1,1);
    col= triLerp(0,col,1,lightness);
    half saturation=_Saturation/100.;
    half minCol=min(col);
    half maxCol=max(col);
    half delta=maxCol-minCol;
    half value=maxCol+minCol;
    half L=value*.5h;
    half S=L<.5h?delta*rcp(value):delta*rcp(2.h-value);
                
    if(saturation>=0.h)
    {
        half alpha=(saturation+S)>=1.h?S:(1.h-saturation);
        alpha= rcp(alpha)-1.h;
        col=col+(col-L)*min(alpha,FLT_MAX);
    }
    else if(saturation<0.h)
    {
        col=L+(col-L)*(1.h+saturation);    
    }
    half3 hsv=RGBtoHSV(col);
    hsv.x+=(_HueShift+180)/360.h+.5h;
    col=HSVtoRGB(hsv);
    return col;
}