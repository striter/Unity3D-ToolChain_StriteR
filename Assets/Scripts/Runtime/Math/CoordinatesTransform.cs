using Unity.Mathematics;

public class UCoordinateTransform
{
    
    public static float3 CylindricalToCartesian(float _rad,float _height,float _radius = 1f)
    {
        umath.sincos(_rad, out var sine, out var cosine);
        return new float3(cosine*_radius, _height,sine*_radius);
    }

    public static float3 CartesianToCylindrical(float3 _cartesian)      //x = radius, y = height, z = azimuth
    {
        return new float3(math.sqrt(_cartesian.x*_cartesian.x + _cartesian.z*_cartesian.z),_cartesian.y,math.atan2(_cartesian.z,_cartesian.x));
    }
    
    
    public static float3 SphericalToCartesian(float _azimuth,float _polar,float _radius = 1f) 
    {
        umath.sincos(_azimuth, out var Asine, out var Acosine);
        umath.sincos(_polar,out var Psine,out var Pcosine);
        return new float3(Psine*Acosine,Pcosine,Psine*Asine) * _radius;
    }
    
    public static float3 CartesianToSpherical(float3 _cartesian)      //x = radius, y = azimuth, z = polar
    {
        var radius = _cartesian.magnitude();
        if(radius > 0)
            _cartesian /= radius;
        return new float3(radius,math.atan2(_cartesian.z,_cartesian.x),math.acos(_cartesian.y));
    }
}