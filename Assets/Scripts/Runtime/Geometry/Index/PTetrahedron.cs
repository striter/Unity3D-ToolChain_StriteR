using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;

namespace Runtime.Geometry
{
    public struct PTetrahedron : IEnumerable<int>
    {
        public int v0, v1, v2, v3;
        public PTetrahedron(int _v0, int _v1, int _v2, int _v3) => (v0, v1, v2, v3) = (_v0, _v1, _v2, _v3);
        public static PTetrahedron kInvalid = new PTetrahedron(-1, -1, -1, -1);

        public int this[int index]
        {
            get => index switch { 0 => v0, 1 => v1, 2 => v2, 3 => v3, _ => throw new System.IndexOutOfRangeException()};
            set {
                switch (index)
                {
                    case 0: v0 = value; break;
                    case 1: v1 = value; break;
                    case 2: v2 = value; break;
                    case 3: v3 = value; break;
                    default: throw new System.IndexOutOfRangeException();
                }
            }
        }
        public IEnumerator<int> GetEnumerator()
        {
            yield return v0;
            yield return v1;
            yield return v2;
            yield return v3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<PTriangle> GetTriangles()
        {
            yield return new PTriangle(v0, v1, v2);
            yield return new PTriangle(v2, v1, v3);
            yield return new PTriangle(v2, v3, v0);
            yield return new PTriangle(v3, v1, v0);
        }
        
        public PTetrahedron Distinct()
        {
            var newTetrahedron = new PTetrahedron(v0, v1, v2, v3);
            foreach (var (index, value) in this.OrderBy(p => p).LoopIndex())
                newTetrahedron[index] = value;
            return newTetrahedron;
        }
        
        public static PTetrahedron operator +(PTetrahedron _tetrahedron,int _index) => new(_tetrahedron.v0 + _index, _tetrahedron.v1 + _index, _tetrahedron.v2 + _index, _tetrahedron.v3 + _index);
        public static PTetrahedron operator -(PTetrahedron _tetrahedron,int _index) => new(_tetrahedron.v0 - _index, _tetrahedron.v1 - _index, _tetrahedron.v2 - _index, _tetrahedron.v3 - _index);
    }
}