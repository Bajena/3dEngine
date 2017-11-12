using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GK_Lab5
{
    public enum ShadingType
    {
        FlatModel,
        PhongModel,
        GouraudModel
    }
    public class Device
    {

        private Panel _canvas;
        private Bitmap _buffer;
        private Graphics _g;

        private int _width, _height;
        private ZBuffer _zBuffer;

        public PhongShadingModel PhongShadingModel { get; set; }

        public Vector3 UpVector = new Vector3(0, 1, 0);
        public double AspectRatio { get { return _buffer != null ? (double)_height / _width : 1; } }

        public Camera CurrentCamera { get; set; }
        public Vector3 LightPos;

        public bool DrawMesh { get; set; }
        public bool DrawNormals { get; set; }

        public ShadingType ShadingType { get; set; }

        public Panel Canvas
        {
            get
            {
                return _canvas;
            }
            set
            {
                _canvas = value;
                if (value != null)
                {
                    _buffer = new Bitmap(this.Canvas.Width, this.Canvas.Height);
                    _g = Graphics.FromImage(_buffer);
                    _width = _buffer.Width;
                    _height = _buffer.Height;
                    _zBuffer = new ZBuffer(_width, _height);
                }
            }
        }

        public Device(Panel canvas)
        {
            this.Canvas = canvas;
            canvas.Paint += Redraw;

            PhongShadingModel = new PhongShadingModel();
        }

        public void Redraw(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_buffer, 0, 0);
        }

        public void Clear(Color color)
        {
            _zBuffer.Clear();
            _g.Clear(color);

        }
        public void DrawPoint(Vector3 point, Color color)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < _width && point.Y < _height)
            {
                PutPixel((int)point.X, (int)point.Y, point.Z, color);
            }
        }

        public void DrawBline(Vertex point0, Vertex point1, Color color)
        {
            int x0 = (int)point0.ScreenPosition.X;
            int y0 = (int)point0.ScreenPosition.Y;
            int x = (int)point0.ScreenPosition.X;
            int y = (int)point0.ScreenPosition.Y;
            int x1 = (int)point1.ScreenPosition.X;
            int y1 = (int)point1.ScreenPosition.Y;

            double z0 = point0.ScreenPosition.Z;
            double z1 = point1.ScreenPosition.Z;

            var dx = Math.Abs(x1 - x);
            var dy = Math.Abs(y1 - y);
            var sx = (x < x1) ? 1 : -1;
            var sy = (y < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                float gradient = (x - x0) / (float)(x1 - x0);
                if (x1 == x0)
                {
                    gradient = 0;
                }

                var z = MathHelpers.Interpolate(z0, z1, gradient);
                DrawPoint(new Vector3(x, y, z), color);

                if ((x == x1) && (y == y1)) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }
        }

        public void PutPixel(int x, int y, double z, Color color)
        {
            if (!_zBuffer.IsCloser(x, y, z))
            {
                return;
            }

            _zBuffer.SetElement(x, y, z);
            _buffer.SetPixel(x, y, color);
        }

        void ProcessScanLineFlat(int y, Vertex pa, Vertex pb, Vertex pc, Vertex pd, Color color, double ndotl)
        {
            const double epsilon = 0.0001;
            var gradient1 = Math.Abs(pa.ScreenPosition.Y - pb.ScreenPosition.Y) > epsilon ? (y - pa.ScreenPosition.Y) / (pb.ScreenPosition.Y - pa.ScreenPosition.Y) : 1;
            var gradient2 = Math.Abs(pc.ScreenPosition.Y - pd.ScreenPosition.Y) > epsilon ? (y - pc.ScreenPosition.Y) / (pd.ScreenPosition.Y - pc.ScreenPosition.Y) : 1;

            int sx = (int)MathHelpers.Interpolate(pa.ScreenPosition.X, pb.ScreenPosition.X, gradient1);
            int ex = (int)MathHelpers.Interpolate(pc.ScreenPosition.X, pd.ScreenPosition.X, gradient2);


            // starting Z & ending Z
            double z1 = MathHelpers.Interpolate(pa.ScreenPosition.Z, pb.ScreenPosition.Z, gradient1);
            double z2 = MathHelpers.Interpolate(pc.ScreenPosition.Z, pd.ScreenPosition.Z, gradient2);

            // drawing a line from left (sx) to right (ex) 
            if (ex < sx)
            {
                Utils.Swap(ref sx, ref ex);
                Utils.Swap(ref gradient1, ref gradient2);
                Utils.Swap(ref z1, ref z2);
            }

            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);
                var z = MathHelpers.Interpolate(z1, z2, gradient);

                var lightCol = Color.FromArgb(255, (int)(color.R * ndotl), (int)(color.G * ndotl), (int)(color.B * ndotl));
                DrawPoint(new Vector3(x, y, z), lightCol);
            }
        }
        void ProcessScanLinePhong(int y, Vertex pa, Vertex pb, Vertex pc, Vertex pd, Color color)
        {
            const double epsilon = 0.0001;
            var gradient1 = Math.Abs(pa.ScreenPosition.Y - pb.ScreenPosition.Y) > epsilon ? (y - pa.ScreenPosition.Y) / (pb.ScreenPosition.Y - pa.ScreenPosition.Y) : 1;
            var gradient2 = Math.Abs(pc.ScreenPosition.Y - pd.ScreenPosition.Y) > epsilon ? (y - pc.ScreenPosition.Y) / (pd.ScreenPosition.Y - pc.ScreenPosition.Y) : 1;

            int sx = (int)MathHelpers.Interpolate(pa.ScreenPosition.X, pb.ScreenPosition.X, gradient1);
            int ex = (int)MathHelpers.Interpolate(pc.ScreenPosition.X, pd.ScreenPosition.X, gradient2);

            // starting Z & ending Z
            double z1 = MathHelpers.Interpolate(pa.ScreenPosition.Z, pb.ScreenPosition.Z, gradient1);
            double z2 = MathHelpers.Interpolate(pc.ScreenPosition.Z, pd.ScreenPosition.Z, gradient2);

            double sx_world = MathHelpers.Interpolate(pa.Position.X, pb.Position.X, gradient1);
            double ex_world = MathHelpers.Interpolate(pc.Position.X, pd.Position.X, gradient2);

            double y1_world = MathHelpers.Interpolate(pa.Position.Y, pb.Position.Y, gradient1);
            double y2_world = MathHelpers.Interpolate(pc.Position.Y, pd.Position.Y, gradient2);
            double z1_world = MathHelpers.Interpolate(pa.Position.Z, pb.Position.Z, gradient1);
            double z2_world = MathHelpers.Interpolate(pc.Position.Z, pd.Position.Z, gradient2);

            var normal1 = MathHelpers.Interpolate(pa.Normal, pb.Normal, gradient1);
            var normal2 = MathHelpers.Interpolate(pc.Normal, pd.Normal, gradient2);
            
            // drawing a line from left (sx) to right (ex) 
            if (ex < sx)
            {
                Utils.Swap(ref sx, ref ex);
                Utils.Swap(ref gradient1, ref gradient2);
                Utils.Swap(ref z1, ref z2);
                Utils.Swap(ref sx_world, ref ex_world);
                Utils.Swap(ref y1_world, ref y2_world);
                Utils.Swap(ref z1_world, ref z2_world);
                Utils.Swap(ref normal1, ref normal2);
            }

            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);
                var z = MathHelpers.Interpolate(z1, z2, gradient);

                var x_world = MathHelpers.Interpolate(sx_world, ex_world, gradient);
                var y_world = MathHelpers.Interpolate(y1_world, y2_world, gradient);
                var z_world = MathHelpers.Interpolate(z1_world, z2_world, gradient);
                var normal = MathHelpers.Interpolate(normal1, normal2, gradient);

                normal = normal.Versor();

                var lightCol = PhongShadingModel.ComputeIntensityAt(
                    new Vertex
                    {
                        Normal = normal,
                        Position = new Vector4(x_world, y_world, z_world, 1)
                    },
                    CurrentCamera,
                    color);
                
                DrawPoint(new Vector3(x, y, z), lightCol);
            }
        }

        double ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal = normal.Versor();
            lightDirection = lightDirection.Versor();

            return Math.Max(0, Vector3.DotProduct(normal, lightDirection));
        }

        public void DrawTriangleMesh(Vertex p1, Vertex p2, Vertex p3, Color color)
        {
            DrawBline(p1, p2, color);
            DrawBline(p1, p3, color);
            DrawBline(p2, p3, color);
        }
        public void DrawTrianglePhong(Vertex p1, Vertex p2, Vertex p3, Color color)
        {
            if (p1.ScreenPosition.Y > p2.ScreenPosition.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            if (p2.ScreenPosition.Y > p3.ScreenPosition.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }

            if (p1.ScreenPosition.Y > p2.ScreenPosition.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            float dP1P2, dP1P3;

            if (p2.ScreenPosition.Y - p1.ScreenPosition.Y > 0)
                dP1P2 = (float)((p2.ScreenPosition.X - p1.ScreenPosition.X) / (p2.ScreenPosition.Y - p1.ScreenPosition.Y));
            else
                dP1P2 = 0;

            if (p3.ScreenPosition.Y - p1.ScreenPosition.Y > 0)
                dP1P3 = (float)((p3.ScreenPosition.X - p1.ScreenPosition.X) / (p3.ScreenPosition.Y - p1.ScreenPosition.Y));
            else
                dP1P3 = 0;

            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.ScreenPosition.Y; y <= (int)p3.ScreenPosition.Y; y++)
                {
                    if (y < p2.ScreenPosition.Y)
                    {
                        ProcessScanLinePhong(y, p1, p3, p1, p2, color);
                    }
                    else
                    {
                        ProcessScanLinePhong(y, p1, p3, p2, p3, color);
                    }
                }
            }
            else
            {
                for (var y = (int)p1.ScreenPosition.Y; y <= (int)p3.ScreenPosition.Y; y++)
                {
                    if (y < p2.ScreenPosition.Y)
                    {
                        ProcessScanLinePhong(y, p1, p2, p1, p3, color);
                    }
                    else
                    {
                        ProcessScanLinePhong(y, p2, p3, p1, p3, color);
                    }
                }
            }
        }
        public void DrawTriangleFlat(Vertex p1, Vertex p2, Vertex p3, Color color,double ndotl)
        {
            if (p1.ScreenPosition.Y > p2.ScreenPosition.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            if (p2.ScreenPosition.Y > p3.ScreenPosition.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }

            if (p1.ScreenPosition.Y > p2.ScreenPosition.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            float dP1P2, dP1P3;

            if (p2.ScreenPosition.Y - p1.ScreenPosition.Y > 0)
                dP1P2 = (float)((p2.ScreenPosition.X - p1.ScreenPosition.X) / (p2.ScreenPosition.Y - p1.ScreenPosition.Y));
            else
                dP1P2 = 0;

            if (p3.ScreenPosition.Y - p1.ScreenPosition.Y > 0)
                dP1P3 = (float)((p3.ScreenPosition.X - p1.ScreenPosition.X) / (p3.ScreenPosition.Y - p1.ScreenPosition.Y));
            else
                dP1P3 = 0;

            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.ScreenPosition.Y; y <= (int)p3.ScreenPosition.Y; y++)
                {
                    if (y < p2.ScreenPosition.Y)
                    {
                        ProcessScanLineFlat(y, p1, p3, p1, p2, color,ndotl);
                    }
                    else
                    {
                        ProcessScanLineFlat(y, p1, p3, p2, p3, color, ndotl);
                    }
                }
            }
            else
            {
                for (var y = (int)p1.ScreenPosition.Y; y <= (int)p3.ScreenPosition.Y; y++)
                {
                    if (y < p2.ScreenPosition.Y)
                    {
                        ProcessScanLineFlat(y, p1, p2, p1, p3, color, ndotl);
                    }
                    else
                    {
                        ProcessScanLineFlat(y, p2, p3, p1, p3, color, ndotl);
                    }
                }
            }
        }

        public void DrawFaceNormals(Face face, Mesh mesh, Matrix transformMatrix, Matrix worldMatrix)
        {
            var vertexA = mesh.Vertices[face.A];
            var vertexB = mesh.Vertices[face.B];
            var vertexC = mesh.Vertices[face.C];

            var normal = face.Normal;

            var mid = (vertexA.Position + vertexB.Position + vertexC.Position) / 3;

            var mm = new Vector3(mid.X, mid.Y, mid.Z);

            var midVer = Project(new Vertex()
            {
                Normal = normal,
                Position = (Vector4)mm
            }, transformMatrix, worldMatrix);

            var moved = mm + normal;
            var midVer2 = Project(new Vertex()
            {
                Normal = normal,
                Position = (Vector4)moved
            }, transformMatrix, worldMatrix);

            DrawBline(midVer, midVer2, Color.RoyalBlue);
        }
        public void DrawCoordinateSystem(Matrix vpMatrix)
        {
            var begin = new Vector4(0, 0, 0, 1);
            var begin_proj = Project(new Vertex()
            {
                Normal = new Vector3(1, 1, 1),
                Position = begin
            }, vpMatrix, Matrix.IdentityMatrix(4, 4));
            var x = new Vector4(4, 0, 0, 1);
            var x_proj = Project(new Vertex()
            {
                Normal = new Vector3(1, 1, 1),
                Position = x
            }, vpMatrix, Matrix.IdentityMatrix(4, 4));
            var y = new Vector4(0, 4, 0, 1);
            var y_proj = Project(new Vertex()
            {
                Normal = new Vector3(1, 1, 1),
                Position = y
            }, vpMatrix, Matrix.IdentityMatrix(4, 4));
            var z = new Vector4(0, 0, 4, 1);
            var z_proj = Project(new Vertex()
            {
                Normal = new Vector3(1, 1, 1),
                Position = z
            }, vpMatrix, Matrix.IdentityMatrix(4, 4));

            DrawBline(begin_proj, x_proj, Color.Red);
            DrawBline(begin_proj, y_proj, Color.Yellow);
            DrawBline(begin_proj, z_proj, Color.Green);


        }

        public Vertex Project(Vertex coord, Matrix transMat, Matrix worldMatrix)
        {
            var worldCoords = Vector4.FromMatrix(worldMatrix * coord.Position);

            var worldNormal = Vector3.FromMatrix(worldMatrix * (Vector4)coord.Normal);

            var projectedCoord = Vector4.FromMatrix(transMat * coord.Position);

            double EPSILON = 0.000001;
            if (Math.Abs(projectedCoord.T - 0) > EPSILON)
            {
                projectedCoord.X /= projectedCoord.T;
                projectedCoord.Y /= projectedCoord.T;
                projectedCoord.Z /= projectedCoord.T;
                projectedCoord.T /= projectedCoord.T;
            }


            double x = (double)_width / 2 * (1 + projectedCoord.X);
            double y = (double)_height / 2 * (1 - projectedCoord.Y);

            var screenPosition = new Vector3(x, y, projectedCoord.Z);

            //if (screenPosition.X < 0)
            //    screenPosition.X = 0;
            //if (screenPosition.X > _width)
            //    screenPosition.X = _width;


            //if (screenPosition.Y < 0)
            //    screenPosition.Y = 0;
            //if (screenPosition.Y > _height)
            //    screenPosition.Y = _height;

            return new Vertex
                {
                    Position = worldCoords,
                    Normal = worldNormal,
                    ScreenPosition = screenPosition
                };
        }
        public Vertex Project(Vertex coord, Matrix transMat, Matrix worldMatrix, Matrix normalMatrix)
        {
            var worldNormal = Vector3.FromMatrix(normalMatrix * (Vector4)coord.Normal);

            var retVal = Project(coord, transMat, worldMatrix);
            retVal.Normal = worldNormal;

            return retVal;
        }

        public void Render(Camera camera, params Mesh[] meshes)
        {
            Clear(Color.Black);
            CurrentCamera = camera;

            var viewMatrix = Matrix.CreateLookAt(camera.Position, camera.Target, UpVector);
            var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(camera.Fov, AspectRatio, camera.NearPlaneDistance, camera.FarPlaneDistance);
            var vpMatrix = projectionMatrix * viewMatrix;

            LightPos = camera.Position;
            PhongShadingModel.Lights.Clear();
            PhongShadingModel.Lights.Add(new PointLight(LightPos, new Vector3(1, 1, 1)));

            foreach (Mesh mesh in meshes)
            {
                var normalWorldMatrix =
                    Matrix.YawPitchRollRotationMatrix(mesh.Rotation.Z, mesh.Rotation.Y, mesh.Rotation.X) * Matrix.ScaleMatrix(mesh.Scale.X, mesh.Scale.Y, mesh.Scale.Z);

                var worldMatrix =
                    Matrix.TranslationMatrix(mesh.Position) * normalWorldMatrix;

                var transformMatrix = vpMatrix * worldMatrix;

                for (int faceIndex = 0; faceIndex < mesh.Faces.Length; )
                {
                    var face = mesh.Faces[faceIndex];
                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    vertexA = Project(vertexA, transformMatrix, worldMatrix, normalWorldMatrix);
                    vertexB = Project(vertexB, transformMatrix, worldMatrix, normalWorldMatrix);
                    vertexC = Project(vertexC, transformMatrix, worldMatrix, normalWorldMatrix);

                    if (DrawNormals)
                        DrawFaceNormals(face, mesh, transformMatrix, worldMatrix);


                    //Połączone ze sprawdzeniem czy trójkąt jest widoczny
                    var ndotl = CalculateFaceFlatShadingIntensity(face, mesh, transformMatrix, worldMatrix, normalWorldMatrix,camera);

                    if (ndotl > 0)
                    {
                        if (DrawMesh)
                            DrawTriangleMesh(vertexA, vertexB, vertexC,Color.LightSkyBlue);
                        else if (ShadingType==ShadingType.PhongModel)
                            DrawTrianglePhong(vertexA, vertexB, vertexC, Color.White);
                        else if (ShadingType==ShadingType.FlatModel)
                            DrawTriangleFlat(vertexA,vertexB,vertexC,Color.White,ndotl);
                    }

                    faceIndex++;
                }

            }
            DrawCoordinateSystem(vpMatrix);

        }

        public double CalculateFaceFlatShadingIntensity(Face face, Mesh mesh, Matrix transformMatrix, Matrix worldMatrix, Matrix normalWorldMatrix, Camera camera)
        {
            var vertexA = mesh.Vertices[face.A];
            var vertexB = mesh.Vertices[face.B];
            var vertexC = mesh.Vertices[face.C];

            var normal = face.Normal;

            var mid = (vertexA.Position + vertexB.Position + vertexC.Position) / 3;

            var mm = new Vector3(mid.X, mid.Y, mid.Z);

            var midVer = Project(new Vertex()
                    {
                        Normal = normal,
                        Position = (Vector4)mm
                    }, transformMatrix, worldMatrix);

            var lookVector = ((Vector3)midVer.Position - camera.Position).Versor();
            var surfaceNormal = Vector3.FromMatrix((normalWorldMatrix * (Vector4)normal));
            double backfaceCos = Vector3.DotProduct(lookVector, surfaceNormal);
            return backfaceCos < 0 ? ComputeNDotL((Vector3)midVer.Position, surfaceNormal, LightPos) : 0;
        }

        public Vector3 CalculateSurfaceNormal(Mesh mesh, Face face)
        {
            var vertexA = mesh.Vertices[face.A];
            var vertexB = mesh.Vertices[face.B];
            var vertexC = mesh.Vertices[face.C];

            var posA = (Vector3)vertexA.Position;
            var posB = (Vector3)vertexB.Position;
            var posC = (Vector3)vertexC.Position;


            return Vector3.CrossProduct((posC - posA), (posB - posA)).Versor();
        }

        public Mesh[] LoadJsonFile(string fileName)
        {
            var meshes = new List<Mesh>();
            var data = File.ReadAllText(fileName);

            dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(data);

            for (var meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var verticesArray = jsonObject.meshes[meshIndex].positions;
                // Trójkąty
                var indicesArray = jsonObject.meshes[meshIndex].indices;

                var normalsArray = jsonObject.meshes[meshIndex].normals;

                var verticesCount = verticesArray.Count / 3;

                var facesCount = indicesArray.Count / 3;

                var mesh = new Mesh(jsonObject.meshes[meshIndex].name.Value, verticesCount, facesCount);

                for (var index = 0; index < 3 * verticesCount; index += 3)
                {
                    var x = (float)verticesArray[index].Value;
                    var y = (float)verticesArray[index + 1].Value;
                    var z = (float)verticesArray[index + 2].Value;

                    //normalne trójkątów
                    var nx = (float)normalsArray[index].Value;
                    var ny = (float)normalsArray[index + 1].Value;
                    var nz = (float)normalsArray[index + 2].Value;

                    mesh.Vertices[index / 3] = new Vertex { Position = new Vector4(x, y, z, 1), Normal = new Vector3(nx, ny, nz) };
                }


                var position = jsonObject.meshes[meshIndex].position;
                var scaling = jsonObject.meshes[meshIndex].scaling;
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);

                mesh.Scale = new Vector3((float)scaling[0].Value, (float)scaling[1].Value, (float)scaling[2].Value);

                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)indicesArray[index * 3].Value;
                    var b = (int)indicesArray[index * 3 + 1].Value;
                    var c = (int)indicesArray[index * 3 + 2].Value;
                    mesh.Faces[index] = new Face { A = a, B = b, C = c };
                    mesh.Faces[index].Normal = CalculateSurfaceNormal(mesh, mesh.Faces[index]);
                }

                for (int i = 0; i < mesh.Vertices.Length; i++)
                {
                    mesh.Vertices[i].Normal = mesh.CalculateVertexNormal(i).Versor();
                }
                meshes.Add(mesh);
            }
            return meshes.ToArray();
        }
    }
}