using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services;
using JLSoft.Wind.Services.Status;

namespace JLSoft.Wind.Settings
{
    public partial class EquipmentStatusFrm : Form
    {
        private DeviceMonitor _deviceMonitor;
        // 设备状态灯映射字典：<设备名称, <状态名称, UILight控件>>
        private readonly Dictionary<string, Dictionary<string, Sunny.UI.UILight>> _statusLights = new();

        public EquipmentStatusFrm(DeviceMonitor deviceMonitor)
        {
            
            InitializeComponent();
            _deviceMonitor = deviceMonitor;
        }

        // 初始化状态灯映射
        private void InitializeStatusMapping()
        {
            // V1设备状态灯映射
            var v1Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running", v1_lig1 },
                { "Idle", v1_lig2 },
                { "Paused", v1_lig3 },
                { "Fault", v1_lig4 },
                { "DownInline", v1_lig5 },
                { "DownTrouble", v1_lig6 },
                { "UpInline", v1_lig7 }, // 第二个DownInline状态
                { "UpTrouble", v1_lig8 } // 第二个DownTrouble状态
            };
            _statusLights.Add("V1", v1Lights);

            // U1设备状态灯映射
            var u1Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running", u1_lig1 },
                { "Idle", u1_lig2 },
                { "Paused", u1_lig3 },
                { "Fault", u1_lig4 },
                { "DownInline", u1_lig5 },
                { "DownTrouble", u1_lig6 },
                { "UpInline", u1_lig7 },
                { "UpTrouble", u1_lig8 }
            };
            _statusLights.Add("U1", u1Lights);
            var s3Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running", s3_lig1 },
                { "Idle", s3_lig2 },
                { "Paused", s3_lig3 },
                { "Fault", s3_lig4 },
                { "DownInline", s3_lig5 },
                { "DownTrouble", s3_lig6 },
                { "UpInline", s3_lig7 },
                { "UpTrouble", s3_lig8 }
            };
            _statusLights.Add("S3", s3Lights);

            var m1Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running", m1_lig1 },
                { "Idle", m1_lig2 },
                { "Paused", m1_lig3 },
                { "Fault", m1_lig4 },
                { "DownInline", m1_lig5 },
                { "DownTrouble", m1_lig6 },
                { "UpInline", m1_lig7 },
                { "UpTrouble", m1_lig8 }
            };
            _statusLights.Add("M1", m1Lights);

            var s4Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running", s4_lig1 },
                { "Idle", s4_lig2 },
                { "Paused", s4_lig3 },
                { "Fault", s4_lig4 },
                { "DownInline", s4_lig5 },
                { "DownTrouble", s4_lig6 },
                { "UpInline", s4_lig7 },
                { "UpTrouble", s4_lig8 }
            };
            _statusLights.Add("S4", s4Lights);

            var c1Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running", c1_lig1 },
                { "Idle", c1_lig2 },
                { "Paused", c1_lig3 },
                { "Fault", c1_lig4 },
                { "DownInline", c1_lig5 },
                { "DownTrouble", c1_lig6 },
                { "UpInline", c1_lig7 },
                { "UpTrouble", c1_lig8 }
            };
            _statusLights.Add("C1", c1Lights);


            var r1Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running", r1_lig1 },
                { "Idle", r1_lig2 },
                { "Paused", r1_lig3 },
                { "Fault", r1_lig4 },
                { "DownInline", r1_lig5 },
                { "DownTrouble", r1_lig6 },
                { "UpInline", r1_lig7 },
                { "UpTrouble", r1_lig8 }
            };
            _statusLights.Add("R1", r1Lights);

            var g1Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running", g1_lig1 },
                { "Idle", g1_lig2 },
                { "Paused", g1_lig3 },
                { "Fault", g1_lig4 },
                { "DownInline", g1_lig5 },
                { "DownTrouble", g1_lig6 },
                { "UpInline", g1_lig7 },
                { "UpTrouble", g1_lig8 }
            };
            _statusLights.Add("G1", g1Lights);

            var a1Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running",  a1_lig1 },
                { "Idle",  a1_lig2 },
                { "Paused",  a1_lig3 },
                { "Fault",  a1_lig4 },
                { "DownInline",  a1_lig5 },
                { "DownTrouble",  a1_lig6 },
                { "UpInline",  a1_lig7 },
                { "UpTrouble",  a1_lig8 }
            };
            _statusLights.Add("A1", a1Lights);
            var a2Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running",  a2_lig1 },
                { "Idle",  a2_lig2 },
                { "Paused",  a2_lig3 },
                { "Fault",  a2_lig4 },
                { "DownInline",  a2_lig5 },
                { "DownTrouble",  a2_lig6 },
                { "UpInline",  a2_lig7 },
                { "UpTrouble",  a2_lig8 }
            };
            _statusLights.Add("A2", a2Lights);
            var a3Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running",  a3_lig1 },
                { "Idle",  a3_lig2 },
                { "Paused",  a3_lig3 },
                { "Fault",  a3_lig4 },
                { "DownInline",  a3_lig5 },
                { "DownTrouble",  a3_lig6 },
                { "UpInline",  a3_lig7 },
                { "UpTrouble",  a3_lig8 }
            };
            _statusLights.Add("A3", a3Lights);
            var a4Lights = new Dictionary<string, Sunny.UI.UILight>
            {
                { "Running",  a4_lig1 },
                { "Idle",  a4_lig2 },
                { "Paused",  a4_lig3 },
                { "Fault",  a4_lig4 },
                { "DownInline",  a4_lig5 },
                { "DownTrouble",  a4_lig6 },
                { "UpInline",  a4_lig7 },
                { "UpTrouble",  a4_lig8 }
            };
            _statusLights.Add("A4", a4Lights);
            // 初始化所有灯为灰色
            ResetAllLights();
        }

        // 重置所有灯为灰色
        private void ResetAllLights()
        {
            foreach (var deviceLights in _statusLights.Values)
            {
                foreach (var light in deviceLights.Values)
                {
                    light.OnColor = Color.Gray;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // 订阅设备状态更新事件


            InitializeStatusMapping();
            if (_deviceMonitor != null)
            {
                _deviceMonitor.DeviceStatesUpdated += OnDeviceStatesUpdated;
            }
            else
            {
                LogManager.Log("设备监控器未初始化",Sunny.UI.LogLevel.Error);
            }

        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            // 取消事件订阅
            if(_deviceMonitor != null)
            {
                _deviceMonitor.DeviceStatesUpdated -= OnDeviceStatesUpdated;

            }
        }

        private void OnDeviceStatesUpdated(Dictionary<string, DeviceMonitor.DeviceState> deviceStates)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnDeviceStatesUpdated(deviceStates)));
                return;
            }

            // 先重置所有灯为灰色
            //ResetAllLights();

            foreach (var (deviceCode, state) in deviceStates)
            {
                if (_statusLights.TryGetValue(deviceCode, out var lights))
                {

                    foreach (var light in lights.Values)
                    {
                        light.OnColor = Color.Gray;
                    }
                    // 根据实际状态点亮对应灯（绿色）
                    if (state.Running) lights["Running"].OnColor = Color.Green;
                    if (state.Idle) lights["Idle"].OnColor = Color.Green;
                    if (state.Paused) lights["Paused"].OnColor = Color.Green;
                    if (state.Fault) lights["Fault"].OnColor = Color.Green;


                    if (state.DownstreamInline) lights["DownInline"].OnColor = Color.Green;
                    if (state.DownstreamTrouble) lights["DownTrouble"].OnColor = Color.Green;
                    // 新增的状态
                    if (state.UpstreamInline) lights["UpInline"].OnColor = Color.Green;
                    if (state.UpstreamTrouble) lights["UpTrouble"].OnColor = Color.Green;
                }
            }
        }
    }
}
