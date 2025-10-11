using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.IServices;
using JLSoft.Wind.Logs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Sunny.UI;

namespace JLSoft.Wind.Services
{
    public class RemoteDatabaseService : IRemoteDatabaseService, IDisposable
    {
        private readonly string _connectionString;
        private readonly int _defaultCommandTimeout;
        private readonly int _connectionTimeout;
        private readonly bool _enableRetry;
        private readonly int _maxRetryCount;
        private readonly int _retryDelayMs;

        private SqlConnection _connection;
        private bool _disposed = false;

        public event EventHandler<DatabaseConnectionEventArgs> ConnectionStateChanged;

        public RemoteDatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("RemoteSQLServer");
            _defaultCommandTimeout = configuration.GetValue<int>("RemoteDatabaseSettings:CommandTimeout", 30);
            _connectionTimeout = configuration.GetValue<int>("RemoteDatabaseSettings:ConnectionTimeout", 15);
            _enableRetry = configuration.GetValue<bool>("RemoteDatabaseSettings:EnableRetry", true);
            _maxRetryCount = configuration.GetValue<int>("RemoteDatabaseSettings:MaxRetryCount", 3);
            _retryDelayMs = configuration.GetValue<int>("RemoteDatabaseSettings:RetryDelayMs", 1000);

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException("远程数据库连接字符串未配置");
            }
        }

        private async Task<SqlConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                return _connection;
            }

            try
            {
                // 关闭现有连接（如果存在）
                if (_connection != null)
                {
                    await _connection.CloseAsync();
                    _connection.Dispose();
                    _connection = null;
                }

                // 创建新连接
                _connection = new SqlConnection(_connectionString);
                _connection.StateChange += OnConnectionStateChange;

                // 使用CancellationToken和超时设置
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_connectionTimeout));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                await _connection.OpenAsync(linkedCts.Token);

                return _connection;
            }
            catch (Exception ex)
            {
                LogManager.Log($"创建数据库连接失败: {ex.Message}", LogLevel.Error, "RemoteDatabase");
                OnConnectionStateChanged(new DatabaseConnectionEventArgs
                {
                    IsConnected = false,
                    Message = "连接失败",
                    Error = ex
                });
                throw;
            }
        }

        private void OnConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            var args = new DatabaseConnectionEventArgs
            {
                IsConnected = e.CurrentState == ConnectionState.Open,
                Message = $"连接状态变化: {e.OriginalState} -> {e.CurrentState}"
            };

            OnConnectionStateChanged(args);
        }

        protected virtual void OnConnectionStateChanged(DatabaseConnectionEventArgs e)
        {
            ConnectionStateChanged?.Invoke(this, e);
        }

        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var testConnection = new SqlConnection(_connectionString);
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_connectionTimeout));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                await testConnection.OpenAsync(linkedCts.Token);
                return testConnection.State == ConnectionState.Open;
            }
            catch (Exception ex)
            {
                LogManager.Log($"连接测试失败: {ex.Message}", LogLevel.Warn, "RemoteDatabase");
                return false;
            }
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters)
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt < (_enableRetry ? _maxRetryCount : 1))
            {
                try
                {
                    attempt++;
                    using var connection = await GetConnectionAsync(cancellationToken);
                    using var command = connection.CreateCommand();

                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = timeoutSeconds;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using var reader = await command.ExecuteReaderAsync(cancellationToken);
                    var result = new DataTable();
                    result.Load(reader);
                    return result;
                }
                catch (Exception ex) when (IsTransientError(ex) && attempt < _maxRetryCount)
                {
                    lastException = ex;
                    LogManager.Log($"查询执行失败 (尝试 {attempt}/{_maxRetryCount}): {ex.Message}", LogLevel.Warn, "RemoteDatabase");

                    if (attempt < _maxRetryCount)
                    {
                        await Task.Delay(_retryDelayMs, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LogManager.Log($"查询执行失败: {ex.Message}", LogLevel.Error, "RemoteDatabase");
                    break;
                }
            }

            throw new Exception("查询执行失败", lastException);
        }

        public async Task<int> ExecuteNonQueryAsync(string commandText, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters)
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt < (_enableRetry ? _maxRetryCount : 1))
            {
                try
                {
                    attempt++;
                    using var connection = await GetConnectionAsync(cancellationToken);
                    using var command = connection.CreateCommand();

                    command.CommandText = commandText;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = timeoutSeconds;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return await command.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (Exception ex) when (IsTransientError(ex) && attempt < _maxRetryCount)
                {
                    lastException = ex;
                    LogManager.Log($"非查询执行失败 (尝试 {attempt}/{_maxRetryCount}): {ex.Message}", LogLevel.Warn, "RemoteDatabase");

                    if (attempt < _maxRetryCount)
                    {
                        await Task.Delay(_retryDelayMs, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LogManager.Log($"非查询执行失败: {ex.Message}", LogLevel.Error, "RemoteDatabase");
                    break;
                }
            }

            throw new Exception("非查询执行失败", lastException);
        }

        public async Task<object> ExecuteScalarAsync(string commandText, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters)
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt < (_enableRetry ? _maxRetryCount : 1))
            {
                try
                {
                    attempt++;
                    using var connection = await GetConnectionAsync(cancellationToken);
                    using var command = connection.CreateCommand();

                    command.CommandText = commandText;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = timeoutSeconds;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return await command.ExecuteScalarAsync(cancellationToken);
                }
                catch (Exception ex) when (IsTransientError(ex) && attempt < _maxRetryCount)
                {
                    lastException = ex;
                    LogManager.Log($"标量查询执行失败 (尝试 {attempt}/{_maxRetryCount}): {ex.Message}", LogLevel.Warn, "RemoteDatabase");

                    if (attempt < _maxRetryCount)
                    {
                        await Task.Delay(_retryDelayMs, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LogManager.Log($"标量查询执行失败: {ex.Message}", LogLevel.Error, "RemoteDatabase");
                    break;
                }
            }

            throw new Exception("标量查询执行失败", lastException);
        }

        public async Task<StoredProcedureResult> ExecuteStoredProcedureAsync(string procedureName, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters)
        {
            var result = new StoredProcedureResult();
            var startTime = DateTime.Now;

            int attempt = 0;
            Exception lastException = null;

            while (attempt < (_enableRetry ? _maxRetryCount : 1))
            {
                try
                {
                    attempt++;
                    using var connection = await GetConnectionAsync(cancellationToken);
                    using var command = connection.CreateCommand();

                    command.CommandText = procedureName;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = timeoutSeconds;

                    // 添加返回值参数
                    var returnParam = command.CreateParameter();
                    returnParam.ParameterName = "@RETURN_VALUE";
                    returnParam.Direction = ParameterDirection.ReturnValue;
                    command.Parameters.Add(returnParam);

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using var reader = await command.ExecuteReaderAsync(cancellationToken);

                    // 读取结果集
                    var resultTable = new DataTable();
                    resultTable.Load(reader);
                    result.ResultData = resultTable;

                    // 获取返回值
                    result.ReturnValue = Convert.ToInt32(returnParam.Value);

                    // 获取输出参数值
                    foreach (DbParameter param in command.Parameters)
                    {
                        if (param.Direction == ParameterDirection.Output ||
                            param.Direction == ParameterDirection.InputOutput)
                        {
                            result.OutputParameters[param.ParameterName] = param.Value;
                        }
                    }

                    result.IsSuccess = true;
                    result.Message = "存储过程执行成功";
                    break;
                }
                catch (Exception ex) when (IsTransientError(ex) && attempt < _maxRetryCount)
                {
                    lastException = ex;
                    LogManager.Log($"存储过程执行失败 (尝试 {attempt}/{_maxRetryCount}): {ex.Message}", LogLevel.Warn, "RemoteDatabase");

                    if (attempt < _maxRetryCount)
                    {
                        await Task.Delay(_retryDelayMs, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LogManager.Log($"存储过程执行失败: {ex.Message}", LogLevel.Error, "RemoteDatabase");
                    result.IsSuccess = false;
                    result.Message = $"执行失败: {ex.Message}";
                    break;
                }
            }

            result.ExecutionDuration = DateTime.Now - startTime;

            if (!result.IsSuccess && lastException != null)
            {
                throw new Exception("存储过程执行失败", lastException);
            }

            return result;
        }

        public DbParameter CreateParameter(string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            var param = new SqlParameter(name, value);
            param.Direction = direction;
            return param;
        }

        public DbParameter CreateOutputParameter(string name, SqlDbType dbType)
        {
            var param = new SqlParameter(name, dbType);
            param.Direction = ParameterDirection.Output;
            return param;
        }

        private bool IsTransientError(Exception ex)
        {
            // 判断是否为暂时性错误（网络问题、超时等）
            if (ex is SqlException sqlEx)
            {
                // SQL Server暂时性错误代码
                int[] transientErrors = {
                    20, 64, 233, 10053, 10054, 10060,
                    10928, 10929, 40197, 40501, 40613
                };

                return Array.Exists(transientErrors, code => code == sqlEx.Number);
            }

            // 网络相关的异常
            if (ex is TimeoutException ||
                ex is System.IO.IOException ||
                (ex.InnerException is System.Net.Sockets.SocketException))
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connection != null)
                {
                    _connection.StateChange -= OnConnectionStateChange;
                    _connection.Dispose();
                    _connection = null;
                }
                _disposed = true;
            }
        }
    }
}
