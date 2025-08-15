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
    public partial class SonForm : Form
    {
        public SonForm()
        {
            InitializeComponent();
        }


        public SonForm(string column3Data, string column4Data, string column5Data, string column6Data)
        {
            

            InitializeComponent();
            label1.Text = "这是产品：" + column3Data + "的生产流程配置页面";
        }
    }
}
