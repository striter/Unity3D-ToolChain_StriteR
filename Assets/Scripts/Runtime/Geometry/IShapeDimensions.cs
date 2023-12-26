using System;

namespace Geometry
{
    public interface IShapeDimension<T> where T:struct
    {
        T Center { get; }
    }
    
    public static class IShape_Extension
    {
        public static void DrawGizmos<T>(this IShapeDimension<T> _shape) where T:struct
        { 
            var method = typeof(Gizmos).GetMethod("DrawGizmos", new[] {_shape.GetType()});
            if (method == null)
                throw new NotImplementedException($"Create a DrawGizmos method in {nameof(Gizmos)} for {_shape.GetType()}");
            method.Invoke(null,new object[]{_shape});
        }
    }
}