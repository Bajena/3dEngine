using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_Lab5
{
    public interface ILight
    {
        Vector3 GetLightningSourceVector(Vertex vertex);
        Vector3 GetIntensityAt(Vertex vertex);
    }

    public class PointLight : ILight
    {
        private Vector3 _position;
        private Vector3 _intensity;
        public PointLight(Vector3 position,Vector3 intensity)
        {
            _position = position;
            _intensity = intensity;
        }

        public Vector3 GetLightningSourceVector(Vertex vertex)
        {
            return (_position - (Vector3)vertex.Position).Versor();
        }

        public Vector3 GetIntensityAt(Vertex vertex)
        {
            //var distance = ((Vector3) vertex.Position - _position).Length/6;
            //if (distance != 0)
                return _intensity;/// (distance);
            //else return new Vector3(0,0,0);
        }
    }
}
