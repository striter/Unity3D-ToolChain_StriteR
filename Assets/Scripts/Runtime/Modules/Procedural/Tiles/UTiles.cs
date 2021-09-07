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

            Top=16,
            Bottom=32,
            
            ForwardRight = 64,
            BackRight = 128,
            BackLeft = 256,
            TopLeft = 512,
        }
        public interface ITileAxis
        {
            Tile m_Axis { get; }
        }
        [System.Serializable]
        public struct Tile
        {
            public int x;
            public int y;
            public Tile(int _axisX, int _axisY)
            {
                x = _axisX;
                y = _axisY;
            }

            public static (ETileDirection,Tile)[] m_NearbyTiles = {(ETileDirection.Left, new Tile(-1, 0)),(ETileDirection.Right, new Tile(1, 0)),
                (ETileDirection.Forward, new Tile(0, 1)),(ETileDirection.Back ,new Tile(0, -1))};
        
            public Tile[] GetNearbyTiles()
            {
                var tile = this; 
                return m_NearbyTiles.Select(p => p.Item2+tile).ToArray();
            }

            public (ETileDirection, Tile)[] GetNearbyTilesDirection()
            {
                var tile = this; 
                return m_NearbyTiles.Select(p =>(p.Item1, p.Item2+tile)).ToArray();
            }

            public static Tile operator -(Tile a) => new Tile(-a.x, -a.y);
            public static bool operator ==(Tile a, Tile b) => a.x == b.x && a.y == b.y;
            public static bool operator !=(Tile a, Tile b) => a.x != b.x || a.y != b.y;
            public static Tile operator -(Tile a, Tile b) => new Tile(a.x - b.x, a.y - b.y);
            public static Tile operator +(Tile a, Tile b) => new Tile(a.x + b.x, a.y + b.y);
            public static Tile operator *(Tile a, Tile b) => new Tile(a.x * b.x, a.y * b.y);
            public static Tile operator /(Tile a, Tile b) => new Tile(a.x / b.x, a.y / b.y);

            public static Tile operator *(Tile a, int b) => new Tile(a.x * b, a.y * b);
            public static Tile operator /(Tile a, int b) => new Tile(a.x / b, a.y / b);
            public Tile Inverse() => new Tile(y, x);
            public override bool Equals(object obj) => base.Equals(obj);
            public override int GetHashCode() => base.GetHashCode();
            public override string ToString() => x + "," + y;
            public int SqrMagnitude => x * x + y * y;
            public static readonly Tile Zero = new Tile(0, 0);
            public static readonly Tile One = new Tile(1, 1);
            public static readonly Tile NegativeOne = new Tile(-1, -1);
            public static readonly Tile Back = new Tile(0, -1);
            public static readonly Tile Right = new Tile(1, 0);
            public static readonly Tile Forward = new Tile(0, 1);
        }
        public struct TileBounds
        {
            public Tile m_Origin { get; private set; }
            public Tile m_Size { get; private set; }
            public Tile m_End { get; private set; }
            public bool Contains(Tile axis) => axis.x >= m_Origin.x && axis.x <= m_End.x && axis.y >= m_Origin.y && axis.y <= m_End.y;
            public override string ToString() => m_Origin.ToString() + "|" + m_Size.ToString();
            public bool Intersects(TileBounds targetBounds)
            {
                Tile[] sourceAxies = new Tile[] { m_Origin, m_End, m_Origin + new Tile(m_Size.x, 0), m_Origin + new Tile(0, m_Size.y) };
                for (int i = 0; i < sourceAxies.Length; i++)
                    if (targetBounds.Contains(sourceAxies[i]))
                        return true;
                Tile[] targetAxies = new Tile[] { targetBounds.m_Origin, targetBounds.m_End, targetBounds.m_Origin + new Tile(targetBounds.m_Size.x, 0), targetBounds.m_Origin + new Tile(0, targetBounds.m_Size.y) };
                for (int i = 0; i < targetAxies.Length; i++)
                    if (Contains(targetAxies[i]))
                        return true;
                return false;
            }

            public TileBounds(Tile origin, Tile size)
            {
                m_Origin = origin;
                m_Size = size;
                m_End = m_Origin + m_Size;
            }
        }
        public static class TileTools
        {
            static int AxisDimensionTransformation(int x, int y, int width) => x + y * width;
            public static bool InRange<T>(this Tile axis, T[,] range) => axis.x >= 0 && axis.x < range.GetLength(0) && axis.y >= 0 && axis.y < range.GetLength(1);
            public static bool InRange<T>(this Tile originSize, Tile sizeAxis, T[,] range) => InRange<T>(originSize + sizeAxis, range);
            public static int Get1DAxisIndex(Tile axis, int width) => AxisDimensionTransformation(axis.x, axis.y, width);
            public static Tile GetAxisByIndex(int index, int width) => new Tile(index % width, index / width);
            public static T Get<T>(this T[,] tileArray, Tile axis) where T : class => axis.InRange(tileArray) ? tileArray[axis.x, axis.y] : null;
            public static bool Get<T>(this T[,] tileArray, Tile axis, Tile size, ref List<T> tileList) where T : class
            {
                tileList.Clear();
                for (int i = 0; i < size.x; i++)
                    for (int j = 0; j < size.y; j++)
                    {
                        if (!InRange(axis + new Tile(i, j), tileArray))
                            return false;
                        tileList.Add(tileArray.Get(axis + new Tile(i, j)));
                    }
                return true;
            }

            public static List<Tile> GetAxisRange(int width, int height, Tile start, Tile end)
            {
                List<Tile> axisList = new List<Tile>();
                for (int i = start.x; i <= end.x; i++)
                    for (int j = start.y; j <= end.y; j++)
                    {
                        if (i < 0 || j < 0 || i >= width || j >= height)
                            continue;
                        axisList.Add(new Tile(i, j));
                    }
                return axisList;
            }

            public static List<Tile> GetAxisRange(int width, int height, Tile centerAxis, int radius)
            {
                List<Tile> axisList = new List<Tile>();
                int sqrRadius = radius * radius;
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        if ((centerAxis - new Tile(i, j)).SqrMagnitude > sqrRadius)
                            continue;
                        axisList.Add(new Tile(i, j));
                    }
                return axisList;
            }

            public static List<Tile> GetDirectionAxies(int width, int height, Tile centerAxis, List<ETileDirection> directions)
            {
                List<Tile> axisList = new List<Tile>();
                foreach (ETileDirection direction in directions)
                {
                    Tile targetAxis = centerAxis.DirectionAxis(direction);
                    if (targetAxis.x < 0 || targetAxis.y < 0 || targetAxis.x >= width || targetAxis.y >= height)
                        continue;
                    axisList.Add(targetAxis);
                }
                return axisList;
            }

            public static Dictionary<ETileDirection, T> GetDirectionAxies<T>(int width, int height, Tile centerAxis, List<ETileDirection> directions, Func<Tile, T> OnItemGet)
            {
                Dictionary<ETileDirection, T> axisList = new Dictionary<ETileDirection, T>();
                foreach (ETileDirection direction in directions)
                {
                    Tile targetAxis = centerAxis.DirectionAxis(direction);
                    if (targetAxis.x < 0 || targetAxis.y < 0 || targetAxis.x >= width || targetAxis.y >= height)
                        continue;
                    axisList.Add(direction, OnItemGet(targetAxis));
                }
                return axisList;
            }


            public static bool CheckIsEdge<T>(this T[,] tileArray, Tile axis) where T : class, ITileAxis => axis.x == 0 || axis.x == tileArray.GetLength(0) - 1 || axis.y == 0 || axis.y == tileArray.GetLength(1) - 1;

            public static Tile GetDirectionedSize(Tile size, ETileDirection direction) => (int)direction % 2 == 0 ? size : size.Inverse();
            public static Vector3 GetUnitScaleBySizeAxis(Tile directionedSize, int tileSize) => new Vector3(directionedSize.x, 1, directionedSize.y) * tileSize;
            public static Vector3 GetLocalPosBySizeAxis(Tile directionedSize) => new Vector3(directionedSize.x, 0, directionedSize.y);
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
            public static readonly Dictionary<ETileDirection, Tile> m_DirectionAxies = new Dictionary<ETileDirection, Tile>() {
                { ETileDirection.Forward,new Tile(0,1) }, { ETileDirection.Right, new Tile(1, 0) }, { ETileDirection.Back, new Tile(0, -1) }, { ETileDirection.Left, new Tile(-1, 0) },
                { ETileDirection.ForwardRight, new Tile(1, 1) }, { ETileDirection.BackRight, new Tile(1, -1) }, { ETileDirection.BackLeft, new Tile(-1, -1) }, { ETileDirection.TopLeft, new Tile(-1, 1) } };

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

            public static ETileDirection OffsetDirection(this Tile sourceAxis, Tile targetAxis)
            {
                Tile offset = targetAxis - sourceAxis;
                if (offset.y == 0) return offset.x < 0 ? ETileDirection.Left : ETileDirection.Right;
                if (offset.x == 0) return offset.y > 0 ? ETileDirection.Forward : ETileDirection.Back;
                return ETileDirection.Invalid;
            }

            public static Tile DirectionAxis(this Tile sourceAxis, ETileDirection direction) => sourceAxis + m_DirectionAxies[direction];

            public static void PathFindForClosestApproch<T>(this T[,] tileArray, T t1, T t2, List<T> tilePathsAdd, Action<T> OnEachTilePath = null, Predicate<T> stopPredicate = null, Predicate<T> invalidPredicate = null) where T : class, ITileAxis       //Temporary Solution, Not Required Yet
            {
                if (!t1.m_Axis.InRange(tileArray) || !t2.m_Axis.InRange(tileArray))
                    Debug.LogError("Error Tile Not Included In Array");


                tilePathsAdd.Add(t1);
                Tile startTile = t1.m_Axis;
                for (; ; )
                {
                    Tile nextTile = startTile;
                    float minDistance = (startTile - t2.m_Axis).SqrMagnitude;
                    float offsetDistance;
                    Tile offsetTile;
                    Tile[] nearbyFourTiles = startTile.GetNearbyTiles();
                    for (int i = 0; i < nearbyFourTiles.Length; i++)
                    {
                        offsetTile = nearbyFourTiles[i];
                        offsetDistance = (offsetTile - t2.m_Axis).SqrMagnitude;
                        if (offsetTile.InRange(tileArray) && offsetDistance < minDistance)
                        {
                            nextTile = offsetTile;
                            minDistance = offsetDistance;
                        }
                    }

                    if (nextTile == t2.m_Axis || (stopPredicate != null && stopPredicate(tileArray.Get(nextTile))))
                    {
                        tilePathsAdd.Add(tileArray.Get(nextTile));
                        break;
                    }

                    if (invalidPredicate != null && invalidPredicate(tileArray.Get(nextTile)))
                    {
                        tilePathsAdd.Clear();
                        break;
                    }
                    startTile = nextTile;
                    T tilePath = tileArray.Get(startTile);
                    OnEachTilePath?.Invoke(tilePath);
                    tilePathsAdd.Add(tilePath);

                    if (tilePathsAdd.Count > tileArray.Length)
                    {
                        Debug.LogError("Error Path Found Failed");
                        break;
                    }
                }
            }

            public static T TileEdgeRandom<T>(this T[,] tileArray, System.Random randomSeed = null, Predicate<T> predicate = null, List<ETileDirection> edgeOutcluded = null, int predicateTryCount = -1) where T : class, ITileAxis        //Target Edges Random Tile
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

            public static bool ArrayNearbyContains<T>(this T[,] tileArray, Tile origin, Predicate<T> predicate) where T : class, ITileAxis
            {
                Tile[] nearbyTiles = origin.GetNearbyTiles();
                for (int i = 0; i < nearbyTiles.Length; i++)
                {
                    if (origin.InRange(tileArray) && !predicate(tileArray.Get(nearbyTiles[i])))
                        return false;
                }
                return true;
            }

            public static List<T> TileRandomFill<T>(this T[,] tileArray, System.Random seed, Tile originAxis, Action<T> OnEachFill, Predicate<T> availableAxis, int fillCount) where T : class, ITileAxis
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
                        Tile axis = temp.m_Axis.DirectionAxis(randomDirection);
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