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

        private int2 pre;
        public void PixelContinuousStart(int2 _pos) => pre = _pos;
        public void PixelContinuousStart(int _x, int _y) => PixelContinuousStart(new int2(_x, _y));
        public void PixelContinuous(int _x,int _y,Color _color) => PixelContinuous(new int2(_x,_y),_color);
        public void PixelContinuous(int2 _pos,Color _color)
        {
            // Pixel(_x , _y , _color);
            foreach (var pos in CartographicGeneralization.BresenhamLine(pre,_pos).ToArray())
                Pixel(pos.x,pos.y, _color);

            pre = _pos;
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