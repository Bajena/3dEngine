using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_Lab5
{
    public static class MathHelpers
    {
        public static float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        public static double Interpolate(double min, double max, double gradient)
        {
            var res = min + (max - min) * Clamp((float)gradient);

            return res;
        }

        public static Vector3 Interpolate(Vector3 min, Vector3 max, double gradient)
        {
            double x = Interpolate(min.X, max.X, gradient);
            double y = Interpolate(min.Y, max.Y, gradient);
            double z = Interpolate(min.Z, max.Z, gradient);

            return new Vector3(x, y, z);
        }
    }
}
