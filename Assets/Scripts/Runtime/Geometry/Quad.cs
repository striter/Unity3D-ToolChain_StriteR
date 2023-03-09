using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    [Flags]
    public enum EQuadCorner
    {
        B=1,
        L=2,
        F=4,
        R=8,
    }
    
    [Flags]
    public enum EQuadFacing
    {
        BL=1,
        LF=2,
        FR=4,
        RB=8,
    }
    
    public partial class KQuad
    {
        public static readonly Quad<bool> kFalse = new Quad<bool>(false, false, false, false);
        public static readonly Quad<bool> kTrue = new Quad<bool>(true, true, true, true);
        
        public static readonly Quad<Vector3> k3SquareCentered = new Quad<Vector3>( Vector3.right+Vector3.back,Vector3.back+Vector3.left, Vector3.left+Vector3.forward ,Vector3.forward+Vector3.right).Resize(.5f);
        public static readonly Quad<Vector3> k3SquareBottomLeft = new Quad<Vector3>(Vector3.zero,Vector3.forward,Vector3.forward+Vector3.right,Vector3.right);
        public static readonly Quad<Vector3> k3SquareCentered45Deg = new Quad<Vector3>(Vector3.back,Vector3.left,Vector3.forward,Vector3.right);
        
        public static readonly Quad<Vector2> k2SquareCentered = k3SquareCentered.Convert(p=>new Vector2(p.x,p.z));
        
    }

    public interface IQuad<T>
    {
        T this[int _index] { get; }
        T this[EQuadCorner _corner] { get; }
        T B { get; }
        T L { get; }
        T F { get; }
        T R { get; }
    }

    [Serializable]
    public struct Quad<T> : IQuad<T>,IEquatable<Quad<T>>,IEqualityComparer<Quad<T>>,IIterate<T>,IEnumerable<T>
    {
        public T vB;
        public T vL;
        public T vF;
        public T vR;
        public Quad(T _vB, T _vL, T _vF, T _vR)
        {
            vB = _vB;
            vL = _vL;
            vF = _vF;
            vR = _vR;
        }
        public int Length => 4;
        public T B => vB;
        public T L => vL;
        public T F => vF;
        public T R => vR;

        public T this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: throw new Exception("Invalid Corner:"+_index);
                    case 0: return vB;
                    case 1: return vL;
                    case 2: return vF;
                    case 3: return vR;
                }
            }
            set
            {
                switch (_index)
                {
                    default: throw new Exception("Invalid Corner:"+_index);
                    case 0: vB = value;  break;
                    case 1: vL = value;  break;
                    case 2: vF = value;  break;
                    case 3: vR = value;  break;
                }
            }
        }
        public T this[EQuadCorner _corner]
        {
            get
            {
                switch (_corner)
                {
                    default: throw new Exception("Invalid Corner:"+_corner);
                    case EQuadCorner.B: return vB;
                    case EQuadCorner.L: return vL;
                    case EQuadCorner.F: return vF;
                    case EQuadCorner.R: return vR;
                }
            }
            set
            {
                switch (_corner)
                {
                    default: throw new Exception("Invalid Corner:"+_corner);
                    case EQuadCorner.B: vB = value;  break;
                    case EQuadCorner.L: vL = value;  break;
                    case EQuadCorner.F: vF = value;  break;
                    case EQuadCorner.R: vR = value;  break;
                }
            }
        }

        public static Quad<T> Convert<Y>(Quad<Y> _srcQuad, Func<Y, T> _convert) => new Quad<T>(_convert(_srcQuad.vB), _convert(_srcQuad.vL), _convert(_srcQuad.vF), _convert(_srcQuad.vR));
        public static Quad<T> Convert<Y>(Quad<Y> _srcQuad, Func<int,Y, T> _convert) => new Quad<T>(_convert(0, _srcQuad.vB), _convert(1, _srcQuad.vL), _convert(2, _srcQuad.vF),_convert(3, _srcQuad.vR));
        
        #region Implements
        public IEnumerator<T> GetEnumerator()
        {
            yield return vB;
            yield return vL;
            yield return vF;
            yield return vR;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(Quad<T> other)
        {
            return vB.Equals(other.vB) && vL.Equals(other.vL) && vF.Equals(other.vF) && vR.Equals(other.vR);
        }

        public override bool Equals(object obj)
        {
            return obj is Quad<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = vB.GetHashCode();
                hashCode = (hashCode * 397) ^ vL.GetHashCode();
                hashCode = (hashCode * 397) ^ vF.GetHashCode();
                hashCode = (hashCode * 397) ^ vR.GetHashCode();
                return hashCode;
            }
        }
        
        public bool Equals(Quad<T> x, Quad<T> y)
        {
            return x.vB.Equals(y.vB) && x.vL.Equals(y.vL) && x.vF.Equals(y.vF) && x.vR.Equals(y.vR);
        }

        public int GetHashCode(Quad<T> obj)
        {
            unchecked
            {
                int hashCode = obj.vB.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.vL.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.vF.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.vR.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }

    
    [Serializable]
    public struct PQuad:IQuad<int>, IEnumerable<int>,IIterate<int>
    {
        public Quad<int> quad;
        public PQuad(Quad<int> _quad) { quad = _quad;  }
        public PQuad(int _index0, int _index1, int _index2, int _index3):this(new Quad<int>(_index0,_index1,_index2,_index3)){}
        public static explicit operator PQuad(Quad<int> _src) => new PQuad(_src);
        
        public (T v0, T v1, T v2, T v3) GetVertices<T>(IList<T> _vertices) => (_vertices[B], _vertices[L],_vertices[F],_vertices[R]);
        public (Y v0, Y v1, Y v2, Y v3) GetVertices<T,Y>(IList<T> _vertices, Func<T, Y> _getVertex) => ( _getVertex( _vertices[B]), _getVertex(_vertices[L]),_getVertex(_vertices[F]),_getVertex(_vertices[R]));
        
        public int Length => 4;
        public IEnumerable<T> GetEnumerator<T>(IList<T> _vertices)
        {
            yield return _vertices[quad[0]];
            yield return _vertices[quad[1]];
            yield return _vertices[quad[2]];
            yield return _vertices[quad[3]];
        }

        public IEnumerator<int> GetEnumerator() => quad.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

        public int this[int _index] => quad[_index];
        public int this[EQuadCorner _corner] => quad[_corner];

        public int B => quad.B;
        public int L => quad.L;
        public int F => quad.F;
        public int R => quad.R;
    }

    
    [Serializable]
    public struct GQuad : IQuad<Vector3>,IIterate<Vector3>
    {
        public Quad<Vector3> quad;
        public Vector3 normal;
        public float area;
        public GQuad(Quad<Vector3> _quad)
        {
            quad = _quad;
            Vector3 srcNormal = Vector3.Cross(_quad.L - _quad.B, _quad.R - _quad.B);
            normal = srcNormal.normalized;
            area = normal.magnitude / 2;
        }
        public GQuad(Vector3 _vb, Vector3 _vl, Vector3 _vf, Vector3 _vr):this(new Quad<Vector3>(_vb,_vl,_vf,_vr)){}
        public GQuad((Vector3 _vb, Vector3 _vl, Vector3 _vf, Vector3 _vr) _tuple) : this(_tuple._vb, _tuple._vl, _tuple._vf, _tuple._vr) { }
        public static explicit operator GQuad(Quad<Vector3> _src) => new GQuad(_src);
        
        public Vector3 this[int _index] => quad[_index];
        public Vector3 this[EQuadCorner _corner] => quad[_corner];
        public Vector3 B => quad.B;
        public Vector3 L => quad.L;
        public Vector3 F => quad.F;
        public Vector3 R => quad.R;
        public int Length => quad.Length;

        public static GQuad operator +(GQuad _src, Vector3 _dst)=> new GQuad(_src.B + _dst, _src.L + _dst, _src.F + _dst,_src.R+_dst);
        public static GQuad operator -(GQuad _src, Vector3 _dst)=> new GQuad(_src.B - _dst, _src.L - _dst, _src.F - _dst,_src.R-_dst);
    }
}