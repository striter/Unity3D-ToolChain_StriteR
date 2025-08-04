using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace Runtime.Geometry
{

    public partial struct PTriangle
    {
        public Triangle<int> triangle;
        public PTriangle(Triangle<int> _triangle) { triangle = _triangle;  }
        public PTriangle(int _index0, int _index1, int _index2):this(new Triangle<int>(_index0,_index1,_index2)){}
        public PTriangle(int[] _indexes):this(new Triangle<int>(_indexes[0],_indexes[1],_indexes[2])){}
    }


    [Serializable]
    public partial struct PTriangle:ITriangle<int>,IEnumerable<int>,IIterate<int>,IEquatable<PTriangle>
    {
        public Triangle<T> Convert<T>(IList<T> _vertices) where T:struct => new(_vertices[V0], _vertices[V1],_vertices[V2]);
        
        public static explicit operator PTriangle(Triangle<int> _src) => new(_src);
        public IEnumerable<T> GetEnumerator<T>(IList<T> _vertices)
        {
            yield return _vertices[V0];
            yield return _vertices[V1];
            yield return _vertices[V2];
        }

        public IEnumerator<int> GetEnumerator() => triangle.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

        public IEnumerable<PLine> GetEdges()
        {
            yield return new PLine(V0, V1);
            yield return new PLine(V1, V2);
            yield return new PLine(V2, V0);
        }

        public PTriangle Distinct()
        {
            var newTriangle = new PTriangle(triangle.v0, triangle.v1, triangle.v2);
            foreach (var (index,value) in this.OrderBy(p=>p).LoopIndex())
                newTriangle[index] = value;
            return newTriangle;
        }
        
        public int Length => 3;
        public int this[int _index]
        {
            get => triangle[_index];
            set => triangle[_index] = value;
        }

        public int V0 => triangle.v0;
        public int V1 => triangle.v1;
        public int V2 => triangle.v2;
        public static readonly PTriangle kInvalid = new PTriangle(-1, -1, -1);

        public bool Equals(PTriangle other)=>triangle.Equals(other.triangle);

        public override bool Equals(object obj)=>obj is PTriangle other && Equals(other);

        public static PTriangle operator +(PTriangle _triangle,int _index) => new(_triangle.V0 + _index, _triangle.V1 + _index, _triangle.V2 + _index);
        public static PTriangle operator -(PTriangle _triangle,int _index) => new(_triangle.V0 - _index, _triangle.V1 - _index, _triangle.V2 - _index);
        public static bool operator ==(PTriangle lhs, PTriangle rhs) => lhs.triangle == rhs.triangle;
        public static bool operator !=(PTriangle lhs, PTriangle rhs) => lhs.triangle != rhs.triangle;
        public override int GetHashCode()=> triangle.GetHashCode();
    }

}