using System;

[Serializable]
public struct CullingMask
{
    public int value;

    public CullingMask(int _value)
    {
        value = _value;
    }
    
    public bool HasLayer(int _layer)
    {
        if (value == -1)
            return true;
        
        return (value & (1 << _layer)) != 0;
    }
    
    public static implicit operator CullingMask(int _value) => new CullingMask(_value);
    public static implicit operator int(CullingMask _value) => _value.value;
    public static bool HasLayer(CullingMask _value, int _layer) => _value.HasLayer(_layer);
    public static CullingMask kAll = -1;
    public static CullingMask kNone = 0;
}
