//Structs
struct SDFInput
{
    float3 position;
    float3 color;
};
SDFInput SDFInput_Ctor(float3 _position,float3 _color)
{
    SDFInput input;
    input.position=_position;
    input.color=_color;
    return input;
}

struct SDFOutput
{
    float distance;
    float3 color;
};
SDFOutput SDFOutput_Ctor(SDFInput _input,float _distance)
{
    SDFOutput output;
    output.distance=_distance;
    output.color=_input.color;
    return output;
}

//Constructive Solid Geometry
SDFOutput SDFIntersect(SDFOutput _outputA,SDFOutput _outputB)
{
    if(_outputA.distance>_outputB.distance)
        return _outputA;
    else
        return _outputB;
}

SDFOutput SDFIntersect(SDFOutput _outputA,SDFOutput _outputB,SDFOutput _outputC)
{
    return  SDFIntersect(_outputA,SDFIntersect(_outputB,_outputC));
}

SDFOutput SDFUnion(SDFOutput _outputA,SDFOutput _outputB)
{
    if(_outputA.distance<_outputB.distance)
        return _outputA;
    else
        return _outputB;
}
SDFOutput SDFUnion(SDFOutput _outputA,SDFOutput _outputB,SDFOutput _outputC)
{
    return SDFUnion(_outputA,SDFUnion(_outputB,_outputC));
}

SDFOutput SDFDifference(SDFOutput _outputA,SDFOutput _outputB)
{
    _outputB.distance=-_outputB.distance;
    if(_outputA.distance>_outputB.distance)
        return _outputA;
    else
        return _outputB;
}
