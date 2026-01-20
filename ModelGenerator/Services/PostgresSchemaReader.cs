using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Serilog;

namespace ModelGenerator.Services
{
    public class PostgresSchemaReader
    {
        private readonly string _connectionString;

        public PostgresSchemaReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// データベース内の全テーブル名を取得
        /// </summary>
        public async Task<List<string>> GetAllTablesAsync(string schemaName = "public")
        {
            var tables = new List<string>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = @schema 
                  AND table_type = 'BASE TABLE'
                ORDER BY table_name";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@schema", schemaName);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            Log.Information($"データベースから {tables.Count} 個のテーブルを検出しました");
            return tables;
        }

        /// <summary>
        /// 指定されたテーブルのカラム情報を取得
        /// </summary>
        public async Task<TableSchema> GetTableSchemaAsync(string tableName, string schemaName = "public")
        {
            var tableSchema = new TableSchema
            {
                TableName = tableName,
                SchemaName = schemaName,
                Columns = new List<ColumnInfo>()
            };

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    c.column_name,
                    c.data_type,
                    c.is_nullable,
                    c.column_default,
                    c.character_maximum_length,
                    c.numeric_precision,
                    c.numeric_scale,
                    (
                        SELECT COUNT(*) 
                        FROM information_schema.table_constraints tc
                        JOIN information_schema.key_column_usage kcu 
                          ON tc.constraint_name = kcu.constraint_name
                         AND tc.table_schema = kcu.table_schema
                        WHERE tc.constraint_type = 'PRIMARY KEY'
                          AND tc.table_schema = @schema
                          AND tc.table_name = @table
                          AND kcu.column_name = c.column_name
                    ) as is_primary_key
                FROM information_schema.columns c
                WHERE c.table_schema = @schema
                  AND c.table_name = @table
                ORDER BY c.ordinal_position";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@schema", schemaName);
            command.Parameters.AddWithValue("@table", tableName);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var columnInfo = new ColumnInfo
                {
                    ColumnName = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetString(2) == "YES",
                    ColumnDefault = reader.IsDBNull(3) ? null : reader.GetString(3),
                    MaxLength = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    NumericPrecision = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    NumericScale = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    IsPrimaryKey = reader.GetInt64(7) > 0
                };

                tableSchema.Columns.Add(columnInfo);
            }

            Log.Information($"テーブル '{tableName}' から {tableSchema.Columns.Count} 個のカラムを読み取りました");
            return tableSchema;
        }

        /// <summary>
        /// データベース内の全テーブルのスキーマを取得
        /// </summary>
        public async Task<List<TableSchema>> GetAllTableSchemasAsync(string schemaName = "public")
        {
            var schemas = new List<TableSchema>();
            var tables = await GetAllTablesAsync(schemaName);

            foreach (var table in tables)
            {
                var tableSchema = await GetTableSchemaAsync(table, schemaName);
                schemas.Add(tableSchema);
            }

            return schemas;
        }
    }

    public class TableSchema
    {
        public string TableName { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
    }

    public class ColumnInfo
    {
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public string? ColumnDefault { get; set; }
        public int? MaxLength { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}
