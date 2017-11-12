
using System;
using System.Drawing;

namespace GK_Lab5
{
    public class Vector2 : Matrix
    {
        public double X
        {
            get { return this[0, 0]; }
            set { this[0, 0] = value; }
        }

        public double Y
        {
            get { return this[1, 0]; }
            set { this[1, 0] = value; }
        }

        public Vector2()
            : base(3, 1)
        {
            X = 0;
            Y = 0;
        }

        public Vector2(double x, double y)
            : base(3, 1)
        {
            X = x;
            Y = y;
        }
    }
    public class Vector3 : Matrix
    {
        public double X
        {
            get { return this[0, 0]; }
            set { this[0, 0] = value; }
        }

        public double Y
        {
            get { return this[1, 0]; }
            set { this[1, 0] = value; }
        }
        public double Z
        {
            get { return this[2, 0]; }
            set { this[2, 0] = value; }
        }
        public Vector3()
            : base(3, 1)
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Vector3(double x, double y, double z)
            : base(3, 1)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Color ToColor(Vector3 color)
        {
            int colorR = (int)(255 * color.X);
            if (colorR < 0)
                colorR = 0;
            else if (colorR > 255)
                colorR = 255;
            int colorG = (int)(255 * color.Y);
            if (colorG < 0)
                colorG = 0;
            else if (colorG > 255)
                colorG = 255;
            int colorB = (int)(255 * color.Z);
            if (colorB < 0)
                colorB = 0;
            else if (colorB > 255)
                colorB = 255;
            return Color.FromArgb(colorR, colorG, colorB);
        }

        public static Vector3 FromColor(Color color)
        {
            return new Vector3(color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }
        public static Vector3 FromMatrix(Matrix matrix)
        {
            if (matrix.cols != 1 || matrix.rows < 3)
                throw new ArgumentException("Macierz nieprawidłowa");
            return new Vector3(matrix[0, 0], matrix[1, 0], matrix[2, 0]);
        }

        public static Vector3 CrossProduct(Vector3 a, Vector3 b)
        {
            return new Vector3(
            a.Y * b.Z - a.Z * b.Y,
            -a.X * b.Z + a.Z * b.X,
            a.X * b.Y - a.Y * b.X
            );
        }

        public Vector3 Versor()
        {
            double length = this.Length;
            return new Vector3(
                 X / length,
                 Y / length,
                 Z / length);
        }
        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public static double DotProduct(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }


        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator *(double a, Vector3 b)
        {
            return new Vector3(a * b.X, a * b.Y, a * b.Z);
        }

        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3 operator /(Vector3 a, double b)
        {
            return new Vector3(a.X / b, a.Y / b, a.Z / b);
        }

        public static explicit operator Vector4(Vector3 vector)
        {
            return new Vector4(vector.X, vector.Y, vector.Z, 1);
        }
    }


    public class Vector4 : Matrix
    {
        public double X
        {
            get { return this[0, 0]; }
            set { this[0, 0] = value; }
        }

        public double Y
        {
            get { return this[1, 0]; }
            set { this[1, 0] = value; }
        }
        public double Z
        {
            get { return this[2, 0]; }
            set { this[2, 0] = value; }
        }
        public double T
        {
            get { return this[3, 0]; }
            set { this[3, 0] = value; }
        }
        public Vector4()
            : base(4, 1)
        {
            X = 0;
            Y = 0;
            Z = 0;
            T = 0;
        }

        public Vector4(double x, double y, double z, double t)
            : base(4, 1)
        {
            X = x;
            Y = y;
            Z = z;
            T = t;
        }

        public static Vector4 FromMatrix(Matrix matrix)
        {
            if (matrix.cols != 1 || matrix.rows != 4)
                throw new ArgumentException("Macierz nieprawidłowa");
            return new Vector4(matrix[0, 0], matrix[1, 0], matrix[2, 0], matrix[3, 0]);
        }

        public static explicit operator Vector3(Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector4 operator -(Vector4 a, Vector4 b)
        {
            return new Vector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.T - b.T);
        }


        public static Vector4 operator +(Vector4 a, Vector4 b)
        {
            return new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.T + b.T);
        }

        public static Vector4 operator *(double a, Vector4 b)
        {
            return new Vector4(a * b.X, a * b.Y, a * b.Z, a * b.T);
        }

        public static Vector4 operator *(Vector4 a, Vector4 b)
        {
            return new Vector4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.T * b.T);
        }

        public static Vector4 operator /(Vector4 a, double b)
        {
            return new Vector4(a.X / b, a.Y / b, a.Z / b, a.T / b);
        }
    }
}
