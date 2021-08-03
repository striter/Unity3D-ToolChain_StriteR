SDFOutput GSphere_SDF(GSphere _sphere,SDFInput _input)
{
    float distance=length(_input.position-_sphere.center)-_sphere.radius;
    return SDFOutput_Ctor(_input,distance);
}

SDFOutput GBox_SDF(GBox _box,SDFInput _input)
{
    float3 q=abs(_input.position-_box.center)-_box.extend;
    return SDFOutput_Ctor(_input,length(max(q,0.0))+min(max(q),0.0));
}

SDFOutput GRoundBox_SDF(GBoxRound _roundBox,SDFInput _input)
{
    return SDFOutput_Ctor(_input, GBox_SDF(_roundBox.box,_input).distance-_roundBox.roundness);
}

SDFOutput GFrameBox_SDF(GBoxFrame _frameBox,SDFInput _input)
{
    float3 p=abs(_input.position-_frameBox.box.center)-_frameBox.box.extend;
    float3 q=abs(p+_frameBox.frameExtend)-_frameBox.frameExtend;
    
    return SDFOutput_Ctor(_input, min(
      length(max(float3(p.x,q.y,q.z),0.0))+min(max(p.x,max(q.y,q.z)),0.0),
      length(max(float3(q.x,p.y,q.z),0.0))+min(max(q.x,max(p.y,q.z)),0.0),
      length(max(float3(q.x,q.y,p.z),0.0))+min(max(q.x,max(q.y,p.z)),0.0)));
}

SDFOutput GTorus_SDF(GTorus _torus,SDFInput _input)
{
    float q=length(_input.position.xz-_torus.center.xz)-_torus.majorRadius;
    return SDFOutput_Ctor(_input,length(float2(q,_input.position.y))-_torus.minorRadius);
}
SDFOutput GTorusLink_SDF(GTorusLink _torusLink,SDFInput _input)
{
    float3 q=_input.position-_torusLink.torus.center;
    q.y=max(abs(q.y)-_torusLink.extend,0.0);
    return SDFOutput_Ctor(_input, length(float2(length(q.xy)-_torusLink.torus.majorRadius,q.z))-_torusLink.torus.minorRadius);
}
SDFOutput GTorusCapped_SDF(GTorusCapped _torusCapped,SDFInput _input)
{
    float2 sc=_torusCapped.capRadianSinCos;
    float ra=_torusCapped.torus.majorRadius;
    float rb=_torusCapped.torus.minorRadius;
    float3 p=_input.position-_torusCapped.torus.center;
    p.x = abs(p.x);
    float k = (sc.y*p.x>sc.x*p.y) ? dot(p.xy,sc) : length(p.xy);
    float distance= sqrt( dot(p,p) + ra*ra - 2.0*ra*k ) - rb;
    return SDFOutput_Ctor(_input,distance);
}

SDFOutput GCylinder_SDF(GCylinder _cylinder,SDFInput _input)
{
    return SDFOutput_Ctor(_input, length((_cylinder.center-_input.position).xy)-_cylinder.radius);
}
SDFOutput GCylinderCapped_SDF(GCylinderCapped _cylinderCapped,SDFInput _input)
{
    float3 p=_input.position;
    float2 d=abs(float2(length((p-_cylinderCapped.cylinder.center).xz),p.y))-float2(_cylinderCapped.cylinder.radius,_cylinderCapped.height);
    return SDFOutput_Ctor( _input,min(max(d),0)+length(max(d,0)));
}
SDFOutput GCylinderRound_SDF(GCylinderRound _cylinderRound,SDFInput _input)
{
    float3 p=_input.position-_cylinderRound.cylinder.center;
    float2 d= float2(length(p.xz)-2.0*_cylinderRound.cylinder.radius+_cylinderRound.roundRadius,abs(p.y)-_cylinderRound.height);
    return SDFOutput_Ctor( _input,min(max(d),0)+length(max(d,0))-_cylinderRound.roundRadius);
}
SDFOutput GCylinderCapsule_SDF(GCylinderCapsule _cylinderCapsule,SDFInput _input)
{
    float3 pa=_input.position-_cylinderCapsule.top;
    float3 ba=_cylinderCapsule.bottom-_cylinderCapsule.top;
    float h=saturate(dot(pa,ba)/dot(ba,ba));
    return SDFOutput_Ctor(_input, length(pa-ba*h)-_cylinderCapsule.cylinder.radius);
}