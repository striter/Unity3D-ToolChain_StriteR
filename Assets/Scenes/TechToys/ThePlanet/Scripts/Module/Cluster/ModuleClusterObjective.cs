using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry;
using MeshFragment;
using UnityEngine;

namespace TechToys.ThePlanet.Module.Cluster
{
    [Serializable]
    public struct ModuleClusterInputData:IEquatable<ModuleClusterInputData>,IEqualityComparer<ModuleClusterInputData>
    {
        public bool valid;
        public int type;
        public byte anchorByte;
        public byte relationByte;
        public EClusterStatus status;
        public ICorner corner;

        public static readonly ModuleClusterInputData kInvalid = new ModuleClusterInputData()
            {valid = false, type = -1, anchorByte = byte.MinValue,relationByte = byte.MinValue};

        public override string ToString() => $"{type},{anchorByte},{status}";
        
#region Implements
        public bool Equals(ModuleClusterInputData other)
        {
            return valid == other.valid &&
                   type == other.type &&
                   anchorByte == other.anchorByte &&
                   relationByte == other.relationByte &&
                   status == other.status && 
                   Equals(corner, other.corner);
        }

        public bool Equals(ModuleClusterInputData x, ModuleClusterInputData y)
        {
            return x.valid == y.valid &&
                   x.type == y.type &&
                   x.anchorByte == y.anchorByte &&
                   x.relationByte == y.relationByte &&
                   x.status == y.status &&
                   Equals(x.corner, y.corner);
        }

        public int GetHashCode(ModuleClusterInputData obj)
        {
            unchecked
            {
                var hashCode = obj.valid.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.type;
                hashCode = (hashCode * 397) ^ obj.anchorByte.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.relationByte.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) obj.status;
                hashCode = (hashCode * 397) ^ (obj.corner != null ? obj.corner.GetHashCode() : 0);
                return hashCode;
            }
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = valid.GetHashCode();
                hashCode = (hashCode * 397) ^ type;
                hashCode = (hashCode * 397) ^ anchorByte.GetHashCode();
                hashCode = (hashCode * 397) ^ relationByte.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) status;
                hashCode = (hashCode * 397) ^ (corner != null ? corner.GetHashCode() : 0);
                return hashCode;
            }
        }
#endregion
    }

    [Serializable]
    public struct ModuleClusterCornerData:IEquatable<ModuleClusterCornerData>,IEqualityComparer<ModuleClusterCornerData>
    {
        public int index;
        public int orientation;
        public int possibility;
        public static readonly ModuleClusterCornerData kInvalid = new ModuleClusterCornerData(){
            index=-1,
            orientation = -1,
            possibility = -1,
        };

#region Implements
        public bool Equals(ModuleClusterCornerData other)
        {
            return index == other.index && orientation == other.orientation && possibility == other.possibility;
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleClusterCornerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = index;
                hashCode = (hashCode * 397) ^ orientation;
                hashCode = (hashCode * 397) ^ possibility;
                return hashCode;
            }
        }

        public bool Equals(ModuleClusterCornerData x, ModuleClusterCornerData y)
        {
            return x.index == y.index && x.orientation == y.orientation && x.possibility == y.possibility;
        }

        public int GetHashCode(ModuleClusterCornerData obj)
        {
            unchecked
            {
                var hashCode = obj.index;
                hashCode = (hashCode * 397) ^ obj.orientation;
                hashCode = (hashCode * 397) ^ obj.possibility;
                return hashCode;
            }
        }
#endregion
    }
}