using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities_Tile
{

    public enum enum_TileDirection
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

        public static List<TileAxis> GetDirectionAxies(int width, int height, TileAxis centerAxis, List<enum_TileDirection> directions)
        {
            List<TileAxis> axisList = new List<TileAxis>();
            directions.Traversal((enum_TileDirection direction) => {
                TileAxis targetAxis = centerAxis.DirectionAxis(direction);
                if (targetAxis.X < 0 || targetAxis.Y < 0 || targetAxis.X >= width || targetAxis.Y >= height)
                    return;
                axisList.Add(targetAxis);
            });
            return axisList;
        }

        public static Dictionary<enum_TileDirection, T> GetDirectionAxies<T>(int width, int height, TileAxis centerAxis, List<enum_TileDirection> directions, Func<TileAxis, T> OnItemGet)
        {
            Dictionary<enum_TileDirection, T> axisList = new Dictionary<enum_TileDirection, T>();
            directions.Traversal((enum_TileDirection direction) => {
                TileAxis targetAxis = centerAxis.DirectionAxis(direction);
                if (targetAxis.X < 0 || targetAxis.Y < 0 || targetAxis.X >= width || targetAxis.Y >= height)
                    return;
                axisList.Add(direction, OnItemGet(targetAxis));
            });
            return axisList;
        }


        public static bool CheckIsEdge<T>(this T[,] tileArray, TileAxis axis) where T : class, ITileAxis => axis.X == 0 || axis.X == tileArray.GetLength(0) - 1 || axis.Y == 0 || axis.Y == tileArray.GetLength(1) - 1;

        public static TileAxis GetDirectionedSize(TileAxis size, enum_TileDirection direction) => (int)direction % 2 == 0 ? size : size.Inverse();
        public static Vector3 GetUnitScaleBySizeAxis(TileAxis directionedSize, int tileSize) => new Vector3(directionedSize.X, 1, directionedSize.Y) * tileSize;
        public static Vector3 GetLocalPosBySizeAxis(TileAxis directionedSize) => new Vector3(directionedSize.X, 0, directionedSize.Y);
        public static Quaternion ToRotation(this enum_TileDirection direction) => Quaternion.Euler(0, (int)direction * 90, 0);
        public static enum_TileDirection Next(this enum_TileDirection direction)
        {
            direction++;
            if (direction > enum_TileDirection.Left)
                direction = enum_TileDirection.Top;
            else if (direction > enum_TileDirection.TopLeft)
                direction = enum_TileDirection.TopRight;
            return direction;
        }

        public static enum_TileDirection EdgeNextCornor(this enum_TileDirection direction, bool clockWise)
        {
            if (!direction.IsEdge())
            {
                Debug.LogError("Invalid Directions Here!");
                return enum_TileDirection.Invalid;
            }
            switch (direction)
            {
                default:
                    Debug.LogError("Invalid Convertions Here!");
                    return enum_TileDirection.Invalid;
                case enum_TileDirection.Top:
                    return clockWise ? enum_TileDirection.TopRight : enum_TileDirection.TopLeft;
                case enum_TileDirection.Right:
                    return clockWise ? enum_TileDirection.BottomRight : enum_TileDirection.TopRight;
                case enum_TileDirection.Bottom:
                    return clockWise ? enum_TileDirection.BottomLeft : enum_TileDirection.BottomRight;
                case enum_TileDirection.Left:
                    return clockWise ? enum_TileDirection.TopLeft : enum_TileDirection.BottomLeft;
            }
        }

        public static enum_TileDirection AngleNextEdge(this enum_TileDirection direction, bool clockWise)
        {
            if (!direction.IsAngle())
            {
                Debug.LogError("Invalid Directions Here!");
                return enum_TileDirection.Invalid;
            }
            switch (direction)
            {
                default:
                    Debug.LogError("Invalid Convertions Here!");
                    return enum_TileDirection.Invalid;
                case enum_TileDirection.TopRight:
                    return clockWise ? enum_TileDirection.Right : enum_TileDirection.Top;
                case enum_TileDirection.BottomRight:
                    return clockWise ? enum_TileDirection.Bottom : enum_TileDirection.Right;
                case enum_TileDirection.BottomLeft:
                    return clockWise ? enum_TileDirection.Left : enum_TileDirection.Bottom;
                case enum_TileDirection.TopLeft:
                    return clockWise ? enum_TileDirection.Top : enum_TileDirection.Left;
            }
        }

        public static bool IsEdge(this enum_TileDirection direction) => m_EdgeDirections.Contains(direction);
        public static bool IsAngle(this enum_TileDirection direction) => m_AngleDirections.Contains(direction);
        public static readonly List<enum_TileDirection> m_EdgeDirections = new List<enum_TileDirection>() { enum_TileDirection.Top, enum_TileDirection.Right, enum_TileDirection.Bottom, enum_TileDirection.Left };
        public static readonly List<enum_TileDirection> m_AngleDirections = new List<enum_TileDirection>() { enum_TileDirection.TopRight, enum_TileDirection.BottomRight, enum_TileDirection.BottomLeft, enum_TileDirection.TopLeft };
        public static readonly List<enum_TileDirection> m_AllDirections = new List<enum_TileDirection>() { enum_TileDirection.Top, enum_TileDirection.Right, enum_TileDirection.Bottom, enum_TileDirection.Left,
            enum_TileDirection.TopRight, enum_TileDirection.BottomRight, enum_TileDirection.BottomLeft, enum_TileDirection.TopLeft };
        public static readonly Dictionary<enum_TileDirection, TileAxis> m_DirectionAxies = new Dictionary<enum_TileDirection, TileAxis>() {
            { enum_TileDirection.Top,new TileAxis(0,1) }, { enum_TileDirection.Right, new TileAxis(1, 0) }, { enum_TileDirection.Bottom, new TileAxis(0, -1) }, { enum_TileDirection.Left, new TileAxis(-1, 0) },
            { enum_TileDirection.TopRight, new TileAxis(1, 1) }, { enum_TileDirection.BottomRight, new TileAxis(1, -1) }, { enum_TileDirection.BottomLeft, new TileAxis(-1, -1) }, { enum_TileDirection.TopLeft, new TileAxis(-1, 1) } };

        public static enum_TileDirection Inverse(this enum_TileDirection direction)
        {
            switch (direction)
            {
                default:
                    Debug.LogError("Error Direction Here");
                    return enum_TileDirection.Invalid;
                case enum_TileDirection.Top:
                    return enum_TileDirection.Bottom;
                case enum_TileDirection.Bottom:
                    return enum_TileDirection.Top;
                case enum_TileDirection.Right:
                    return enum_TileDirection.Left;
                case enum_TileDirection.Left:
                    return enum_TileDirection.Right;
                case enum_TileDirection.TopRight:
                    return enum_TileDirection.BottomLeft;
                case enum_TileDirection.BottomLeft:
                    return enum_TileDirection.TopRight;
                case enum_TileDirection.TopLeft:
                    return enum_TileDirection.BottomRight;
                case enum_TileDirection.BottomRight:
                    return enum_TileDirection.TopLeft;
            }
        }

        public static enum_TileDirection OffsetDirection(this TileAxis sourceAxis, TileAxis targetAxis)
        {
            TileAxis offset = targetAxis - sourceAxis;
            if (offset.Y == 0) return offset.X < 0 ? enum_TileDirection.Left : enum_TileDirection.Right;
            if (offset.X == 0) return offset.Y > 0 ? enum_TileDirection.Top : enum_TileDirection.Bottom;
            return enum_TileDirection.Invalid;
        }

        public static TileAxis DirectionAxis(this TileAxis sourceAxis, enum_TileDirection direction) => sourceAxis + m_DirectionAxies[direction];

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

        public static T TileEdgeRandom<T>(this T[,] tileArray, System.Random randomSeed = null, Predicate<T> predicate = null, List<enum_TileDirection> edgeOutcluded = null, int predicateTryCount = -1) where T : class, ITileAxis        //Target Edges Random Tile
        {
            if (edgeOutcluded != null && edgeOutcluded.Count > 3)
                Debug.LogError("Can't Outclude All Edges!");

            if (predicateTryCount == -1) predicateTryCount = int.MaxValue;

            List<enum_TileDirection> edgesRandom = new List<enum_TileDirection>(m_EdgeDirections) { };
            if (edgeOutcluded != null) edgesRandom.RemoveAll(p => edgeOutcluded.Contains(p));

            int axisX = -1, axisY = -1;
            int tileWidth = tileArray.GetLength(0), tileHeight = tileArray.GetLength(1);
            T targetTile = null;
            for (int i = 0; i < predicateTryCount; i++)
            {
                enum_TileDirection randomDirection = edgesRandom.RandomItem(randomSeed);
                switch (randomDirection)
                {
                    case enum_TileDirection.Bottom:
                        axisX = randomSeed.Next(tileWidth - 1) + 1;
                        axisY = 0;
                        break;
                    case enum_TileDirection.Top:
                        axisX = randomSeed.Next(tileWidth - 1);
                        axisY = tileHeight - 1;
                        break;
                    case enum_TileDirection.Left:
                        axisX = 0;
                        axisY = randomSeed.Next(tileHeight - 1);
                        break;
                    case enum_TileDirection.Right:
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
                m_EdgeDirections.TraversalRandomBreak((enum_TileDirection randomDirection) => {
                    TileAxis axis = temp.m_Axis.DirectionAxis(randomDirection);
                    if (axis.InRange(tileArray))
                    {
                        targetAdd = tileArray.Get(axis);
                        if (availableAxis(targetAdd))
                        {
                            OnEachFill(targetAdd);
                            targetList.Add(targetAdd);
                            return true;
                        }
                    }
                    return false;
                }, seed);
            }
            return targetList;
        }
    }
}

namespace TTile_Hexagon
{
    public static class HexagonHelper
    {
        public static readonly float C_SQRT3 = Mathf.Sqrt(3);
        public static readonly float C_SQRT3Half = C_SQRT3/2f;
        public static readonly Vector2[] C_UnitHexagonPoints = new Vector2[6] {new Vector2(0,1),new Vector2(C_SQRT3Half, .5f),new Vector2(C_SQRT3Half,-.5f),new Vector2(0,-1),new Vector2(-C_SQRT3Half,-.5f),new Vector2(-C_SQRT3Half,.5f) }; 

        public static Vector3 GetHexagonOriginOffset(float radius, int xIndex, int yIndex)=>new Vector3(xIndex *C_SQRT3Half , 0, yIndex * 3 + (xIndex % 2) * 1.5f) * radius;
        public static Vector3[] GetHexagonPoints(Vector3 origin, float radius) => GetHexagonPoints(origin, radius, Vector3.forward, Vector3.up);
        public static Vector3[] GetHexagonPoints(Vector3 origin, float radius, Vector3 forward, Vector3 normal)
        {
            Vector3[] points = new Vector3[6];
            for (int i = 0; i < 6; i++)
                points[i] = origin + Quaternion.AngleAxis(60 * i, normal) * forward * radius;
            return points;
        }

    }
}
