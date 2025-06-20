struct GBox
{
    float3 center;
    float3 size;
    float3 extent;
    float3 boxMin;
    float3 boxMax;
    float3 GetNormalizedPoint(float3 _point) { return(_point - boxMin)/size;}
    float SDF(float3 _position)
    {
        float3 q = abs( _position - center ) - extent;
        return length(max(q,0.0))+min(max(q),0.0);
    }
};

GBox GBox_Ctor(float3 _center, float3 _size)
{
    GBox box;
    box.center=_center;
    box.size=_size;
    box.extent=_size*.5;
    box.boxMin = _center-box.extent;
    box.boxMax = _center+box.extent;
    return box;
}

GBox GBox_Ctor_Extent(float3 _center, float3 _extent)
{
    GBox box;
    box.center=_center;
    box.size= _extent * 2;
    box.extent= _extent ;
    box.boxMin = _center-box.extent;
    box.boxMax = _center+box.extent;
    return box;
}

struct GBoxRound
{
    GBox box;
    float roundness;

    float SDF(float3 _position) { return box.SDF(_position)-roundness; }
};

GBoxRound GRoundBox_Ctor(float3 _center,float3 _size,float _roundness)
{
    GBoxRound roundBox;
    roundBox.box=GBox_Ctor(_center,_size-_roundness*2);
    roundBox.roundness=_roundness;
    return roundBox;
}

struct GBoxFrame
{
    GBox box;
    float frameExtend;

    float SDF(float3 _position)
    {
        float3 p=abs(_position-box.center)-box.extent;
        float3 q=abs(p+frameExtend)-frameExtend;

        return min(
          length(max(float3(p.x,q.y,q.z),0.0))+min(max(p.x,max(q.y,q.z)),0.0),
          length(max(float3(q.x,p.y,q.z),0.0))+min(max(q.x,max(p.y,q.z)),0.0),
          length(max(float3(q.x,q.y,p.z),0.0))+min(max(q.x,max(q.y,p.z)),0.0));
    }
};

GBoxFrame GFrameBox_Ctor(float3 _center,float3 _size,float _frameExtend)
{
    GBoxFrame frameBox;
    frameBox.box=GBox_Ctor(_center,_size);
    frameBox.frameExtend=_frameExtend;
    return frameBox;
}