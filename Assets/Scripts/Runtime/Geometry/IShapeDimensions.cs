using System;

namespace Runtime.Geometry
{
    public interface IShapeGizmos {
        
    }
    
    public interface IShapeDimension<T>:IShapeGizmos where T:struct 
    {
        T Center { get; }
    }
    
    public static class IShape_Extension
    {
        public static void DrawGizmos(this IShapeGizmos _shape)
        { 
            var method = typeof(Gizmos_Geometry).GetMethod("DrawGizmos", new[] {_shape.GetType()});
            if (method == null)
                throw new NotImplementedException($"Create a DrawGizmos method in {nameof(Gizmos_Geometry)} for {_shape.GetType()}");
            method.Invoke(null,new object[]{_shape});
        }
    }
}