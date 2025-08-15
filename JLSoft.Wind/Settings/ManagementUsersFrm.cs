using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using DuckDB.NET.Data;
using DuckDB.NET.Native;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services;
using JLSoft.Wind.Services.DuckDb;
using Newtonsoft.Json;
using Sunny.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace JLSoft.Wind.Settings
{
    public partial class ManagementUsersFrm : Form
    {

        private readonly DuckDbService _dbService; 
        private readonly ConfigService _configService = ConfigService.Instance;


        private const string JsonFilePath = "BasicConfiguration.json";
        private List<UserData> users;
        public ManagementUsersFrm(DuckDbService dbService)
        {
            _dbService = dbService;
            InitializeComponent();
            uiDataGridView1.AutoGenerateColumns = true;
            uiDataGridView1.ReadOnly = true;
            uiDataGridView1.DataSource = bindingSource1; // 先建立绑定关系


            LoadUsers();
        }


        private void LoadUsers()
        {
            users = _configService.GetUsers();
            // 绑定到DataGridView
        }


        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton1_Click(object sender, EventArgs e)
        {
            QueryUsers("json");
        }
        /// <summary>
        /// 查询用户信息JSon
        /// </summary>
        /// <param name="json"></param>
        private void QueryUsers(string json)
        {
            var filteredUsers = users.AsQueryable();

            if (!string.IsNullOrEmpty(txt_userid.Text.Trim()))
            {
                int userId = int.Parse(txt_userid.Text.Trim());
                filteredUsers = filteredUsers.Where(u => u.UserId == userId);
            }

            if (!string.IsNullOrEmpty(txt_username.Text.Trim()))
            {
                string username = txt_username.Text.Trim();
                filteredUsers = filteredUsers.Where(u => u.Username == username);
            }

            var dataTable = new DataTable();
            dataTable.Columns.Add("用户ID", typeof(int));
            dataTable.Columns.Add("用户名", typeof(string));
            dataTable.Columns.Add("密码", typeof(string));
            dataTable.Columns.Add("职阶", typeof(string));
            dataTable.Columns.Add("最后更新时间", typeof(DateTime));

            foreach (var user in filteredUsers)
            {
                dataTable.Rows.Add(user.UserId, user.Username, user.Password, user.Grade, user.LastUpdate);
            }

            bindingSource1.DataSource = dataTable;
            uiDataGridView1.Columns["密码"].Visible = false; // 隐藏密码列
            uiDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        /// <summary>
        /// DuckDB
        /// </summary>
        private void QueryUsers()
        {
            var parameters = new List<DuckDBParameter>();
            if (txt_userid.Text.Trim().Length == 0 && txt_username.Text.Trim().Length == 0)
            {
                string sql = @"SELECT userid AS 用户ID,
                                      username AS 用户名,
                                      password AS 密码,
                                      grade AS 职阶,
                                      last_update AS 最后更新时间 FROM users";
                var dt = _dbService.ExecuteQuery(sql);
                bindingSource1.DataSource = dt;
                uiDataGridView1.Columns["密码"].Visible = false; // 隐藏密码列
                uiDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            else if (txt_userid.Text.Trim().Length > 0 && txt_username.Text.Trim().Length == 0)
            {
                string sql = @"SELECT userid AS 用户ID,
                                      username AS 用户名,
                                      password AS 密码,
                                      grade AS 职阶,
                                      last_update AS 最后更新时间 FROM users WHERE userid = ?";
                var dt = _dbService.ExecuteQuery(sql, new DuckDBParameter { Value = int.Parse(txt_userid.Text.Trim()) });
                bindingSource1.DataSource = dt;
                uiDataGridView1.Columns["密码"].Visible = false; // 隐藏密码列
                uiDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            else if (txt_username.Text.Trim().Length > 0 && txt_userid.Text.Trim().Length == 0)
            {
                string sql = @"SELECT userid AS 用户ID,
                                      username AS 用户名,
                                      password AS 密码,
                                      grade AS 职阶,
                                      last_update AS 最后更新时间 FROM users WHERE username = ?";
                var dt = _dbService.ExecuteQuery(sql, new DuckDBParameter { Value = txt_username.Text.Trim() });
                bindingSource1.DataSource = dt;
                uiDataGridView1.Columns["密码"].Visible = false; // 隐藏密码列
                uiDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            else
            {
                string sql = @"SELECT userid AS 用户ID,
                                      username AS 用户名,
                                      password AS 密码,
                                      grade AS 职阶,
                                      last_update AS 最后更新时间 FROM users WHERE 1=1";

                if (!string.IsNullOrEmpty(txt_userid.Text.Trim()))
                {
                    sql += " AND userid = ?";
                    parameters.Add(new DuckDBParameter { Value = int.Parse(txt_userid.Text.Trim()) });
                }

                if (!string.IsNullOrEmpty(txt_username.Text.Trim()))
                {
                    sql += " AND username = ?";
                    parameters.Add(new DuckDBParameter { Value = txt_username.Text.Trim() });
                }
                var dt = _dbService.ExecuteQuery(sql, parameters.ToArray());
                bindingSource1.DataSource = dt;
                uiDataGridView1.Columns["密码"].Visible = false; // 隐藏密码列
                uiDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(in_userid.Text.Trim()))
            {
                MessageBox.Show("请输入用户ID");
                return;
            }
            if (string.IsNullOrEmpty(in_username.Text.Trim()))
            {
                MessageBox.Show("请输入用户名");
                return;
            }
            if (string.IsNullOrEmpty(in_password.Text.Trim()))
            {
                MessageBox.Show("请输入密码");
                return;
            }
            if (in_grade.SelectedItem == null)
            {
                MessageBox.Show("请输入用户等级");
                return;
            }
            updateJsonUser();

        }
        /// <summary>
        /// JSON用户更新
        /// </summary>
        private void updateJsonUser()
        {
            int userId = int.Parse(in_userid.Text.Trim());
            string username = in_username.Text.Trim();
            string password = in_password.Text.Trim();
            string grade = in_grade.SelectedItem.ToString();
            DateTime lastUpdate = DateTime.Now;
            var count =  users.Where(u => u.UserId == userId).ToList().Count;
            if(count > 0)
            {
                MessageBox.Show("用户ID已存在，请使用其他ID");
                return;
            }
            var newUser = new UserData
            {
                UserId = userId,
                Username = username,
                Password = password,
                Grade = grade,
                LastUpdate = lastUpdate
            };

            _configService.AddUser(newUser);

            LogManager.Log("用户：" + userId + "添加成功");
            LogManager.Log("姓名：" + username);
            LogManager.Log("职阶：" + grade);
            // 刷新界面
            LoadUsers();
            QueryUsers("json");
        }
        /// <summary>
        /// DuckDB用户更新
        /// </summary>
        private void updateDatabaseUser()
        {
            string sql = @"INSERT INTO users (userid, username, password, grade, last_update) 
               VALUES (?, ?, ?, ?, CURRENT_TIMESTAMP)";

            // 添加参数时按顺序传递
            var parameters = new[]
            {
                new DuckDBParameter { Value = int.Parse(in_userid.Text.Trim()) },
                new DuckDBParameter { Value = in_username.Text.Trim() },
                new DuckDBParameter { Value = in_password.Text.Trim() },
                new DuckDBParameter { Value = in_grade.SelectedItem.ToString() }
            };

            var dt = _dbService.ExecuteNonQuery(sql, parameters);
            if (dt.Success)
            {
                MessageBox.Show("插入成功");
                QueryUsers();
            }
            else
            {
                MessageBox.Show("新增失败：" + dt.ErrorMessage);
            }
        }
    }
}
