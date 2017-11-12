using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GK_Lab5
{
    public class FpsCounter : Timer
    {
        private int previousSecondFps = 0;
        private int frames = 0;
        private int milisecond;

        public FpsCounter()
        {
            this.Interval = 1000;
            milisecond = DateTime.Now.Millisecond;
            this.Tick += FpsCounter_Tick;
        }

        void FpsCounter_Tick(object sender, EventArgs e)
        {
            previousSecondFps = frames;
            frames = 0;
        }

        public void IncreaseFrames()
        {
            frames++;
        }

        public double GetFps()
        {
            return previousSecondFps;
        }
    }
}
