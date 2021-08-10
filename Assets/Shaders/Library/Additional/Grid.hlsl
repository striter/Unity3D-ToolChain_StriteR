//#pragma multi_compile _ _EDITGRID

half4 _GridSize;
half4 _GridColor;

float3 MixGrid(float3 positionWS, half3 color)
{
    #ifndef _EDITGRID
        return color;
    #endif
    half2 gridPos=abs(positionWS.xz)-_GridSize.xy;
    gridPos%=_GridSize.z;
    half gridOffset=min(min(_GridSize.z-gridPos),min(gridPos));
    half grid=step(gridOffset,_GridSize.w);

    grid*=lerp(.35h,1.h,step(abs((positionWS.x+positionWS.z +positionWS.y+_Time.y)%_GridSize.z),.35h));
    grid*=_GridColor.a;
    half3 gridColor=_GridColor.rgb*grid;
    return lerp(color,gridColor,grid);
}
