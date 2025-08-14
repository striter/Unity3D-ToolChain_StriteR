using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public class FTextureDrawer
    {
        public Color[] colors;
        public int totalSize;
        public int2 size;
        public int SizeX => size.x;
        public int SizeY => size.y;
        public FTextureDrawer(int2 _size, Color _initial = default)
        {
            size = _size;
            totalSize = _size.x * _size.y;
            colors = new Color[totalSize];
            colors.FillDefault(_initial);
        }
        public FTextureDrawer(int _x, int _y, Color _initial = default) : this(new int2(_x, _y), _initial) { }
        public void Line(float2 _start,float2 _end,Color _color) => Line((int2)(_start * size),(int2)(_end * size),_color);
        public void Line(int2 start,int2 end,Color _color)
        {
            start = start.clamp(0, size);
            end = end.clamp(0, size);
            UCartographicGeneralization.Bresenham.Line(start, end,
                (pos, opacity) => Pixel(pos.x, pos.y, _color.SetA(opacity)));
        }

        public void LineWidth(float2 _start, float2 _end, float wd, Color _color)
        {
            if(_start.x < 0 || _start.x > 1 || _start.y < 0 || _start.y > 1 || _end.x < 0 || _end.x > 1 || _end.y < 0 || _end.y > 1) 
                return;
            var start = (int2)(_start * size);
            var end = (int2)(_end * size);
            UCartographicGeneralization.Bresenham.LineWidth(start,end,wd,(pos,opacity) => Pixel(pos.x,pos.y, _color.SetA(_color.a * opacity)));
        }

        public void Pixel(float2 _uv,Color _color)
        {
            var pos = (int2)(_uv * size);
            Pixel(pos.x,pos.y, _color);
        }
        
        void Pixel(int _x,int _y,Color _color)
        {
            var dst = (_x + _y * size.x);
            if (dst < 0 || dst >= totalSize)
                return;
            colors[dst] = _color;
        }
        
        public void Clear(Color _color) => colors.FillDefault(_color);

        private int2 pre;
        public void PixelContinuousStart(float2 _uv) => PixelContinuousStart((int2)(_uv * size));
        public void PixelContinuousStart(int2 _uv) => pre = _uv;
        public void PixelContinuous(float2 _uv,Color _color) => PixelContinuous((int2)(_uv * size),_color); 
        public void PixelContinuous(int2 _pixel,Color _color)
        {
            Line(pre, _pixel , _color);
            pre = _pixel;
        }
        
        public void Circle(float2 _centreUV,int _radius = 5,Color _color = default) => Circle((int2)(_centreUV * size),_radius,_color);
        public void Circle(int2 _centerPixel, int _radius = 5, Color _color = default)
        {
            if (_centerPixel.x < 0 || _centerPixel.x > size.x || _centerPixel.y < 0 || _centerPixel.y > size.y)
                return;
            UCartographicGeneralization.Bresenham.Circle(_centerPixel,_radius,(pos,opacity) => Pixel(pos.x,pos.y, _color.SetA(opacity)));
        }

        static readonly float2 kDigitCellSize = new(4,5);
        float SampleDigit(int digitBinary ,float2 uv)
        {
            if(uv.x<0||uv.x>1||uv.y<0||uv.y>1)
                return 0;

            var pixel= (int2)math.floor(uv * kDigitCellSize);
            var fIndex = pixel.x + (pixel.y*4);
            return math.fmod(math.floor(digitBinary/math.pow(2,fIndex)),2);
        }
        
        public void Digit(IEnumerable<char> _digitValue,float2 _uv,Color _color = default,int _digitSize = 5, int gap = 1)
        {
            if (_uv.x < 0 || _uv.x > 1 || _uv.y < 0 || _uv.y > 1)
                return;

            var digits = _digitValue.FillList(PoolList<char>.Empty("Digit"));
            var digitOriginPixel = (int2)(_uv * size);
            var digitCellSize = kDigitCellSize * _digitSize;
            for (var i = 0; i < digits.Count; i++)
            {
                var digitBinary = digits[i].GetDigitBinary3x5();
                for (var j = 0; j < kDigitCellSize.x * _digitSize; j++)
                {
                    for (var k = 0; k < kDigitCellSize.y * _digitSize; k++)
                    {
                        var digitLocalUV = new float2(j / (kDigitCellSize.x * _digitSize), k / (kDigitCellSize.y * _digitSize));
                        var digitSample = SampleDigit(digitBinary,digitLocalUV);
                        if (digitSample > .5f)
                        {
                            var pixel = digitOriginPixel  + (int2)(digitLocalUV * digitCellSize);
                            Pixel(pixel.x,pixel.y ,_color.SetA(digitSample));
                        }
                    }
                }
                digitOriginPixel.x += (int)digitCellSize.x  + gap;
            }
        }
    }
}