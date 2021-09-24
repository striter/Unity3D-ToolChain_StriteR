using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;

namespace Geometry.Extend
{

    public struct BoolQuad : IQuad<bool>
    {
        public bool vB { get; set; }
        public bool vL { get; set; }
        public bool vF { get; set; }
        public bool vR { get; set; }

        public BoolQuad(bool _vB, bool _vL, bool _vF, bool _vR)
        {
            vB = _vB;
            vL = _vL;
            vF = _vF;
            vR = _vR;
        }
        public bool this[int _index]=>this.GetVertex<BoolQuad,bool>(_index); 
        public bool this[EQuadCorner _corner] =>this.GetVertex<BoolQuad,bool>(_corner);
    }

    public struct BCubeFacing:ICubeFace<bool>
    {
        public bool fBL { get; set; }
        public bool fLF { get; set; }
        public bool fFR { get; set; }
        public bool fRB { get; set; }
        public bool fT { get; set; }
        public bool fD { get; set; }

        public BCubeFacing( bool _fBL, bool _fLF, bool _fFR, bool _fRB,bool _fT,bool _fD)
        {
            fBL = _fBL;
            fLF = _fLF;
            fFR = _fFR;
            fRB = _fRB;
            fT = _fT;
            fD = _fD;
        }

        public bool this[int _index] => this.GetFacing<BCubeFacing,bool>(_index);

        public bool this[ECubeFacing _facing] => this.GetFacing<BCubeFacing, bool>(_facing);
    }
    [Serializable]
    public struct BoolQube : IQube<bool>,IEnumerable<bool>,IEquatable<BoolQube>
    {
        public bool DB;
        public bool DL;
        public bool DF;
        public bool DR;
        public bool TB;
        public bool TL;
        public bool TF;
        public bool TR;
        public bool vDB { get=>DB; set=>DB=value; }
        public bool vDL { get=>DL; set=>DL=value; }
        public bool vDF { get=>DF; set=>DF=value; }
        public bool vDR { get=>DR; set=>DR=value; }
        public bool vTB { get=>TB; set=>TB=value; }
        public bool vTL { get=>TL; set=>TL=value; }
        public bool vTF { get=>TF; set=>TF=value; }
        public bool vTR { get=>TR; set=>TR=value; }

        public bool this[int _index]
        {
            get => this.GetCorner<BoolQube,bool>(_index);
            set => this.SetCorner(_index,value);
        }

        public bool this[EQubeCorner _corner]
        {
            get => this.GetCorner<BoolQube, bool>(_corner);
            set => this.SetCorner(_corner, value);
        }
        public IEnumerator<bool> GetEnumerator() => this.GetEnumerator<BoolQube, bool>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(BoolQube other)=>DB == other.DB && DL == other.DL && DF == other.DF && DR == other.DR && TB == other.TB && TL == other.TL && TF == other.TF && TR == other.TR;

        public override bool Equals(object obj)=> obj is BoolQube other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = DB.GetHashCode();
                hashCode = (hashCode * 397) ^ DL.GetHashCode();
                hashCode = (hashCode * 397) ^ DF.GetHashCode();
                hashCode = (hashCode * 397) ^ DR.GetHashCode();
                hashCode = (hashCode * 397) ^ TB.GetHashCode();
                hashCode = (hashCode * 397) ^ TL.GetHashCode();
                hashCode = (hashCode * 397) ^ TF.GetHashCode();
                hashCode = (hashCode * 397) ^ TR.GetHashCode();
                return hashCode;
            }
        }
    }

    [Serializable]
    public struct ByteQube : IQube<byte>
    {
        public byte DB;
        public byte DL;
        public byte DF;
        public byte DR;
        public byte TB;
        public byte TL;
        public byte TF;
        public byte TR;
        public byte vDB { get=>DB; set=>DB=value; }
        public byte vDL { get=>DL; set=>DL=value; }
        public byte vDF { get=>DF; set=>DF=value; }
        public byte vDR { get=>DR; set=>DR=value; }
        public byte vTB { get=>TB; set=>TB=value; }
        public byte vTL { get=>TL; set=>TL=value; }
        public byte vTF { get=>TF; set=>TF=value; }
        public byte vTR { get=>TR; set=>TR=value; }

        public byte this[int _index]
        {
            get => this.GetCorner<ByteQube, byte>(_index);
            set => this.SetCorner(_index, value);
        }

        public byte this[EQubeCorner _corner]
        {
            get => this.GetCorner<ByteQube, byte>(_corner);
            set => this.SetCorner(_corner, value);
        }
    }   
    
    [Serializable]
    public struct IntQube : IQube<int>
    {
        public int DB;
        public int DL;
        public int DF;
        public int DR;
        public int TB;
        public int TL;
        public int TF;
        public int TR;
        public int vDB { get=>DB; set=>DB=value; }
        public int vDL { get=>DL; set=>DL=value; }
        public int vDF { get=>DF; set=>DF=value; }
        public int vDR { get=>DR; set=>DR=value; }
        public int vTB { get=>TB; set=>TB=value; }
        public int vTL { get=>TL; set=>TL=value; }
        public int vTF { get=>TF; set=>TF=value; }
        public int vTR { get=>TR; set=>TR=value; }
        public int this[int _index]
        {
            get => this.GetCorner<IntQube, int>(_index);
            set => this.SetCorner(_index,value);
        }

        public int this[EQubeCorner _corner]
        {
            get => this.GetCorner<IntQube, int>(_corner);
            set => this.SetCorner(_corner, value);
        }

        IntQube(int _index)  { DB = _index;  DL = _index; DF = _index; DR = _index; TB = _index; TL = _index; TF = _index; TR = _index; }
        public static readonly IntQube Zero = new IntQube(0);
        public static readonly IntQube NegOne = new IntQube(-1);
        public static readonly IntQube One = new IntQube(1);
    }
    
    public struct EnumQube<T> : IQube<T> where T:struct,Enum
    {
        public T vDB { get; set; }
        public T vDL { get; set; }
        public T vDF { get; set; }
        public T vDR { get; set; }
        public T vTB { get; set; }
        public T vTL { get; set; }
        public T vTF { get; set; }
        public T vTR { get; set; }

        public T this[int _index]
        {
           get=> this.GetCorner<EnumQube<T>, T>(_index);
           set => this.SetCorner(_index,value);
        }

        public T this[EQubeCorner _corner]
        {
            get => this.GetCorner<EnumQube<T>, T>(_corner);
            set => this.SetCorner(_corner, value);
        }
        
        public static EnumQube<T> Invalid = new EnumQube<T>()
        {
            vDB = UEnum.GetInvalid<T>(),vDL = UEnum.GetInvalid<T>(),vDF = UEnum.GetInvalid<T>(),vDR = UEnum.GetInvalid<T>(),
            vTB = UEnum.GetInvalid<T>(),vTL = UEnum.GetInvalid<T>(),vTF = UEnum.GetInvalid<T>(),vTR = UEnum.GetInvalid<T>()
        };
    }
}