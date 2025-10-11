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
using static JLSoft.Wind.Services.Leisai_Axis;

namespace JLSoft.Wind.Settings
{
    public partial class AxisFrm : Form
    {

        private System.Windows.Forms.Timer refreshTimer;
        public AxisFrm()
        {
            InitializeComponent();
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 100;
            refreshTimer.Tick += RefreshTimer_Tick;
        }
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateAxisData();
        }

        private void UpdateAxisData()
        {
            // 确保跨线程安全更新UI
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateAxisData));
                return;
            }

            // 更新轴状态显示
            uiTextBox1.Text = Leisai_Axis.EtherCat_Status;
            uiTextBox1.BackColor = Leisai_Axis.EtherCat_Status.Contains("Error") ? Color.Red : Color.Lime;

            uiXtype.Text = Leisai_Axis.X_Axis_Status;
            uiYtype.Text = Leisai_Axis.Y_Axis_Status;
            uiZtype.Text = Leisai_Axis.Z_Axis_Status;

            // 更新轴位置显示
            uiX.Text = Axis_INP.X_pos.ToString("F3");
            uiY.Text = Axis_INP.Y_pos.ToString("F3");
            uiZ.Text = Axis_INP.Z_pos.ToString("F3");

            // 更新按钮状态（根据轴是否到位）
            UpdateButtonState(uiXtype_but, Leisai_Axis.Leisai_CheckDone((ushort)Leisai_Axis.AxisName.X));
            UpdateButtonState(uiYtype_but, Leisai_Axis.Leisai_CheckDone((ushort)Leisai_Axis.AxisName.Y));
            UpdateButtonState(uiZtype_but, Leisai_Axis.Leisai_CheckDone((ushort)Leisai_Axis.AxisName.Z));
        }
        private void AxisFrm_Load(object sender, EventArgs e)
        {
            refreshTimer.Start(); // 窗体加载时启动定时器
        }

        private void AxisFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshTimer.Stop(); // 窗体关闭时停止定时器
            refreshTimer.Dispose();
        }
        private void UpdateButtonState(Button button, short state)
        {
            if (state == 0)
            {
                button.BackColor = Color.Green;
                button.Text = "Run";
            }
            else
            {
                button.BackColor = Color.Yellow;
                button.Text = "Ready";
            }
        }
        private void AxisSettingX_Leave(object sender, EventArgs e)
        {
            Leisai_Axis.Axis_X_Speed = Convert.ToDouble(AxisSettingX.Text);
            Leisai_Axis.Axis_Y_Speed = Convert.ToDouble(AxisSettingY.Text);
            Leisai_Axis.Axis_Z_Speed = Convert.ToDouble(AxisSettingZ.Text);
        }

        private void AxisSettingY_Leave(object sender, EventArgs e)
        {

            Leisai_Axis.Axis_X_Speed = Convert.ToDouble(AxisSettingX.Text);
            Leisai_Axis.Axis_Y_Speed = Convert.ToDouble(AxisSettingY.Text);
            Leisai_Axis.Axis_Z_Speed = Convert.ToDouble(AxisSettingZ.Text);
        }

        private void AxisSettingZ_Leave(object sender, EventArgs e)
        {

            Leisai_Axis.Axis_X_Speed = Convert.ToDouble(AxisSettingX.Text);
            Leisai_Axis.Axis_Y_Speed = Convert.ToDouble(AxisSettingY.Text);
            Leisai_Axis.Axis_Z_Speed = Convert.ToDouble(AxisSettingZ.Text);
        }

        private async void uiButton1_Click(object sender, EventArgs e)
        {
            uiButton1.Enabled = false;
            uiButton1.BackColor = Color.Lime;
            try
            {
                if (await Leisai_Axis.Leisai_Home((ushort) Leisai_Axis.AxisName.X)) // X轴
                {
                    uiButton1.BackColor = Color.White;
                }
                else
                {
                    uiButton1.BackColor = Color.Red;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                uiButton1.Enabled = true;
            }
        }

        private async void uiButton2_Click(object sender, EventArgs e)
        {
            uiButton2.Enabled = false;
            uiButton2.BackColor = Color.Lime;
            try
            {
                if (await Leisai_Home((ushort)AxisName.Y)) // Y轴
                {
                    await Leisai_Axis.Leisai_Axis_Y_SafetyPoint_Pmov();
                    uiButton2.BackColor = Color.White;
                }
                else
                {
                    uiButton2.BackColor = Color.Red;
                }
            }
            catch (Exception) { throw; }
            finally
            {
                uiButton2.Enabled = true;
            }
        }

        private async void uiButton3_Click(object sender, EventArgs e)
        {
            uiButton3.Enabled = false;
            uiButton3.BackColor = Color.Lime;
            try
            {
                if (await Leisai_Home((ushort)AxisName.Z)) // Z轴
                {
                    uiButton3.BackColor = Color.White;
                }
                else
                {
                    uiButton3.BackColor = Color.Red;
                }
            }
            catch (Exception) { throw; }
            finally
            {
                uiButton3.Enabled = true;
            }
        }
    }
}
