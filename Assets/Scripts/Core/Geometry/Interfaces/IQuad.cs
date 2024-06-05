using System;
using System.Collections;
using System.Collections.Generic;

namespace Runtime.Geometry
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
    
    public interface IQuad<T>
    {
        T this[int _index] { get; }
        T this[EQuadCorner _corner] { get; }
        T B { get; }
        T L { get; }
        T F { get; }
        T R { get; }
    }

    public partial struct Quad<T> 
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
    }

    [Serializable]
    public partial struct Quad<T> : IQuad<T>, IEquatable<Quad<T>>, IEqualityComparer<Quad<T>>, IIterate<T>, IEnumerable<T>
    {
        
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
    }
    
}