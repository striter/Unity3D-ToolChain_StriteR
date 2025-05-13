using System.Collections.Generic;
using Unity.Mathematics;
using static kmath;
using static Unity.Mathematics.math;
public static partial class ULowDiscrepancySequences
{
    private static List<float3> kPositionHelper3D = new();
    public static float3[] BCCLattice3D(float _spacing) // normalized spacing
    {
        kPositionHelper3D.Clear();
        var halfSpacing = _spacing / 2.0f;
        var hasOffset = false;
        var position = kfloat3.zero;
        for (var k = 0; k * halfSpacing <= 1f ; ++k) {
            position.z = k * halfSpacing;

            var offset = (hasOffset) ? halfSpacing : 0f;

            for (var j = 0; j * _spacing + offset <= 1f; ++j) {
                position.y = j * _spacing + offset;

                for (var i = 0; i * _spacing + offset <= 1f; ++i) {
                    position.x = i * _spacing + offset;
                    kPositionHelper3D.Add(position);
                }
            }

            hasOffset = !hasOffset;
        }

        return kPositionHelper3D.ToArray();
    }
}
