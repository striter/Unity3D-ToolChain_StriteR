using Geometry.Explicit.Mesh;
using Procedural;
using Procedural.Tile;

namespace Geometry.Explicit
{
    public static class USphereExplicit
    {
        public static int GetCubeSphereQuadCount(int resolution)
        {
            return resolution * resolution * KGeometryMesh.kCubeFacingAxisCount;
        }
        
        public static int GetCubeSphereVertexCount(int resolution)
        {
            return  (resolution + 1) * (resolution + 1) +
                (resolution + 1) * resolution +
                resolution * resolution +
                resolution * resolution +
                (resolution - 1) * (resolution) +
                (resolution - 1) * (resolution - 1);
        }

        public static int GetCubeSphereIndex(int _i,int _j,int _resolution,int _sideIndex)
        {
            bool firstColumn = _j == 0;
            bool lastColumn = _j == _resolution;
            bool firstRow = _i == 0;
            bool lastRow = _i == _resolution;
            int index = -1;
            if (_sideIndex == 0)
            {
                index = new Int2(_i, _j).ToIndex(_resolution + 1);
            }
            else if (_sideIndex == 1)
            {
                if (firstColumn)
                    index = GetCubeSphereIndex(_j, _i,_resolution, 0);
                else
                    index = (_resolution + 1) * (_resolution + 1) + new Int2(_i, _j - 1).ToIndex(_resolution + 1);
            }
            else if (_sideIndex == 2)
            {
                if (firstRow)
                    index = GetCubeSphereIndex(_j, _i,_resolution, 0);
                else if (firstColumn)
                    index = GetCubeSphereIndex(_j, _i,_resolution, 1);
                else
                    index = (_resolution + 1) * (_resolution + 1) + (_resolution + 1) * _resolution +
                            new Int2(_i - 1, _j - 1).ToIndex(_resolution);
            }
            else if (_sideIndex == 3)
            {
                if (firstColumn)
                    index = GetCubeSphereIndex(_i, _resolution,_resolution, 1);
                else if (firstRow)
                    index = GetCubeSphereIndex(_resolution, _j,_resolution, 2);
                else
                    index = (_resolution + 1) * (_resolution + 1) + (_resolution + 1) * _resolution +
                            _resolution * _resolution + new Int2(_i - 1, _j - 1).ToIndex(_resolution);
            }
            else if (_sideIndex == 4)
            {
                if (firstColumn)
                    index = GetCubeSphereIndex(_i, _resolution,_resolution, 2);
                else if (firstRow)
                    index = GetCubeSphereIndex(_resolution, _j,_resolution, 0);
                else if (lastRow)
                    index = GetCubeSphereIndex(_j, _resolution,_resolution, 3);
                else
                    index = (_resolution + 1) * (_resolution + 1) + (_resolution + 1) * _resolution +
                            _resolution * _resolution + (_resolution * _resolution) +
                            new Int2(_i - 1, _j - 1).ToIndex(_resolution - 1);
            }
            else if (_sideIndex == 5)
            {
                if (firstColumn)
                    index = GetCubeSphereIndex(_i, _resolution,_resolution, 0);
                else if (lastColumn)
                    index = GetCubeSphereIndex(_resolution, _i,_resolution, 3);
                else if (firstRow)
                    index = GetCubeSphereIndex(_resolution, _j,_resolution, 1);
                else if (lastRow)
                    index = GetCubeSphereIndex(_j, _resolution,_resolution, 4);
                else
                    index = (_resolution + 1) * (_resolution + 1) + (_resolution + 1) * _resolution +
                            _resolution * _resolution + (_resolution * _resolution) + (_resolution - 1) * (_resolution) +
                            new Int2(_i - 1, _j - 1).ToIndex(_resolution - 1);
            }

            return index;
        }
    }
}