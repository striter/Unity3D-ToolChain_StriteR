using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace TheVoxel
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ChunkVoxel : IEquatable<ChunkVoxel>
    {
        public Int3 identity;
        public EVoxelType type;
        public byte sideGeometry;
        public byte cornerGeometry;
        public ushort intervalGeometry;
        
        public static readonly ChunkVoxel kInvalid = new ChunkVoxel() {type = EVoxelType.Air};


        public override bool Equals(object obj)
        {
            return obj is ChunkVoxel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return identity.GetHashCode();
        }

        public bool Equals(ChunkVoxel other)
        {
            return identity.Equals(other.identity) && type == other.type && sideGeometry == other.sideGeometry && cornerGeometry == other.cornerGeometry && intervalGeometry == other.intervalGeometry;
        }
    }
}