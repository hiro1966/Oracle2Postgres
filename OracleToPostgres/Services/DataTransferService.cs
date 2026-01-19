using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Threading.Tasks;
using Npgsql;
using Serilog;

namespace OracleToPostgres.Services
{
    public class DataTransferService
    {
        private readonly string _oracleConnectionString;
        private readonly string _postgresConnectionString;
        private readonly string _oracleQuery;
        private readonly string _postgresTableName;
        private readonly int _batchSize;

        public event EventHandler<ProgressEventArgs>? ProgressChanged;
        public event EventHandler<string>? LogMessageReceived;

        public DataTransferService(
            string oracleConnectionString,
            string postgresConnectionString,
            string oracleQuery,
            string postgresTableName,
            int batchSize)
        {
            _oracleConnectionString = oracleConnectionString;
            _postgresConnectionString = postgresConnectionString;
            _oracleQuery = oracleQuery;
            _postgresTableName = postgresTableName;
            _batchSize = batchSize;
        }

        public async Task<TransferResult> TransferDataAsync()
        {
            var result = new TransferResult();
            var startTime = DateTime.Now;

            try
            {
                LogMessage("データ転送を開始します...");
                LogMessage($"Oracle接続: {MaskConnectionString(_oracleConnectionString)}");
                LogMessage($"PostgreSQL接続: {MaskConnectionString(_postgresConnectionString)}");

                // Oracleからデータを読み込み
                var dataTable = await ReadFromOracleAsync();
                result.TotalRecords = dataTable.Rows.Count;

                LogMessage($"Oracleから {result.TotalRecords} 件のレコードを読み込みました");

                // PostgreSQLへデータを書き込み
                await WriteToPostgresAsync(dataTable, result);

                result.IsSuccess = true;
                result.Duration = DateTime.Now - startTime;
                LogMessage($"データ転送完了: {result.ProcessedRecords}/{result.TotalRecords} 件");
                LogMessage($"処理時間: {result.Duration.TotalSeconds:F2} 秒");
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                result.Duration = DateTime.Now - startTime;
                
                Log.Error(ex, "データ転送中にエラーが発生しました");
                LogMessage($"エラー: {ex.Message}");
            }

            return result;
        }

        private async Task<DataTable> ReadFromOracleAsync()
        {
            var dataTable = new DataTable();

            await Task.Run(() =>
            {
                using var connection = new OdbcConnection(_oracleConnectionString);
                connection.Open();
                LogMessage("Oracle接続成功");

                using var command = new OdbcCommand(_oracleQuery, connection);
                command.CommandTimeout = 300; // 5分

                using var adapter = new OdbcDataAdapter(command);
                adapter.Fill(dataTable);
            });

            return dataTable;
        }

        private async Task WriteToPostgresAsync(DataTable dataTable, TransferResult result)
        {
            await using var connection = new NpgsqlConnection(_postgresConnectionString);
            await connection.OpenAsync();
            LogMessage("PostgreSQL接続成功");

            // テーブルが存在しない場合は作成
            await CreateTableIfNotExistsAsync(connection, dataTable);

            var processedCount = 0;
            var batch = new List<DataRow>();

            foreach (DataRow row in dataTable.Rows)
            {
                batch.Add(row);

                if (batch.Count >= _batchSize)
                {
                    await InsertBatchAsync(connection, batch, dataTable.Columns);
                    processedCount += batch.Count;
                    result.ProcessedRecords = processedCount;
                    
                    OnProgressChanged(new ProgressEventArgs(result.TotalRecords, processedCount));
                    LogMessage($"進捗: {processedCount}/{result.TotalRecords} 件処理完了");

                    batch.Clear();
                }
            }

            // 残りのデータを挿入
            if (batch.Count > 0)
            {
                await InsertBatchAsync(connection, batch, dataTable.Columns);
                processedCount += batch.Count;
                result.ProcessedRecords = processedCount;
                
                OnProgressChanged(new ProgressEventArgs(result.TotalRecords, processedCount));
                LogMessage($"進捗: {processedCount}/{result.TotalRecords} 件処理完了");
            }
        }

        private async Task CreateTableIfNotExistsAsync(NpgsqlConnection connection, DataTable dataTable)
        {
            var columns = new List<string>();

            foreach (DataColumn column in dataTable.Columns)
            {
                var pgType = MapToPgType(column.DataType);
                columns.Add($"\"{column.ColumnName}\" {pgType}");
            }

            var createTableSql = $@"
                CREATE TABLE IF NOT EXISTS ""{_postgresTableName}"" (
                    {string.Join(",\n                    ", columns)}
                )";

            await using var command = new NpgsqlCommand(createTableSql, connection);
            await command.ExecuteNonQueryAsync();
            
            LogMessage($"テーブル '{_postgresTableName}' を確認/作成しました");
        }

        private async Task InsertBatchAsync(NpgsqlConnection connection, List<DataRow> batch, DataColumnCollection columns)
        {
            var columnNames = new List<string>();
            foreach (DataColumn column in columns)
            {
                columnNames.Add($"\"{column.ColumnName}\"");
            }

            var valuePlaceholders = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            for (int i = 0; i < batch.Count; i++)
            {
                var rowPlaceholders = new List<string>();
                for (int j = 0; j < columns.Count; j++)
                {
                    var paramName = $"@p{i}_{j}";
                    rowPlaceholders.Add(paramName);
                    parameters.Add(new NpgsqlParameter(paramName, batch[i][j] ?? DBNull.Value));
                }
                valuePlaceholders.Add($"({string.Join(", ", rowPlaceholders)})");
            }

            var insertSql = $@"
                INSERT INTO ""{_postgresTableName}"" ({string.Join(", ", columnNames)})
                VALUES {string.Join(", ", valuePlaceholders)}";

            await using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddRange(parameters.ToArray());
            await command.ExecuteNonQueryAsync();
        }

        private string MapToPgType(Type dotNetType)
        {
            return dotNetType.Name switch
            {
                "Int16" => "SMALLINT",
                "Int32" => "INTEGER",
                "Int64" => "BIGINT",
                "Decimal" => "NUMERIC",
                "Double" => "DOUBLE PRECISION",
                "Single" => "REAL",
                "Boolean" => "BOOLEAN",
                "DateTime" => "TIMESTAMP",
                "String" => "TEXT",
                "Byte[]" => "BYTEA",
                _ => "TEXT"
            };
        }

        private string MaskConnectionString(string connectionString)
        {
            // パスワードをマスク
            return System.Text.RegularExpressions.Regex.Replace(
                connectionString,
                @"(Pwd|Password)=([^;]+)",
                "$1=****",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private void OnProgressChanged(ProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        private void LogMessage(string message)
        {
            Log.Information(message);
            LogMessageReceived?.Invoke(this, message);
        }
    }

    public class ProgressEventArgs : EventArgs
    {
        public int TotalRecords { get; }
        public int ProcessedRecords { get; }

        public ProgressEventArgs(int totalRecords, int processedRecords)
        {
            TotalRecords = totalRecords;
            ProcessedRecords = processedRecords;
        }
    }

    public class TransferResult
    {
        public bool IsSuccess { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public TimeSpan Duration { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
