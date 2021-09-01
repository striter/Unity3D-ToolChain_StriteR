using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Procedural.Hexagon.Area
{
    public static class UHexagonArea
    {
        public static int radius { get; private set; } = 1;
        public static int tilling { get; private set; } = 1;
        public static bool welded { get; private set; }
        private static int r;
        private static int s;
        private static int a;
        private static float sf;
        static HexagonCoordC[] m_NearbyCoordsCS;

        public static void Init(int _radius,int _tilling=1,bool _welded=false)
        {
            radius =_radius;
            tilling = _tilling;
            welded = _welded;

            r = radius * tilling;
            a= 3 * r * r + 3 *r +1;
            s= 3 * r + 2;
            sf = s;
            
            m_NearbyCoordsCS = UHexagon.GetCoordsNearby(HexagonCoordC.zero).Select(p => p * tilling).ToArray();
        }
        
        public static HexagonCoordC TransformCSToAS(this HexagonArea _area,HexagonCoordC _positionCS)
        {
            return (_positionCS - _area.centerCS)/tilling;
        }


        public static HexagonCoordC TransformASToCS(this HexagonArea _area,HexagonCoordC _positionAS)
        {
            return _positionAS*tilling + _area.centerCS;
        }

        public static HexagonCoordC[] GetNearbyAreaCoordsCS(HexagonCoordC _positionCS)=>m_NearbyCoordsCS.Select(p=>p+_positionCS).ToArray();
        public static HexagonCoordC GetNearbyAreaCoordCS(HexagonCoordC _positionCS, int direction)=> m_NearbyCoordsCS[direction%6]+_positionCS;
        public static HexagonCoordC GetBelongAreaCoord(HexagonCoordC _positionCS)
        {
            ref var x = ref _positionCS.x;
            ref var y = ref _positionCS.y;
            ref var z = ref _positionCS.z;
            
            int xh=welded? x:y;
            int yh=welded?y:z;
            int zh=welded?z:x;
            xh = Mathf.FloorToInt( (xh + sf * x) / a);
            yh = Mathf.FloorToInt( (yh + sf * y) / a);
            zh = Mathf.FloorToInt( (zh + sf * z) / a);
            int i = Mathf.FloorToInt((1 + xh - yh) / 3f);
            int j = Mathf.FloorToInt((1 + yh - zh) / 3f);
            int k = Mathf.FloorToInt((1 + zh - xh) / 3f);
            //var xyzH = new Int3(xh, yh, zh);
            return new HexagonCoordC(i, j, k);
        }
        public static HexagonArea GetArea(HexagonCoordC _areaCoord)
        {
            ref var i=ref _areaCoord.x;
            ref var j=ref _areaCoord.y;
            ref var k=ref _areaCoord.z;

            var centerCS = new HexagonCoordC((r + 1) * i - r * k, (r + 1) * j - r * i, (r + 1) * k - r * j);
            if (welded)
                centerCS -= _areaCoord;
            return new HexagonArea() {m_Coord =_areaCoord, centerCS = centerCS};
        }
        public static HexagonArea GetBelongingArea(HexagonCoordC _positionCS) => GetArea( GetBelongAreaCoord(_positionCS));


        public static IEnumerable<HexagonCoordC> IterateAreaCoordsCS(this HexagonArea _area)
        {
            foreach (var coordsAS in HexagonCoordC.zero.GetCoordsInRadius(radius))
                yield return _area.TransformASToCS(coordsAS);
        }
        public static IEnumerable<(int dir,bool first,HexagonCoordC coord)> IterateAreaCoordsCSRinged(this HexagonArea _area,int _radius)
        {
            foreach (var tuple in HexagonCoordC.zero.GetCoordsRinged(_radius))
                yield return (tuple.dir,tuple.first,_area.TransformASToCS(tuple.coord));
        }

        public static IEnumerable<HexagonCoordC> IterateAllCoordsCS(this HexagonArea _area)
        {
            foreach (var coordsCS in _area.centerCS.GetCoordsInRadius(r))
                yield return coordsCS;
        }

        public static IEnumerable<(int radius,int dir,bool first, HexagonCoordC coord)> IterateAllCoordsCSRinged(this HexagonArea _area,bool insideOut=true)
        {
            for (int i = 0; i <= radius; i++)
            {
                int ring = insideOut ? i : (radius - i);
                foreach (var coords in HexagonCoordC.zero.GetCoordsRinged(ring))
                    yield return (ring,coords.dir,coords.first,_area.TransformASToCS(coords.coord));
            }
        }
        public static bool InRange(this HexagonArea _area, HexagonCoordC _positionCS)
        {
            HexagonCoordC localCCoord = _positionCS - _area.centerCS;
            return localCCoord.InRange(r);
        }
        
        public static int TransformASToID(HexagonCoordC _positionAS)
        {
            ref var x = ref _positionAS.x;
            ref var y = ref _positionAS.y;
            return (y + s * x +a) % a;
        }
        public static HexagonCoordC TransformIDtoAS(int _identityAS)
        {
            ref var m = ref _identityAS;
            var ms =Mathf.FloorToInt( (m + radius) / sf);
            var mcs = Mathf.FloorToInt((m + 2 * radius) / (sf - 1));
            var x=ms * (radius + 1) + mcs * -radius;
            var y = m + ms * (-2 * radius - 1) + mcs * (-radius - 1);
            var z = -m + ms *radius + mcs * (2 * radius + 1);
            return new HexagonCoordC(x, y, z);
        }

    }
}