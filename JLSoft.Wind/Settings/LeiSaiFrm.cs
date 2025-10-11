using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Services;
using csIOC0640;

namespace JLSoft.Wind.Settings
{
    public partial class LeiSaiFrm : Form
    {
        private System.Windows.Forms.Timer refreshTimer;
        public LeiSaiFrm()
        {
            InitializeComponent();
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 100;
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateIOData();
        }

        private void UpdateIOData()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateIOData));
                return;
            }

            // 调用 LeisaiIO 的方法来读取输入和输出状态
            LeisaiIO.ReadInputState();
            LeisaiIO.ReadOutputState();

            // 更新输入点状态
            UpdateInputButtons();

            // 更新输出点状态
            UpdateOutputButtons();
        }

        private void UpdateInputButtons()
        {
            // 使用 LeisaiIO 的静态属性来更新输入按钮状态
            UpdateButtonState(Input1, LeisaiIO.EMO);
            UpdateButtonState(Input2, LeisaiIO.Robot_Ready);
            UpdateButtonState(Input3, LeisaiIO.Robot_Run);
            UpdateButtonState(Input4, LeisaiIO.Robot_Fault);
            UpdateButtonState(Input5, LeisaiIO.Robot_R_Vac);
            UpdateButtonState(Input6, LeisaiIO.P1_Wafer_8);
            UpdateButtonState(Input7, LeisaiIO.P1_Wafer_12);
            UpdateButtonState(Input8, LeisaiIO.P1_EMO);
            UpdateButtonState(Input9, LeisaiIO.P1_Red_Btn);
            UpdateButtonState(Input10, LeisaiIO.P1_Yellow_Btn);
            UpdateButtonState(Input11, LeisaiIO.P1_Green_Btn);
            UpdateButtonState(Input12, LeisaiIO.P1_Door);
            UpdateButtonState(Input13, LeisaiIO.X1_Door);
            UpdateButtonState(Input14, LeisaiIO.Aligner_Door);
            UpdateButtonState(Input15, LeisaiIO.X1_Wafer_8);
            UpdateButtonState(Input16, LeisaiIO.X1_Wafer_12);
            UpdateButtonState(Input17, LeisaiIO.X1_EMO);
            UpdateButtonState(Input18, LeisaiIO.X1_Red_Btn);
            UpdateButtonState(Input19, LeisaiIO.X1_Green_Btn);
            UpdateButtonState(Input20, LeisaiIO.CST_Temp1_8_1);
            UpdateButtonState(Input21, LeisaiIO.CST_Temp1_8_2);
            UpdateButtonState(Input22, LeisaiIO.CST_Temp1_12_1);
            UpdateButtonState(Input23, LeisaiIO.CST_Temp1_12_2);
            UpdateButtonState(Input24, LeisaiIO.CST_Temp2_8_1);
            UpdateButtonState(Input25, LeisaiIO.CST_Temp2_8_2);
            UpdateButtonState(Input26, LeisaiIO.CST_Temp2_12_1);
            UpdateButtonState(Input27, LeisaiIO.CST_Temp2_12_2);
            UpdateButtonState(Input28, LeisaiIO.CST_Temp3_8_1);
            UpdateButtonState(Input29, LeisaiIO.CST_Temp3_8_2);
            UpdateButtonState(Input30, LeisaiIO.CST_Temp3_12_1);
            UpdateButtonState(Input31, LeisaiIO.CST_Temp3_12_2);
            UpdateButtonState(Input32, LeisaiIO.Axis_X_Seftey_S);
        }

        private void UpdateOutputButtons()
        {
            // 直接读取输出点状态并更新按钮
            for (int i = 1; i <= 26; i++)
            {
                var outputButton = this.Controls.Find($"Output{i}", true).FirstOrDefault() as Button;
                if (outputButton != null)
                {
                    ushort bitno = (ushort)i;
                    int state = IOC0640.ioc_read_outbit(0, bitno);
                    outputButton.BackColor = state == 1 ? Color.Green : Color.Red;
                }
            }
        }

        private void UpdateButtonState(Button button, bool state)
        {
            if (button != null)
            {
                button.BackColor = state ? Color.Green : Color.Red;
            }
        }

        private void LeiSaiFrm_Load(object sender, EventArgs e)
        {
            refreshTimer.Start();
        }

        private void LeiSaiFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshTimer.Stop();
            refreshTimer.Dispose();
        }
    }
}
