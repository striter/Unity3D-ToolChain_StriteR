using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public partial struct G2Voxels
    {
        public G2Box bounding;
        public int2 size;
        public bool[] voxels;

        public static G2Voxels kDefault = new() { bounding = G2Box.kDefault, size = 2, voxels = new bool[4] { true, false, true, false } };
        public static G2Voxels kEmpty = new (){bounding = G2Box.kZero,size = 0,voxels = Array.Empty<bool>()};
    }

    public partial struct G2Voxels : ISerializationCallbackReceiver
    {
        public static G2Voxels FromPixelsNormalized( int2 _srcSize, Color[] _pixels, Predicate<Color> _convert)
        {
            if (_srcSize.x * _srcSize.y != _pixels.Length)
            {
                Debug.LogError($"[{nameof(G2Voxels)}]: Invalid size {nameof(_srcSize)}:{_srcSize} != {nameof(_pixels)}.length:{_pixels.Length}");
                return kEmpty;
            }

            var minTileIndex = (int2)int.MaxValue;
            var maxTileIndex = (int2)int.MinValue;
            foreach (var (index,pixel) in _pixels.WithIndex())
            {
                var valid = _convert(pixel);
                if (!valid)
                    continue;
                var tileIndex = UCoordinates.Tile.ToTile(index,_srcSize.x);
                minTileIndex = math.min(minTileIndex, tileIndex);
                maxTileIndex = math.max(maxTileIndex, tileIndex);
            }

            maxTileIndex += 1;

            var newSize = maxTileIndex - minTileIndex;
            var voxels = new bool[newSize.x * newSize.y];
            for (var i = 0; i < newSize.x; i++)
            {
                for (var j = 0; j < newSize.y; j++)
                {
                    var newTile = new int2(i, j);
                    var srcTile = minTileIndex + newTile;
                    voxels[UCoordinates.Tile.ToIndex(newTile, newSize.x)] = _convert(_pixels[UCoordinates.Tile.ToIndex(srcTile,_srcSize.x)]);
                }
            }
            var normalizedMin = minTileIndex / (float2)_srcSize;
            var normalizedMax = maxTileIndex / (float2)_srcSize;

            return new G2Voxels {
                bounding = G2Box.Minmax(normalizedMin,normalizedMax),
                size = newSize,
                voxels = voxels,
            };
        }
        
        public bool Contains(float2 _position)
        {
            if (!bounding.Contains(_position))
                return false;

            var uv = bounding.GetUV(_position);
            var voxelBounds = GetBounding((int2)(uv * size), out var valid);
            return valid;
        }
        
        public G2Box GetBounding(int2 _index,out bool _valid)
        {
            if(_index.x < 0 || _index.x >= size.x || _index.y < 0 || _index.y >= size.y)
            {
                _valid = false;
                return G2Box.kDefault;
            }
            
            var cellSize = bounding.size / size;
            _valid = voxels[UCoordinates.Tile.ToIndex(_index,size.x)];
            return G2Box.Minmax(bounding.min + cellSize * _index, bounding.min + cellSize * (_index + 1)).Resize(.99f);
        }
        
        public void DrawGizmos()
        {
            bounding.DrawGizmos();
            for (var i = 0; i < size.x; i++)
            {
                for (var j = 0; j < size.y; j++)
                {
                    var bounds = GetBounding(new int2(i, j), out var valid);
                    Gizmos.color = valid ? Color.yellow.SetA(.5f) : Color.red;
                    bounds.Resize(.99f).DrawGizmos();
                }
            }
        }

        public void DrawGizmosXY()
        {
            bounding.DrawGizmosXY();
            for (var i = 0; i < size.x; i++)
            {
                for (var j = 0; j < size.y; j++)
                {
                    var bounds = GetBounding(new int2(i, j), out var valid);
                    Gizmos.color = valid ? Color.yellow.SetA(.5f) : Color.red.SetA(.1f);
                    bounds.Resize(.99f).DrawGizmosXY();
                }
            }
        }

        public IEnumerable<G2Box> GetVoxels(bool _validOnly = true)
        {
            for (var i = 0; i < size.x; i++)
            for (var j = 0; j < size.y; j++)
            {
                var bounding = GetBounding(new int2(i, j), out var valid);
                if (!_validOnly || valid)
                    yield return bounding;
            }
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            voxels = voxels.Resize(size.x * size.y);
        }
    }
}