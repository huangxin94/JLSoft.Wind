using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TextBox = System.Windows.Forms.TextBox;

namespace JLSoft.Wind.UserControl
{
    public partial class LoadPort2 : System.Windows.Forms.UserControl
    {

        private TextBox[] _textBoxes;
        public LoadPort2()
        {
            InitializeComponent();
            InitializeTextBoxArray();
            DisableTextBoxInput();

        }

        public void UpdateMapping(string mappingString)
        {
            if (mappingString == null || mappingString.Length != 25)
                return;

            for (int i = 0; i < 25; i++)
            {
                char state = mappingString[i];
                Color color = Color.White; // 默认白色

                switch (state)
                {
                    case '0': color = Color.Gainsboro; break;
                    case '1': color = Color.Lime; break;
                    case '2': color = Color.Red; break;
                    case '3': color = Color.Yellow; break;
                    case '4': color = Color.Blue; break;
                }

                SetTextBoxColor(i + 1, color); // 更新第i+1个文本框
            }
        }
        private void InitializeTextBoxArray()
        {
            _textBoxes = new TextBox[]
            {
            txtbox1, txtbox2, txtbox3, txtbox4, txtbox5,
            txtbox6, txtbox7, txtbox8, txtbox9, txtbox10,
            txtbox11, txtbox12, txtbox13, txtbox14, txtbox15,
            txtbox16, txtbox17, txtbox18, txtbox19, txtbox20,
            txtbox21, txtbox22, txtbox23, txtbox24, txtbox25
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



        /// <summary>
        /// 获取或设置GroupBox的标题文本
        /// </summary>
        [Browsable(true)] // 在属性窗口中可见
        [Category("Appearance")] // 在属性窗口的分类
        [Description("设置GroupBox的标题文本")]
        public string GroupBoxText
        {
            get { return groupBox1.Text; }
            set { groupBox1.Text = value; }
        }
    }
}
