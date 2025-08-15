using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Services.DuckDb;
using Sunny.UI;

namespace JLSoft.Wind.Settings
{
    public partial class ActualProcessItemFrm : Form
    {
        private readonly DuckDbService _dbService;
        private readonly string _mainid;
        public ActualProcessItemFrm(DuckDbService dbService, string mainid)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _mainid = mainid;
            InitializeComponent();
        }

        private void ActualProcessItemFrm_Load(object sender, EventArgs e)
        {
            ItemQuery();
        }

        public void ItemQuery()
        {
            try
            {
                string sql = @"SELECT
	                            a.ActualProcessItemID as 主键,
	                            a.Sequence as 序号,
	                            case
		                            a.Status when '0' then '未生产'
		                            when '1' then '生产中'
		                        when '2' then '完成生产'
		                            else '异常完结'
	                            end as 状态,
	                            a.WorkstageName as 工序名称,
	                            a.Qtime as 设定Qtime值,
	                            a.ActualQtime as 实际Qtime,
	                            a.StartQtime as 开始计算Qtime时间,
	                            a.EndQtime as 结束计算Qtime时间 ,
	                            a.StartTime as 开始工序时间,
	                            a.EndTime as 结束时间
                            FROM
	                        actualprocessitem AS a
                            where PPMainId  = ?";
                var parameters = new List<DuckDB.NET.Data.DuckDBParameter>();
                parameters.Add(new DuckDB.NET.Data.DuckDBParameter { Value = _mainid });

                // 3. 查询数据库
                var dataTable = _dbService.ExecuteQuery(sql, parameters.ToArray());
                dataGridView1.DataSource = dataTable;
                if (dataTable != null)
                    dataGridView1.Columns["主键"].Visible = false;
            }
            catch(Exception ex) { 
                MessageBox.Show($"实际工艺流程项查询失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
