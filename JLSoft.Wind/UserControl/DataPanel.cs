using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.UserControl
{
    public class DataPanel : System.Windows.Forms.UserControl
    {
        private DataGridView _grid;

        public DataPanel()
        {
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Columns = {
                new DataGridViewTextBoxColumn { HeaderText = "设备编码", DataPropertyName = "Code" },
                new DataGridViewTextBoxColumn { HeaderText = "当前值", DataPropertyName = "Value" }
            }
            };
            Controls.Add(_grid);
        }

        public void BindData(List<DeviceBlock> devices)
        {
            _grid.DataSource = devices.Select(d => new {
                Code = d.DeviceCode
            }).ToList();
        }
    }
}
