using Unity.Mathematics;

public static class UCoordinates
{
    public static class Tile
    {
        public static int2 ToTile(int _index,int _width) => new int2(_index % _width, _index / _width);
        public static int ToIndex(int2 _pixel, int _width) => _pixel.x + _pixel.y * _width;
        public static int2 ToIndex(int _index,int _width) => new int2(_index % _width, _index / _width);
    }
    
    public static class Polar
    {
        public static float2 ToPolar(float2 _cartesian)
        {
            var radius = math.length(_cartesian);
            var theta = math.atan2(_cartesian.x, _cartesian.y);
            if (theta < 0)
                theta += kmath.kPI2;
            theta *= kmath.kInv2PI;
            return new float2(radius, theta);
        }

        public static float2 ToCartesian(float2 _polar)
        {
            var radius = _polar.x;
            var angle = _polar.y * kmath.kPI2;
            return new float2(radius * math.cos(angle), radius * math.sin(angle));
        }

        //http://psgraphics.blogspot.com/2011/01/improved-code-for-concentric-map.html
        public static float2 ToCartesian_ShirleyChiu(float2 _uv)
        {
            float theta, r;
            var x = _uv.x;
            var y = _uv.y;
            var a = 2*x - 1;
            var b = 2*y - 1;
            if (a == 0 && b == 0) {
                r = theta = 0;
            }
            else if (a*a> b*b) { 
                r = a;
                theta = (kmath.kPI/4)*(b/a);
            } else {
                r = b;
                theta = (kmath.kPI/2) - (kmath.kPI/4)*(a/b);
            }

            r /= 2;
            return new float2( r*math.cos(theta),r*math.sin(theta));
        }
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