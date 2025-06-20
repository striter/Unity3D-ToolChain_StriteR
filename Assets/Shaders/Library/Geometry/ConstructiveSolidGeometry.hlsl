struct SDFSurface
{
    float distance;
    float3 color;
    bool Valid() { return distance>=0 && color.x > 0; }
};

SDFSurface SDFSurface_Ctor(float _distance, float3 _color)
{
    SDFSurface output;
    output.distance=_distance;
    output.color=_color;
    return output;
}

SDFSurface SDFSurface_Invalid()
{
    return SDFSurface_Ctor(-1,0);
}

SDFSurface Union(SDFSurface _a, SDFSurface _b)
{
    if(_a.distance<_b.distance)
        return _a;
    return _b;
}

SDFSurface Union(SDFSurface _a, SDFSurface _b, SDFSurface _c){
    return Union(Union(_a,_b),_c);
}

SDFSurface Union(SDFSurface _a, SDFSurface _b, SDFSurface _c, SDFSurface _d){
    return Union(Union(_a,_b),Union(_c,_d));
}

SDFSurface Intersection(SDFSurface _a, SDFSurface _b)
{
    if(_a.distance<_b.distance)
        return _b;
    return _a;
}

SDFSurface Difference(SDFSurface _outputA,SDFSurface _outputB)
{
    _outputB.distance=-_outputB.distance;
    if(_outputA.distance>_outputB.distance)
        return _outputA;
    return _outputB;
}

SDFSurface UnionSmin(SDFSurface _outputA,SDFSurface _outputB,float k,float n)
{
    SDFSurface output;
    float a = _outputA.distance;
    float b = _outputB.distance;
    float h =  max( k-abs(a-b), 0.0 )/k;
    float m = pow(h, n)*0.5;
    float s = m*k/n;
    if(a<b)
    {
        output.color = lerp(_outputA.color,_outputB.color,m);
        output.distance = a-s;
    }
    else
    {
        output.color = lerp(_outputA.color,_outputB.color,1.0 - m);
        output.distance = b-s;
    }
    
    return output;
}
