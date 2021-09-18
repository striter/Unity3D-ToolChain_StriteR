using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Extend;
using Geometry.Pixel;
using Geometry.Voxel;
using Procedural;
using UnityEngine;

namespace ConvexGrid
{
    public static class UModule
    {
        public static readonly GQuad unitGQuad = new GQuad( Vector3.right+Vector3.back,Vector3.back+Vector3.left, Vector3.left+Vector3.forward ,Vector3.forward+Vector3.right).Resize<GQuad,Vector3>(.5f);
        public static readonly G2Quad unitG2Quad = unitGQuad.ConvertToG2Quad(p=>p.ToCoord());
        public static readonly GQube unitQube = unitGQuad.ExpandToQUbe(Vector3.up,0f);

        public static readonly GQube halfUnitQube = unitQube.Resize<GQube, Vector3>(.5f);

        public static readonly (byte _srcByte, int _orientation)[] orientedBytes;

        public static readonly ByteQube[] voxelModule;
        static readonly (int _moduleIndex,byte _srcByte,int _orientation)[] voxelModuleIndexer;
        static bool CheckByteValid(byte _srcByte) => _srcByte != byte.MinValue && _srcByte != byte.MaxValue;
        public static void GetVoxelModuleUnit(byte _voxelCorner,out int _moduleIndex,out int _orientation)
        {
            _moduleIndex = -1;
            _orientation = 0;
            var orientedByte = voxelModuleIndexer[_voxelCorner];
            if (!CheckByteValid(orientedByte._srcByte))
                return;

            _moduleIndex = orientedByte._moduleIndex;
            _orientation = orientedByte._orientation;
        }

        public static IEnumerable<byte> IterateAllVoxelModuleBytes()
        {
            List<int> _iteratedIndexes = new List<int>();
            foreach (var moduleIndexer in voxelModuleIndexer)
            {
                if(!CheckByteValid(moduleIndexer._srcByte))
                    continue;
                if (_iteratedIndexes.Contains(moduleIndexer._moduleIndex))
                    continue;
                _iteratedIndexes.Add(moduleIndexer._moduleIndex);
                yield return moduleIndexer._srcByte;
            }
        }
        
        static UModule()
        {
            List<BoolQube> existQubes = new List<BoolQube>();
            orientedBytes = new (byte _srcByte, int _orientation)[byte.MaxValue+1];
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                var srcByte = (byte) i;
                var qube = new BoolQube();
                qube.SetByteCorners(srcByte);

                byte orientation = 0; 
                for (ushort j = 1; j <= 3; j++)
                {
                    var rotatedQube = qube.RotateYawCW<BoolQube, BoolQuad, bool>(j);
                    int existIndex = existQubes.FindIndex(p => p.Equals( rotatedQube));
                    if (existIndex==-1)
                        continue;

                    srcByte = rotatedQube.ToByte();
                    orientation = (byte)(4-j);
                }
                
                if(orientation==0)
                    existQubes.Add(qube);
                orientedBytes[i]=(srcByte,orientation);
            }

            voxelModule = new ByteQube[byte.MaxValue+1];
            voxelModuleIndexer = new (int _moduleIndex,byte _srcByte,int _orientation)[byte.MaxValue + 1];
            List<byte> validModules = new List<byte>();
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                var posByte = (byte) i;
                BoolQube corner = default;
                corner.SetByteCorners(posByte);
                var qubes = corner.SplitByteQubes();

                ByteQube qubeModule = default;
                for (int j = 0; j < 8; j++)
                {
                    var moduleByte = qubes[j].ToByte();
                    var moduleIndex = validModules.Count;
                    var moduleOrientation = 0;
                    var orientedByte = orientedBytes[moduleByte];
                    if(!CheckByteValid(moduleByte))
                        continue;

                    var index = validModules.FindIndex(p => p == orientedByte._srcByte);
                    if (index != -1)
                    {
                        moduleIndex = index;
                        moduleOrientation = orientedByte._orientation;
                    }
                    else
                    {
                        validModules.Add(orientedByte._srcByte);
                    }
                    
                    qubeModule.SetCorner(j,moduleByte);
                    voxelModuleIndexer[moduleByte] = (moduleIndex,moduleByte,moduleOrientation);
                }
                voxelModule[i] = qubeModule;
            }
        }

        public static Vector3 ObjectToModuleVertex(Vector3 _srcVertexOS)
        {
            var uv=unitG2Quad.GetUV<G2Quad, Vector2>(new Vector2(_srcVertexOS.x,_srcVertexOS.z));
            return new Vector3(uv.x,_srcVertexOS.y,uv.y);
        }
        public static Vector3 ModuleToObjectVertex(int _qubeIndex,int _orientation,Vector3 _orientedVertex, G2Quad[] _moduleShapes,float _height)
        {
            ref var quad = ref _moduleShapes[_qubeIndex % 4];
            var uv = (new Vector2(_orientedVertex.x, _orientedVertex.z));
            uv -= Vector2.one * .5f;
            uv = UMath.m_Rotate2DCW[(4-_orientation)%4].MultiplyVector(uv);
            uv += Vector2.one * .5f;
            
            var point = quad.GetPoint<G2Quad,Vector2>(uv);
            var offset = _qubeIndex < 4 ? -1 : 0;
            return new Vector3(point.x,offset*_height + _orientedVertex.y*_height,point.y);
        }
    }
}