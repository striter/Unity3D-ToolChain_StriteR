using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Geometry
{
    
    public interface ITriangle<T> where T : struct
    {
        T this[int _index] { get; }
        T V0 { get;}
        T V1 { get; }
        T V2 { get; }
    }

    [Serializable]
    public partial struct Triangle<T>: ITriangle<T>, IEquatable<Triangle<T>>, IIterate<T>, IEnumerable<T> where T : struct
    {
        public T v0,v1,v2;
        public Triangle(T _v0, T _v1, T _v2) { v0 = _v0; v1 = _v1; v2 = _v2; }
        public Triangle((T v0,T v1,T v2) _tuple) : this(_tuple.v0,_tuple.v1,_tuple.v2) { }
        public int Length => 3;
        public T this[int _index]
        {
            get => _index switch
                {
                    0 => v0,
                    1 => v1,
                    2 => v2,
                    _ => throw new Exception("Invalid Index:" + _index)
                };
            set
            {
                switch (_index)
                {
                    default:  throw new Exception("Invalid Index:" + _index);
                    case 0: v0 = value; break;
                    case 1: v1 = value; break;
                    case 2: v2 = value; break;
                }
            }
        }

        public T V0 => v0;
        public T V1 => v1;
        public T V2 => v2;

        public bool Equals(Triangle<T> other)
        {
            return v0.Equals(other.v0) && v1.Equals(other.v1) && v2.Equals(other.v2);
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return v0;
            yield return v1;
            yield return v2;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = v0.GetHashCode();
                hashCode = (hashCode * 397) ^ v1.GetHashCode();
                hashCode = (hashCode * 397) ^ v2.GetHashCode();
                return hashCode;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(Triangle<T> x, Triangle<T> y)
        {
            return x.v0.Equals(y.v0) && x.v1.Equals(y.v1) && x.v2.Equals(y.v2);
        }


        public int GetHashCode(Triangle<T> obj)
        {
            unchecked
            {
                int hashCode = obj.v0.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.v1.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.v2.GetHashCode();
                return hashCode;
            }
        }
        
        public static bool operator ==(Triangle<T> left, Triangle<T> right) => left.v0.Equals( right.v0) && left.v1.Equals(right.v1) && left.v2.Equals(right.v2);
        public static bool operator !=(Triangle<T> left, Triangle<T> right) => !(left == right);
    }
}