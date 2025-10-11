using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.IServices;
using JLSoft.Wind.Logs;
using JLSoft.Wind.Services;
using Microsoft.Data.SqlClient;
using Sunny.UI;

namespace JLSoft.Wind.Database.ToMes
{
    public static class RemoteDatabaseHelper
    {
        private static IRemoteDatabaseService _remoteService;
        public static void Initialize(IRemoteDatabaseService remoteService)
        {
            _remoteService = remoteService;
        }

        public static async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_remoteService == null)
            {
                LogManager.Log("远程数据库服务未初始化", LogLevel.Error, "RemoteDatabase");
                return false;
            }

            return await _remoteService.TestConnectionAsync(cancellationToken);
        }

        public static async Task<DataTable> ExecuteQueryAsync(string query, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters)
        {
            if (_remoteService == null)
            {
                LogManager.Log("远程数据库服务未初始化", LogLevel.Error, "RemoteDatabase");
                return new DataTable();
            }

            try
            {
                return await _remoteService.ExecuteQueryAsync(query, timeoutSeconds, cancellationToken, parameters);
            }
            catch (Exception ex)
            {
                LogManager.Log($"执行远程查询失败: {ex.Message}", LogLevel.Error, "RemoteDatabase");
                return new DataTable();
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(string commandText, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters)
        {
            if (_remoteService == null)
            {
                LogManager.Log("远程数据库服务未初始化", LogLevel.Error, "RemoteDatabase");
                return -1;
            }

            try
            {
                return await _remoteService.ExecuteNonQueryAsync(commandText, timeoutSeconds, cancellationToken, parameters);
            }
            catch (Exception ex)
            {
                LogManager.Log($"执行远程非查询失败: {ex.Message}", LogLevel.Error, "RemoteDatabase");
                return -1;
            }
        }

        public static async Task<object> ExecuteScalarAsync(string commandText, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters)
        {
            if (_remoteService == null)
            {
                LogManager.Log("远程数据库服务未初始化", LogLevel.Error, "RemoteDatabase");
                return null;
            }

            try
            {
                return await _remoteService.ExecuteScalarAsync(commandText, timeoutSeconds, cancellationToken, parameters);
            }
            catch (Exception ex)
            {
                LogManager.Log($"执行远程标量查询失败: {ex.Message}", LogLevel.Error, "RemoteDatabase");
                return null;
            }
        }

        public static async Task<StoredProcedureResult> ExecuteStoredProcedureAsync(string procedureName, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters)
        {
            if (_remoteService == null)
            {
                LogManager.Log("远程数据库服务未初始化", LogLevel.Error, "RemoteDatabase");
                return new StoredProcedureResult
                {
                    IsSuccess = false,
                    Message = "远程数据库服务未初始化"
                };
            }

            try
            {
                return await _remoteService.ExecuteStoredProcedureAsync(procedureName, timeoutSeconds, cancellationToken, parameters);
            }
            catch (Exception ex)
            {
                LogManager.Log($"执行远程存储过程失败: {ex.Message}", LogLevel.Error, "RemoteDatabase");
                return new StoredProcedureResult
                {
                    IsSuccess = false,
                    Message = $"执行失败: {ex.Message}"
                };
            }
        }

        public static DbParameter CreateParameter(string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            if (_remoteService == null)
            {
                LogManager.Log("远程数据库服务未初始化", LogLevel.Error, "RemoteDatabase");
                return null;
            }

            return _remoteService.CreateParameter(name, value, direction);
        }

        public static DbParameter CreateOutputParameter(string name, SqlDbType dbType)
        {
            if (_remoteService == null)
            {
                LogManager.Log("远程数据库服务未初始化", LogLevel.Error, "RemoteDatabase");
                return null;
            }

            return _remoteService.CreateOutputParameter(name, dbType);
        }
    }
}
