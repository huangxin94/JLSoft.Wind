using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Database.Models;
using JLSoft.Wind.Services;

namespace JLSoft.Wind.Settings
{
    public partial class LoginFrm : Form
    {
        private List<UserData> allUsers;
        public LoginFrm()
        {
            InitializeComponent();
            LoadUsersFromJson();
        }
        private void LoadUsersFromJson()
        {
            try
            {
                string jsonFilePath = "BasicConfiguration.json";
                if (File.Exists(jsonFilePath))
                {
                    string json = File.ReadAllText(jsonFilePath);
                    var config = JsonSerializer.Deserialize<BasicConfiguration>(json);
                    allUsers = config.Users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取用户信息时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void butLogin_Click(object sender, EventArgs e)
        {
            int username = int.Parse(txtUsername.Text);
            string password = txtPassword.Text;

            var user = allUsers.FirstOrDefault(u => u.UserId == username && u.Password == password);
            if (user != null)
            {
                SessionManager.Login(user);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("用户名或密码错误，请重试。", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
