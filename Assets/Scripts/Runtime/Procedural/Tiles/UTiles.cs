using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Procedural
{
    namespace Tile
    {
        [Flags]
        public enum ETileDirection
        {
            Invalid = 0,
            Forward = 1,
            Right = 2,
            Back = 4,
            Left = 8,

            ForwardRight = 64,
            BackRight = 128,
            BackLeft = 256,
            TopLeft = 512,
        }
        public interface ITile
        {
            TileID m_Axis { get; }
        }
        [Serializable]
        public struct TileID
        {
            public int x;
            public int y;
            public TileID(int _axisX, int _axisY)
            {
                x = _axisX;
                y = _axisY;
            }

            public static (ETileDirection,TileID)[] m_NearbyTiles = {(ETileDirection.Left, new TileID(-1, 0)),(ETileDirection.Right, new TileID(1, 0)),
                (ETileDirection.Forward, new TileID(0, 1)),(ETileDirection.Back ,new TileID(0, -1))};
        
            public TileID[] GetNearbyTiles()
            {
                var tile = this; 
                return m_NearbyTiles.Select(p => p.Item2+tile).ToArray();
            }

            public (ETileDirection, TileID)[] GetNearbyTilesDirection()
            {
                var tile = this; 
                return m_NearbyTiles.Select(p =>(p.Item1, p.Item2+tile)).ToArray();
            }

            public static TileID operator -(TileID a) => new TileID(-a.x, -a.y);
            public static bool operator ==(TileID a, TileID b) => a.x == b.x && a.y == b.y;
            public static bool operator !=(TileID a, TileID b) => a.x != b.x || a.y != b.y;
            public static TileID operator -(TileID a, TileID b) => new TileID(a.x - b.x, a.y - b.y);
            public static TileID operator +(TileID a, TileID b) => new TileID(a.x + b.x, a.y + b.y);
            public static TileID operator *(TileID a, TileID b) => new TileID(a.x * b.x, a.y * b.y);
            public static TileID operator /(TileID a, TileID b) => new TileID(a.x / b.x, a.y / b.y);

            public static TileID operator *(TileID a, int b) => new TileID(a.x * b, a.y * b);
            public static TileID operator /(TileID a, int b) => new TileID(a.x / b, a.y / b);
            public TileID Inverse() => new TileID(y, x);
            public override bool Equals(object obj) => base.Equals(obj);
            public override int GetHashCode() => base.GetHashCode();
            public override string ToString() => x + "," + y;
            public int SqrMagnitude => x * x + y * y;
            public static readonly TileID Zero = new TileID(0, 0);
            public static readonly TileID One = new TileID(1, 1);
            public static readonly TileID NegativeOne = new TileID(-1, -1);
            public static readonly TileID Back = new TileID(0, -1);
            public static readonly TileID Right = new TileID(1, 0);
            public static readonly TileID Forward = new TileID(0, 1);
        }
        public struct TileBounds
        {
            public TileID m_Origin { get; private set; }
            public TileID m_Size { get; private set; }
            public TileID m_End { get; private set; }
            public bool Contains(TileID axis) => axis.x >= m_Origin.x && axis.x <= m_End.x && axis.y >= m_Origin.y && axis.y <= m_End.y;
            public override string ToString() => m_Origin.ToString() + "|" + m_Size.ToString();
            public bool Intersects(TileBounds targetBounds)
            {
                TileID[] sourceAxies = new TileID[] { m_Origin, m_End, m_Origin + new TileID(m_Size.x, 0), m_Origin + new TileID(0, m_Size.y) };
                for (int i = 0; i < sourceAxies.Length; i++)
                    if (targetBounds.Contains(sourceAxies[i]))
                        return true;
                TileID[] targetAxies = new TileID[] { targetBounds.m_Origin, targetBounds.m_End, targetBounds.m_Origin + new TileID(targetBounds.m_Size.x, 0), targetBounds.m_Origin + new TileID(0, targetBounds.m_Size.y) };
                for (int i = 0; i < targetAxies.Length; i++)
                    if (Contains(targetAxies[i]))
                        return true;
                return false;
            }

            public TileBounds(TileID origin, TileID size)
            {
                m_Origin = origin;
                m_Size = size;
                m_End = m_Origin + m_Size;
            }
        }
        public static class UTile
        {
            static int AxisDimensionTransformation(int x, int y, int width) => x + y * width;
            public static bool InRange<T>(this TileID axis, T[,] range) => axis.x >= 0 && axis.x < range.GetLength(0) && axis.y >= 0 && axis.y < range.GetLength(1);
            public static bool InRange<T>(this TileID originSize, TileID sizeAxis, T[,] range) => InRange<T>(originSize + sizeAxis, range);
            public static int Get1DAxisIndex(TileID axis, int width) => AxisDimensionTransformation(axis.x, axis.y, width);
            public static TileID GetAxisByIndex(int index, int width) => new TileID(index % width, index / width);
            public static T Get<T>(this T[,] tileArray, TileID _begin) where T : class => _begin.InRange(tileArray) ? tileArray[_begin.x, _begin.y] : null;
            public static bool Get<T>(this T[,] tileArray, TileID _begin, TileID _size, ref List<T> tileList) where T : class
            {
                tileList.Clear();
                for (int i = 0; i < _size.x; i++)
                    for (int j = 0; j < _size.y; j++)
                    {
                        if (!InRange(_begin + new TileID(i, j), tileArray))
                            return false;
                        tileList.Add(tileArray.Get(_begin + new TileID(i, j)));
                    }
                return true;
            }

            public static List<TileID> GetAxisRange(int width, int height, TileID start, TileID end)
            {
                List<TileID> axisList = new List<TileID>();
                for (int i = start.x; i <= end.x; i++)
                    for (int j = start.y; j <= end.y; j++)
                    {
                        if (i < 0 || j < 0 || i >= width || j >= height)
                            continue;
                        axisList.Add(new TileID(i, j));
                    }
                return axisList;
            }

            public static List<TileID> GetAxisRange(int width, int height, TileID centerAxis, int radius)
            {
                List<TileID> axisList = new List<TileID>();
                int sqrRadius = radius * radius;
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        if ((centerAxis - new TileID(i, j)).SqrMagnitude > sqrRadius)
                            continue;
                        axisList.Add(new TileID(i, j));
                    }
                return axisList;
            }

            public static List<TileID> GetDirectionAxies(int width, int height, TileID centerAxis, List<ETileDirection> directions)
            {
                List<TileID> axisList = new List<TileID>();
                foreach (ETileDirection direction in directions)
                {
                    TileID targetAxis = centerAxis.DirectionAxis(direction);
                    if (targetAxis.x < 0 || targetAxis.y < 0 || targetAxis.x >= width || targetAxis.y >= height)
                        continue;
                    axisList.Add(targetAxis);
                }
                return axisList;
            }

            public static Dictionary<ETileDirection, T> GetDirectionAxies<T>(int width, int height, TileID centerAxis, List<ETileDirection> directions, Func<TileID, T> OnItemGet)
            {
                Dictionary<ETileDirection, T> axisList = new Dictionary<ETileDirection, T>();
                foreach (ETileDirection direction in directions)
                {
                    TileID targetAxis = centerAxis.DirectionAxis(direction);
                    if (targetAxis.x < 0 || targetAxis.y < 0 || targetAxis.x >= width || targetAxis.y >= height)
                        continue;
                    axisList.Add(direction, OnItemGet(targetAxis));
                }
                return axisList;
            }


            public static bool CheckIsEdge<T>(this T[,] tileArray, TileID axis) where T : class, ITile => axis.x == 0 || axis.x == tileArray.GetLength(0) - 1 || axis.y == 0 || axis.y == tileArray.GetLength(1) - 1;

            public static TileID GetDirectionedSize(TileID size, ETileDirection direction) => (int)direction % 2 == 0 ? size : size.Inverse();
            public static Vector3 GetUnitScaleBySizeAxis(TileID directionedSize, int tileSize) => new Vector3(directionedSize.x, 1, directionedSize.y) * tileSize;
            public static Vector3 GetLocalPosBySizeAxis(TileID directionedSize) => new Vector3(directionedSize.x, 0, directionedSize.y);
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

            public static ETileDirection EdgeNextCornor(this ETileDirection direction, bool clockWise)
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
            public static readonly Dictionary<ETileDirection, TileID> m_DirectionAxies = new Dictionary<ETileDirection, TileID>() {
                { ETileDirection.Forward,new TileID(0,1) }, { ETileDirection.Right, new TileID(1, 0) }, { ETileDirection.Back, new TileID(0, -1) }, { ETileDirection.Left, new TileID(-1, 0) },
                { ETileDirection.ForwardRight, new TileID(1, 1) }, { ETileDirection.BackRight, new TileID(1, -1) }, { ETileDirection.BackLeft, new TileID(-1, -1) }, { ETileDirection.TopLeft, new TileID(-1, 1) } };

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

            public static ETileDirection OffsetDirection(this TileID sourceAxis, TileID targetAxis)
            {
                TileID offset = targetAxis - sourceAxis;
                if (offset.y == 0) return offset.x < 0 ? ETileDirection.Left : ETileDirection.Right;
                if (offset.x == 0) return offset.y > 0 ? ETileDirection.Forward : ETileDirection.Back;
                return ETileDirection.Invalid;
            }

            public static TileID DirectionAxis(this TileID sourceAxis, ETileDirection direction) => sourceAxis + m_DirectionAxies[direction];

            public static void PathFindForClosestApproch<T>(this T[,] tileArray, T t1, T t2, List<T> tilePathsAdd, Action<T> OnEachTilePath = null, Predicate<T> stopPredicate = null, Predicate<T> invalidPredicate = null) where T : class, ITile       //Temporary Solution, Not Required Yet
            {
                if (!t1.m_Axis.InRange(tileArray) || !t2.m_Axis.InRange(tileArray))
                    Debug.LogError("Error Tile Not Included In Array");


                tilePathsAdd.Add(t1);
                TileID _startTileID = t1.m_Axis;
                for (; ; )
                {
                    TileID _nextTileID = _startTileID;
                    float minDistance = (_startTileID - t2.m_Axis).SqrMagnitude;
                    float offsetDistance;
                    TileID _offsetTileID;
                    TileID[] nearbyFourTiles = _startTileID.GetNearbyTiles();
                    for (int i = 0; i < nearbyFourTiles.Length; i++)
                    {
                        _offsetTileID = nearbyFourTiles[i];
                        offsetDistance = (_offsetTileID - t2.m_Axis).SqrMagnitude;
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

            public static bool ArrayNearbyContains<T>(this T[,] tileArray, TileID origin, Predicate<T> predicate) where T : class, ITile
            {
                TileID[] nearbyTiles = origin.GetNearbyTiles();
                for (int i = 0; i < nearbyTiles.Length; i++)
                {
                    if (origin.InRange(tileArray) && !predicate(tileArray.Get(nearbyTiles[i])))
                        return false;
                }
                return true;
            }

            public static List<T> TileRandomFill<T>(this T[,] tileArray, System.Random seed, TileID originAxis, Action<T> OnEachFill, Predicate<T> availableAxis, int fillCount) where T : class, ITile
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
                        TileID axis = temp.m_Axis.DirectionAxis(randomDirection);
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

}