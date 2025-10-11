using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Services;

namespace JLSoft.Wind.IServices
{
    public interface IRemoteDatabaseService
    {
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
        Task<DataTable> ExecuteQueryAsync(string query, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters);
        Task<int> ExecuteNonQueryAsync(string commandText, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters);
        Task<object> ExecuteScalarAsync(string commandText, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters);
        Task<StoredProcedureResult> ExecuteStoredProcedureAsync(string procedureName, int timeoutSeconds = 30, CancellationToken cancellationToken = default, params DbParameter[] parameters);

        DbParameter CreateParameter(string name, object value, ParameterDirection direction = ParameterDirection.Input);
        DbParameter CreateOutputParameter(string name, SqlDbType dbType);

        event EventHandler<DatabaseConnectionEventArgs> ConnectionStateChanged;
    }

    public class DatabaseConnectionEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }
    }
}
