using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Flags]
    public enum ECubeCorner
    {
        DB=1,
        DL=2,
        DF=4,
        DR=8,
        
        TB=16,
        TL=32,
        TF=64,
        TR=128,
    }
    
    [Flags]
    public enum ECubeFacing
    {
        B=1,        //BL
        L=2,        //LF
        F=4,        //FR
        R=8,        //RB
        T=16,
        D=32,
    }

    
    public partial class KQube
    {
        public static readonly Qube<float3> kUnitQubeBottomed = KQuad.k3SquareCentered.ExpandToQube(kfloat3.up,0f);
        public static readonly Qube<float3> kHalfUnitQubeBottomed = kUnitQubeBottomed.Resize(.5f);
        
        public static readonly Qube<float3> kUnitQubeCentered = KQuad.k3SquareCentered.ExpandToQube(kfloat3.up,.5f);
        public static readonly Qube<float3> kHalfUnitQubeCentered = kUnitQubeCentered.Resize(.5f);
        
        public static readonly Qube<int> kZero = new Qube<int>(0);
        public static readonly Qube<int> kNegOne = new Qube<int>(-1);
        public static readonly Qube<int> kOne = new Qube<int>(1);
        public static readonly Qube<bool> kTrue = new Qube<bool>(true);
        public static readonly Qube<bool> kFalse = new Qube<bool>(false);
        public static readonly Qube<byte> kMaxByte = new Qube<byte>(byte.MaxValue);
        public static readonly Qube<byte> kMinByte = new Qube<byte>(byte.MinValue);
    }


    public partial class KCube
    {
        public static readonly float3[] kPositions = new float3[]
        {
            new float3(.5f,-.5f,-.5f),new float3(-.5f,-.5f,-.5f),new float3(-.5f,-.5f,.5f),new float3(.5f,-.5f,.5f),
            new float3(.5f,.5f,-.5f),new float3(-.5f,.5f,-.5f),new float3(-.5f,.5f,.5f),new float3(.5f,.5f,.5f),
        };

        public static readonly Int3[] kIntervalIdentity = new Int3[]
        {
            new Int3(1,-1,0),new Int3(0,-1,-1),new Int3(-1,-1,0),new Int3(0,-1,1),
            new Int3(1,0,-1),new Int3(-1,0,-1),new Int3(-1,0,1),new Int3(1,0,1),
            new Int3(1,1,0),new Int3(0,1,-1),new Int3(-1,1,0),new Int3(0,1,1),
        };

        public static readonly Int3[] kCornerIdentity = new Int3[]
        {
            new Int3(1, -1, -1), new Int3(-1, -1, -1), new Int3(-1, -1, 1), new Int3(1, -1, 1),
            new Int3(1, 1, -1), new Int3(-1, 1, -1), new Int3(-1, 1, 1), new Int3(1, 1, 1),
        };
    }
    
    public partial class KCubeFacing
    {
        public static readonly CubeSides<Vector3> kUnitSides = new CubeSides<Vector3>(Vector3.back*.5f,Vector3.left*.5f,Vector3.forward*.5f,Vector3.right*.5f,Vector3.up*.5f,Vector3.down*.5f);
    }
    
    public static class KEnumQube<T> where T :struct, Enum
    {
        public static Qube<T> kInvalid = new Qube<T>()
        {
            vDB = UEnum.GetInvalid<T>(),vDL = UEnum.GetInvalid<T>(),vDF = UEnum.GetInvalid<T>(),vDR = UEnum.GetInvalid<T>(),
            vTB = UEnum.GetInvalid<T>(),vTL = UEnum.GetInvalid<T>(),vTF = UEnum.GetInvalid<T>(),vTR = UEnum.GetInvalid<T>()
        };
    }
    
    public interface IQube<T>
    {
        T this[int _index] { get; }
        public T DB { get; }
        public T DL { get; }
        public T DF { get; }
        public T DR { get; }
        public T TB { get; }
        public T TL { get; }
        public T TF { get; }
        public T TR { get; }
    }

    [Serializable]
    public struct Qube<T> : IQube<T>, IEquatable<Qube<T>>,IEqualityComparer<Qube<T>>,IIterate<T>,IEnumerable<T>
    {
        public T vDB;
        public T vDL;
        public T vDF;
        public T vDR;
        public T vTB;
        public T vTL;
        public T vTF;
        public T vTR;
        public Qube(T _value)  { vDB = _value;  vDL = _value; vDF = _value; vDR = _value; vTB = _value; vTL = _value; vTF = _value; vTR = _value; }
        public Qube( T _vertDB, T _vertDL, T _vertDF, T _vertDR, 
            T _vertTB, T _vertTL, T _vertTF, T _vertTR)
        {
            vDB = _vertDB;
            vDL = _vertDL;
            vDF = _vertDF;
            vDR = _vertDR;
            vTB = _vertTB;
            vTL = _vertTL;
            vTF = _vertTF;
            vTR = _vertTR;
        }

        public Qube(Quad<T> _downQuad,Quad<T> _topQuad):this(
            _downQuad.vB,_downQuad.vL,_downQuad.vF,_downQuad.vR,
            _topQuad.vB, _topQuad.vL,_topQuad.vF,_topQuad.vR) 
        {
        }

        public T this[ECubeCorner _corner]
        {
            get => this[_corner.CornerToIndex()];
            set => this[_corner.CornerToIndex()]=value;
        }
        public T this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: throw new IndexOutOfRangeException();
                    case 0: return vDB;
                    case 1: return vDL;
                    case 2: return vDF;
                    case 3: return vDR;
                    case 4: return vTB;
                    case 5: return vTL;
                    case 6: return vTF;
                    case 7: return vTR;
                }
            }
            set
            {
                switch (_index)
                {
                    default: throw new IndexOutOfRangeException();
                    case 0: vDB = value; break;
                    case 1: vDL = value; break;
                    case 2: vDF = value; break;
                    case 3: vDR = value; break;
                    case 4: vTB = value; break;
                    case 5: vTL = value; break;
                    case 6: vTF = value; break;
                    case 7: vTR = value; break;
                }
            }
        }

        public T GetElement(int _index) => this[_index];
        public int Length => 8;
        public T DB => vDB;
        public T DL => vDL;
        public T DF => vDF;
        public T DR => vDR;
        public T TB => vTB;
        public T TL => vTL;
        public T TF => vTF;
        public T TR => vTR;
        
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            yield return vDB;
            yield return vDL;
            yield return vDF;
            yield return vDR;
            yield return vTB;
            yield return vTL;
            yield return vTF;
            yield return vTR;
        }

        public IEnumerable<T> IterateTop()
        {
            yield return vTB;
            yield return vTL;
            yield return vTF;
            yield return vTR;
        }
        
        public IEnumerable<T> IterateDown()
        {
            yield return vDB;
            yield return vDL;
            yield return vDF;
            yield return vDR;
        }
        
        static readonly EqualityComparer<T> kComparer= EqualityComparer<T>.Default;
        public bool Equals(Qube<T> other)
        {
            return kComparer.Equals(vDB,other.vDB) &&
                   kComparer.Equals(vDL,other.vDL) &&
                   kComparer.Equals(vDF,other.vDF) &&
                   kComparer.Equals(vDR,other.vDR) && 
                   kComparer.Equals(vTB,other.vTB) &&
                   kComparer.Equals(vTL,other.vTL) &&
                   kComparer.Equals(vTF,other.vTF) &&
                   kComparer.Equals(vTR,other.vTR);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = vDB.GetHashCode();
                hashCode = (hashCode * 397) ^ vDL.GetHashCode();
                hashCode = (hashCode * 397) ^ vDF.GetHashCode();
                hashCode = (hashCode * 397) ^ vDR.GetHashCode();
                hashCode = (hashCode * 397) ^ vTB.GetHashCode();
                hashCode = (hashCode * 397) ^ vTL.GetHashCode();
                hashCode = (hashCode * 397) ^ vTF.GetHashCode();
                hashCode = (hashCode * 397) ^ vTR.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()=> $"{vDB} {vDL} {vDF} {vDR} / {vTB} {vTL} {vTF} {vTR}";
        
        public static Qube<T> Convert<Y>(Qube<Y> _srcQuad, Func<Y, T> _convert)
        {
            return new Qube<T>(_convert(_srcQuad.vDB), _convert(_srcQuad.vDL), _convert(_srcQuad.vDF), _convert(_srcQuad.vDR),
                _convert(_srcQuad.vTB), _convert(_srcQuad.vTL), _convert(_srcQuad.vTF), _convert(_srcQuad.vTR));
        }
        public static Qube<T> Convert<Y>(Qube<Y> _srcQuad, Func<int,Y, T> _convert)
        {
            return new Qube<T>(_convert(0,_srcQuad.vDB), _convert(1,_srcQuad.vDL), _convert(2,_srcQuad.vDF), _convert(3,_srcQuad.vDR),
                _convert(4,_srcQuad.vTB), _convert(5,_srcQuad.vTL), _convert(6,_srcQuad.vTF), _convert(7,_srcQuad.vTR));
        }

        public bool Equals(Qube<T> x, Qube<T> y)
        {
            return EqualityComparer<T>.Default.Equals(x.vDB, y.vDB) && EqualityComparer<T>.Default.Equals(x.vDL, y.vDL) && EqualityComparer<T>.Default.Equals(x.vDF, y.vDF) && EqualityComparer<T>.Default.Equals(x.vDR, y.vDR) && EqualityComparer<T>.Default.Equals(x.vTB, y.vTB) && EqualityComparer<T>.Default.Equals(x.vTL, y.vTL) && EqualityComparer<T>.Default.Equals(x.vTF, y.vTF) && EqualityComparer<T>.Default.Equals(x.vTR, y.vTR);
        }

        public int GetHashCode(Qube<T> obj)
        {
            unchecked
            {
                var hashCode = EqualityComparer<T>.Default.GetHashCode(obj.vDB);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vDL);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vDF);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vDR);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vTB);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vTL);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vTF);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vTR);
                return hashCode;
            }
        }
    }
    
    [Serializable]
    public struct CubeSides<T>:IEnumerable<T> 
    {
        public T fBL;
        public T fLF;
        public T fFR;
        public T fRB;
        public T fT;
        public T fD;

        public CubeSides(T _fBL, T _fLF, T _fFR, T _fRB,T _fT,T _fD)
        {
            fBL = _fBL;
            fLF = _fLF;
            fFR = _fFR;
            fRB = _fRB;
            fT = _fT;
            fD = _fD;
        }

        public static CubeSides<T> Create<Y>(CubeSides<Y> _src, Func<Y, T> _convert)  =>
            new CubeSides<T>(_convert(_src.fBL),_convert(_src.fLF),_convert(_src.fFR),_convert(_src.fRB),_convert(_src.fT),_convert(_src.fD));
        
        public T this[ECubeFacing _facing]
        {
            get => this[_facing.FacingToIndex()];
            set => this[_facing.FacingToIndex()] = value;
        }

        public T this[EQuadFacing _quadFacing]
        {
            get
            {
                switch (_quadFacing)
                {
                    default: throw new InvalidEnumArgumentException();
                    case EQuadFacing.BL: return fBL;
                    case EQuadFacing.FR: return fFR;
                    case EQuadFacing.LF: return fLF;
                    case EQuadFacing.RB: return fRB;
                }
            }
        }
        
        public T this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: throw new Exception("Invalid Facing:"+_index);
                    case 0: return fBL;
                    case 1: return fLF;
                    case 2: return fFR;
                    case 3: return fRB;
                    case 4: return fT;
                    case 5: return fD;
                }
            }
            set
            {
                switch (_index)
                {
                    default: throw new Exception("Invalid Facing:"+_index);
                    case 0: fBL=value; break;
                    case 1: fLF=value; break;
                    case 2: fFR=value; break;
                    case 3: fRB=value; break;
                    case 4: fT=value; break;
                    case 5: fD=value; break;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return fBL;
            yield return fLF;
            yield return fFR;
            yield return fRB;
            yield return fT;
            yield return fD;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
}