using System;
using System.Data;
using System.Data.Odbc;
using System.Threading.Tasks;
using Serilog;

namespace OracleToPostgres.Services
{
    /// <summary>
    /// 本番のOracle接続を行うデータソース
    /// </summary>
    public class OracleDataSource : IDataSource
    {
        private readonly string _connectionString;

        public OracleDataSource(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<DataTable> ReadDataAsync(string query, string taskName)
        {
            return await Task.Run(() =>
            {
                var dataTable = new DataTable();

                using var connection = new OdbcConnection(_connectionString);
                connection.Open();
                Log.Information($"[{taskName}] Oracle接続成功");

                using var command = new OdbcCommand(query, connection);
                command.CommandTimeout = 300; // 5分

                using var adapter = new OdbcDataAdapter(command);
                adapter.Fill(dataTable);

                return dataTable;
            });
        }
    }
}
