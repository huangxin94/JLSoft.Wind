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
    public partial class WaferSelectionForm : Form
    {
        public string SelectedWaferType { get; private set; }
        public string SelectedSize { get; private set; }

        private RadioButton rbWafer;
        private RadioButton rbSquare;
        private RadioButton rb8Inch;
        private RadioButton rb12Inch;
        private Button btnOK;
        private Button btnCancel;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label label1;
        private Panel panel1;

        public WaferSelectionForm(string currentWaferType, string currentSize)
        {
            InitializeComponent();
            if (currentWaferType == "Wafer")
                rbWafer.Checked = true;
            else if (currentWaferType == "Square")
                rbSquare.Checked = true;

            if (currentSize == "8英寸")
                rb8Inch.Checked = true;
            else if (currentSize == "12英寸")
                rb12Inch.Checked = true;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            // 获取选择的Wafer类型
            if (rbWafer.Checked)
                SelectedWaferType = "Wafer";
            else if (rbSquare.Checked)
                SelectedWaferType = "Square";

            // 获取选择的尺寸
            if (rb8Inch.Checked)
                SelectedSize = "8英寸";
            else if (rb12Inch.Checked)
                SelectedSize = "12英寸";

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
