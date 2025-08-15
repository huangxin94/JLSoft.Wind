using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services.DuckDb;
using Sunny.UI;

namespace JLSoft.Wind.Services
{
    public class MainDataProcessing
    {
        private readonly DuckDbService _dbService;
        public MainDataProcessing(DuckDbService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        DataTable dataTable = new DataTable();
        public DataTable QueryMainData()
        {
            try
            {
                
                // 构建SQL和参数
                string sql = @"select 
                                    mainid as 主键,
                                    Status as 状态,
                                    CurrentSite as 当前站点,
                                    Progress as 完成进度,
                                    type as 类型,
                                    ProductName as 产品名称,
                                    ProductCode as 产品编码,
                                    StartTime as 开始时间,
                                    EndTime as 完成时间
                                    from ProductiveProcessMain 
                                    where Status = ?";
                var parameters = new List<DuckDB.NET.Data.DuckDBParameter>();
                parameters.Add(new DuckDB.NET.Data.DuckDBParameter { Value = "生产" });

                // 3. 查询数据库
                dataTable = _dbService.ExecuteQuery(sql, parameters.ToArray());

            }
            catch (Exception ex)
            {
                LogManager.Log($"生产主数据查询失败: {ex.Message}");
            }
            return dataTable;
        }
    }
}
