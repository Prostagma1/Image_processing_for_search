using System;
using System.Drawing;

namespace LABA3
{
    public class myCluster
    {

        public Point Start;
        public Point Center;
        public Point End;
        public int Width;
        public int Height;
        public static int Count = -1;

        public myCluster(int x, int y)
        {
            Start = new Point(x, y);
            End = new Point(x, y);
            Center = new Point(x, y);
            Count++;
        }
        public myCluster(myPixel pixel)
        {
            int x = pixel.x;
            int y = pixel.y;

            Start = new Point(x, y);
            End = new Point(x, y);
            Center = new Point(x, y);
            Count++;
        }
        private void WidthAndHeightAndCenter()
        {
            Width = End.X - Start.X;
            Height = End.Y - Start.Y;
            Center = new Point((End.X + Start.X)/2, (End.Y + Start.Y)/ 2);
        }
        public void CheckPoints(int x, int y)
        {
            if (x > End.X)
            {
                End.X = x;
            }
            if (y > End.Y)
            {
                End.Y = y;
            }
            if (x < Start.X)
            {
                Start.X = x;
            }
            if (y < Start.Y)
            {
                Start.Y = y;
            }
            WidthAndHeightAndCenter();
        }
        public void CheckPoints(myPixel pixel)
        {
            int x = pixel.x;
            int y = pixel.y;

            if (x > End.X)
            {
                End.X = x;
            }
            if (y > End.Y)
            {
                End.Y = y;
            }
            if (x < Start.X)
            {
                Start.X = x;
            }
            if (y < Start.Y)
            {
                Start.Y = y;
            }
            WidthAndHeightAndCenter();
        }
        public Rectangle GetRectangle()
        {
            return new Rectangle(Start.X, Start.Y,Width,Height);
        }
        public int LenToPoint (int x, int y)
        {
            double tempX = Math.Pow((Center.X - x),2);
            double tempY = Math.Pow((Center.Y - y), 2);
            return (int)Math.Sqrt(tempY + tempX);
        }
        public int LenToPoint(myPixel pixel) 
        {
            double tempX = Math.Pow((Center.X - pixel.x), 2);
            double tempY = Math.Pow((Center.Y - pixel.y), 2);
            return (int)Math.Sqrt(tempY + tempX);
        }
    }
}
