using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_Lab5
{
    public class VectorPropertChangedEventArgs : EventArgs
    {
        public Vector3 Data { get; set; }

        public VectorPropertChangedEventArgs(Vector3 data)
        {
            this.Data = data;
        }
    }
    public class Face
    {
        public Vector3 Normal { get; set; }

        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
    }
    public class Mesh
    {
        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale;

        public event EventHandler PositionChanged;
        public event EventHandler RotationChanged;
        public event EventHandler ScaleChanged;

        public string Name { get; set; }
        public Vertex[] Vertices { get; private set; }
        public Face[] Faces { get; set; }
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                this._position = value;
                if (PositionChanged!=null)
                {
                    PositionChanged(this, new VectorPropertChangedEventArgs(value));
                }
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                this._rotation = value;
                if (RotationChanged != null)
                {
                    RotationChanged(this, new VectorPropertChangedEventArgs(value));
                }
            }
        }

        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                this._scale = value;
                if (ScaleChanged != null)
                {
                    ScaleChanged(this, new VectorPropertChangedEventArgs(value));
                }
            }
        }

        public Mesh(string name, int verticesCount, int facesCount)
        {
            Vertices = new Vertex[verticesCount];
            Faces = new Face[facesCount];
            Name = name;

            Position = new Vector3(0, 0, 0);
            Rotation = new Vector3(0, 0, 0);
            Scale = new Vector3(1,1,1);
        }

        public Vector3 CalculateVertexNormal(int vertexIndex)
        {
            List<Face> facesContainingVertex = new List<Face>();
            Vector3 vertexNormalSum = new Vector3(0, 0, 0);
            foreach (var face in Faces)
            {
                if (face.A == vertexIndex || face.B == vertexIndex || face.C == vertexIndex)
                {
                    facesContainingVertex.Add(face);
                    vertexNormalSum += face.Normal;
                }
            }

            return vertexNormalSum / facesContainingVertex.Count;

        }

    }
}
