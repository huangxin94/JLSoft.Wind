using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Services;

namespace JLSoft.Wind.Settings
{
    public partial class RobotSettingFrm : Form
    {

        private DeviceIndices _currentDevice;
        private SubStations _currentSubStation;
        public RobotSettingFrm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RobotSettingFrm_Load(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.Columns.Add("序号", 50);
            listView1.Columns.Add("X", 80);
            listView1.Columns.Add("Y", 80);
            listView1.Columns.Add("Z", 80);
            listView1.FullRowSelect = true; // 整行选中

            // 添加示例数据
            List<PointRecord> pointList = new List<PointRecord>();
            pointList.Add(new PointRecord { Index = 1, X = 100, Y = 102, Z = 101 });
            pointList.Add(new PointRecord { Index = 2, X = 101, Y = 100, Z = 101 });
            pointList.Add(new PointRecord { Index = 3, X = 102, Y = 101, Z = 100 });
            pointList.Add(new PointRecord { Index = 4, X = 100, Y = 100, Z = 100 });

            foreach (var itemp in pointList)
            {
                var item = new ListViewItem(itemp.Index.ToString());
                item.SubItems.Add(itemp.X.ToString());
                item.SubItems.Add(itemp.Y.ToString());
                item.SubItems.Add(itemp.Z.ToString());
                listView1.Items.Add(item);
            }
            var deviceIndices = ConfigService.GetDeviceStations();
            var deviceCodes = deviceIndices.ToList();
            CbxFacility.DataSource = deviceCodes;
            CbxFacility.DisplayMember = "DeviceCode";   // 显示设备编号
            CbxFacility.ValueMember = "Index";   // 选中时取设备索引
            // 点击恢复逻辑
            listView1.SelectedIndexChanged += (sender, e) =>
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var selected = listView1.SelectedItems[0];
                    double x = double.Parse(selected.SubItems[1].Text);
                    double y = double.Parse(selected.SubItems[2].Text);
                    double z = double.Parse(selected.SubItems[3].Text);
                    txt_AxisX.Text = x.ToString();
                    txt_AxisY.Text = y.ToString();
                    txt_AxisZ.Text = z.ToString();
                }
            };
        }

        private void CbxFacility_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CbxFacility.SelectedItem is DeviceIndices selectedDevice)
            {
                if (selectedDevice.SubStations != null && selectedDevice.SubStations.Any())
                {
                    // 绑定子站
                    CbxSlot.DataSource = selectedDevice.SubStations;
                    CbxSlot.DisplayMember = "Name";  // 显示子站名称
                    CbxSlot.ValueMember = "Name";    // 值取子站名称
                    CbxSlot.Enabled = true;
                }
                else
                {
                    // 没有子站：清空ComboBox并禁用
                    CbxSlot.DataSource = null; // 清除数据源
                    CbxSlot.Items.Clear();     // 清除所有项
                    CbxSlot.Text = string.Empty; // 清空显示文本
                    CbxSlot.Enabled = false;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbx_MoveFacility_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbx_MoveFacility.SelectedItem is DeviceIndices selectedDevice)
            {
                if (selectedDevice.SubStations != null && selectedDevice.SubStations.Any())
                {
                    // 绑定子站
                    cbx_MoveSlot.DataSource = selectedDevice.SubStations;
                    cbx_MoveSlot.DisplayMember = "Name";  // 显示子站名称
                    cbx_MoveSlot.ValueMember = "Name";    // 值取子站名称
                    cbx_MoveSlot.Enabled = true;
                }
                else
                {
                    // 没有子站：清空ComboBox并禁用
                    cbx_MoveSlot.DataSource = null; // 清除数据源
                    cbx_MoveSlot.Items.Clear();     // 清除所有项
                    cbx_MoveSlot.Text = string.Empty; // 清空显示文本
                    cbx_MoveSlot.Enabled = false;
                }
            }
        }

        private void uiButton3_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证输入
                if (!double.TryParse(txt_AxisX.Text, out double x) ||
                    !double.TryParse(txt_AxisY.Text, out double y) ||
                    !double.TryParse(txt_AxisZ.Text, out double z))
                {
                    MessageBox.Show("请输入有效的数值", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 获取配置服务实例
                var allDevices = ConfigService.GetDeviceStations();
                // 查找当前设备
                var device = allDevices.FirstOrDefault(d => d.DeviceCode == _currentDevice.DeviceCode);
                if (device == null)
                {
                    MessageBox.Show("找不到对应的设备配置", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 更新点位数据
                if (_currentSubStation != null)
                {
                    // 更新子站点位
                    var subStation = device.SubStations.FirstOrDefault(s => s.Name == _currentSubStation.Name);
                    if (subStation != null)
                    {
                        subStation.Positions.X = x;
                        subStation.Positions.Y = y;
                        subStation.Positions.Z = z;
                    }
                }
                else
                {
                    // 更新设备主站点位
                    device.Positions.X = x;
                    device.Positions.Y = y;
                    device.Positions.Z = z;
                }

                // 保存配置到文件
                ConfigService.Instance.SaveConfiguration();

                // 更新ListView显示
                if (listView1.SelectedItems.Count > 0)
                {
                    var selected = listView1.SelectedItems[0];
                    selected.SubItems[1].Text = x.ToString();
                    selected.SubItems[2].Text = y.ToString();
                    selected.SubItems[3].Text = z.ToString();
                }

                MessageBox.Show("保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
