using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JLSoft.Wind.Settings
{
    public partial class DeviceStateFrm : Form
    {
        public DeviceStateFrm()
        {
            InitializeComponent();
        }

        public DeviceStateFrm(string name)
        {
            InitializeComponent();
            textBox1.Text = name;
        }
    }
}
