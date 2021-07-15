float4 _GridSize;
float4 _GridColor;


float3 MixGrid(float3 positionWS, float3 color)
{
    float2 gridPos=positionWS.xz-_GridSize.xy;
    gridPos%=_GridSize.z;
    float gridOffset=min(min(_GridSize.z-gridPos),min(gridPos));
    float grid=step(gridOffset,_GridSize.w);

    grid*=lerp(.35f,1.f,step((positionWS.x+positionWS.z +positionWS.y+_Time.y)%_GridSize.z,.35f));
    
    return lerp(color,_GridColor.rgb,grid*_GridColor.a);
}
