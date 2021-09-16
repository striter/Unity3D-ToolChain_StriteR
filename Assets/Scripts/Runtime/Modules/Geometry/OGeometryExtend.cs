using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;

namespace Geometry.Extend
{

    public struct BQuad : IQuad<bool>
    {
        public bool vB { get; set; }
        public bool vL { get; set; }
        public bool vF { get; set; }
        public bool vR { get; set; }

        public BQuad(bool _vB, bool _vL, bool _vF, bool _vR)
        {
            vB = _vB;
            vL = _vL;
            vF = _vF;
            vR = _vR;
        }
        public bool this[int _index]=>this.GetVertex<BQuad,bool>(_index); 
        public bool this[EQuadCorner _corner] =>this.GetVertex<BQuad,bool>(_corner);
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
    public struct BoolQube : IQube<bool>,IEnumerable<bool>
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
        public bool this[int _index] => this.GetVertex<BoolQube,bool>(_index);
        public bool this[EQubeCorner _corner] =>  this.GetVertex<BoolQube,bool>(_corner);
        public IEnumerator<bool> GetEnumerator() => this.GetEnumerator<BoolQube, bool>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
        public byte this[int _index] => this.GetVertex<ByteQube, byte>(_index);
        public byte this[EQubeCorner _corner] => this.GetVertex<ByteQube, byte>(_corner);
    }   
    
    [Serializable]
    public struct ShortQube : IQube<short>
    {
        public short DB;
        public short DL;
        public short DF;
        public short DR;
        public short TB;
        public short TL;
        public short TF;
        public short TR;
        public short vDB { get=>DB; set=>DB=value; }
        public short vDL { get=>DL; set=>DL=value; }
        public short vDF { get=>DF; set=>DF=value; }
        public short vDR { get=>DR; set=>DR=value; }
        public short vTB { get=>TB; set=>TB=value; }
        public short vTL { get=>TL; set=>TL=value; }
        public short vTF { get=>TF; set=>TF=value; }
        public short vTR { get=>TR; set=>TR=value; }
        public short this[int _index] => this.GetVertex<ShortQube, short>(_index);
        public short this[EQubeCorner _corner] => this.GetVertex<ShortQube, short>(_corner);
    }
}