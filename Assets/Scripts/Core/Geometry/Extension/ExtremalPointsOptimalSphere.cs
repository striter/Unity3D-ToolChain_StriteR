using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    //https://ep.liu.se/ecp/034/009/ecp083409.pdf
    public static class ExtremalPointsOptimalSphere
    {
        public enum ENormals
        {
            k001,
            k111,
            k011,
            k012,
            k112,
            k122
        }
        
        public static class Normals
        {
            public static float3[] k001 = { new(1, 0, 0), new(0, 1, 0), new(0, 0, 1) };
            public static float3[] k111 = { new(1, 1, 1), new(1, 1, -1), new(1, -1, 1), new(1, -1, -1) };
            public static float3[] k011 = { new (0,1,2),new (1,-1,0),new(1,0,1),new(1,0,1),new(0,1,1),new(0,1,-1)};
            public static float3[] k012 = { new(0, 1, 2),new(0, 2, 1),new(1, 0, 2),new(2, 0, 1),new(1, 2, 0),new(2, 1, 0),
                                            new(0,1,-2),new(0,2,-1),new(1,0,-2),new(2,0,1),new(1,-2,0),new(2,-1,0) };
            public static float3[] k112 = { new(1, 1, 2), new(2, 1, 1), new(1, 2, 1), new(1, -1, 2), new(1, 1, -2), new(1, -1, -2),
                                            new(2,-1,1),new(2,1,-1),new(2,-1,-1),new(1,-2,1),new(1,2,-1),new(1,-2,1) };

            public static float3[] k122 = { new(2, 2, 1), new(1, 2, 2), new(2, 1, 2), new(2, -2, 1), new(2, 2, -1), new(2, -2, -1),
                                            new(1, -2, 2), new(1, 2, -2),new(1,-2,-2), new(2, -1, 2), new(2, 1, -2), new(2, -1, -2) };

            // public static float3[] GetNormals(ENormals _normals)
            // {
            //     switch (_normals)
            //     {
            //         case 
            //     }
            // }
        }

        // public static GSphere Evaluate(IEnumerable<float3> _positions)
    }
}