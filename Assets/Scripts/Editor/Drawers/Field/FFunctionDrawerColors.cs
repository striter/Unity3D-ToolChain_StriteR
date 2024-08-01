using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public class FFunctionDrawerColors
    {
        public Color[] colors;
        public int sizeX, sizeY, totalSize;
        public FFunctionDrawerColors(int _x,int _y,Color _initial)
        {
            sizeX = _x;
            sizeY = _y;
            totalSize = sizeX * sizeY;
            colors = new Color[sizeX*sizeY];
            colors.FillDefault(_initial);
        }

        public void Pixel(int _x,int _y,Color _color)
        {
            var dst = (_x + _y * sizeX);
            if (dst < 0 || dst >= totalSize)
                return;
            colors[dst] = _color;
        }

        private int preX = 0;
        private int preY = 0;
        public void PixelContinuousStart(int _x, int _y)
        {
            preX = _x;
            preY = _y;
        }
        
        public void PixelContinuous(int _x,int _y,Color _color)
        {
            var transparent = _color.SetA(.5f);
            Pixel(_x , _y , _color);
            Pixel(_x + 1 , _y , transparent);
            Pixel(_x - 1 , _y , transparent);
            Pixel(_x , _y + 1 , transparent);
            Pixel(_x , _y - 1 , transparent);
            int xStart = Mathf.Min(_x, preX);
            int xEnd = Mathf.Max(_x, preX);
            int yStart = Mathf.Min(_y, preY)+1;
            int yEnd = Mathf.Max(_y, preY)-1;
            for(int i = xStart;i < xEnd;i++)
            for (int j = yStart; j < yEnd; j++)
            {
                Pixel(i , j , _color);
                Pixel(i + 1 , j , transparent);
                Pixel(i - 1 , j , transparent);
                Pixel(i , j + 1 , transparent);
                Pixel(i , j - 1 , transparent);
            }

            preX = _x;
            preY = _y;
        }
        
        
        void plot1(int x,int y,Unity.Mathematics.int2 _centre,Color _color)
        {
            Pixel(_centre.x + x, _centre.y + y, _color);
        }
        void plot8(int x,int y,Unity.Mathematics.int2 _centre,Color _color){
            plot1(x,y,_centre,_color);plot1(y,x,_centre,_color);
            plot1(x,-y,_centre,_color);plot1(y,-x,_centre,_color);
            plot1(-x,-y,_centre,_color);plot1(-y,-x,_centre,_color);
            plot1(-x,y,_centre,_color);plot1(-y,x,_centre,_color);
        }

        public void Circle(Unity.Mathematics.int2 _centre,int _radius,Color _color)
        {
            int x = 0;
            int y = _radius;
            int d = 1 - _radius;
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