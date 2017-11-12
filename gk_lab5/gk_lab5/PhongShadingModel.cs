using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_Lab5
{
    public class PhongShadingModel
    {

        //Ia
        public Vector3 AmbientIntensity { get; set; }

        //ka
        public Vector3 AmbientReflectance { get; set; }

        //kd
        public Vector3 DiffusionReflectance { get; set; }

        //ks
        public Vector3 SpecularReflectance { get; set; }

        public double M { get; set; }

        public List<ILight> Lights { get; set; }

        public PhongShadingModel()
        {
            AmbientIntensity = new Vector3(1, 1, 1);
            AmbientReflectance = new Vector3(0.1, 0.1, 0.1);
            DiffusionReflectance = new Vector3(0.4,0.4,0.4);
            SpecularReflectance = new Vector3(0.4,0.4,0.4);

            M = 3;

            Lights = new List<ILight>();
        }

        public Color ComputeIntensityAt(Vertex vertex, Camera camera, Color baseColor)
        {
            Vector3 v = (camera.Position - (Vector3)vertex.Position).Versor();
            Vector3 intensity = AmbientReflectance * AmbientIntensity;

            foreach (var light in Lights)
            {
                var li = light.GetLightningSourceVector(vertex);
                var normal_lightningAngle = Vector3.DotProduct(vertex.Normal, li);

                var ri = (2*(normal_lightningAngle)*vertex.Normal - li).Versor();

                var Ii = light.GetIntensityAt(vertex);

                intensity = intensity + 
                    (
                    Math.Max(0, normal_lightningAngle) * DiffusionReflectance + 
                    Math.Max(0, Math.Pow(Vector3.DotProduct(v, ri), M)) * SpecularReflectance
                    ) 
                    * Ii;
            }

            return ScaleColor(intensity, baseColor);

        }

        Color ScaleColor(Vector3 intensity,Color color)
        {
            var r = (int) (intensity.X*color.R);
            if (r > 255) r = 255;
            else if (r < 0) r = 0;

            var g = (int)(intensity.Y * color.G);
            if (g > 255) g = 255;
            else if (g < 0) g = 0;


            var b = (int)(intensity.Z * color.B);
            if (b > 255) b = 255;
            else if (b < 0) b = 0;

            return Color.FromArgb(255,r, g,b);
        }

    }
}
