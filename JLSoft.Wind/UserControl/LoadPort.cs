using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JLSoft.Wind.UserControl
{
    public partial class LoadPort : System.Windows.Forms.UserControl
    {

        private TextBox[] _textBoxes;
        public LoadPort()
        {
            InitializeComponent();
            InitializeTextBoxArray();
            DisableTextBoxInput();

        }
        private void InitializeTextBoxArray()
        {
            _textBoxes = new TextBox[]
            {
            txtbox1, txtbox2, txtbox3, txtbox4, txtbox5,
            txtbox6, txtbox7, txtbox8, txtbox9, txtbox10,
            txtbox11, txtbox12, txtbox13, txtbox14, txtbox15,
            txtbox16, txtbox17, txtbox18, txtbox19, txtbox20,
            textBox21, txtbox22, txtbox23, txtbox24, txtbox25
            };
        }

        // 禁用所有文本框输入
        private void DisableTextBoxInput()
        {
            foreach (var textBox in _textBoxes)
            {
                textBox.ReadOnly = true;      // 禁用输入
                textBox.TabStop = false;      // 移除Tab键焦点
            }
        }

        // 公共方法：设置指定文本框背景色
        public void SetTextBoxColor(int index, Color color)
        {
            if (index >= 1 && index <= 25)
            {
                _textBoxes[index - 1].BackColor = color;
            }
        }

        // 公共方法：设置所有文本框背景色
        public void SetAllTextBoxColors(Color color)
        {
            foreach (var textBox in _textBoxes)
            {
                textBox.BackColor = color;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
