using System;
using Geometry.Extend;
using Geometry.Pixel;
using Geometry.Voxel;
using Procedural;
using UnityEngine;

namespace ConvexGrid
{
    public enum EModuleType
    {
        Invalid=-1,
        Green,
        Red,
    }
    
    public struct PileQube : IQube<PileID>
    {
        public PileID vDB { get; set; }
        public PileID vDL { get; set; }
        public PileID vDF { get; set; }
        public PileID vDR { get; set; }
        public PileID vTB { get; set; }
        public PileID vTL { get; set; }
        public PileID vTF { get; set; }
        public PileID vTR { get; set; }

        public PileID this[int _index]
        {
            get => this.GetCorner<PileQube, PileID>(_index);
            set => this.SetCorner(_index, value);
        }

        public PileID this[EQubeCorner _corner]
        {
            get => this.GetCorner<PileQube, PileID>(_corner);
            set => this.SetCorner(_corner, value);
        }
    }

    public interface IVoxel
    { 
        Transform Transform { get; }
        PileID Identity { get; }
        PileQube QubeCorners { get; }
        BoolQube CornerRelations { get; }
        BCubeFacing SideRelations { get;}
        G2Quad[] CornerShapeLS { get; }
    }

    public interface ICorner
    {
        Transform Transform { get; }
        PileID Identity { get; }
    }
}