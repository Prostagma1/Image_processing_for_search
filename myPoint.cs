using System;
using System.Drawing;

namespace LABA3
{
    internal class myPoint
    {
        public int X0 { get; set; }
        public int Y0 { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Radius { get; set; }
        public int CountPixel { get; set; }
        public int CountWhitePixel { get; set; }
        public double Density { get; set; }

        public void СalculationDensity()
        {
            Density = CountWhitePixel / CountPixel;
        }

        public bool CheckValue(int w = 20, int h = 100)
        {
            return Width >= w && Height <= h;
        }
        public void ChangeCoords(int x, int y)
        {
            if (X0 > x)
            {
                X0 = x;
            }
            else if (X1 < x)
            {
                X1 = x;
            }
            if (Y0 > y)
            {
                Y0 = y;
            }
            else if (Y1 < y)
            {
                Y1 = y;
            }
            WidthAndHeight();
            СalculationDensity();
        }
        public void WidthAndHeight()
        {
            Width = Math.Abs(X1 - X0);
            Height = Math.Abs(Y1 - Y0);
            Radius = Math.Max(Width, Height)/2;
        }
        public int Diff()
        {
            return Math.Abs(Width - Height);
        }
        public Rectangle RetRect()
        {
            return new Rectangle(X0, Y0, Width, Height);
        }
        public double Area()
        {
            return (double)Width * (double)Height;
        }

        public string stringForListbox()
        {
            return $"({X0};{Y0}) ({X1};{Y1})";
        }
    }
}
