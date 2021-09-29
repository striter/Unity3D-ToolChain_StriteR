using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using Procedural;
using UnityEngine;

namespace PolyGrid.Module
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
    }

    public static class UModuleByte
    {
        struct ModuleByteData
        {
            public byte moduleByte;
            public int moduleIndex;
            public int orientation;

            public bool Valid => moduleByte > 0 && moduleIndex >= 0;
            public static readonly ModuleByteData Invalid = new ModuleByteData()
                { moduleByte = 0, moduleIndex = -1, orientation = -1 };
        }
        
        private static readonly Qube<byte>[] byteQubeIndexer;
        private static readonly Dictionary<ECornerStatus, ModuleByteData[]> typedModuleIndexes=new Dictionary<ECornerStatus, ModuleByteData[]>();
        public static byte GetByte(byte _srcByte,ECornerStatus _status)=> typedModuleIndexes[_status][_srcByte].moduleByte;
        public static bool IsValidByte(byte _srcByte)
        {
            if (_srcByte == byte.MinValue || _srcByte == byte.MaxValue)
                return false;
            return true;
        }
        static bool IsValidByte(byte _srcByte, ECornerStatus _status)
        {
            if (!IsValidByte(_srcByte))
                return false;
            if (_status == ECornerStatus.Body)
                return true;
            Qube<bool> _srcBool = default;
            _srcBool.SetByteCorners(_srcByte);
            switch (_status)
            {
                default: throw new InvalidEnumArgumentException();
                case ECornerStatus.Rooftop:
                    if (ECubeFacing.T.GetFacingCorners().Any(p => _srcBool[p] == true))
                        return false;
                    break;
                case ECornerStatus.Bottom:
                    if (ECubeFacing.D.GetFacingCorners().Any(p => _srcBool[p] == true))
                        return false;
                    break;
            }
            return true;
        }
        
        
        static UModuleByte()
        {
            //Orientated Byte Indexer
            var byteOrientation = new (byte _byte, int _orientation)[byte.MaxValue+1];
            List<Qube<bool>> existQubes = new List<Qube<bool>>();
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
            existQubes.Clear();

            //Create Byte Qube Indexer
            byteQubeIndexer = new Qube<byte>[byte.MaxValue+1];
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                var posByte = (byte) i;
                Qube<bool> corner = default;
                corner.SetByteCorners(posByte);
                byteQubeIndexer[i] =  corner.SplitByteQubes();
            }
            
            //Typed Modules Indexer
            List<byte> validModule = new List<byte>();
            foreach (var status in UEnum.GetValues<ECornerStatus>())
            {
                validModule.Clear();
                var moduleIndexes = new ModuleByteData[byte.MaxValue + 1];
                for (int i = 0; i <= byte.MaxValue; i++)
                {
                    var byteQubes = byteQubeIndexer[(byte) i];
                    Qube<byte> qubeModule = default;
                    for (int j = 0; j < 8; j++)
                    {
                        var qubeByte = byteQubes[j];
                        var orientedByte = byteOrientation[qubeByte];
                        if (!IsValidByte(byteQubes[j], status))
                        {
                            moduleIndexes[qubeByte]= ModuleByteData.Invalid;
                            continue;
                        }

                        var moduleIndex = validModule.FindIndex(p => p == orientedByte._byte);
                        if (moduleIndex == -1)
                        {
                            moduleIndex = validModule.Count;
                            validModule.Add(qubeByte);
                        }
                    
                        qubeModule[j]=qubeByte;
                        moduleIndexes[qubeByte] = new ModuleByteData(){moduleByte=orientedByte._byte ,moduleIndex=moduleIndex,orientation = orientedByte._orientation};
                    }
                }
                typedModuleIndexes.Add(status,moduleIndexes);
            }
        }
        
        public static IEnumerable<byte> IterateAllValidBytes(ECornerStatus _status)
        {
            List<int> _iteratedIndexes = new List<int>();
            foreach (var moduleIndexer in typedModuleIndexes[_status])
            {
                if(!moduleIndexer.Valid)
                    continue;
                if (_iteratedIndexes.Contains(moduleIndexer.moduleIndex))
                    continue;
                _iteratedIndexes.Add(moduleIndexer.moduleIndex);
                yield return moduleIndexer.moduleByte;
            }
        }
        public static byte GetCornerBytes(this Qube<EModuleType> _typeQube,int _qubeIndex)
        {
            Qube<bool> qube = default;
            var compare = _typeQube[_qubeIndex];
            for (int i = 0; i < 8; i++)
                qube[i]=_typeQube[i]==compare;
            return byteQubeIndexer[qube.ToByte()][_qubeIndex] ;
        }

        public static void GetOrientedIndex(this ModuleRuntimeData _data, byte _qubeByte, ECornerStatus _status,out int _moduleIndex,out int _orientation)
        {
            _moduleIndex = -1;
            _orientation = 0;
            var moduleIndexes = typedModuleIndexes[_status];
            if (!_data.m_AvailableStatus.IsFlagEnable(_status))
                moduleIndexes = typedModuleIndexes[ECornerStatus.Body];
            
            var moduleIndexer=moduleIndexes[_qubeByte];
            if (moduleIndexer.moduleIndex == -1)
                return;
            
            _moduleIndex = moduleIndexer.moduleIndex;
            _orientation = moduleIndexer.orientation;
        }
    }
}