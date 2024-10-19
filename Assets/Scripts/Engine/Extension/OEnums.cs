using System;

[Flags]
public enum EVertexAttribute
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
