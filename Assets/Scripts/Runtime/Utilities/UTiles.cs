using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UTile
{
    public enum ETileDirection
    {
        Invalid = -1,
        Top = 0,
        Right = 1,
        Bottom = 2,
        Left = 3,

        TopRight = 10,
        BottomRight = 11,
        BottomLeft = 12,
        TopLeft = 13,
    }
    public interface ITileAxis
    {
        TileAxis m_Axis { get; }
    }
    [System.Serializable]
    public struct TileAxis
    {
        public int X;
        public int Y;
        public TileAxis(int _axisX, int _axisY)
        {
            X = _axisX;
            Y = _axisY;
        }

        public TileAxis[] nearbyFourTiles => new TileAxis[4] { new TileAxis(X - 1, Y), new TileAxis(X + 1, Y), new TileAxis(X, Y + 1), new TileAxis(X, Y - 1) };
        public static TileAxis operator -(TileAxis a) => new TileAxis(-a.X, -a.Y);
        public static bool operator ==(TileAxis a, TileAxis b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(TileAxis a, TileAxis b) => a.X != b.X || a.Y != b.Y;
        public static TileAxis operator -(TileAxis a, TileAxis b) => new TileAxis(a.X - b.X, a.Y - b.Y);
        public static TileAxis operator +(TileAxis a, TileAxis b) => new TileAxis(a.X + b.X, a.Y + b.Y);
        public static TileAxis operator *(TileAxis a, TileAxis b) => new TileAxis(a.X * b.X, a.Y * b.Y);
        public static TileAxis operator /(TileAxis a, TileAxis b) => new TileAxis(a.X / b.X, a.Y / b.Y);

        public static TileAxis operator *(TileAxis a, int b) => new TileAxis(a.X * b, a.Y * b);
        public static TileAxis operator /(TileAxis a, int b) => new TileAxis(a.X / b, a.Y / b);
        public TileAxis Inverse() => new TileAxis(Y, X);
        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => X + "," + Y;
        public int SqrMagnitude => X * X + Y * Y;
        public static readonly TileAxis Zero = new TileAxis(0, 0);
        public static readonly TileAxis One = new TileAxis(1, 1);
        public static readonly TileAxis NegativeOne = new TileAxis(-1, -1);
        public static readonly TileAxis Back = new TileAxis(0, -1);
        public static readonly TileAxis Right = new TileAxis(1, 0);
        public static readonly TileAxis Forward = new TileAxis(0, 1);
    }
    public struct TileBounds
    {
        public TileAxis m_Origin { get; private set; }
        public TileAxis m_Size { get; private set; }
        public TileAxis m_End { get; private set; }
        public bool Contains(TileAxis axis) => axis.X >= m_Origin.X && axis.X <= m_End.X && axis.Y >= m_Origin.Y && axis.Y <= m_End.Y;
        public override string ToString() => m_Origin.ToString() + "|" + m_Size.ToString();
        public bool Intersects(TileBounds targetBounds)
        {
            TileAxis[] sourceAxies = new TileAxis[] { m_Origin, m_End, m_Origin + new TileAxis(m_Size.X, 0), m_Origin + new TileAxis(0, m_Size.Y) };
            for (int i = 0; i < sourceAxies.Length; i++)
                if (targetBounds.Contains(sourceAxies[i]))
                    return true;
            TileAxis[] targetAxies = new TileAxis[] { targetBounds.m_Origin, targetBounds.m_End, targetBounds.m_Origin + new TileAxis(targetBounds.m_Size.X, 0), targetBounds.m_Origin + new TileAxis(0, targetBounds.m_Size.Y) };
            for (int i = 0; i < targetAxies.Length; i++)
                if (Contains(targetAxies[i]))
                    return true;
            return false;
        }

        public TileBounds(TileAxis origin, TileAxis size)
        {
            m_Origin = origin;
            m_Size = size;
            m_End = m_Origin + m_Size;
        }
    }
    public static class TileTools
    {
        static int AxisDimensionTransformation(int x, int y, int width) => x + y * width;
        public static bool InRange<T>(this TileAxis axis, T[,] range) => axis.X >= 0 && axis.X < range.GetLength(0) && axis.Y >= 0 && axis.Y < range.GetLength(1);
        public static bool InRange<T>(this TileAxis originSize, TileAxis sizeAxis, T[,] range) => InRange<T>(originSize + sizeAxis, range);
        public static int Get1DAxisIndex(TileAxis axis, int width) => AxisDimensionTransformation(axis.X, axis.Y, width);
        public static TileAxis GetAxisByIndex(int index, int width) => new TileAxis(index % width, index / width);
        public static T Get<T>(this T[,] tileArray, TileAxis axis) where T : class => axis.InRange(tileArray) ? tileArray[axis.X, axis.Y] : null;
        public static bool Get<T>(this T[,] tileArray, TileAxis axis, TileAxis size, ref List<T> tileList) where T : class
        {
            tileList.Clear();
            for (int i = 0; i < size.X; i++)
                for (int j = 0; j < size.Y; j++)
                {
                    if (!InRange(axis + new TileAxis(i, j), tileArray))
                        return false;
                    tileList.Add(tileArray.Get(axis + new TileAxis(i, j)));
                }
            return true;
        }

        public static List<TileAxis> GetAxisRange(int width, int height, TileAxis start, TileAxis end)
        {
            List<TileAxis> axisList = new List<TileAxis>();
            for (int i = start.X; i <= end.X; i++)
                for (int j = start.Y; j <= end.Y; j++)
                {
                    if (i < 0 || j < 0 || i >= width || j >= height)
                        continue;
                    axisList.Add(new TileAxis(i, j));
                }
            return axisList;
        }

        public static List<TileAxis> GetAxisRange(int width, int height, TileAxis centerAxis, int radius)
        {
            List<TileAxis> axisList = new List<TileAxis>();
            int sqrRadius = radius * radius;
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if ((centerAxis - new TileAxis(i, j)).SqrMagnitude > sqrRadius)
                        continue;
                    axisList.Add(new TileAxis(i, j));
                }
            return axisList;
        }

        public static List<TileAxis> GetDirectionAxies(int width, int height, TileAxis centerAxis, List<ETileDirection> directions)
        {
            List<TileAxis> axisList = new List<TileAxis>();
            foreach (ETileDirection direction in directions)
            {
                TileAxis targetAxis = centerAxis.DirectionAxis(direction);
                if (targetAxis.X < 0 || targetAxis.Y < 0 || targetAxis.X >= width || targetAxis.Y >= height)
                    continue;
                axisList.Add(targetAxis);
            }
            return axisList;
        }

        public static Dictionary<ETileDirection, T> GetDirectionAxies<T>(int width, int height, TileAxis centerAxis, List<ETileDirection> directions, Func<TileAxis, T> OnItemGet)
        {
            Dictionary<ETileDirection, T> axisList = new Dictionary<ETileDirection, T>();
            foreach (ETileDirection direction in directions)
            {
                TileAxis targetAxis = centerAxis.DirectionAxis(direction);
                if (targetAxis.X < 0 || targetAxis.Y < 0 || targetAxis.X >= width || targetAxis.Y >= height)
                    continue;
                axisList.Add(direction, OnItemGet(targetAxis));
            }
            return axisList;
        }


        public static bool CheckIsEdge<T>(this T[,] tileArray, TileAxis axis) where T : class, ITileAxis => axis.X == 0 || axis.X == tileArray.GetLength(0) - 1 || axis.Y == 0 || axis.Y == tileArray.GetLength(1) - 1;

        public static TileAxis GetDirectionedSize(TileAxis size, ETileDirection direction) => (int)direction % 2 == 0 ? size : size.Inverse();
        public static Vector3 GetUnitScaleBySizeAxis(TileAxis directionedSize, int tileSize) => new Vector3(directionedSize.X, 1, directionedSize.Y) * tileSize;
        public static Vector3 GetLocalPosBySizeAxis(TileAxis directionedSize) => new Vector3(directionedSize.X, 0, directionedSize.Y);
        public static Quaternion ToRotation(this ETileDirection direction) => Quaternion.Euler(0, (int)direction * 90, 0);
        public static ETileDirection Next(this ETileDirection direction)
        {
            direction++;
            if (direction > ETileDirection.Left)
                direction = ETileDirection.Top;
            else if (direction > ETileDirection.TopLeft)
                direction = ETileDirection.TopRight;
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
                case ETileDirection.Top:
                    return clockWise ? ETileDirection.TopRight : ETileDirection.TopLeft;
                case ETileDirection.Right:
                    return clockWise ? ETileDirection.BottomRight : ETileDirection.TopRight;
                case ETileDirection.Bottom:
                    return clockWise ? ETileDirection.BottomLeft : ETileDirection.BottomRight;
                case ETileDirection.Left:
                    return clockWise ? ETileDirection.TopLeft : ETileDirection.BottomLeft;
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
                case ETileDirection.TopRight:
                    return clockWise ? ETileDirection.Right : ETileDirection.Top;
                case ETileDirection.BottomRight:
                    return clockWise ? ETileDirection.Bottom : ETileDirection.Right;
                case ETileDirection.BottomLeft:
                    return clockWise ? ETileDirection.Left : ETileDirection.Bottom;
                case ETileDirection.TopLeft:
                    return clockWise ? ETileDirection.Top : ETileDirection.Left;
            }
        }

        public static bool IsEdge(this ETileDirection direction) => m_EdgeDirections.Contains(direction);
        public static bool IsAngle(this ETileDirection direction) => m_AngleDirections.Contains(direction);
        public static readonly List<ETileDirection> m_EdgeDirections = new List<ETileDirection>() { ETileDirection.Top, ETileDirection.Right, ETileDirection.Bottom, ETileDirection.Left };
        public static readonly List<ETileDirection> m_AngleDirections = new List<ETileDirection>() { ETileDirection.TopRight, ETileDirection.BottomRight, ETileDirection.BottomLeft, ETileDirection.TopLeft };
        public static readonly List<ETileDirection> m_AllDirections = new List<ETileDirection>() { ETileDirection.Top, ETileDirection.Right, ETileDirection.Bottom, ETileDirection.Left,
            ETileDirection.TopRight, ETileDirection.BottomRight, ETileDirection.BottomLeft, ETileDirection.TopLeft };
        public static readonly Dictionary<ETileDirection, TileAxis> m_DirectionAxies = new Dictionary<ETileDirection, TileAxis>() {
            { ETileDirection.Top,new TileAxis(0,1) }, { ETileDirection.Right, new TileAxis(1, 0) }, { ETileDirection.Bottom, new TileAxis(0, -1) }, { ETileDirection.Left, new TileAxis(-1, 0) },
            { ETileDirection.TopRight, new TileAxis(1, 1) }, { ETileDirection.BottomRight, new TileAxis(1, -1) }, { ETileDirection.BottomLeft, new TileAxis(-1, -1) }, { ETileDirection.TopLeft, new TileAxis(-1, 1) } };

        public static ETileDirection Inverse(this ETileDirection direction)
        {
            switch (direction)
            {
                default:
                    Debug.LogError("Error Direction Here");
                    return ETileDirection.Invalid;
                case ETileDirection.Top:
                    return ETileDirection.Bottom;
                case ETileDirection.Bottom:
                    return ETileDirection.Top;
                case ETileDirection.Right:
                    return ETileDirection.Left;
                case ETileDirection.Left:
                    return ETileDirection.Right;
                case ETileDirection.TopRight:
                    return ETileDirection.BottomLeft;
                case ETileDirection.BottomLeft:
                    return ETileDirection.TopRight;
                case ETileDirection.TopLeft:
                    return ETileDirection.BottomRight;
                case ETileDirection.BottomRight:
                    return ETileDirection.TopLeft;
            }
        }

        public static ETileDirection OffsetDirection(this TileAxis sourceAxis, TileAxis targetAxis)
        {
            TileAxis offset = targetAxis - sourceAxis;
            if (offset.Y == 0) return offset.X < 0 ? ETileDirection.Left : ETileDirection.Right;
            if (offset.X == 0) return offset.Y > 0 ? ETileDirection.Top : ETileDirection.Bottom;
            return ETileDirection.Invalid;
        }

        public static TileAxis DirectionAxis(this TileAxis sourceAxis, ETileDirection direction) => sourceAxis + m_DirectionAxies[direction];

        public static void PathFindForClosestApproch<T>(this T[,] tileArray, T t1, T t2, List<T> tilePathsAdd, Action<T> OnEachTilePath = null, Predicate<T> stopPredicate = null, Predicate<T> invalidPredicate = null) where T : class, ITileAxis       //Temporary Solution, Not Required Yet
        {
            if (!t1.m_Axis.InRange(tileArray) || !t2.m_Axis.InRange(tileArray))
                Debug.LogError("Error Tile Not Included In Array");


            tilePathsAdd.Add(t1);
            TileAxis startTile = t1.m_Axis;
            for (; ; )
            {
                TileAxis nextTile = startTile;
                float minDistance = (startTile - t2.m_Axis).SqrMagnitude;
                float offsetDistance;
                TileAxis offsetTile;
                TileAxis[] nearbyFourTiles = startTile.nearbyFourTiles;
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
                    case ETileDirection.Bottom:
                        axisX = randomSeed.Next(tileWidth - 1) + 1;
                        axisY = 0;
                        break;
                    case ETileDirection.Top:
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

        public static bool ArrayNearbyContains<T>(this T[,] tileArray, TileAxis origin, Predicate<T> predicate) where T : class, ITileAxis
        {
            TileAxis[] nearbyTiles = origin.nearbyFourTiles;
            for (int i = 0; i < nearbyTiles.Length; i++)
            {
                if (origin.InRange(tileArray) && !predicate(tileArray.Get(nearbyTiles[i])))
                    return false;
            }
            return true;
        }

        public static List<T> TileRandomFill<T>(this T[,] tileArray, System.Random seed, TileAxis originAxis, Action<T> OnEachFill, Predicate<T> availableAxis, int fillCount) where T : class, ITileAxis
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
                    TileAxis axis = temp.m_Axis.DirectionAxis(randomDirection);
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
