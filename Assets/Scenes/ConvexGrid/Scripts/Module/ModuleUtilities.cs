using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using Procedural;
using UnityEngine;

namespace ConvexGrid
{
    public static class UModule
    {
        public static readonly Quad<Vector3> unitGQuad = new Quad<Vector3>( Vector3.right+Vector3.back,Vector3.back+Vector3.left, Vector3.left+Vector3.forward ,Vector3.forward+Vector3.right).Resize(.5f);
        public static readonly Quad<Vector2> unitG2Quad = unitGQuad.ConvertToG2Quad(p=>p.ToCoord());
        public static readonly Qube<Vector3> unitQube = unitGQuad.ExpandToQube(Vector3.up,0f);

        public static readonly Qube<Vector3> halfUnitQube = unitQube.Resize(.5f);

        public static Vector3 ObjectToModuleVertex(Vector3 _srcVertexOS)
        {
            var uv=unitG2Quad.GetUV(new Vector2(_srcVertexOS.x,_srcVertexOS.z));
            return new Vector3(uv.x,_srcVertexOS.y,uv.y);
        }
        public static Vector3 ModuleToObjectVertex(int _qubeIndex,int _orientation,Vector3 _orientedVertex, Quad<Vector2>[] _moduleShapes,float _height)
        {
            ref var quad = ref _moduleShapes[_qubeIndex % 4];
            var uv = (new Vector2(_orientedVertex.x, _orientedVertex.z));
            uv -= Vector2.one * .5f;
            uv = UMath.m_Rotate2DCW[(4-_orientation)%4].MultiplyVector(uv);
            uv += Vector2.one * .5f;
            
            var point =  quad.GetPoint(uv);
            var offset = _qubeIndex < 4 ? -1 : 0;
            return new Vector3(point.x,offset*_height + _orientedVertex.y*_height,point.y);
        }

        public static Color ToColor(this EModuleType _type)
        {
            switch (_type)
            {
                default: return Color.magenta;
                case EModuleType.Red: return Color.red;
                case EModuleType.Green: return Color.green;
            }
        }
    }

    public static class UModuleByteDealer
    {
        
        public static readonly (byte _byte, int _orientation)[] byteOrientation;
        static readonly Qube<byte>[] voxelModule;
        static readonly (byte _moduleByte,int _moduleIndex,int _orientation)[] moduleByteIndexes;
        
        static UModuleByteDealer()
        {
            List<Qube<bool>> existQubes = new List<Qube<bool>>();
            byteOrientation = new (byte _byte, int _orientation)[byte.MaxValue+1];
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                var srcByte = (byte) i;
                var qube = new Qube<bool>();
                qube.SetByteCorners(srcByte);

                byte orientation = 0; 
                for (ushort j = 1; j <= 3; j++)
                {
                    var rotatedQube = qube.RotateYawCW(j);
                    int existIndex = existQubes.FindIndex(p => p.Equals( rotatedQube));
                    if (existIndex==-1)
                        continue;

                    srcByte = rotatedQube.ToByte();
                    orientation = (byte)(4-j);
                }
                
                if(orientation==0)
                    existQubes.Add(qube);
                byteOrientation[i]=(srcByte,orientation);
            }

            voxelModule = new Qube<byte>[byte.MaxValue+1];
            moduleByteIndexes = new (byte _moduleByte,int _moduleIndex,int _orientation)[byte.MaxValue + 1];
            List<byte> validModules = new List<byte>();
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                var posByte = (byte) i;
                Qube<bool> corner = default;
                corner.SetByteCorners(posByte);
                var qubes = corner.SplitByteQubes();
    
                Qube<byte> qubeModule = default;
                for (int j = 0; j < 8; j++)
                {
                    var moduleByte = qubes[j];
                    var orientedByte = byteOrientation[moduleByte];
                    if(!CheckByteValid(moduleByte))
                        continue;

                    var moduleIndex = validModules.FindIndex(p => p == orientedByte._byte);
                    if (moduleIndex == -1)
                    {
                        moduleIndex = validModules.Count;
                        validModules.Add(moduleByte);
                    }
                    
                    qubeModule[j]=moduleByte;
                    moduleByteIndexes[moduleByte] = (orientedByte._byte ,moduleIndex,orientedByte._orientation);
                }
                voxelModule[i] = qubeModule;
            }
        }

        static bool CheckByteValid(byte _srcByte) => _srcByte != byte.MinValue && _srcByte != byte.MaxValue;
        public static IEnumerable<byte> IterateAllVoxelModuleBytes()
        {
            List<int> _iteratedIndexes = new List<int>();
            foreach (var moduleIndexer in moduleByteIndexes)
            {
                if(!CheckByteValid(moduleIndexer._moduleByte))
                    continue;
                if (_iteratedIndexes.Contains(moduleIndexer._moduleIndex))
                    continue;
                _iteratedIndexes.Add(moduleIndexer._moduleIndex);
                yield return moduleIndexer._moduleByte;
            }
        }
        public static byte CalculateCornerByte(this Qube<EModuleType> _typeQube,int _qubeIndex)
        {
            Qube<bool> qube = default;
            var compare = _typeQube[_qubeIndex];
            for (int i = 0; i < 8; i++)
                qube[i]=_typeQube[i]==compare;
            return voxelModule[qube.ToByte()][_qubeIndex] ;
        }


        public static byte GetModuleByte(byte _voxelCorner)=> moduleByteIndexes[_voxelCorner]._moduleByte;
        public static void GetModuleOrientedIndex(byte _voxelCorner,out int _moduleIndex,out int _orientation)
        {
            _moduleIndex = -1;
            _orientation = 0;
            var orientedByte = moduleByteIndexes[_voxelCorner];
            if (!CheckByteValid(_voxelCorner))
                return;

            _moduleIndex = orientedByte._moduleIndex;
            _orientation = orientedByte._orientation;
        }
    }
}