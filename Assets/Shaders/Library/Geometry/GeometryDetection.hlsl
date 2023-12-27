
bool Contains(GBox _box, float3 _pos)
{
    return _box.boxMin.x <= _pos.x && _pos.x <= _box.boxMax.x 
        && _box.boxMin.y <= _pos.y && _pos.y <= _box.boxMax.y 
        && _box.boxMin.z <= _pos.z && _pos.z <= _box.boxMax.z;
}