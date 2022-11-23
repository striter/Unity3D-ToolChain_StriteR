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
        public int sideCount;
        public int cornerAO;

        public static readonly ChunkVoxel kInvalid = new ChunkVoxel() {type = EVoxelType.Air};

        public bool Equals(ChunkVoxel other)
        {
            return identity.Equals(other.identity) && type == other.type && sideGeometry == other.sideGeometry && sideCount == other.sideCount && other.cornerAO == cornerAO;
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkVoxel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(identity);
        }
    }
}