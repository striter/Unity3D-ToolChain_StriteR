using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;

namespace Geometry.Extend
{
    public static class UGeometryExtend
    {
        public static ByteQube SplitByteQubes(this BoolQube _qube)
        {
            BoolQube[] splitQubes = new BoolQube[8];
            for (int i = 0; i < 8; i++)
            {
                splitQubes[i] = default;
                splitQubes[i].SetByteCorners(_qube[i]?byte.MaxValue:byte.MinValue);
            }

            foreach (var corner in UEnum.GetValues<EQubeCorner>())
            {
                if(_qube[corner])
                    continue;

                var diagonal = corner.DiagonalCorner();
                
                if (_qube[diagonal])
                    splitQubes[diagonal.CornerToIndex()].SetCorner(corner,false);

                foreach (var tuple in corner.NearbyValidQubeFacing())
                {
                    var qube = tuple._cornerQube;
                    var facing = tuple._facingDir;
                    if(!_qube[qube])
                        continue;
                    splitQubes[qube.CornerToIndex()].SetFacingCorners(facing.Opposite(),false);
                }
                
                foreach (var tuple in corner.NearbyValidCornerQube())
                {
                    var qube = tuple._qube;
                    if(!_qube[qube])
                        continue;
                    splitQubes[qube.CornerToIndex()].SetCorner(tuple._adjactileCorner1,false);
                    splitQubes[qube.CornerToIndex()].SetCorner(tuple._adjactileCorner2,false);
                }

            }

            ByteQube byteQube = default;
            byteQube.SetQubeCorners(splitQubes[0].ToByte(),splitQubes[1].ToByte(),splitQubes[2].ToByte(),splitQubes[3].ToByte(),
                splitQubes[4].ToByte(),splitQubes[5].ToByte(),splitQubes[6].ToByte(),splitQubes[7].ToByte());
            return byteQube;
        }
        
        public static byte ToByte(this IQube<bool> _qube)
        {
            return UByte.ToByte(_qube[0],_qube[1],_qube[2],_qube[3],
                _qube[4],_qube[5],_qube[6],_qube[7]);
        }

        public static void SetByteCorners(ref this BoolQube _qube, byte _byte)
        {
            _qube.SetQubeCorners(UByte.PosValid(_byte,0),UByte.PosValid(_byte,1),UByte.PosValid(_byte,2),UByte.PosValid(_byte,3),
                UByte.PosValid(_byte,4),UByte.PosValid(_byte,5),UByte.PosValid(_byte,6),UByte.PosValid(_byte,7));
        }

    }
}