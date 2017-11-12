using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_Lab5
{
    public enum CameraType
    {
        Static,
        Observing,
        Following,

    }
    public class Camera : IDisposable
    {
        private Vector3 _target;
        private Vector3 _position;

        public event EventHandler PositionChanged;
        public event EventHandler TargetChanged;

        public double NearPlaneDistance { get; set; }// = 0.1;
        public double FarPlaneDistance { get; set; }// = 100;
        public double Fov { get; set; }// = 45;
        
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                this._position = value;
                if (PositionChanged != null)
                {
                    PositionChanged(this, new VectorPropertChangedEventArgs(value));
                }
            }
        }

        public Vector3 Target
        {
            get
            {
                return _target;
            }
            set
            {
                this._target = value;
                if (TargetChanged != null)
                {
                    TargetChanged(this, new VectorPropertChangedEventArgs(value));
                }
            }
        }
        public Camera(Vector3 position,Vector3 target)
        {
            Position = position;
            Target = target;
            NearPlaneDistance = 0.1;
            FarPlaneDistance = 100;
            Fov = 45;
        }


        public void Dispose()
        {
        }
    }
   
    public class ObservingCamera : Camera , IDisposable
    {

        public Mesh TargetMesh { get; set; }
        public ObservingCamera(Vector3 position,Mesh target) : base(position,target.Position)
        {
            this.TargetMesh = target;
            TargetMesh.PositionChanged += target_PositionChanged;
            
        }

        protected void target_PositionChanged(object sender, EventArgs e)
        {
            Target = TargetMesh.Position;
        }

        public void Dispose()
        {
            base.Dispose();
            TargetMesh.PositionChanged -= target_PositionChanged;
        }
    }


    public class FollowingCamera : Camera,IDisposable
    {

        public Vector3 TranslationFromTarget { get; set; }
        public Mesh TargetMesh { get; set; }

        public FollowingCamera(Vector3 translationFromTarget, Mesh target)
            : base(target.Position+translationFromTarget, target.Position)
        {
            this.TranslationFromTarget = translationFromTarget;
            this.TargetMesh = target;
            TargetMesh.PositionChanged += TargetMesh_PositionChanged;
        }

        void TargetMesh_PositionChanged(object sender, EventArgs e)
        {
            this.Position = TargetMesh.Position + TranslationFromTarget;
            this.Target = TargetMesh.Position;
        }

        public void Dispose()
        {
            base.Dispose();
            TargetMesh.PositionChanged -= TargetMesh_PositionChanged;
        }
    }
}
