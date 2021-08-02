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
SDFOutput SDTorusCapped(GTorusCapped cappedTorus,SDFInput input)
{
    float3 p=input.position;
    p.x=abs(p.x);
    float k=(cappedTorus.torus.minorRadius*p.x>cappedTorus.torus.majorRadius*p.y)?dot(p.xy,float2(cappedTorus.torus.majorRadius,cappedTorus.torus.minorRadius)):length(p.xy);
    float distance=sqrt(sqrDistance(p,p)+sqrDistance( cappedTorus.capRadianBegin)-2.0*k*cappedTorus.capRadianBegin)-cappedTorus.capRadianEnd;
    return SDFOutput_Ctor(input,distance);
}