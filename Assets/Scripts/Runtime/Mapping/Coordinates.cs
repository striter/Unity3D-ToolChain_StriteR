using Unity.Mathematics;

public static class UCoordinates
{
    public static class Tile
    {
        public static int2 ToIndex(int _index,int _width) => new int2(_index % _width, _index / _width);
    }
    
    public static class Cylindrical
    {
        public static float3 ToCartesian(float _rad,float _height,float _radius = 1f)
        {
            umath.sincos_fast(_rad, out var sine, out var cosine);
            return new float3(cosine*_radius, _height,sine*_radius);
        }

        public static float3 ToCylindrical(float3 _cartesian)      //x = radius, y = height, z = azimuth
        {
            return new float3(math.sqrt(_cartesian.x*_cartesian.x + _cartesian.z*_cartesian.z),_cartesian.y,math.atan2(_cartesian.z,_cartesian.x));
        }
    }
    
    public static class Spherical
    {
        public static float3 ToCartesian(float _azimuth,float _polar,float _radius = 1f) 
        {
            umath.sincos_fast(_azimuth, out var Asine, out var Acosine);
            umath.sincos_fast(_polar,out var Psine,out var Pcosine);
            return new float3(Psine*Acosine,Pcosine,Psine*Asine) * _radius;
        }
    
        public static float3 ToSpherical(float3 _cartesian)      //x = radius, y = azimuth, z = polar
        {
            var radius = _cartesian.magnitude();
            if(radius > 0)
                _cartesian /= radius;
            return new float3(radius,math.atan2(_cartesian.z,_cartesian.x),math.acos(_cartesian.y));
        }
    }
}