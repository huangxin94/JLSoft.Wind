using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckDB.NET.Data;
using JLSoft.Wind.Database.DB;
using JLSoft.Wind.Services.DuckDb;

namespace JLSoft.Wind.Services
{
    public class ProductionTrackingService
    {
        private readonly DuckDbService _dbService;
        private readonly Dictionary<Guid, ActualProcessItem> _activeSteps = new();

        public ProductionTrackingService(DuckDbService dbService)
        {
            _dbService = dbService;
        }

        // 创建主数据和步骤数据
        public Guid StartProduction(string productName, string productCode, List<ProcessFlowItem> processFlow)
        {
            // 创建主数据
            var mainId = Guid.NewGuid();
            var mainSql = @"INSERT INTO ProductiveProcessMain 
                        (MainId, ProductName, ProductCode, Status) 
                        VALUES (?, ?, ?, '生产开始')";
            _dbService.ExecuteNonQuery(mainSql,
                new DuckDBParameter(mainId),
                new DuckDBParameter(productName),
                new DuckDBParameter(productCode));

            // 创建步骤数据
            foreach (var step in processFlow.OrderBy(s => s.Seq))
            {
                var item = new ActualProcessItem
                {
                    ActualProcessItemId = Guid.NewGuid(),
                    PPMainId = mainId,
                    WorkstageName = step.WorkstageName,
                    Sequence = step.Seq,
                    Qtime = step.Qtime,
                    Status = "0"
                };

                var itemSql = @"INSERT INTO ActualProcessItem 
                            (ActualProcessItemId, PPMainId, WorkstageName, Sequence, Qtime, Status) 
                            VALUES (?, ?, ?, ?, ?, ?)";
                _dbService.ExecuteNonQuery(itemSql,
                    new DuckDBParameter(item.ActualProcessItemId),
                    new DuckDBParameter(item.PPMainId),
                    new DuckDBParameter(item.WorkstageName),
                    new DuckDBParameter(item.Sequence),
                    new DuckDBParameter(item.Qtime),
                    new DuckDBParameter(item.Status));
            }

            return mainId;
        }

        // 开始一个步骤（设备开始加工）
        public void StartProcessStep(Guid mainId, string processName)
        {
            var sql = @"UPDATE ActualProcessItem 
                    SET StartTime = ?, Status = '1'
                    WHERE PPMainId = ? AND WorkstageName = ? 
                    AND Status = '0'";

            _dbService.ExecuteNonQuery(sql,
                new DuckDBParameter(DateTime.Now),
                new DuckDBParameter(mainId),
                new DuckDBParameter(processName));
        }

        // 完成一个步骤（设备加工完成）
        public void CompleteProcessStep(Guid mainId, string processName)
        {
            var completeTime = DateTime.Now;

            // 1. 更新步骤结束时间
            var updateSql = @"UPDATE ActualProcessItem 
                          SET EndTime = ?, Status = '2'
                          WHERE PPMainId = ? AND WorkstageName = ? 
                          AND Status = '0'";

            _dbService.ExecuteNonQuery(updateSql,
                new DuckDBParameter(completeTime),
                new DuckDBParameter(mainId),
                new DuckDBParameter(processName));

            // 2. 记录步骤完成时间（用于后续QTime计算）
            var item = GetCurrentItem(mainId, processName);
            if (item != null)
            {
                _activeSteps[item.ActualProcessItemId] = new ActualProcessItem
                {
                    ActualProcessItemId = item.ActualProcessItemId,
                    EndTime = completeTime
                };
            }
        }

        // 机器人移动产品到下一步（计算QTime）
        public void MoveToNextStep(Guid mainId, string nextProcessName)
        {
            var moveTime = DateTime.Now;

            // 获取所有等待QTime计算的步骤
            var itemsToUpdate = _activeSteps.Values
                .Where(i => i.PPMainId == mainId)
                .ToList();

            foreach (var item in itemsToUpdate)
            {
                if (item.EndTime.HasValue)
                {
                    // 计算实际QTime（从步骤完成到机器人移动到下一步的时间）
                    var actualQtime = (moveTime - item.EndTime.Value).TotalSeconds;

                    // 更新数据库
                    var qtimeSql = @"UPDATE ActualProcessItem 
                                 SET ActualQtime = ?
                                 WHERE ActualProcessItemId = ?";

                    _dbService.ExecuteNonQuery(qtimeSql,
                        new DuckDBParameter(actualQtime),
                        new DuckDBParameter(item.ActualProcessItemId));

                    // 从缓存移除
                    _activeSteps.Remove(item.ActualProcessItemId);
                }
            }

            // 更新当前站点
            var updateMainSql = @"UPDATE ProductiveProcessMain 
                              SET CurrentSite = ? 
                              WHERE MainId = ?";

            _dbService.ExecuteNonQuery(updateMainSql,
                new DuckDBParameter(nextProcessName),
                new DuckDBParameter(mainId));
        }

        private ActualProcessItem GetCurrentItem(Guid mainId, string processName)
        {
            var sql = @"SELECT * FROM ActualProcessItem 
                    WHERE PPMainId = ? AND WorkstageName = ?
                    ORDER BY Sequence DESC LIMIT 1";

            var dt = _dbService.ExecuteQuery(sql,
                new DuckDBParameter(mainId),
                new DuckDBParameter(processName));

            if (dt.Rows.Count > 0)
            {
                return new ActualProcessItem
                {
                    ActualProcessItemId = (Guid)dt.Rows[0]["ActualProcessItemId"],
                    PPMainId = mainId,
                    WorkstageName = processName,
                    EndTime = dt.Rows[0]["EndTime"] as DateTime?
                };
            }
            return null;
        }
    }
}
