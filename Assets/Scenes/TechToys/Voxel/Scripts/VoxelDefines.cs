using UnityEngine;

namespace TheVoxel
{
    public enum EVoxelType
    {
        Air = -1,
        Dirt,
        Grass,
        Stone,
    }
    
    public static class DVoxel
    {
        public const float kVoxelSize = 2f;

        public const int kVisualizeRange = 1;
        
        public const int kChunkVoxelSize = 128;
        public const int kTerrainHeight = 80;

        public static Int2 GetChunkID(Vector3 _positionWS) => new Int2((int)(_positionWS.x/kVoxelSize*kChunkVoxelSize),(int)(_positionWS.z/kVoxelSize*kChunkVoxelSize));
        public static Vector3 GetChunkPositionWS(Int2 _chunkID) => new Vector3(_chunkID.x*kChunkVoxelSize*kVoxelSize,-kTerrainHeight*kVoxelSize,_chunkID.y*kChunkVoxelSize*kVoxelSize);
        public static Vector3 GetVoxelPositionOS(Int3 _voxelID) => new Vector3(_voxelID.x*kVoxelSize,_voxelID.y*kVoxelSize,_voxelID.z*kVoxelSize);
    }

    public static class UVoxel
    {
    }
    
}