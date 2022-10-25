using System.Collections.Generic;
using Geometry;
using TPoolStatic;

namespace PCG.Module.Prop
{
    using static PCGDefines<int>;
    public static class UModuleProp
    {
        private static readonly List<PCGID> kIdentityHelper = new List<PCGID>();

        public static List<PCGID> GetAdjacentCornersInRange(PCGID _origin, int _radius, Dictionary<PCGID, ICorner> _corners)
        {
            kIdentityHelper.Clear();
            TSPoolList<PCGID>.Spawn(out var previous);
            TSPoolHashset<PCGID>.Spawn(out var current);
            TSPoolHashset<PCGID>.Spawn(out var yielded);
            current.Add(_origin);
            for (int i = 0; i < _radius; i++)
            {
                previous.Clear();
                foreach (var ranger in current)
                {
                    foreach (var yield in _corners[ranger].m_AdjacentConnectedCorners)
                    {
                        if(yielded.Contains(yield))
                            continue;
                        yielded.Add(yield);
                        previous.Add(yield);
                        kIdentityHelper.Add(yield);
                    }
                }
                current.Clear();
                var previousCount = previous.Count;
                for(int j=0;j<previousCount;j++)
                    current.Add(previous[j]);
            }
            TSPoolList<PCGID>.Recycle(previous);
            TSPoolHashset<PCGID>.Recycle(current);
            TSPoolHashset<PCGID>.Recycle(yielded);
            return kIdentityHelper;
        } 
        
        public static List<PCGID> GetAdjacentVoxelInRange(PCGID _origin, int _radius, Dictionary<PCGID, ModulePropContainer> _voxels)
        {
            kIdentityHelper.Clear();
            TSPoolList<PCGID>.Spawn(out var previous);
            TSPoolHashset<PCGID>.Spawn(out var current);
            TSPoolHashset<PCGID>.Spawn(out var yielded);
            current.Add(_origin);
            for (int i = 0; i < _radius; i++)
            {
                previous.Clear();
                foreach (var ranger in current)
                {
                    var voxel = _voxels[ranger].m_Voxel;
                    foreach (var facing in UEnum.GetEnums<ECubeFacing>())
                    {
                        if(facing>ECubeFacing.RB)
                            continue;
                        
                        if((voxel.m_CubeSidesExists & facing) != facing)
                            continue;
                        var yield = voxel.m_CubeSides[facing];
                        
                        if(yielded.Contains(yield))
                            continue;
                        yielded.Add(yield);
                        previous.Add(yield);
                        kIdentityHelper.Add(yield);
                    }
                }
                current.Clear();
                
                var previousCount = previous.Count;
                for(int j=0;j<previousCount;j++)
                    current.Add(previous[j]);
            }
            TSPoolList<PCGID>.Recycle(previous);
            TSPoolHashset<PCGID>.Recycle(current);
            TSPoolHashset<PCGID>.Recycle(yielded);
            return kIdentityHelper;
        }
    }
}