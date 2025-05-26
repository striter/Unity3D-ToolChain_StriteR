using System;

public enum EVertexAttribute
{
    UV0 = EVertexAttributeFlags.UV0,
    UV1 = EVertexAttributeFlags.UV1,
    UV2 = EVertexAttributeFlags.UV2,
    UV3 = EVertexAttributeFlags.UV3,
    UV4 = EVertexAttributeFlags.UV4,
    UV5 = EVertexAttributeFlags.UV5,
    UV6 = EVertexAttributeFlags.UV6,
    UV7 = EVertexAttributeFlags.UV7,
    Normal = EVertexAttributeFlags.Normal,
    Tangent = EVertexAttributeFlags.Tangent,
    Color = EVertexAttributeFlags.Color,
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
