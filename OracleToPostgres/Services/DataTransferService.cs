using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Threading.Tasks;
using Npgsql;
using Serilog;
using OracleToPostgres.Models;

namespace OracleToPostgres.Services
{
    public class DataTransferService
    {
        private readonly string _oracleConnectionString;
        private readonly string _defaultPostgresConnectionString;
        private readonly int _batchSize;
        private readonly DataTransformService _transformService;
        private readonly ConfigurationService _configService;

        public event EventHandler<ProgressEventArgs>? ProgressChanged;
        public event EventHandler<string>? LogMessageReceived;
        public event EventHandler<TaskProgressEventArgs>? TaskProgressChanged;

        public DataTransferService(
            string oracleConnectionString,
            string postgresConnectionString,
            int batchSize,
            ConfigurationService configService)
        {
            _oracleConnectionString = oracleConnectionString;
            _defaultPostgresConnectionString = postgresConnectionString;
            _batchSize = batchSize;
            _configService = configService;
            _transformService = new DataTransformService();
        }

        /// <summary>
        /// 複数のタスクを順次実行してデータ転送を行う
        /// </summary>
        public async Task<MultiTaskTransferResult> TransferDataAsync(List<DataTransferTask> tasks)
        {
            var multiResult = new MultiTaskTransferResult();
            var startTime = DateTime.Now;

            try
            {
                LogMessage($"データ転送を開始します（タスク数: {tasks.Count}）");
                LogMessage($"Oracle接続: {MaskConnectionString(_oracleConnectionString)}");

                multiResult.TotalTasks = tasks.Count;

                foreach (var task in tasks)
                {
                    LogMessage($"========================================");
                    LogMessage($"タスク開始: {task.TaskName}");
                    LogMessage($"========================================");

                    var taskResult = await TransferSingleTaskAsync(task);
                    multiResult.TaskResults.Add(taskResult);

                    multiResult.CompletedTasks++;
                    multiResult.TotalRecordsProcessed += taskResult.ProcessedRecords;

                    OnTaskProgressChanged(new TaskProgressEventArgs(
                        task.TaskName,
                        multiResult.CompletedTasks,
                        multiResult.TotalTasks,
                        taskResult.IsSuccess
                    ));

                    if (!taskResult.IsSuccess)
                    {
                        LogMessage($"⚠ タスク '{task.TaskName}' でエラーが発生しましたが、次のタスクを続行します");
                    }
                }

                multiResult.IsSuccess = multiResult.TaskResults.TrueForAll(r => r.IsSuccess);
                multiResult.Duration = DateTime.Now - startTime;

                LogMessage($"========================================");
                LogMessage($"全タスク完了: {multiResult.CompletedTasks}/{multiResult.TotalTasks}");
                LogMessage($"総処理レコード数: {multiResult.TotalRecordsProcessed} 件");
                LogMessage($"総処理時間: {multiResult.Duration.TotalSeconds:F2} 秒");
                LogMessage($"========================================");
            }
            catch (Exception ex)
            {
                multiResult.IsSuccess = false;
                multiResult.ErrorMessage = ex.Message;
                multiResult.Duration = DateTime.Now - startTime;

                Log.Error(ex, "データ転送中に予期しないエラーが発生しました");
                LogMessage($"エラー: {ex.Message}");
            }

            return multiResult;
        }

        /// <summary>
        /// 単一のタスクを実行
        /// </summary>
        private async Task<TransferResult> TransferSingleTaskAsync(DataTransferTask task)
        {
            var result = new TransferResult { TaskName = task.TaskName };
            var startTime = DateTime.Now;

            try
            {
                // PostgreSQL接続文字列を取得
                string postgresConnectionString;
                if (!string.IsNullOrEmpty(task.PostgresServerKey))
                {
                    var serverInfo = _configService.GetPostgresServer(task.PostgresServerKey);
                    if (serverInfo != null)
                    {
                        postgresConnectionString = _configService.BuildPostgresConnectionString(serverInfo);
                        LogMessage($"[{task.TaskName}] PostgreSQLサーバー: {serverInfo.Name} ({serverInfo.Host}:{serverInfo.Port}/{serverInfo.MaintenanceDB})");
                    }
                    else
                    {
                        LogMessage($"[{task.TaskName}] ⚠ PostgreSQLサーバーキー '{task.PostgresServerKey}' が見つかりません。デフォルト接続を使用します。");
                        postgresConnectionString = _defaultPostgresConnectionString;
                    }
                }
                else
                {
                    postgresConnectionString = _defaultPostgresConnectionString;
                    LogMessage($"[{task.TaskName}] デフォルトのPostgreSQL接続を使用します");
                }

                // 1. データの読み込み（Oracle）
                LogMessage($"[{task.TaskName}] Oracleからデータを読み込んでいます...");
                var dataTable = await ReadFromOracleAsync(task.OracleQuery, task.TaskName);
                result.TotalRecords = dataTable.Rows.Count;
                LogMessage($"[{task.TaskName}] {result.TotalRecords} 件のレコードを読み込みました");

                // 2. データの変換
                if (task.EnableTransform)
                {
                    LogMessage($"[{task.TaskName}] データ変換を実行しています...");
                    dataTable = _transformService.Transform(dataTable, task.TaskName);
                    LogMessage($"[{task.TaskName}] データ変換が完了しました");
                }
                else
                {
                    LogMessage($"[{task.TaskName}] データ変換はスキップされます");
                }

                // 3. データの書き込み（PostgreSQL）
                LogMessage($"[{task.TaskName}] PostgreSQLへデータを書き込んでいます...");
                await WriteToPostgresAsync(dataTable, task.PostgresTableName, task.TaskName, result, postgresConnectionString);

                result.IsSuccess = true;
                result.Duration = DateTime.Now - startTime;
                LogMessage($"[{task.TaskName}] 完了: {result.ProcessedRecords}/{result.TotalRecords} 件（{result.Duration.TotalSeconds:F2} 秒）");
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                result.Duration = DateTime.Now - startTime;

                Log.Error(ex, $"[{task.TaskName}] データ転送中にエラーが発生しました");
                LogMessage($"[{task.TaskName}] エラー: {ex.Message}");
            }

            return result;
        }

        private async Task<DataTable> ReadFromOracleAsync(string query, string taskName)
        {
            var dataTable = new DataTable();

            await Task.Run(() =>
            {
                using var connection = new OdbcConnection(_oracleConnectionString);
                connection.Open();
                LogMessage($"[{taskName}] Oracle接続成功");

                using var command = new OdbcCommand(query, connection);
                command.CommandTimeout = 300; // 5分

                using var adapter = new OdbcDataAdapter(command);
                adapter.Fill(dataTable);
            });

            return dataTable;
        }

        private async Task WriteToPostgresAsync(DataTable dataTable, string tableName, string taskName, TransferResult result, string connectionString)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            LogMessage($"[{taskName}] PostgreSQL接続成功");

            // テーブルが存在しない場合は作成
            await CreateTableIfNotExistsAsync(connection, dataTable, tableName, taskName);

            var processedCount = 0;
            var batch = new List<DataRow>();

            foreach (DataRow row in dataTable.Rows)
            {
                batch.Add(row);

                if (batch.Count >= _batchSize)
                {
                    await InsertBatchAsync(connection, batch, dataTable.Columns, tableName);
                    processedCount += batch.Count;
                    result.ProcessedRecords = processedCount;

                    OnProgressChanged(new ProgressEventArgs(result.TotalRecords, processedCount));
                    LogMessage($"[{taskName}] 進捗: {processedCount}/{result.TotalRecords} 件処理完了");

                    batch.Clear();
                }
            }

            // 残りのデータを挿入
            if (batch.Count > 0)
            {
                await InsertBatchAsync(connection, batch, dataTable.Columns, tableName);
                processedCount += batch.Count;
                result.ProcessedRecords = processedCount;

                OnProgressChanged(new ProgressEventArgs(result.TotalRecords, processedCount));
                LogMessage($"[{taskName}] 進捗: {processedCount}/{result.TotalRecords} 件処理完了");
            }
        }

        private async Task CreateTableIfNotExistsAsync(NpgsqlConnection connection, DataTable dataTable, string tableName, string taskName)
        {
            var columns = new List<string>();

            foreach (DataColumn column in dataTable.Columns)
            {
                var pgType = MapToPgType(column.DataType);
                columns.Add($"\"{column.ColumnName}\" {pgType}");
            }

            var createTableSql = $@"
                CREATE TABLE IF NOT EXISTS ""{tableName}"" (
                    {string.Join(",\n                    ", columns)}
                )";

            await using var command = new NpgsqlCommand(createTableSql, connection);
            await command.ExecuteNonQueryAsync();

            LogMessage($"[{taskName}] テーブル '{tableName}' を確認/作成しました");
        }

        private async Task InsertBatchAsync(NpgsqlConnection connection, List<DataRow> batch, DataColumnCollection columns, string tableName)
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
                INSERT INTO ""{tableName}"" ({string.Join(", ", columnNames)})
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

        private void OnTaskProgressChanged(TaskProgressEventArgs e)
        {
            TaskProgressChanged?.Invoke(this, e);
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

    public class TaskProgressEventArgs : EventArgs
    {
        public string TaskName { get; }
        public int CompletedTasks { get; }
        public int TotalTasks { get; }
        public bool IsSuccess { get; }

        public TaskProgressEventArgs(string taskName, int completedTasks, int totalTasks, bool isSuccess)
        {
            TaskName = taskName;
            CompletedTasks = completedTasks;
            TotalTasks = totalTasks;
            IsSuccess = isSuccess;
        }
    }

    public class TransferResult
    {
        public string TaskName { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public TimeSpan Duration { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class MultiTaskTransferResult
    {
        public bool IsSuccess { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalRecordsProcessed { get; set; }
        public TimeSpan Duration { get; set; }
        public string? ErrorMessage { get; set; }
        public List<TransferResult> TaskResults { get; set; } = new List<TransferResult>();
    }
}
