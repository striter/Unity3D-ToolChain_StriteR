using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rendering.IndirectDiffuse.SphericalHarmonics
{
    [System.Serializable]
    public struct SHL2Data
    {
        public Vector3 l00;
        public Vector3 l10;
        public Vector3 l11;
        public Vector3 l12;
        public Vector3 l20;
        public Vector3 l21;
        public Vector3 l22;
        public Vector3 l23;
        public Vector3 l24;

        public void OutputSH(out Vector4 _SHAr, out Vector4 _SHAg, out Vector4 _SHAb,
            out Vector4 _SHBr, out Vector4 _SHBg, out Vector4 _SHBb, out Vector3 _SHC)
        {
            _SHAr = new Vector4(SHBasis.kL10 * l10.x, SHBasis.kL11 * l11.x, SHBasis.kL12 * l12.x, SHBasis.kL00 * l00.x);
            _SHAg = new Vector4(SHBasis.kL10 * l10.y, SHBasis.kL11 * l11.y, SHBasis.kL12 * l12.y, SHBasis.kL00 * l00.y);
            _SHAb = new Vector4(SHBasis.kL10 * l10.z, SHBasis.kL11 * l11.z, SHBasis.kL12 * l12.z, SHBasis.kL00 * l00.z);

            _SHBr = new Vector4(SHBasis.kL20 * l20.x, SHBasis.kL21 * l21.x, SHBasis.kL22 * l22.x, SHBasis.kL23 * l23.x);
            _SHBg = new Vector4(SHBasis.kL20 * l20.y, SHBasis.kL21 * l21.y, SHBasis.kL22 * l22.y, SHBasis.kL23 * l23.y);
            _SHBb = new Vector4(SHBasis.kL20 * l20.z, SHBasis.kL21 * l21.z, SHBasis.kL22 * l22.z, SHBasis.kL23 * l23.z);
            _SHC = l24 * SHBasis.kL24;
        }
    }

    public static class SHBasis
    {
        public static readonly float kL00 = 0.5f * Mathf.Sqrt(1.0f / UMath.PI); //Constant

        //L1
        static readonly float kL1P = Mathf.Sqrt(3f / (4f * Mathf.PI));
        public static readonly float kL10 = kL1P; //*y
        public static readonly float kL11 = kL1P; //*z
        public static readonly float kL12 = kL1P; //*x

        //L2
        static readonly float kL2P = Mathf.Sqrt(15f / Mathf.PI);
        public static readonly float kL20 = 0.5f * kL2P; //*x*y
        public static readonly float kL21 = 0.5f * kL2P; //*y*z
        public static readonly float kL22 = 0.25f * Mathf.Sqrt(5f / Mathf.PI); //*z * z
        public static readonly float kL23 = 0.5f * kL2P; //* z * x
        public static readonly float kL24 = 0.25f * kL2P; //*(x*x - z*z)
    }

    public static class SphericalHarmonicsExport
    {
        static SHL2Data ExportData(int _sampleCount,Func<Vector3, Color> _sampleColor,string _randomSeed)
        {
            SHL2Data data = default;
            var random = _randomSeed == null ? null : new System.Random(_randomSeed.GetHashCode());
            for (int i = 0; i < _sampleCount; i++)
            {
                Vector3 randomPos = URandom.RandomDirection(random);
                Vector3 color = _sampleColor(randomPos).ToVector();
                
                float x = randomPos.x;
                float y = randomPos.y;
                float z = randomPos.z;
                data.l00 += color * SHBasis.kL00;

                data.l10 += color * SHBasis.kL10 * z;
                data.l11 += color * SHBasis.kL11 * y;
                data.l12 += color * SHBasis.kL12 * x;

                data.l20 += color * SHBasis.kL20 * x * y;
                data.l21 += color * SHBasis.kL21 * y * z;
                data.l22 += color * SHBasis.kL22 * (-x*x - y*y + 2 * z * z);
                data.l23 += color * SHBasis.kL23 * z * x;
                data.l24 += color * SHBasis.kL24 * (x * x - y * y);
            }

            float pi4d = 4f * UMath.PI/_sampleCount;
            data.l00 = pi4d * data.l00;
            data.l10 = pi4d * data.l10;
            data.l11 = pi4d * data.l11;
            data.l12 = pi4d * data.l12;
            data.l20 = pi4d * data.l20;
            data.l21 = pi4d * data.l21;
            data.l22 = pi4d * data.l22;
            data.l23 = pi4d * data.l23;
            data.l24 = pi4d * data.l24;
            return data;
        }
        
        public static SHL2Data ExportL2Gradient(int _sampleCount,Color _top,Color _equator,Color _bottom,string _randomSeed=null)
        {
            //Closest take cause i can't get the source code
            var top = (_top + _equator) * .5f;
            var bottom = (_equator + _bottom) * .5f;
            var center = _equator * .9f + (_top + _bottom) * .1f;

            return ExportData(_sampleCount, _p =>
            {
                float value = _p.y;
                Color tb = Color.Lerp(center, top, Mathf.SmoothStep(0, 1, value));
                return Color.Lerp(tb, bottom, Mathf.SmoothStep(0, 1, -value));
            },_randomSeed);
        }

        public static SHL2Data ExportL2Cubemap(int _sampleCount, Cubemap _cubemap, string _randomSeed)=> ExportData(_sampleCount, _p =>
            {
                float xAbs = Mathf.Abs(_p.x);
                float yAbs = Mathf.Abs(_p.y);
                float zAbs = Mathf.Abs(_p.z);
                int index = -1;
                Vector2 uv = new Vector2();
                if (xAbs >= yAbs && xAbs >= zAbs)
                {
                    index = _p.x > 0 ? 0 : 1;
                    uv.x = _p.y / _p.x;
                    uv.y = _p.z / _p.x;
                }
                else if (yAbs >= xAbs && yAbs >= zAbs)
                {
                    index = _p.y > 0 ? 2 : 3;
                    uv.x = _p.x / _p.y;
                    uv.y = _p.z / _p.y;
                }
                else
                {
                    index = _p.z > 0 ? 4 : 5;
                    uv.x = _p.x / _p.z;
                    uv.y = _p.y / _p.z;
                }

                uv = (uv + Vector2.one) / 2f;
                int width = _cubemap.width - 1;
                int x = (int) (width * uv.x);
                int y = (int) (width * uv.y);
                return _cubemap.GetPixel((CubemapFace) index, x, y);
            },_randomSeed);
        }
}