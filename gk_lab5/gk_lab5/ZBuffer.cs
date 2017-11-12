using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_Lab5
{
    public class ZBuffer
    {
        public double[,] Elements;
        public int Width;
        public int Height;

        public ZBuffer(int width, int height)
        {
            Width = width;
            Height = height;

            Elements = new double[width, height];

            Clear();
        }

        public bool IsCloser(int x, int y, double z)
        {
            return (x < Width && y < Height && x >= 0 && y >= 0 && z <= Elements[x, y]);
        }


        public void Clear()
        {
            for (int i = 0; i < Width; ++i)
                for (int j = 0; j < Height; ++j)
                    Elements[i, j] = double.MaxValue;
        }

        public void SetElement(int x,int y,double z)
        {
            Elements[x, y] = z;
        }
    }
}