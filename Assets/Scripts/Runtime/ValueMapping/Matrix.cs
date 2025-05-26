using Unity.Mathematics;

public static partial class umath
{
    public static float4x4 inverse(this float4x4 a)
    {
        var r = 1 / determinant(a);
        var x00 = (a.c1.z * a.c2.w * a.c3.y - a.c1.w * a.c2.z * a.c3.y + a.c1.w * a.c2.y * a.c3.z -
            a.c1.y * a.c2.w * a.c3.z - a.c1.z * a.c2.y * a.c3.w + a.c1.y * a.c2.z * a.c3.w) * r;
        var x01 = (a.c0.w * a.c2.z * a.c3.y - a.c0.z * a.c2.w * a.c3.y - a.c0.w * a.c2.y * a.c3.z +
            a.c0.y * a.c2.w * a.c3.z + a.c0.z * a.c2.y * a.c3.w - a.c0.y * a.c2.z * a.c3.w) * r;
        var x02 = (a.c0.z * a.c1.w * a.c3.y - a.c0.w * a.c1.z * a.c3.y + a.c0.w * a.c1.y * a.c3.z -
            a.c0.y * a.c1.w * a.c3.z - a.c0.z * a.c1.y * a.c3.w + a.c0.y * a.c1.z * a.c3.w) * r;
        var x03 = (a.c0.w * a.c1.z * a.c2.y - a.c0.z * a.c1.w * a.c2.y - a.c0.w * a.c1.y * a.c2.z +
            a.c0.y * a.c1.w * a.c2.z + a.c0.z * a.c1.y * a.c2.w - a.c0.y * a.c1.z * a.c2.w) * r;
        var x10 = (a.c1.w * a.c2.z * a.c3.x - a.c1.z * a.c2.w * a.c3.x - a.c1.w * a.c2.x * a.c3.z +
            a.c1.x * a.c2.w * a.c3.z + a.c1.z * a.c2.x * a.c3.w - a.c1.x * a.c2.z * a.c3.w) * r;
        var x11 = (a.c0.z * a.c2.w * a.c3.x - a.c0.w * a.c2.z * a.c3.x + a.c0.w * a.c2.x * a.c3.z -
            a.c0.x * a.c2.w * a.c3.z - a.c0.z * a.c2.x * a.c3.w + a.c0.x * a.c2.z * a.c3.w) * r;
        var x12 = (a.c0.w * a.c1.z * a.c3.x - a.c0.z * a.c1.w * a.c3.x - a.c0.w * a.c1.x * a.c3.z +
            a.c0.x * a.c1.w * a.c3.z + a.c0.z * a.c1.x * a.c3.w - a.c0.x * a.c1.z * a.c3.w) * r;
        var x13 = (a.c0.z * a.c1.w * a.c2.x - a.c0.w * a.c1.z * a.c2.x + a.c0.w * a.c1.x * a.c2.z -
            a.c0.x * a.c1.w * a.c2.z - a.c0.z * a.c1.x * a.c2.w + a.c0.x * a.c1.z * a.c2.w) * r;
        var x20 = (a.c1.y * a.c2.w * a.c3.x - a.c1.w * a.c2.y * a.c3.x + a.c1.w * a.c2.x * a.c3.y -
            a.c1.x * a.c2.w * a.c3.y - a.c1.y * a.c2.x * a.c3.w + a.c1.x * a.c2.y * a.c3.w) * r;
        var x21 = (a.c0.w * a.c2.y * a.c3.x - a.c0.y * a.c2.w * a.c3.x - a.c0.w * a.c2.x * a.c3.y +
            a.c0.x * a.c2.w * a.c3.y + a.c0.y * a.c2.x * a.c3.w - a.c0.x * a.c2.y * a.c3.w) * r;
        var x22 = (a.c0.y * a.c1.w * a.c3.x - a.c0.w * a.c1.y * a.c3.x + a.c0.w * a.c1.x * a.c3.y -
            a.c0.x * a.c1.w * a.c3.y - a.c0.y * a.c1.x * a.c3.w + a.c0.x * a.c1.y * a.c3.w) * r;
        var x23 = (a.c0.w * a.c1.y * a.c2.x - a.c0.y * a.c1.w * a.c2.x - a.c0.w * a.c1.x * a.c2.y +
            a.c0.x * a.c1.w * a.c2.y + a.c0.y * a.c1.x * a.c2.w - a.c0.x * a.c1.y * a.c2.w) * r;
        var x30 = (a.c1.z * a.c2.y * a.c3.x - a.c1.y * a.c2.z * a.c3.x - a.c1.z * a.c2.x * a.c3.y +
            a.c1.x * a.c2.z * a.c3.y + a.c1.y * a.c2.x * a.c3.z - a.c1.x * a.c2.y * a.c3.z) * r;
        var x31 = (a.c0.y * a.c2.z * a.c3.x - a.c0.z * a.c2.y * a.c3.x + a.c0.z * a.c2.x * a.c3.y -
            a.c0.x * a.c2.z * a.c3.y - a.c0.y * a.c2.x * a.c3.z + a.c0.x * a.c2.y * a.c3.z) * r;
        var x32 = (a.c0.z * a.c1.y * a.c3.x - a.c0.y * a.c1.z * a.c3.x - a.c0.z * a.c1.x * a.c3.y +
            a.c0.x * a.c1.z * a.c3.y + a.c0.y * a.c1.x * a.c3.z - a.c0.x * a.c1.y * a.c3.z) * r;
        var xx33 = (a.c0.y * a.c1.z * a.c2.x - a.c0.z * a.c1.y * a.c2.x + a.c0.z * a.c1.x * a.c2.y -
            a.c0.x * a.c1.z * a.c2.y - a.c0.y * a.c1.x * a.c2.z + a.c0.x * a.c1.y * a.c2.z) * r;
        return new float4x4(x00, x01, x02, x03, x10, x11, x12, x13, x20, x21, x22, x23, x30, x31, x32, xx33);
    }

    public static float determinant(this float4x4 a) => a.c0.x * a.c1.y * a.c2.z * a.c3.w -
        a.c0.x * a.c1.y * a.c2.w * a.c3.z
        + a.c0.x * a.c1.z * a.c2.w * a.c3.y - a.c0.x * a.c1.z * a.c2.y * a.c3.w
        + a.c0.x * a.c1.w * a.c2.y * a.c3.z - a.c0.x * a.c1.w * a.c2.z * a.c3.y
                                            - a.c0.y * a.c1.z * a.c2.w * a.c3.x + a.c0.y * a.c1.z * a.c2.x * a.c3.w
        - a.c0.y * a.c1.w * a.c2.x * a.c3.z + a.c0.y * a.c1.w * a.c2.z * a.c3.x
        - a.c0.y * a.c1.x * a.c2.z * a.c3.w + a.c0.y * a.c1.x * a.c2.w * a.c3.z
                                            + a.c0.z * a.c1.w * a.c2.x * a.c3.y - a.c0.z * a.c1.w * a.c2.y * a.c3.x
        + a.c0.z * a.c1.x * a.c2.y * a.c3.w - a.c0.z * a.c1.x * a.c2.w * a.c3.y
        + a.c0.z * a.c1.y * a.c2.w * a.c3.x - a.c0.z * a.c1.y * a.c2.x * a.c3.w
                                            - a.c0.w * a.c1.x * a.c2.y * a.c3.z + a.c0.w * a.c1.x * a.c2.z * a.c3.y
        - a.c0.w * a.c1.y * a.c2.z * a.c3.x + a.c0.w * a.c1.y * a.c2.x * a.c3.z
        - a.c0.w * a.c1.z * a.c2.x * a.c3.y + a.c0.w * a.c1.z * a.c2.y * a.c3.x;

    public static float3 mulPossition(this float4x4 a, float3 b) => new float3(
        a.c0.x * b.x + a.c0.y * b.y + a.c0.z * b.z + a.c0.w,
        a.c1.x * b.x + a.c1.y * b.y + a.c1.z * b.z + a.c1.w,
        a.c2.x * b.x + a.c2.y * b.y + a.c2.z * b.z + a.c2.w);
    
    
    public static float4x4 add(this float4x4 a, float4x4 b) => new float4x4(a.c0 + b.c0, a.c1 + b.c1, a.c2 + b.c2, a.c3 + b.c3);
}
