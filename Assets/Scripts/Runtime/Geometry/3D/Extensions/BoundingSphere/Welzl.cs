using System.Collections.Generic;
using System.Linq.Extensions;

namespace Runtime.Geometry.Extension.BoundingSphere
{
    public static class Welzl<Shape,Dimension> where Shape: IRound<Dimension>,ISDF<Dimension> where Dimension:struct
    {
        private static Shape kHelper;
        private static readonly List<Dimension> kBoundaryPoints = new List<Dimension>(4);
        private static readonly List<Dimension> kContainedPoints = new List<Dimension>();
        public static Shape Evaluate(IEnumerable<Dimension> _positions)
        {
            kContainedPoints.Clear();
            kContainedPoints.TryAddRange(_positions);
            URandom.Shuffle(kContainedPoints,kContainedPoints.Count,1);
            kBoundaryPoints.Clear();
            return GetBoundingSphereWelzl(kContainedPoints,kBoundaryPoints);
        }
        
        static Shape GetBoundingSphereWelzl(IList<Dimension> _positions,IList<Dimension> _boundaries)            //Welzl Algorithm
        {
            if (_positions.Count == 0 || _boundaries.Count == kHelper.kMaxBoundsCount)
                return (Shape)kHelper.Create(_boundaries);

            var lastIndex = _positions.Count - 1;
            var removed = _positions[lastIndex];
            _positions.RemoveAt(lastIndex);
            var sphere = GetBoundingSphereWelzl(_positions,_boundaries);
            if (!sphere.Contains(removed))
            {
                _boundaries.Add(removed);
                sphere = GetBoundingSphereWelzl(_positions,_boundaries);
                _boundaries.RemoveAt(_boundaries.Count-1);
            }
            _positions.Add(removed);
            return sphere;
        }
    }
}