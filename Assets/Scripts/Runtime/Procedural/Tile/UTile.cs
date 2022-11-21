using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Procedural.Tile
{
    public static partial class UTile
    {
        public static IEnumerable<Int2> GetCoordsInRadius(this Int2 _center,int _radius)
        {
            int sqrRadius = UMath.Pow2(_radius+1);
            for(int i=-_radius;i<=_radius;i++)
                for(int j=-_radius;j<=_radius;j++)
                {
                    var coord = new Int2(i, j);
                    if (!(UMath.Pow2(Mathf.Abs(coord.x)) + UMath.Pow2(Mathf.Abs(coord.y)) <= sqrRadius))
                        continue;
                    yield return _center + coord;
                }
        }

        public static Coord ToCoord(this Int2 _coord) => new Coord(){x=_coord.x,y=_coord.y};
        public static int ToIndex(this Int2 axis, int width) => axis.x + axis.y * width;
    }
    
    public static partial class UTile       //Deprecated
    {
        
        public static readonly (ETileDirection,Int2)[] kNearbyTiles = {
            (ETileDirection.Left, new Int2(-1, 0)),
            (ETileDirection.Right, new Int2(1, 0)),
            (ETileDirection.Forward, new Int2(0, 1)),
            (ETileDirection.Back ,new Int2(0, -1))};
    
        public static Int2[] GetNearbyTiles(this Int2 _tile) => kNearbyTiles.Select(p => p.Item2+_tile).ToArray();

        public static (ETileDirection, Int2)[] GetNearbyTilesDirection(this Int2 _tile) => kNearbyTiles.Select(p =>(p.Item1, p.Item2+_tile)).ToArray();

        
        public static bool InRange<T>(this Int2 axis, T[,] range) => axis.x >= 0 && axis.x < range.GetLength(0) && axis.y >= 0 && axis.y < range.GetLength(1);
        public static bool InRange<T>(this Int2 originSize, Int2 sizeAxis, T[,] range) => InRange<T>(originSize + sizeAxis, range);
        public static Int2 GetAxisByIndex(int index, int width) => new Int2(index % width, index / width);
        public static T Get<T>(this T[,] tileArray, Int2 _begin) where T : class => _begin.InRange(tileArray) ? tileArray[_begin.x, _begin.y] : null;
        public static bool Get<T>(this T[,] tileArray, Int2 _begin, Int2 _size, ref List<T> tileList) where T : class
        {
            tileList.Clear();
            for (int i = 0; i < _size.x; i++)
                for (int j = 0; j < _size.y; j++)
                {
                    if (!InRange(_begin + new Int2(i, j), tileArray))
                        return false;
                    tileList.Add(tileArray.Get(_begin + new Int2(i, j)));
                }
            return true;
        }

        public static List<Int2> GetAxisRange(int width, int height, Int2 start, Int2 end)
        {
            List<Int2> axisList = new List<Int2>();
            for (int i = start.x; i <= end.x; i++)
                for (int j = start.y; j <= end.y; j++)
                {
                    if (i < 0 || j < 0 || i >= width || j >= height)
                        continue;
                    axisList.Add(new Int2(i, j));
                }
            return axisList;
        }

        public static List<Int2> GetAxisRange(Int2 centerAxis, int radius)
        {
            List<Int2> axisList = new List<Int2>();
            int sqrRadius = radius * radius;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    if ((centerAxis - new Int2(i, j)).sqrMagnitude > sqrRadius)
                        continue;
                    axisList.Add(new Int2(i, j));
                }
            return axisList;
        }

        public static List<Int2> GetDirectionAxies(int width, int height, Int2 centerAxis, List<ETileDirection> directions)
        {
            List<Int2> axisList = new List<Int2>();
            foreach (ETileDirection direction in directions)
            {
                Int2 targetAxis = centerAxis.DirectionAxis(direction);
                if (targetAxis.x < 0 || targetAxis.y < 0 || targetAxis.x >= width || targetAxis.y >= height)
                    continue;
                axisList.Add(targetAxis);
            }
            return axisList;
        }

        public static Dictionary<ETileDirection, T> GetDirectionAxis<T>(int width, int height, Int2 centerAxis, List<ETileDirection> directions, Func<Int2, T> OnItemGet)
        {
            Dictionary<ETileDirection, T> axisList = new Dictionary<ETileDirection, T>();
            foreach (ETileDirection direction in directions)
            {
                Int2 targetAxis = centerAxis.DirectionAxis(direction);
                if (targetAxis.x < 0 || targetAxis.y < 0 || targetAxis.x >= width || targetAxis.y >= height)
                    continue;
                axisList.Add(direction, OnItemGet(targetAxis));
            }
            return axisList;
        }


        public static bool CheckIsEdge<T>(this T[,] tileArray, Int2 axis) where T : class, ITile => axis.x == 0 || axis.x == tileArray.GetLength(0) - 1 || axis.y == 0 || axis.y == tileArray.GetLength(1) - 1;

        public static Int2 GetDirectionedSize(Int2 size, ETileDirection direction) => (int)direction % 2 == 0 ? size : size.Inverse();
        public static Vector3 GetUnitScaleBySizeAxis(Int2 directionedSize, int tileSize) => new Vector3(directionedSize.x, 1, directionedSize.y) * tileSize;
        public static Vector3 GetLocalPosBySizeAxis(Int2 directionedSize) => new Vector3(directionedSize.x, 0, directionedSize.y);
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
        public static readonly Dictionary<ETileDirection, Int2> m_DirectionAxies = new Dictionary<ETileDirection, Int2>() {
            { ETileDirection.Forward,new Int2(0,1) }, { ETileDirection.Right, new Int2(1, 0) }, { ETileDirection.Back, new Int2(0, -1) }, { ETileDirection.Left, new Int2(-1, 0) },
            { ETileDirection.ForwardRight, new Int2(1, 1) }, { ETileDirection.BackRight, new Int2(1, -1) }, { ETileDirection.BackLeft, new Int2(-1, -1) }, { ETileDirection.TopLeft, new Int2(-1, 1) } };

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

        public static ETileDirection OffsetDirection(this Int2 sourceAxis, Int2 targetAxis)
        {
            Int2 offset = targetAxis - sourceAxis;
            if (offset.y == 0) return offset.x < 0 ? ETileDirection.Left : ETileDirection.Right;
            if (offset.x == 0) return offset.y > 0 ? ETileDirection.Forward : ETileDirection.Back;
            return ETileDirection.Invalid;
        }

        public static Int2 DirectionAxis(this Int2 sourceAxis, ETileDirection direction) => sourceAxis + m_DirectionAxies[direction];

        public static void PathFindForClosestApproach<T>(this T[,] tileArray, T t1, T t2, List<T> tilePathsAdd, Action<T> OnEachTilePath = null, Predicate<T> stopPredicate = null, Predicate<T> invalidPredicate = null) where T : class, ITile       //Temporary Solution, Not Required Yet
        {
            if (!t1.m_Axis.InRange(tileArray) || !t2.m_Axis.InRange(tileArray))
                Debug.LogError("Error Tile Not Included In Array");

            tilePathsAdd.Add(t1);
            Int2 _startTileID = t1.m_Axis;
            for (; ; )
            {
                Int2 _nextTileID = _startTileID;
                float minDistance = (_startTileID - t2.m_Axis).sqrMagnitude;
                float offsetDistance;
                Int2 _offsetTileID;
                Int2[] nearbyFourTiles = _startTileID.GetNearbyTiles();
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

        public static bool ArrayNearbyContains<T>(this T[,] tileArray, Int2 origin, Predicate<T> predicate) where T : class, ITile
        {
            Int2[] nearbyTiles = origin.GetNearbyTiles();
            for (int i = 0; i < nearbyTiles.Length; i++)
            {
                if (origin.InRange(tileArray) && !predicate(tileArray.Get(nearbyTiles[i])))
                    return false;
            }
            return true;
        }

        public static List<T> TileRandomFill<T>(this T[,] tileArray, System.Random seed, Int2 originAxis, Action<T> OnEachFill, Predicate<T> availableAxis, int fillCount) where T : class, ITile
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
                    Int2 axis = temp.m_Axis.DirectionAxis(randomDirection);
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