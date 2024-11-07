using Unity.Mathematics;
using UnityEngine;

namespace TheVoxel
{
    public enum EBiome
    {
        Ocean = 0,
        Plane = 1,
        Mountains = 2,
        Max,
    }
    
    public enum EVoxelType
    {
        Void = -2,
        Air = -1,
        Dirt,
        Grass,
        Stone,
        
        Snow,
        BedRock,
        
        GeometryEnd = -1,
        
        Ocean,
    }
    
    public static class DVoxel
    {
        public const float kVoxelSize = 2f;

        public const int kChunkSize = 32;
        public const int kChunkSizeM1 = kChunkSize - 1;

        public static Int2 GetChunkID(Vector3 _positionWS) => new Int2((int)math.floor(_positionWS.x/(kVoxelSize*kChunkSize)),(int)math.floor(_positionWS.z/(kVoxelSize*kChunkSize)));
        public static Vector3 GetChunkPositionWS(Int2 _chunkID) => new Vector3(_chunkID.x*kChunkSize*kVoxelSize,0,_chunkID.y*kChunkSize*kVoxelSize);
        public static Vector3 GetVoxelPositionOS(Int3 _voxelID) => new Vector3(_voxelID.x*kVoxelSize,_voxelID.y*kVoxelSize,_voxelID.z*kVoxelSize);

        public static float4 GetVoxelBaseColor(EVoxelType _type)
        {
            switch (_type)
            {
                case EVoxelType.Dirt: return new float4(0.2745098f, 0.2350964f, 0.1764706f, 1f);
                case EVoxelType.Grass: return new float4(0.2741982f, 0.5377358f, 0.2671769f, 1f);
                case EVoxelType.Stone: return new float4(0.612775f, 0.6320754f, 0.6121988f, 1f);
                case EVoxelType.Snow: return new float4(.9f, .9f, .9f, 1f);
                case EVoxelType.BedRock: return new float4(.1f, .1f, .1f, 1f);
            }

            return default;
        }
    }
}