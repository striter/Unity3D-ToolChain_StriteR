using System;
public enum EVertexAttribute
{
    UV0 = 0,
    UV1 = 1,
    UV2 = 2,
    UV3 = 3,
    UV4 = 4,
    UV5 = 5,
    UV6 = 6,
    UV7 = 7,
    Normal = 8,
    Tangent = 9,
    Color = 10
}

[Flags]
public enum EVertexAttributeFlags
{
    None = 0,
    UV0 = 1 << 0,
    UV1= 1 << 1,
    UV2= 1 << 2,
    UV3= 1 << 3,
    UV4= 1 << 4,
    UV5= 1 << 5,
    UV6= 1 << 6,
    UV7= 1 << 7,
    Normal = 1 << 8,
    Tangent = 1 << 9,
    Color = 1 << 10,
}