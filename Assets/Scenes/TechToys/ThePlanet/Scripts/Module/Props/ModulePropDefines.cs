using System.Collections.Generic;
using System.Linq;
using Geometry;
using Procedural;
using TPoolStatic;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.ThePlanet.Module.Prop
{
    public enum EModulePropType
    {
        Default=0,
        Light=1,
        Flower=2,
    }

    public static class DModuleProp
    {
        private static readonly byte kWFCVoxelPath = new Qube<bool>(false, false, false, false, true, true, true, true).ToByte();
        public static bool IsPath(this Qube<byte> _src) => _src.IterateTop().All(_p => _p == kWFCVoxelPath);
        // public static bool IsVoxelEntrance(this Qube<byte> _src) => _src.IterateTop().Count(p => p % 48 == 0)>=2;
        
        public static readonly List<Quad<bool>> kAllPossibilities = new List<Quad<bool>>();
        static DModuleProp()
        {
            for (byte i = 0; i < 1 << 4; i++)
                kAllPossibilities.Add(new  Quad<bool>(UByte.PosValid(i,0),UByte.PosValid(i,1),UByte.PosValid(i,2),UByte.PosValid(i,3)));
        }

        public static bool IsValidPath(byte _paths)
        {
            Quad<bool> facing = default;
            facing.SetByteElement(_paths);
            return true;
        }

        public static bool IsValidDecoration(bool _voxel,byte _qubeByte)
        {
            if (_qubeByte == byte.MinValue||_qubeByte==byte.MaxValue)
                return false;
            
            if (_voxel)
                return true;
            
            Qube<bool> parts = default;
            parts.SetByteElement(_qubeByte);
            return !parts.vDB && !parts.vDL && !parts.vDF && !parts.vDR;
        }

        private static int ToPossibilityPercentage(this Quad<bool> _quad)
        {
            var count=_quad.Count(p => p);
            switch (count)      //Sum 100
            {
                default: return 0;
                case 1: return 20;
                case 2: return 35;
                case 3: return 25;
                case 4: return 20;
            }
        }

        public static Quad<bool> SelectPossibility(this List<Quad<bool>> _possibilities, float _random)
        {
            TSPoolList<int>.Spawn(out var totalRandom);
            int totalAmount = 0;
            totalRandom.Clear();
            foreach (var possibility in _possibilities)
            {
                totalAmount += possibility.ToPossibilityPercentage();
                totalRandom.Add(totalAmount);
            }
            int percentage = (int)(_random * totalAmount);
            var index=totalRandom.FindIndex(p=>p>=percentage);
            TSPoolList<int>.Recycle(totalRandom);
            return _possibilities[index];
        }

        public static float3 ObjectToOrientedVertex(float3 _vertexOS)
        {
            var uv=KQuad.k2SquareCentered.GetUV(new Vector2(_vertexOS.x,_vertexOS.z));
            return new float3(uv.x, _vertexOS.y,uv.y);
        }

        public static float3 OrientedToObjectVertex(TrapezoidQuad _qubeShape , float3 _orientedVertex,int _orientation)
        {
            var uv = new Vector2(_orientedVertex.x, _orientedVertex.z);
            uv -= Vector2.one * .5f;
            uv = KRotation.kRotate2DCW[(4-_orientation)%4].MultiplyVector(uv);     //Inverted Cause CC Bilinear Lerp Below
            uv += Vector2.one * .5f;
            return _qubeShape.GetPoint(uv.x, uv.y,_orientedVertex.y*KPCG.kUnitSize*2);
        }

        static float InterpolateYaw(float _src, float _dst, float _value)
        {
            _src = (_src + 360f) % 360f;
            _dst = (_dst + 360f) % 360f;
            
            float interpolation = _dst - _src;

            float forwardInterpolate = Mathf.Abs(interpolation);
            float inverseInterpolate = 360f - forwardInterpolate;
            if (inverseInterpolate < forwardInterpolate)
                interpolation = inverseInterpolate;
            
            return _src + interpolation * _value;
        }
        
        public static void OrientedToObjectVertex(int _orientation,
            Vector3 _orientedVertex, TrapezoidQuad _quadShape,out Vector3 _objectPosition,
            Quaternion _orientedRotation,out Quaternion _objectRotation)
        {
            _objectPosition = OrientedToObjectVertex(_quadShape,_orientedVertex,_orientation); 
            _objectRotation = Quaternion.Euler(0f,_orientation*90,0f) * _orientedRotation;
        }

    }
}