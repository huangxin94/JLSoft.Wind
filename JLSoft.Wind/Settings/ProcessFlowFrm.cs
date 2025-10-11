using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DuckDB.NET.Data;
using JLSoft.Wind.Database.DB;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Services;
using JLSoft.Wind.Services.DuckDb;

namespace JLSoft.Wind.Settings
{
    public partial class ProcessFlowFrm : Form
    {
        private readonly DuckDbService _dbService;
        private Guid? _selectedFlowId = null;

        public ProcessFlowFrm(DuckDbService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            InitializeComponent();
        }
        /// <summary>
        /// 查询按钮点击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_Query_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. 获取查询条件
                string productName = txt_AxisX.Text.Trim(); // 产品名称
                string status = uiComboBox1.SelectedItem?.ToString(); // 生效/失效

                // 2. 构建SQL和参数
                string sql = @"SELECT 
                                process_flow_id AS 流程ID,
                                ProductName AS 产品名称,
                                ProductProcessName AS 产品工艺名称,
                                CASE Type WHEN 1 THEN '生效' ELSE '失效' END AS 生效状态,
                                Versions AS 版本号,
                                CreateName AS 创建人,
                                CreateTime AS 创建时间,
                                ComeName AS 生效人,
                                ComeTime AS 生效时间,
                                FailureName AS 失效人,
                                FailureTime AS 失效时间,
                                Spec AS 规格,
                                last_update AS 最后更新时间
                                FROM process_flow
                                WHERE 1=1";
                var parameters = new List<DuckDB.NET.Data.DuckDBParameter>();

                if (!string.IsNullOrEmpty(productName))
                {
                    sql += " AND ProductName = ?";
                    parameters.Add(new DuckDB.NET.Data.DuckDBParameter { Value = productName });
                }
                if (!string.IsNullOrEmpty(status))
                {
                    int typeValue = status == "生效" ? 1 : 0;
                    sql += " AND Type = ?";
                    parameters.Add(new DuckDB.NET.Data.DuckDBParameter { Value = typeValue });
                }

                // 3. 查询数据库
                DataTable result = _dbService.ExecuteQuery(sql, parameters.ToArray());
                // 4. 绑定到DataGridView
                dataGridView1.DataSource = result;
                dataGridView1.Columns["流程ID"].Visible = false;
                dataGridView1.Columns["规格"].Visible = false;
                // 列宽自适应
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dataGridView1.AutoResizeColumns();
                dataGridView1.Refresh();


                // 自动选中第一行并触发CellClick
                if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[0].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[dataGridView1.Columns.GetFirstColumn(DataGridViewElementStates.Visible).Index];
                    // 触发CellClick事件
                    dataGridView1_CellClick(dataGridView1, new DataGridViewCellEventArgs(
                        dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex));
                }
                else
                {
                    dataGridView2.DataSource = null;
                    dataGridView3.DataSource = null;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 新增按钮点击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_addrocess_Click(object sender, EventArgs e)
        {

            try
            {
                if (tbx_addmaterialname.Text.Trim() == "" || tbx_addprocessname.Text.Trim() == "")
                {
                    MessageBox.Show("产品名称和工艺名称不能为空！");
                }
                string sqlquy = $"SELECT max(Versions) AS 版本号 FROM process_flow WHERE ProductName = '{tbx_addmaterialname.Text.Trim()}' and ProductProcessName= '{tbx_addprocessname.Text.Trim()}'";
                var maxver = _dbService.ExecuteQuery(sqlquy);
                int versions = 1; // 可根据实际输入
                if (maxver != null)
                {
                    if (maxver.Rows.Count > 0 && maxver.Rows[0]["版本号"] != DBNull.Value)
                    {
                        int currentVersion = Convert.ToInt32(maxver.Rows[0]["版本号"]);
                        versions = currentVersion + 1;
                    }
                }
                // 1. 获取输入
                string productName = tbx_addmaterialname.Text.Trim();
                string productProcessName = tbx_addprocessname.Text.Trim();
                string createName = SessionManager.CurrentUser == null ? "user" : SessionManager.CurrentUser.Username;

                DateTime now = DateTime.Now;
                var processFlowId = Guid.NewGuid();
                // 2. 构建SQL
                string sql = @"INSERT INTO process_flow
                    (process_flow_id, Versions, CreateName, CreateTime, ProductName, ProductProcessName)
                    VALUES (?, ?, ?, ?, ?, ?)";

                var parameters = new[]
                {
                    new DuckDBParameter { Value = processFlowId },
                    new DuckDBParameter { Value = versions },
                    new DuckDBParameter { Value = createName },
                    new DuckDBParameter { Value = now },
                    new DuckDBParameter { Value = productName },
                    new DuckDBParameter { Value = productProcessName },
                };

                // 3. 执行插入
                var result = _dbService.ExecuteNonQuery(sql, parameters);
                if (result.Success)
                {
                    MessageBox.Show("新增成功！");
                    but_Query_Click(null, null); // 新增后刷新
                }
                else
                {
                    MessageBox.Show("新增失败：" + result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("新增异常：" + ex.Message);
            }
        }
        /// <summary>
        /// 生效
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_effectlose_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先选择要生效的流程行！");
                    return;
                }

                // 1. 获取选中行的流程ID、产品名称、工艺名称
                var row = dataGridView1.SelectedRows[0];
                var flowId = row.Cells["流程ID"].Value;
                var productName = row.Cells["产品名称"].Value?.ToString();
                var processName = row.Cells["产品工艺名称"].Value?.ToString();

                if (flowId == null || productName == null || processName == null)
                {
                    MessageBox.Show("选中行数据不完整，无法生效！");
                    return;
                }

                // 2. 先将同产品、同工艺的所有流程全部置为失效
                string sqlLose = @"UPDATE process_flow 
                                   SET Type = 0 ,
                                       FailureName = ?, 
                                       FailureTime = ?
                                   WHERE ProductName = ? AND ProductProcessName = ? AND process_flow_id <> ?";
                var loseParams = new[]
                {
                    new DuckDBParameter { Value = SessionManager.CurrentUser?.Username ?? "user" },
                    new DuckDBParameter { Value = DateTime.Now },
                    new DuckDBParameter { Value = productName },
                    new DuckDBParameter { Value = processName },
                    new DuckDBParameter { Value = flowId }
                };
                _dbService.ExecuteNonQuery(sqlLose, loseParams);

                // 3. 将当前选中流程置为生效
                string sqlEffect = @"UPDATE process_flow 
                                     SET Type = 1 ,
                                         ComeName = ?, 
                                         ComeTime = ?
                                     WHERE process_flow_id = ?";
                var effectParams = new[]
                {
                    new DuckDBParameter { Value = SessionManager.CurrentUser?.Username ?? "user" },
                    new DuckDBParameter { Value = DateTime.Now },
                    new DuckDBParameter { Value = flowId }
                };
                _dbService.ExecuteNonQuery(sqlEffect, effectParams);

                MessageBox.Show("生效操作成功！");
                but_Query_Click(null, null); // 刷新
            }
            catch (Exception ex)
            {
                MessageBox.Show("生效操作失败：" + ex.Message);
            }
        }
        /// <summary>
        /// 删除流程及其所有工序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_deleteprocess_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("请先选择要删除的流程行！");
                return;
            }

            var row = dataGridView1.CurrentRow;
            var flowId = row.Cells["流程ID"].Value;

            if (MessageBox.Show("删除操作将会删除流程及其所有工序，确认继续？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                // 1. 先删子表
                string sqlDeleteChild = "DELETE FROM process_flow_item WHERE ProcessFlowId = ?";
                var childParams = new[] { new DuckDBParameter { Value = flowId } };
                var childResult = _dbService.ExecuteNonQuery(sqlDeleteChild, childParams);
                if (!childResult.Success)
                {
                    MessageBox.Show("删除工序失败：" + childResult.ErrorMessage);
                    return;
                }
                // 2. 再删主表
                string sqlDeleteMain = "DELETE FROM process_flow WHERE process_flow_id = ?";
                var mainParams = new[] { new DuckDBParameter { Value = flowId } };
                var mainResult = _dbService.ExecuteNonQuery(sqlDeleteMain, mainParams);
                if (!mainResult.Success)
                {
                    MessageBox.Show("删除流程失败：" + mainResult.ErrorMessage);
                    return;
                }
                but_Query_Click(null, null); // 刷新
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除操作失败：" + ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridView2Query();


                // 自动选中dataGridView2第一行并触发CellClick
                if (dataGridView2.Rows.Count > 0)
                {
                    dataGridView2.ClearSelection();
                    // 选中第一行第一个可见单元格
                    int firstVisibleCol = dataGridView2.Columns.GetFirstColumn(DataGridViewElementStates.Visible).Index;
                    dataGridView2.Rows[0].Selected = true;
                    dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[firstVisibleCol];
                    dataGridView2_CellClick(dataGridView2, new DataGridViewCellEventArgs(firstVisibleCol, 0));
                }
            }
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _selectedFlowId.HasValue)
            {
                dataGridView3Query();
            }
        }
        private void dataGridView2Query()
        {
            var row = dataGridView1.CurrentRow;
            if (row == null)
            {
                dataGridView2.DataSource = null;
                return;
            }
            object cellValue = row.Cells["流程ID"].Value;
            if (cellValue == null || cellValue == DBNull.Value)
            {
                return;
            }
            if (cellValue != null && Guid.TryParse(cellValue.ToString(), out Guid guidValue))
            {
                _selectedFlowId = guidValue;
            }
            else
            {
                _selectedFlowId = null;
            }

            // 加载固定三项到 dataGridView2
            var dt = new DataTable();
            dt.Columns.Add("类型", typeof(string));
            dt.Rows.Add("Wafer");
            dt.Rows.Add("Glass");
            dt.Rows.Add("Assembled");
            dataGridView2.DataSource = dt;

        }

        private void dataGridView3Query()
        {
            var row = dataGridView2.CurrentRow;
            if (row == null || !_selectedFlowId.HasValue)
            {
                dataGridView3.DataSource = null;
                return;
            }
            string type = row.Cells["类型"].Value.ToString();

            // 查询 process_flow_item
            string sql = @"SELECT 
                                Seq AS 序号,
                                ProcessFlowItemId AS 明细ID,
                                WorkstageName AS 工序名称,
                                Qtime as Qtime,
                            FROM process_flow_item
                            WHERE ProcessFlowId = ? AND Type = ?
                            ORDER BY Seq";
            var parameters = new[]
            {
                    new DuckDB.NET.Data.DuckDBParameter { Value = _selectedFlowId.Value },
                    new DuckDB.NET.Data.DuckDBParameter { Value = type }
                };
            var dt = _dbService.ExecuteQuery(sql, parameters);
            dataGridView3.DataSource = dt;
            dataGridView3.Columns["明细ID"].Visible = false;
            // 列宽自适应
            dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView3.AutoResizeColumns();
        }

        /// <summary>
        /// 工序添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_addprocessitem_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. 校验流程ID和类型
                if (!_selectedFlowId.HasValue)
                {
                    MessageBox.Show("请先选择流程！");
                    return;
                }
                var row2 = dataGridView2.CurrentRow;
                if (row2 == null)
                {
                    MessageBox.Show("请先选择类型！");
                    return;
                }
                string type = row2.Cells["类型"].Value?.ToString();
                if (string.IsNullOrEmpty(type))
                {
                    MessageBox.Show("类型不能为空！");
                    return;
                }

                // 2. 获取最大序号
                int maxSeq = 0;
                foreach (DataGridViewRow row in dataGridView3.Rows)
                {
                    if (row.Cells["序号"].Value != null && int.TryParse(row.Cells["序号"].Value.ToString(), out int seq))
                    {
                        if (seq > maxSeq) maxSeq = seq;
                    }
                }
                int newSeq = maxSeq + 1;

                // 3. 获取工序名称和站点
                //string workstageName = cbx_addsite.Text.Trim();
                //if (string.IsNullOrEmpty(workstageName))
                //{
                //    MessageBox.Show("工序名称不能为空！");
                //    return;
                //}
                string site = cbx_addsite.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(site))
                {
                    MessageBox.Show("请选择站点！");
                    return;
                }
                var processFlowItemId = Guid.NewGuid();
                var Qtime = tbx_Qtime.Text.Trim();
                // 4. 插入数据库
                string sql = @"INSERT INTO process_flow_item (ProcessFlowItemId, ProcessFlowId, WorkstageName, Type, Seq, Qtime)
                               VALUES (?, ?, ?, ?, ?, ?)";
                var parameters = new[]
                {

                    new DuckDBParameter { Value = processFlowItemId },
                    new DuckDBParameter { Value = _selectedFlowId.Value },
                    new DuckDBParameter { Value = site },
                    new DuckDBParameter { Value = type },
                    new DuckDBParameter { Value = newSeq },
                    new DuckDBParameter { Value = Qtime }
                };
                var result = _dbService.ExecuteNonQuery(sql, parameters);
                if (result.Success)
                {
                    // 重新加载dataGridView3
                    if (dataGridView2.CurrentRow != null)
                    {
                        dataGridView2_CellClick(dataGridView2, new DataGridViewCellEventArgs(dataGridView2.CurrentRow.Index, 0));
                    }
                }
                else
                {
                    MessageBox.Show("添加失败：" + result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加异常：" + ex.Message);
            }
        }
        /// <summary>
        /// 子项删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_deleteitem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("删除操作将会删除工序，确认继续？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            try
            {
                if (dataGridView3.CurrentRow == null)
                {
                    MessageBox.Show("请先选择要删除的工序行！");
                    return;
                }

                // 假设明细ID列名为“明细ID”，如有不同请修改
                var row = dataGridView3.CurrentRow;
                var itemId = row.Cells["明细ID"]?.Value;
                if (itemId == null)
                {
                    MessageBox.Show("未找到要删除的工序ID！");
                    return;
                }

                // 确认删除
                if (MessageBox.Show("确定要删除该工序吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                string sql = "DELETE FROM process_flow_item WHERE ProcessFlowItemId = ?";
                var parameters = new[]
                {
                    new DuckDBParameter { Value = itemId }
                };
                var result = _dbService.ExecuteNonQuery(sql, parameters);
                if (result.Success)
                {
                    // 重新加载dataGridView3
                    if (dataGridView2.SelectedRows.Count > 0)
                    {
                        dataGridView2_CellClick(dataGridView2, new DataGridViewCellEventArgs(dataGridView2.SelectedRows[0].Index, 0));
                    }

                }
                else
                {
                    MessageBox.Show("删除失败：" + result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除异常：" + ex.Message);
            }
        }
        /// <summary>
        /// 插入操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_insert_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentRow == null)
            {
                MessageBox.Show("请先选择要插入的工序下行！");
                return;
            }
            var row = dataGridView3.CurrentRow;
            var itemSeq = row.Cells["序号"]?.Value;
            var itemType = dataGridView2.CurrentRow?.Cells["类型"].Value?.ToString();
            var itemWorkstageName = cbx_addsite.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(itemType) || itemSeq == null || string.IsNullOrEmpty(itemWorkstageName))
            {
                MessageBox.Show("类型、序号或站点不能为空！");
                return;
            }

            var itemId = Guid.NewGuid();

            // 参数化SQL，防止注入
            string upsql = "UPDATE process_flow_item SET Seq = Seq + 1 WHERE ProcessFlowId = ? AND Type = ? AND Seq >= ?";
            string insertsql = @"INSERT INTO process_flow_item (ProcessFlowItemId, ProcessFlowId, WorkstageName, Type, Seq)
                                 VALUES (?, ?, ?, ?, ?)";

            using (var conn = _dbService.GetType()
                                        .GetMethod("GetConnection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                                        .Invoke(_dbService, null) as DuckDBConnection)
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. 更新序号
                        using (var upcmd = conn.CreateCommand())
                        {
                            upcmd.CommandText = upsql;
#pragma warning disable CS8629 // 可为 null 的值类型可为 null。
                            upcmd.Parameters.Add(new DuckDBParameter { Value = _selectedFlowId.Value });
#pragma warning restore CS8629 // 可为 null 的值类型可为 null。
                            upcmd.Parameters.Add(new DuckDBParameter { Value = itemType });
                            upcmd.Parameters.Add(new DuckDBParameter { Value = itemSeq });
                            upcmd.Transaction = tran;
                            upcmd.ExecuteNonQuery();
                        }

                        // 2. 插入新工序
                        using (var inscmd = conn.CreateCommand())
                        {
                            inscmd.CommandText = insertsql;
                            inscmd.Parameters.Add(new DuckDBParameter { Value = itemId });
                            inscmd.Parameters.Add(new DuckDBParameter { Value = _selectedFlowId.Value });
                            inscmd.Parameters.Add(new DuckDBParameter { Value = itemWorkstageName });
                            inscmd.Parameters.Add(new DuckDBParameter { Value = itemType });
                            inscmd.Parameters.Add(new DuckDBParameter { Value = itemSeq });
                            inscmd.Transaction = tran;
                            inscmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        // 重新加载dataGridView3
                        if (dataGridView2.CurrentRow != null)
                        {
                            dataGridView2_CellClick(dataGridView2, new DataGridViewCellEventArgs(dataGridView2.CurrentRow.Index, 0));
                        }
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        MessageBox.Show("插入失败：" + ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 子项修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var row = dataGridView3.Rows[e.RowIndex];
            var itemId = row.Cells["明细ID"].Value;
            var currentQtime = row.Cells["Qtime"].Value?.ToString();

            // 创建输入对话框
            using (var dialog = new QtimeInFo("设置Qtime值", currentQtime ?? "0"))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // 验证输入是否为有效的double
                    if (double.TryParse(dialog.InputValue, out double newQtime))
                    {
                        UpdateQtimeInDatabase(itemId.ToString(), newQtime);

                        // 刷新当前行显示
                        row.Cells["Qtime"].Value = newQtime;
                    }
                    else
                    {
                        MessageBox.Show("请输入有效的数值！");
                    }
                }
            }
        }
        /// <summary>
        /// 更改Qtime
        /// </summary>
        /// <param name="processFlowItemId"></param>
        /// <param name="newQtime"></param>
        private void UpdateQtimeInDatabase(string processFlowItemId, double newQtime)
        {
            if (!Guid.TryParse(processFlowItemId, out var itemId))
            {
                MessageBox.Show("无效的工序ID！");
                return;
            }

            try
            {
                string sql = "UPDATE process_flow_item SET Qtime = ? WHERE ProcessFlowItemId = ?";
                var parameters = new[]
                {
            new DuckDBParameter { Value = newQtime },
            new DuckDBParameter { Value = itemId }
        };

                var result = _dbService.ExecuteNonQuery(sql, parameters);

                if (!result.Success)
                {
                    MessageBox.Show($"更新失败: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新异常: {ex.Message}");
            }
        }
    }
}
