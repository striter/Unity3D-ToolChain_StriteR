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

//Signed Distance Functions
SDFOutput SDSphere(GSphere sphere,SDFInput input)
{
    float distance=length(input.position-sphere.center)-sphere.radius;
    return SDFOutput_Ctor(input,distance);
}

SDFOutput SDBox(GBox box,SDFInput input)
{
    float3 q=abs(input.position-box.center)-box.extend;
    return SDFOutput_Ctor(input,length(max(q,0.0))+min(max(q),0.0));
}

SDFOutput SDRoundBox(GRoundBox roundBox,SDFInput input)
{
    return SDFOutput_Ctor(input, SDBox(roundBox.box,input).distance-roundBox.roundness);
}
SDFOutput SDFrameBox(GFrameBox frameBox,SDFInput input)
{
    float3 p=abs(input.position-frameBox.box.center)-frameBox.box.extend;
    float3 q=abs(p-frameBox.box.center+frameBox.frameExtend)-frameBox.frameExtend;
    return SDFOutput_Ctor(input, min(
        length(max(float3(p.x,q.y,q.z),0.0))+min(max(p.x,q.y,q.z),0.0),
        length(max(float3(q.x,p.y,q.z),0.0))+min(max(q.x,p.y,q.z),0.0),
        length(max(float3(q.x,q.y,p.z),0.0))+min(max(q.x,q.y,p.z),0.0)
    ));
}
SDFOutput SDTorus(GTorus torus,SDFInput input)
{
    float q=length(input.position.xz-torus.center.xz)-torus.majorRadius;
    return SDFOutput_Ctor(input,length(float2(q,input.position.y))-torus.minorRadius);
}