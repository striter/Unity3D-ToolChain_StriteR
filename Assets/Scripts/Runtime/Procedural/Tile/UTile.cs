using System;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Tile
{
    public static partial class UTile
    {
        public static IEnumerable<TileCoord> GetCoordsInRadius(this TileCoord _center,int _radius)
        {
            int sqrRadius = UMath.Pow2(_radius+1);
            for(int i=-_radius;i<=_radius;i++)
                for(int j=-_radius;j<=_radius;j++)
                {
                    var coord = new TileCoord(i, j);
                    if (!(UMath.Pow2(Mathf.Abs(coord.x)) + UMath.Pow2(Mathf.Abs(coord.y)) <= sqrRadius))
                        continue;
                    yield return _center + coord;
                }
        }

        public static Coord ToCoord(this TileCoord _coord) => new Coord(){x=_coord.x,y=_coord.y};
    }
    
    public static partial class UTile       //Deprecated
    {
        static int AxisDimensionTransformation(int x, int y, int width) => x + y * width;

        public static bool InRange<T>(this TileCoord axis, T[,] range) => axis.x >= 0 && axis.x < range.GetLength(0) && axis.y >= 0 && axis.y < range.GetLength(1);
        public static bool InRange<T>(this TileCoord originSize, TileCoord sizeAxis, T[,] range) => InRange<T>(originSize + sizeAxis, range);
        public static int Get1DAxisIndex(TileCoord axis, int width) => AxisDimensionTransformation(axis.x, axis.y, width);
        public static TileCoord GetAxisByIndex(int index, int width) => new TileCoord(index % width, index / width);
        public static T Get<T>(this T[,] tileArray, TileCoord _begin) where T : class => _begin.InRange(tileArray) ? tileArray[_begin.x, _begin.y] : null;
        public static bool Get<T>(this T[,] tileArray, TileCoord _begin, TileCoord _size, ref List<T> tileList) where T : class
        {
            tileList.Clear();
            for (int i = 0; i < _size.x; i++)
                for (int j = 0; j < _size.y; j++)
                {
                    if (!InRange(_begin + new TileCoord(i, j), tileArray))
                        return false;
                    tileList.Add(tileArray.Get(_begin + new TileCoord(i, j)));
                }
            return true;
        }

        public static List<TileCoord> GetAxisRange(int width, int height, TileCoord start, TileCoord end)
        {
            List<TileCoord> axisList = new List<TileCoord>();
            for (int i = start.x; i <= end.x; i++)
                for (int j = start.y; j <= end.y; j++)
                {
                    if (i < 0 || j < 0 || i >= width || j >= height)
                        continue;
                    axisList.Add(new TileCoord(i, j));
                }
            return axisList;
        }

        public static List<TileCoord> GetAxisRange(int width, int height, TileCoord centerAxis, int radius)
        {
            List<TileCoord> axisList = new List<TileCoord>();
            int sqrRadius = radius * radius;
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if ((centerAxis - new TileCoord(i, j)).sqrMagnitude > sqrRadius)
                        continue;
                    axisList.Add(new TileCoord(i, j));
                }
            return axisList;
        }

        public static List<TileCoord> GetDirectionAxies(int width, int height, TileCoord centerAxis, List<ETileDirection> directions)
        {
            List<TileCoord> axisList = new List<TileCoord>();
            foreach (ETileDirection direction in directions)
            {
                TileCoord targetAxis = centerAxis.DirectionAxis(direction);
                if (targetAxis.x < 0 || targetAxis.y < 0 || targetAxis.x >= width || targetAxis.y >= height)
                    continue;
                axisList.Add(targetAxis);
            }
            return axisList;
        }

        public static Dictionary<ETileDirection, T> GetDirectionAxies<T>(int width, int height, TileCoord centerAxis, List<ETileDirection> directions, Func<TileCoord, T> OnItemGet)
        {
            Dictionary<ETileDirection, T> axisList = new Dictionary<ETileDirection, T>();
            foreach (ETileDirection direction in directions)
            {
                TileCoord targetAxis = centerAxis.DirectionAxis(direction);
                if (targetAxis.x < 0 || targetAxis.y < 0 || targetAxis.x >= width || targetAxis.y >= height)
                    continue;
                axisList.Add(direction, OnItemGet(targetAxis));
            }
            return axisList;
        }


        public static bool CheckIsEdge<T>(this T[,] tileArray, TileCoord axis) where T : class, ITile => axis.x == 0 || axis.x == tileArray.GetLength(0) - 1 || axis.y == 0 || axis.y == tileArray.GetLength(1) - 1;

        public static TileCoord GetDirectionedSize(TileCoord size, ETileDirection direction) => (int)direction % 2 == 0 ? size : size.Inverse();
        public static Vector3 GetUnitScaleBySizeAxis(TileCoord directionedSize, int tileSize) => new Vector3(directionedSize.x, 1, directionedSize.y) * tileSize;
        public static Vector3 GetLocalPosBySizeAxis(TileCoord directionedSize) => new Vector3(directionedSize.x, 0, directionedSize.y);
        public static Quaternion ToRotation(this ETileDirection direction) => Quaternion.Euler(0, (int)direction * 90, 0);
        public static ETileDirection Next(this ETileDirection direction)
        {
            direction++;
            if (direction > ETileDirection.Left)
                direction = ETileDirection.Forward;
            else if (direction > ETileDirection.TopLeft)
                direction = ETileDirection.ForwardRight;
            return direction;
        }

        public static ETileDirection EdgeNextCorner(this ETileDirection direction, bool clockWise)
        {
            if (!direction.IsEdge())
            {
                Debug.LogError("Invalid Directions Here!");
                return ETileDirection.Invalid;
            }
            switch (direction)
            {
                default:
                    Debug.LogError("Invalid Convertions Here!");
                    return ETileDirection.Invalid;
                case ETileDirection.Forward:
                    return clockWise ? ETileDirection.ForwardRight : ETileDirection.TopLeft;
                case ETileDirection.Right:
                    return clockWise ? ETileDirection.BackRight : ETileDirection.ForwardRight;
                case ETileDirection.Back:
                    return clockWise ? ETileDirection.BackLeft : ETileDirection.BackRight;
                case ETileDirection.Left:
                    return clockWise ? ETileDirection.TopLeft : ETileDirection.BackLeft;
            }
        }

        public static ETileDirection AngleNextEdge(this ETileDirection direction, bool clockWise)
        {
            if (!direction.IsAngle())
            {
                Debug.LogError("Invalid Directions Here!");
                return ETileDirection.Invalid;
            }
            switch (direction)
            {
                default:
                    Debug.LogError("Invalid Convertions Here!");
                    return ETileDirection.Invalid;
                case ETileDirection.ForwardRight:
                    return clockWise ? ETileDirection.Right : ETileDirection.Forward;
                case ETileDirection.BackRight:
                    return clockWise ? ETileDirection.Back : ETileDirection.Right;
                case ETileDirection.BackLeft:
                    return clockWise ? ETileDirection.Left : ETileDirection.Back;
                case ETileDirection.TopLeft:
                    return clockWise ? ETileDirection.Forward : ETileDirection.Left;
            }
        }

        public static bool IsEdge(this ETileDirection direction) => m_EdgeDirections.Contains(direction);
        public static bool IsAngle(this ETileDirection direction) => m_AngleDirections.Contains(direction);
        public static readonly List<ETileDirection> m_EdgeDirections = new List<ETileDirection>() { ETileDirection.Forward, ETileDirection.Right, ETileDirection.Back, ETileDirection.Left };
        public static readonly List<ETileDirection> m_AngleDirections = new List<ETileDirection>() { ETileDirection.ForwardRight, ETileDirection.BackRight, ETileDirection.BackLeft, ETileDirection.TopLeft };
        public static readonly List<ETileDirection> m_AllDirections = new List<ETileDirection>() { ETileDirection.Forward, ETileDirection.Right, ETileDirection.Back, ETileDirection.Left,
            ETileDirection.ForwardRight, ETileDirection.BackRight, ETileDirection.BackLeft, ETileDirection.TopLeft };
        public static readonly Dictionary<ETileDirection, TileCoord> m_DirectionAxies = new Dictionary<ETileDirection, TileCoord>() {
            { ETileDirection.Forward,new TileCoord(0,1) }, { ETileDirection.Right, new TileCoord(1, 0) }, { ETileDirection.Back, new TileCoord(0, -1) }, { ETileDirection.Left, new TileCoord(-1, 0) },
            { ETileDirection.ForwardRight, new TileCoord(1, 1) }, { ETileDirection.BackRight, new TileCoord(1, -1) }, { ETileDirection.BackLeft, new TileCoord(-1, -1) }, { ETileDirection.TopLeft, new TileCoord(-1, 1) } };

        public static ETileDirection Inverse(this ETileDirection direction)
        {
            switch (direction)
            {
                default:
                    Debug.LogError("Error Direction Here");
                    return ETileDirection.Invalid;
                case ETileDirection.Forward:
                    return ETileDirection.Back;
                case ETileDirection.Back:
                    return ETileDirection.Forward;
                case ETileDirection.Right:
                    return ETileDirection.Left;
                case ETileDirection.Left:
                    return ETileDirection.Right;
                case ETileDirection.ForwardRight:
                    return ETileDirection.BackLeft;
                case ETileDirection.BackLeft:
                    return ETileDirection.ForwardRight;
                case ETileDirection.TopLeft:
                    return ETileDirection.BackRight;
                case ETileDirection.BackRight:
                    return ETileDirection.TopLeft;
            }
        }

        public static ETileDirection OffsetDirection(this TileCoord sourceAxis, TileCoord targetAxis)
        {
            TileCoord offset = targetAxis - sourceAxis;
            if (offset.y == 0) return offset.x < 0 ? ETileDirection.Left : ETileDirection.Right;
            if (offset.x == 0) return offset.y > 0 ? ETileDirection.Forward : ETileDirection.Back;
            return ETileDirection.Invalid;
        }

        public static TileCoord DirectionAxis(this TileCoord sourceAxis, ETileDirection direction) => sourceAxis + m_DirectionAxies[direction];

        public static void PathFindForClosestApproach<T>(this T[,] tileArray, T t1, T t2, List<T> tilePathsAdd, Action<T> OnEachTilePath = null, Predicate<T> stopPredicate = null, Predicate<T> invalidPredicate = null) where T : class, ITile       //Temporary Solution, Not Required Yet
        {
            if (!t1.m_Axis.InRange(tileArray) || !t2.m_Axis.InRange(tileArray))
                Debug.LogError("Error Tile Not Included In Array");

            tilePathsAdd.Add(t1);
            TileCoord _startTileID = t1.m_Axis;
            for (; ; )
            {
                TileCoord _nextTileID = _startTileID;
                float minDistance = (_startTileID - t2.m_Axis).sqrMagnitude;
                float offsetDistance;
                TileCoord _offsetTileID;
                TileCoord[] nearbyFourTiles = _startTileID.GetNearbyTiles();
                for (int i = 0; i < nearbyFourTiles.Length; i++)
                {
                    _offsetTileID = nearbyFourTiles[i];
                    offsetDistance = (_offsetTileID - t2.m_Axis).sqrMagnitude;
                    if (_offsetTileID.InRange(tileArray) && offsetDistance < minDistance)
                    {
                        _nextTileID = _offsetTileID;
                        minDistance = offsetDistance;
                    }
                }

                if (_nextTileID == t2.m_Axis || (stopPredicate != null && stopPredicate(tileArray.Get(_nextTileID))))
                {
                    tilePathsAdd.Add(tileArray.Get(_nextTileID));
                    break;
                }

                if (invalidPredicate != null && invalidPredicate(tileArray.Get(_nextTileID)))
                {
                    tilePathsAdd.Clear();
                    break;
                }
                _startTileID = _nextTileID;
                T tilePath = tileArray.Get(_startTileID);
                OnEachTilePath?.Invoke(tilePath);
                tilePathsAdd.Add(tilePath);

                if (tilePathsAdd.Count > tileArray.Length)
                {
                    Debug.LogError("Error Path Found Failed");
                    break;
                }
            }
        }

        public static T TileEdgeRandom<T>(this T[,] tileArray, System.Random randomSeed = null, Predicate<T> predicate = null, List<ETileDirection> edgeOutcluded = null, int predicateTryCount = -1) where T : class, ITile        //Target Edges Random Tile
        {
            if (edgeOutcluded != null && edgeOutcluded.Count > 3)
                Debug.LogError("Can't Outclude All Edges!");

            if (predicateTryCount == -1) predicateTryCount = int.MaxValue;

            List<ETileDirection> edgesRandom = new List<ETileDirection>(m_EdgeDirections) { };
            if (edgeOutcluded != null) edgesRandom.RemoveAll(p => edgeOutcluded.Contains(p));

            int axisX = -1, axisY = -1;
            int tileWidth = tileArray.GetLength(0), tileHeight = tileArray.GetLength(1);
            T targetTile = null;
            for (int i = 0; i < predicateTryCount; i++)
            {
                ETileDirection randomDirection = edgesRandom.RandomItem(randomSeed);
                switch (randomDirection)
                {
                    case ETileDirection.Back:
                        axisX = randomSeed.Next(tileWidth - 1) + 1;
                        axisY = 0;
                        break;
                    case ETileDirection.Forward:
                        axisX = randomSeed.Next(tileWidth - 1);
                        axisY = tileHeight - 1;
                        break;
                    case ETileDirection.Left:
                        axisX = 0;
                        axisY = randomSeed.Next(tileHeight - 1);
                        break;
                    case ETileDirection.Right:
                        axisX = tileWidth - 1;
                        axisY = randomSeed.Next(tileHeight - 1) + 1;
                        break;
                }
                targetTile = tileArray[axisX, axisY];
                if (predicate == null || predicate(targetTile))
                {
                    if (edgeOutcluded != null) edgeOutcluded.Add(randomDirection);
                    break;
                }
            }
            return targetTile;
        }

        public static bool ArrayNearbyContains<T>(this T[,] tileArray, TileCoord origin, Predicate<T> predicate) where T : class, ITile
        {
            TileCoord[] nearbyTiles = origin.GetNearbyTiles();
            for (int i = 0; i < nearbyTiles.Length; i++)
            {
                if (origin.InRange(tileArray) && !predicate(tileArray.Get(nearbyTiles[i])))
                    return false;
            }
            return true;
        }

        public static List<T> TileRandomFill<T>(this T[,] tileArray, System.Random seed, TileCoord originAxis, Action<T> OnEachFill, Predicate<T> availableAxis, int fillCount) where T : class, ITile
        {
            List<T> targetList = new List<T>();
            T targetAdd = tileArray.Get(originAxis);
            OnEachFill(targetAdd);
            targetList.Add(targetAdd);
            for (int i = 0; i < fillCount; i++)
            {
                T temp = targetList[i];
                foreach (ETileDirection randomDirection in m_EdgeDirections.RandomLoop(seed))
                {
                    TileCoord axis = temp.m_Axis.DirectionAxis(randomDirection);
                    if (axis.InRange(tileArray))
                    {
                        targetAdd = tileArray.Get(axis);
                        if (availableAxis(targetAdd))
                        {
                            OnEachFill(targetAdd);
                            targetList.Add(targetAdd);
                            break;
                        }
                    }
                }
            }
            return targetList;
        }
    }
}