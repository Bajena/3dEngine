using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_Lab5
{
    public static class Utils
    {
        public static void Swap<T>(ref T e1, ref T e2)
        {
            var temp = e1;
            e1 = e2;
            e2 = temp;
        }
    }
}