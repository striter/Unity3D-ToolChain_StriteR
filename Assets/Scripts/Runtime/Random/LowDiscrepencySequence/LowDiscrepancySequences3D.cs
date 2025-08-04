using System.Collections.Generic;
using Unity.Mathematics;
using static kmath;
using static Unity.Mathematics.math;
public static partial class ULowDiscrepancySequences
{
    public static float3 Hammersley3D(uint _index, uint _size)=>new float3(Hammersley(_index,0,_size),Hammersley(_index,1,_size),Hammersley(_index,2,_size));
    public static float3 Halton3D(uint _index) => new float3( Halton(_index,0),Halton(_index,kPrimes128[1]),Halton(_index,kPrimes128[2]));
    
    private static List<float3> kPositionHelper3D = new();
    public static float3[] BCCLattice3D(float3 _spacing,float _bias = float.Epsilon) // normalized spacing
    {
        kPositionHelper3D.Clear();
        var halfSpacing = _spacing / 2.0f;
        var hasOffset = false;
        var position = kfloat3.zero;
        for (var k = 0; k * halfSpacing.y <= 1f ; ++k) {
            position.y = k * halfSpacing.y;

            var offset = (hasOffset) ? halfSpacing.xz : 0f;

            for (var j = 0; j * _spacing.z + offset.y <= 1f; ++j) {
                position.z = j * _spacing.z + offset.y;

                for (var i = 0; i * _spacing.x + offset.x <= 1f; ++i) {
                    position.x = i * _spacing.x + offset.x;
                    kPositionHelper3D.Add(position);
                }
            }

            hasOffset = !hasOffset;
        }

        return kPositionHelper3D.ToArray();
    }
}
