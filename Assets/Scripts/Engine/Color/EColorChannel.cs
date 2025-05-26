using System;

public enum EColorChannel
{
    None = 0,
    Red = 1,
    Green = 2,
    Blue = 3,
    Alpha = 4,
}

[Flags]
public enum EColorChannelFlags
{
    None = 0,
    R = 1 << 0,
    G = 1 << 1,
    B = 1 << 2,
    A = 1 << 3,
    RGB = R | G | B,
    RGBA = R | G | B | A,
}
