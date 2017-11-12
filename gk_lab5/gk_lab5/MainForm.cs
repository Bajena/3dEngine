using System;
using System.Windows.Forms;

namespace GK_Lab5
{
    public partial class MainForm : Form
    {


        private enum MeshAction
        {
            Static,
            Rotating
        }

        private Camera Camera
        {
            get
            {
                return _camera;
            }
            set
            {
                if (value!=null)
                {
                    _camera = value;
                    _camera.TargetChanged += Camera_TargetChanged;
                    _camera.PositionChanged += Camera_PositionChanged;
                }
            }
        }
        private MeshAction MeshActionType { get; set; }

        private double _r = 3, _fi = 0;

        private Device _device;
        private Mesh _centerMesh;
        private Camera _camera;
        private Mesh _mainMesh;

        FpsCounter _fpsCounter = new FpsCounter();

        public MainForm()
        {
            InitializeComponent();
            SetupInitialCamera();
            SetupMeshes();
        }

        private void SetupInitialCamera()
        {
            double n = 1;
            double f = 100;
            double fov = 45;

            Vector3 cameraPosition;
            Vector3 cameraTarget;

            //Set camera
            cameraPosition = new Vector3(3, 9.30, 9.30);
            cameraTarget = new Vector3(0, 0, 0);

            Camera = new Camera(cameraPosition, cameraTarget)
            {
                NearPlaneDistance = n,
                FarPlaneDistance = f,
                Fov = fov,
            };

            //Setup rendering object
            _device = new Device(panel1);

            SetCameraPositionControlsValues(Camera.Position);
            SetCameraTargetControlsValues(Camera.Target);
        }

        private void SetupMeshes()
        {
            this.MeshActionType = MeshAction.Static;
            //Load meshes from Json
            var mainMeshFile = _device.LoadJsonFile("sphere.babylon");
            var centerMeshFile = _device.LoadJsonFile("cube.babylon");

            _mainMesh = mainMeshFile[0];
            _centerMesh = centerMeshFile[0];
            _mainMesh.PositionChanged += _mainMesh_PositionChanged;
            _mainMesh.RotationChanged += _mainMesh_RotationChanged;
            _mainMesh.ScaleChanged += _mainMesh_ScaleChanged;

            _mainMesh.Position = new Vector3(2, 0, 2);
            _mainMesh.Rotation = new Vector3(0, 0, 0);
            _centerMesh.Scale = new Vector3(0.5, 0.5, 0.5);

        }

        void _mainMesh_ScaleChanged(object sender, EventArgs e)
        {
            var mesh = (sender as Mesh);
            SetObjectScaleControlsValues(mesh.Scale);
        }

        void _mainMesh_RotationChanged(object sender, EventArgs e)
        {
            var mesh = (sender as Mesh);
            SetObjectRotationControlsValues(mesh.Rotation);
        }

        void _mainMesh_PositionChanged(object sender, EventArgs e)
        {
            var mesh = (sender as Mesh);
            SetObjectPositionControlsValues(mesh.Position);
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!_fpsCounter.Enabled)
            {
                _fpsCounter.Enabled = true;
            }

            switch (MeshActionType)
            {
                case MeshAction.Static:
                    break;
                case MeshAction.Rotating:
                    _fi = (_fi % 360) + 4;
                    var angle = Math.PI * _fi / 180;
                    _mainMesh.Position = new Vector3(_r * Math.Sin(angle), _mainMesh.Position.Y, _r * Math.Cos(angle));
                    _mainMesh.Rotation.Y = _fi;
                    break;
            }

            _device.Render(Camera, _mainMesh, _centerMesh);
            panel1.Refresh();
            _fpsCounter.IncreaseFrames();
            this.Text = "FPS: " + _fpsCounter.GetFps().ToString() + " Resolution: " + panel1.Width + " x " + panel1.Height;
        }
        private void panel1_SizeChanged(object sender, EventArgs e)
        {

            //_device.Canvas = panel1;
        }

        //Shading
        private void checkBox_drawMesh_CheckedChanged(object sender, EventArgs e)
        {
            _device.DrawMesh = !_device.DrawMesh;
        }
        private void checkBox_drawFaceNormals_CheckedChanged(object sender, EventArgs e)
        {
            _device.DrawNormals = !_device.DrawNormals;
        }
        private void radioButton_flatShading_CheckedChanged(object sender, EventArgs e)
        {
            _device.ShadingType = ShadingType.FlatModel;
            SetPhongModelControlsEnabled(false);
        }
        private void radioButton_phongShading_CheckedChanged(object sender, EventArgs e)
        {
            _device.ShadingType = ShadingType.PhongModel;
            SetPhongModelControlsEnabled(true);
        }
        private void textBox_ambient_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (!success || value < 0 || value > 1)
                {
                    MessageBox.Show("Nieprawidłowa wartość!");
                    return;
                }

                _device.PhongShadingModel.AmbientReflectance = new Vector3(value, value, value);

            }
        }
        private void textBox_diffusion_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (!success || value < 0 || value > 1)
                {
                    MessageBox.Show("Nieprawidłowa wartość!");
                    return;
                }

                _device.PhongShadingModel.DiffusionReflectance = new Vector3(value, value, value);

            }
        }
        private void textBox_specular_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (!success || value < 0 || value > 1)
                {
                    MessageBox.Show("Nieprawidłowa wartość!");
                    return;
                }

                _device.PhongShadingModel.SpecularReflectance = new Vector3(value, value, value);

            }
        }
        private void textBox_m_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (!success)
                {
                    MessageBox.Show("Nieprawidłowa wartość!");
                    return;
                }

                _device.PhongShadingModel.M = (int)value;

            }
        }

        private void SetPhongModelControlsEnabled(bool state)
        {
            textBox_ambient.Enabled = state;
            textBox_diffusion.Enabled = state;
            textBox_specular.Enabled = state;
            textBox_m.Enabled = state;
        }


        //Camera
        private void radioButton_cameraObserving_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_cameraObserving.Checked)
            {
                SetCameraPositionControlsEnabled(true);
                SetCameraTargetControlsEnabled(false);
                this.Camera = new ObservingCamera(Camera.Position, _mainMesh);
            }
            else
            {
                (this.Camera as ObservingCamera).Dispose();
            }
        }
        private void radioButton_cameraStatic_CheckedChanged(object sender, EventArgs e)
        {
            SetCameraPositionControlsEnabled(true);
            SetCameraTargetControlsEnabled(true);
            this.Camera = new Camera(Camera.Position, Camera.Target);
        }
        private void radioButton_cameraFollowing_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_cameraFollowing.Checked)
            {
                SetCameraPositionControlsEnabled(false);
                SetCameraTargetControlsEnabled(false);

                this.Camera = new FollowingCamera(new Vector3(0, 10, 0.1), _mainMesh);
            }
            else
            {
                (this.Camera as FollowingCamera).Dispose();
            }
        }

        private void textBox_cameraPositionX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    Camera.Position.X = value;
                    trackBar_cameraPositionX.Value = (int)(10 * value);
                }
                else if (Camera.Position != null)
                {
                    //textBox.Text = String.Format("{0:0.00}", Camera.Position.X);
                }
            }
        }
        private void textBox_cameraPositionY_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    Camera.Position.Y = value;
                    trackBar_cameraPositionY.Value = (int)(10 * value);
                }
                else if (Camera.Position != null)
                {
                    //textBox.Text = String.Format("{0:0.00}", Camera.Position.X);
                }
            }
        }
        private void textBox_cameraPositionZ_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    Camera.Position.Z = value;
                    trackBar_cameraPositionZ.Value = (int)(10 * value);
                }
                else if (Camera.Position != null)
                {
                    //textBox.Text = String.Format("{0:0.00}", Camera.Position.X);
                }
            }
        }
        private void textBox_cameraTargetX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    Camera.Target.X = value;
                    trackBar_cameraTargetX.Value = (int)(10 * value);
                }
                else if (Camera.Target != null)
                {
                    //textBox.Text = String.Format("{0:0.00}", Camera.Position.X);
                }
            }
        }
        private void textBox_cameraTargetY_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    Camera.Target.Y = value;
                    trackBar_cameraTargetY.Value = (int)(10 * value);
                }
                else if (Camera.Target != null)
                {
                    //textBox.Text = String.Format("{0:0.00}", Camera.Position.X);
                }
            }
        }
        private void textBox_cameraTargetZ_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    Camera.Target.Z = value;
                    trackBar_cameraTargetZ.Value = (int)(10 * value);
                }
                else if (Camera.Target != null)
                {
                    //textBox.Text = String.Format("{0:0.00}", Camera.Position.X);
                }
            }
        }

        private void trackBar_cameraPositionX_Scroll(object sender, EventArgs e)
        {
            double pos = trackBar_cameraPositionX.Value / 10.0;
            Camera.Position = new Vector3(pos, Camera.Position.Y, Camera.Position.Z);
            textBox_cameraPositionX.Text = String.Format("{0:0.00}", pos);
        }
        private void trackBar_cameraPositionY_Scroll(object sender, EventArgs e)
        {
            double pos = trackBar_cameraPositionY.Value / 10.0;
            Camera.Position = new Vector3(Camera.Position.X, pos, Camera.Position.Z);
            textBox_cameraPositionY.Text = String.Format("{0:0.00}", pos);
        }
        private void trackBar_cameraPositionZ_Scroll(object sender, EventArgs e)
        {
            double pos = trackBar_cameraPositionZ.Value / 10.0;
            Camera.Position = new Vector3(Camera.Position.X, Camera.Position.Y, pos);
            textBox_cameraPositionZ.Text = String.Format("{0:0.00}", pos);
        }
        private void trackBar_cameraTargetX_Scroll(object sender, EventArgs e)
        {

            double pos = trackBar_cameraTargetX.Value / 10.0;
            Camera.Target = new Vector3(pos, Camera.Target.Y, Camera.Target.Z);
            textBox_cameraTargetX.Text = String.Format("{0:0.00}", pos);
        }
        private void trackBar_cameraTargetY_Scroll(object sender, EventArgs e)
        {
            double pos = trackBar_cameraTargetY.Value / 10.0;
            Camera.Target = new Vector3(Camera.Target.X, pos, Camera.Target.Z);
            textBox_cameraTargetY.Text = String.Format("{0:0.00}", pos);
        }
        private void trackBar_cameraTargetZ_Scroll(object sender, EventArgs e)
        {

            double pos = trackBar_cameraTargetZ.Value / 10.0;
            Camera.Target = new Vector3(Camera.Target.X, Camera.Target.Y, pos);
            textBox_cameraTargetZ.Text = String.Format("{0:0.00}", pos);
        }

        void Camera_TargetChanged(object sender, EventArgs e)
        {
            var camera = (sender as Camera);
            SetCameraTargetControlsValues(camera.Target);
        }
        void Camera_PositionChanged(object sender, EventArgs e)
        {
            var camera = (sender as Camera);
            SetCameraPositionControlsValues(camera.Position);
        }

        void SetCameraPositionControlsValues(Vector3 value)
        {
            textBox_cameraPositionX.Text = String.Format("{0:0.00}", value.X); ;
            trackBar_cameraPositionX.Value = (int)(value.X * 10);
            textBox_cameraPositionY.Text = String.Format("{0:0.00}", value.Y); ;
            trackBar_cameraPositionY.Value = (int)(value.Y * 10);
            textBox_cameraPositionZ.Text = String.Format("{0:0.00}", value.Z); ;
            trackBar_cameraPositionZ.Value = (int)(value.Z * 10);
        }
        void SetCameraTargetControlsValues(Vector3 value)
        {
            textBox_cameraTargetX.Text = String.Format("{0:0.00}", value.X); ;
            trackBar_cameraTargetX.Value = (int)(value.X * 10);
            textBox_cameraTargetY.Text = String.Format("{0:0.00}", value.Y); ;
            trackBar_cameraTargetY.Value = (int)(value.Y * 10);
            textBox_cameraTargetZ.Text = String.Format("{0:0.00}", value.Z); ;
            trackBar_cameraTargetZ.Value = (int)(value.Z * 10);
        }
        private void SetCameraPositionControlsEnabled(bool state)
        {
            textBox_cameraPositionX.Enabled = state;
            trackBar_cameraPositionX.Enabled = state;
            textBox_cameraPositionY.Enabled = state;
            trackBar_cameraPositionY.Enabled = state;
            textBox_cameraPositionZ.Enabled = state;
            trackBar_cameraPositionZ.Enabled = state;
        }
        private void SetCameraTargetControlsEnabled(bool state)
        {
            textBox_cameraTargetX.Enabled = state;
            trackBar_cameraTargetX.Enabled = state;
            textBox_cameraTargetY.Enabled = state;
            trackBar_cameraTargetY.Enabled = state;
            textBox_cameraTargetZ.Enabled = state;
            trackBar_cameraTargetZ.Enabled = state;
        }

        //Object
        private void radioButton_objectStatic_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_objectStatic.Checked)
            {
                SetObjectPositionControlsEnabled(true);
                this.MeshActionType = MeshAction.Static;

            }
        }

        private void radioButton_objectRotating_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_objectRotating.Checked)
            {
                SetObjectPositionControlsEnabled(false);

                this.MeshActionType = MeshAction.Rotating;
            }
        }

        private void textBox_objectPositionX_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    _mainMesh.Position.X = value;
                    trackBar_objectPositionX.Value = (int)(10 * value);
                }
            }
        }
        private void textBox_objectPositionY_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    _mainMesh.Position.Y = value;
                    trackBar_objectPositionY.Value = (int)(10 * value);
                }
            }
        }
        private void textBox_objectPositionZ_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    _mainMesh.Position.Z = value;
                    trackBar_objectPositionZ.Value = (int)(10 * value);
                }
            }
        }

        private void textBox_objectRotationX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    _mainMesh.Rotation.X = value;
                    trackBar_objectRotationX.Value = (int)(value);
                }
            }
        }
        private void textBox_objectRotationY_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    _mainMesh.Rotation.Y = value;
                    trackBar_objectRotationY.Value = (int)(value);
                }
            }
        }
        private void textBox_objectRotationZ_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (success)
                {
                    _mainMesh.Rotation.Z = value;
                    trackBar_objectRotationZ.Value = (int)(value);
                }
            }
        }

        private void textBox_objectScale_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (sender as TextBox);
                double value;
                var text = textBox.Text;
                bool success = double.TryParse(text, out value);

                if (value <= 0)
                {
                    MessageBox.Show("Wartość nieprawidłowa");
                    return;
                }

                if (success)
                {
                    _mainMesh.Scale.X = value;
                    _mainMesh.Scale.Y = value;
                    _mainMesh.Scale.Z = value;
                    trackBar_objectScale.Value = (int)(100 * value);
                }
            }
        }

        private void trackBar_objectPositionX_Scroll(object sender, EventArgs e)
        {
            double pos = trackBar_objectPositionX.Value / 10.0;
            _mainMesh.Position = new Vector3(pos, _mainMesh.Position.Y, _mainMesh.Position.Z);
            textBox_objectPositionX.Text = String.Format("{0:0.00}", pos);
        }
        private void trackBar_objectPositionY_Scroll(object sender, EventArgs e)
        {
            double pos = trackBar_objectPositionY.Value / 10.0;
            _mainMesh.Position = new Vector3(_mainMesh.Position.X, pos, _mainMesh.Position.Z);
            textBox_objectPositionY.Text = String.Format("{0:0.00}", pos);
        }
        private void trackBar_objectPositionZ_Scroll(object sender, EventArgs e)
        {

            double pos = trackBar_objectPositionZ.Value / 10.0;
            _mainMesh.Position = new Vector3(_mainMesh.Position.X, _mainMesh.Position.Y, pos);
            textBox_objectPositionZ.Text = String.Format("{0:0.00}", pos);
        }

        private void trackBar_objectRotationX_Scroll(object sender, EventArgs e)
        {
            double pos = trackBar_objectRotationX.Value;
            _mainMesh.Rotation = new Vector3(pos, _mainMesh.Rotation.Y, _mainMesh.Rotation.Z);
            textBox_objectRotationX.Text = String.Format("{0:0.00}", pos);
        }
        private void trackBar_objectRotationY_Scroll(object sender, EventArgs e)
        {
            double pos = trackBar_objectRotationY.Value;
            _mainMesh.Rotation = new Vector3(_mainMesh.Rotation.X, pos, _mainMesh.Rotation.Z);
            textBox_objectRotationY.Text = String.Format("{0:0.00}", pos);
        }
        private void trackBar_objectRotationZ_Scroll(object sender, EventArgs e)
        {
            double pos = trackBar_objectRotationZ.Value;
            _mainMesh.Rotation = new Vector3(_mainMesh.Rotation.X, _mainMesh.Rotation.Y, pos);
            textBox_objectRotationZ.Text = String.Format("{0:0.00}", pos);
        }

        private void trackBar_objectScale_Scroll(object sender, EventArgs e)
        {
            double pos = (double)trackBar_objectScale.Value / 100;
            _mainMesh.Scale = new Vector3(pos, pos, pos);
            textBox_objectScale.Text = String.Format("{0:0.00}", pos);
        }

        void SetObjectPositionControlsValues(Vector3 value)
        {
            textBox_objectPositionX.Text = String.Format("{0:0.00}", value.X); ;
            trackBar_objectPositionX.Value = (int)(value.X * 10);
            textBox_objectPositionY.Text = String.Format("{0:0.00}", value.Y); ;
            trackBar_objectPositionY.Value = (int)(value.Y * 10);
            textBox_objectPositionZ.Text = String.Format("{0:0.00}", value.Z); ;
            trackBar_objectPositionZ.Value = (int)(value.Z * 10);
        }
        void SetObjectRotationControlsValues(Vector3 value)
        {
            textBox_objectRotationX.Text = String.Format("{0:0.00}", value.X); ;
            trackBar_objectRotationX.Value = (int)(value.X);
            textBox_objectRotationY.Text = String.Format("{0:0.00}", value.Y); ;
            trackBar_objectRotationY.Value = (int)(value.Y);
            textBox_objectRotationZ.Text = String.Format("{0:0.00}", value.Z); ;
            trackBar_objectRotationZ.Value = (int)(value.Z);
        }
        private void SetObjectScaleControlsValues(Vector3 value)
        {
            textBox_objectScale.Text = String.Format("{0:0.00}", value.X);
            trackBar_objectScale.Value = (int)(value.X * 100);
        }

        private void SetObjectPositionControlsEnabled(bool state)
        {
            textBox_objectPositionX.Enabled = state;
            trackBar_objectPositionX.Enabled = state;
            textBox_objectPositionY.Enabled = state;
            trackBar_objectPositionY.Enabled = state;
            textBox_objectPositionZ.Enabled = state;
            trackBar_objectPositionZ.Enabled = state;
        }
        private void SetObjectRotationControlsEnabled(bool state)
        {
            textBox_objectRotationX.Enabled = state;
            trackBar_objectRotationX.Enabled = state;
            textBox_objectRotationY.Enabled = state;
            trackBar_objectRotationY.Enabled = state;
            textBox_objectRotationZ.Enabled = state;
            trackBar_objectRotationZ.Enabled = state;
        }
        private void SetObjectScaleControlsEnabled(bool state)
        {
            textBox_objectScale.Enabled = state;
            trackBar_objectScale.Enabled = state;
        }

    }
}
