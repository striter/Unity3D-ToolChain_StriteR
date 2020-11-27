using System;

[Serializable]
public struct RangeFloat
{
    public float start;
    public float length;
    public float end => start + length;
    public RangeFloat(float _start, float _length)
    {
        start = _start;
        length = _length;
    }
}
[Serializable]
public struct RangeInt
{

    public int start;
    public int length;
    public int end => start + length;
    public RangeInt(int _start, int _length)
    {
        start = _start;
        length = _length;
    }
}
