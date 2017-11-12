using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GK_Lab5
{
    class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            
            this.SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint,
                true);
        }
    }
}
