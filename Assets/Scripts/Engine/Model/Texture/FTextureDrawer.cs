using System.Linq.Extensions;
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
        
        public void Pixel(int _x,int _y,Color _color)
        {
            var dst = (_x + _y * SizeX);
            if (dst < 0 || dst >= totalSize)
                return;
            colors[dst] = _color;
        }

        public void Clear(Color _color) => colors.FillDefault(_color);
        
        int preX,preY;
        public void PixelContinuous(int2 _pos,Color _color) => PixelContinuous(_pos.x,_pos.y,_color);
        public void PixelContinuous(int _x,int _y,Color _color)
        {
            var transparent = _color.SetA(.5f);
            Pixel(_x , _y , _color);
            // Pixel(_x + 1 , _y , transparent);
            // Pixel(_x - 1 , _y , transparent);
            // Pixel(_x , _y + 1 , transparent);
            // Pixel(_x , _y - 1 , transparent);
            var x1 = preX;
            var x2 = _x;
            var y1 = preY;
            var y2 = _y;
            
            var dx = math.abs(x2 - x1);
            var dy = math.abs(y2 - y1);

            var sx = x1 < x2 ? 1 : -1;
            var sy = y1 < y2 ? 1 : -1;

            var err = dx - dy;

            var drawCount = dy * dx;
            while (drawCount-- > 0)
            {
                // Set the pixel color at the current position
                Pixel(x1, y1, _color);
                // Pixel(x1 + 1 , y1 , transparent);
                // Pixel(x1 - 1 , y1 , transparent);
                // Pixel(x1 , y1 + 1 , transparent);
                // Pixel(x1 , y1 - 1 , transparent);

                if (x1 == x2 && y1 == y2)
                {
                    break;
                }

                var e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
            preX = _x;
            preY = _y;
        }
        
        
        void plot1(int x,int y,int2 _centre,Color _color)
        {
            Pixel(_centre.x + x, _centre.y + y, _color);
        }
        void plot8(int x,int y,int2 _centre,Color _color){
            plot1(x,y,_centre,_color);plot1(y,x,_centre,_color);
            plot1(x,-y,_centre,_color);plot1(y,-x,_centre,_color);
            plot1(-x,-y,_centre,_color);plot1(-y,-x,_centre,_color);
            plot1(-x,y,_centre,_color);plot1(-y,x,_centre,_color);
        }
        public void Circle(int2 _centre,int _radius,Color _color)
        {
            var x = 0;
            var y = _radius;
            var d = 1 - _radius;
            while(x < y)
            {
                if(d < 0)
                {
                    d += 2 * x + 3;
                }
                else
                {
                    d += 2 * (x-y) + 5;
                    y--;
                }
                plot8(x,y, _centre,_color);
                x++;
            }
        }
    }
}