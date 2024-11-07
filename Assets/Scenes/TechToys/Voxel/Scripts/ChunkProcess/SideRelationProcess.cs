using Runtime.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace TheVoxel.ChunkProcess
{
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct RefreshRelationJob : IJobFor
        {
            [NativeDisableContainerSafetyRestriction] private NativeArray<int> sideCount;
            [NativeDisableContainerSafetyRestriction] private NativeHashMap<Int3,ChunkVoxel> values;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeHashMap<Int3,ChunkVoxel> backValues,leftValues,forwardValues,rightValues;
            
            public RefreshRelationJob(NativeArray<int> _sideCount,NativeHashMap<Int3,ChunkVoxel> _indexes,
                NativeHashMap<Int3,ChunkVoxel> _backVoxels,
                NativeHashMap<Int3,ChunkVoxel> _leftVoxels,
                NativeHashMap<Int3,ChunkVoxel> _forwardVoxels,
                NativeHashMap<Int3,ChunkVoxel> _rightVoxels)
            {
                sideCount = _sideCount;
                values = _indexes;
                backValues = _backVoxels;
                leftValues = _leftVoxels;
                forwardValues = _forwardVoxels;
                rightValues = _rightVoxels;
            }
            
            ChunkVoxel GetVoxel(Int3 _voxelID)
            {
                if(_voxelID.y < 0)
                    return ChunkVoxel.kVoid;
                
                var curVoxels = values;
                if (_voxelID.x < 0)
                {
                    curVoxels = leftValues;
                }

                if (_voxelID.x >= DVoxel.kChunkSize)
                {
                    curVoxels = rightValues;
                }

                if (_voxelID.z < 0)
                {
                    curVoxels = backValues;
                }

                if (_voxelID.z >= DVoxel.kChunkSize)
                {
                    curVoxels = forwardValues;
                }

                _voxelID.x = (_voxelID.x + DVoxel.kChunkSize) % DVoxel.kChunkSize;
                _voxelID.z = (_voxelID.z + DVoxel.kChunkSize) % DVoxel.kChunkSize;

                if(curVoxels.TryGetValue(_voxelID,out var voxel))
                    return voxel;
                return ChunkVoxel.kInvalid;
            }
            
            public void Execute(int _jobIndex)
            {
                var length = values.Count;
                var renderFaces = 0;
                var keys = values.GetKeyArray(Allocator.Temp);
                for(var keyIndex=0;keyIndex<length;keyIndex++)
                {
                    var identity = keys[keyIndex];
                    var srcVoxel = values[identity];
                    var faceToRender = 0;
                    var replaceVoxel = new ChunkVoxel() {identity = srcVoxel.identity, type = srcVoxel.type};
                    for (var i = 0; i < 6; i++)
                    {
                        var facing = UCube.IndexToFacing(i);
                        var facingVoxel = GetVoxel(identity + UCube.GetCubeOffset(facing));
                        var shouldRenderFace = facingVoxel.type == EVoxelType.Air;
                        if(shouldRenderFace)
                            faceToRender += 1;
                        else
                            replaceVoxel.sideGeometry += (byte)(1 << i);
                    }

                    if (faceToRender != 6)
                        renderFaces += faceToRender;

                    if (replaceVoxel.sideGeometry != ChunkVoxel.kEmptyGeometry && replaceVoxel.sideGeometry != ChunkVoxel.kFullGeometry)
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            var cornerVoxel =  GetVoxel(identity+KCube.kCornerIdentity[i]);
                            var cornerValid = cornerVoxel.type != EVoxelType.Air;
                            if (!cornerValid)
                                continue;
                            replaceVoxel.cornerGeometry += (byte) (1 << i);
                        }

                        for (var i = 0; i < 12; i++)
                        {
                            var intervalVoxel =  GetVoxel(identity + KCube.kIntervalIdentity[i]);
                            var interValid = intervalVoxel.type != EVoxelType.Air;
                            var intervalByte = (interValid ? 1 : 0) << i;
                            replaceVoxel.intervalGeometry += (ushort)intervalByte;
                        }

                    }
                    
                    values[identity] = replaceVoxel;
                }

                sideCount[0] = renderFaces;
                keys.Dispose();
            }
        }
        
}