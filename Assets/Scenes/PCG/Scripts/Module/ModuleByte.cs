using System.Collections.Generic;
using Geometry;
using TPoolStatic;

namespace PCG.Module
{
    public struct OrientedModuleIndexer
    {
        public byte srcByte;
        public int index;
        public int orientation;
        public static readonly OrientedModuleIndexer Invalid = new OrientedModuleIndexer()
            { srcByte = 0, index = -1, orientation = -1 };
    }
    
    public static class UModuleByte
    {
        public static readonly Qube<byte>[] kByteQubeIndexer= new Qube<byte>[byte.MaxValue+1];
        public static readonly Qube<byte>[] kByteQubeIndexerFilled= new Qube<byte>[byte.MaxValue+1];
        public static readonly (byte _byte,int _orientation)[] kByteOrientation = new (byte _byte,int _orientation)[byte.MaxValue+1];      //256
        public static readonly (byte _byte,int _orientation)[] kQuadOrientation = new (byte _byte,int _orientation)[1 << 4];      //16

        static UModuleByte()
        {
            {   //Create Byte Qube Indexer
                for (int i = 0; i <= byte.MaxValue; i++)
                {
                    var posByte = (byte) i;
                    Qube<bool> corner = default;
                    corner.SetByteElement(posByte);
                    kByteQubeIndexer[i] = corner.SplitByteQubes(false);
                    kByteQubeIndexerFilled[i] = corner.SplitByteQubes(true);
                }
            }
            
            {   //Orientated Qube Byte Indexer
                TSPoolList<Qube<bool>>.Spawn(out var existQubes);
                for (int i = 0; i <= byte.MaxValue; i++)
                {
                    var srcByte = (byte) i;
                    var qube = new Qube<bool>();
                    qube.SetByteElement(srcByte);

                    byte orientation = 0; 
                    for (byte j = 1; j <= 3; j++)
                    {
                        var rotatedQube = qube.RotateYawCW(j);
                        int existIndex = existQubes.FindIndex(p => p.Equals( rotatedQube));
                        if (existIndex==-1)
                            continue;

                        srcByte = rotatedQube.ToByte();     //Inverse Rotation Matches
                        orientation = (byte)(4-j);
                    }
            
                    if(orientation==0)
                        existQubes.Add(qube);
                    kByteOrientation[i]=(srcByte,orientation);
                }
                TSPoolList<Qube<bool>>.Recycle(existQubes);
            }
            
            {   //Oriented Quad Indexer
                TSPoolList<Quad<bool>>.Spawn(out var existQuads);
                for (byte i = 0; i < 1 << 4; i++)
                {
                    var srcByte = i;
                    var quad = KQuad.kFalse;
                    quad.SetByteElement(i);

                    byte orientation = 0;
                    for (byte j = 1; j <= 3; j++)
                    {
                        var rotatedQuad = quad.RotateYawCW(j);
                        var existIndex = existQuads.FindIndex(p => p.Equals(rotatedQuad));
                        if (existIndex == -1)
                            continue;

                        srcByte = rotatedQuad.ToByte();
                        orientation =  (byte)(4-j);
                    }
                    if(orientation==0)
                        existQuads.Add(quad);
                    kQuadOrientation[i]=(srcByte,orientation);
                }
                TSPoolList<Quad<bool>>.Recycle(existQuads);
            }
        }
        
        public static IEnumerable<byte> IterateClusterBytes(OrientedModuleIndexer[] _indexers)
        {
            TSPoolList<int>.Spawn(out var iteratedIndexes);
            foreach (var moduleIndexer in _indexers)
            {
                var index = moduleIndexer.index;
                if(index==-1)
                    continue;
                if (iteratedIndexes.Contains(index))
                    continue;
                iteratedIndexes.Add(index);
                yield return moduleIndexer.srcByte;
            }
            TSPoolList<int>.Recycle(iteratedIndexes);
        }
    }
}