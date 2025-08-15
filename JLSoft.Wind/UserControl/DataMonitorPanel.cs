using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.UserControl
{
    public class DataMonitorPanel : TableLayoutPanel
    {
        private readonly Dictionary<string, Label> _valueLabels = new Dictionary<string, Label>();

        public void UpdateValue(string deviceCode, int value)
        {
            if (_valueLabels.TryGetValue(deviceCode, out var label))
            {
                label.Text = value.ToString();
                label.BackColor = GetStatusColor(value);
            }
        }

        private Color GetStatusColor(int value) => value switch
        {
            > 200 => Color.Red,
            > 100 => Color.Orange,
            _ => Color.LightGreen
        };

        public void AddDevice(string deviceCode)
        {
            var row = AddRow();
            Controls.Add(new Label { Text = deviceCode }, 0, row);
            var valueLabel = new Label { Dock = DockStyle.Fill };
            Controls.Add(valueLabel, 1, row);
            _valueLabels.Add(deviceCode, valueLabel);
        }

        private int AddRow()
        {
            RowCount++;
            RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            return RowCount - 1;
        }
    }
}
