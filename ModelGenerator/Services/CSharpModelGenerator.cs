using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;

namespace ModelGenerator.Services
{
    public class CSharpModelGenerator
    {
        private readonly string _outputDirectory;
        private readonly string _namespace;
        private readonly bool _generateDataAnnotations;
        private readonly bool _generateJsonAttributes;
        private readonly bool _useRecordTypes;

        public CSharpModelGenerator(
            string outputDirectory,
            string namespaceName,
            bool generateDataAnnotations = true,
            bool generateJsonAttributes = false,
            bool useRecordTypes = false)
        {
            _outputDirectory = outputDirectory;
            _namespace = namespaceName;
            _generateDataAnnotations = generateDataAnnotations;
            _generateJsonAttributes = generateJsonAttributes;
            _useRecordTypes = useRecordTypes;
        }

        /// <summary>
        /// テーブルスキーマからC#モデルを生成
        /// </summary>
        public void GenerateModel(TableSchema tableSchema)
        {
            var className = ToPascalCase(tableSchema.TableName);
            var fileName = $"{className}.cs";
            var filePath = Path.Combine(_outputDirectory, fileName);

            // 出力ディレクトリが存在しない場合は作成
            Directory.CreateDirectory(_outputDirectory);

            var code = GenerateClassCode(tableSchema, className);

            File.WriteAllText(filePath, code);
            Log.Information($"モデルファイルを生成しました: {filePath}");
        }

        /// <summary>
        /// 複数のテーブルスキーマからC#モデルを一括生成
        /// </summary>
        public void GenerateModels(List<TableSchema> tableSchemas)
        {
            Log.Information($"=== モデル生成開始: {tableSchemas.Count} 個のテーブル ===");

            foreach (var tableSchema in tableSchemas)
            {
                try
                {
                    GenerateModel(tableSchema);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"テーブル '{tableSchema.TableName}' のモデル生成に失敗しました");
                }
            }

            Log.Information($"=== モデル生成完了 ===");
        }

        private string GenerateClassCode(TableSchema tableSchema, string className)
        {
            var sb = new StringBuilder();

            // Using statements
            var usings = new List<string>
            {
                "using System;"
            };

            if (_generateDataAnnotations)
            {
                usings.Add("using System.ComponentModel.DataAnnotations;");
                usings.Add("using System.ComponentModel.DataAnnotations.Schema;");
            }

            if (_generateJsonAttributes)
            {
                usings.Add("using System.Text.Json.Serialization;");
            }

            foreach (var usingStatement in usings.OrderBy(u => u))
            {
                sb.AppendLine(usingStatement);
            }

            sb.AppendLine();

            // Namespace
            sb.AppendLine($"namespace {_namespace}");
            sb.AppendLine("{");

            // Class or Record
            if (_generateDataAnnotations)
            {
                sb.AppendLine($"    [Table(\"{tableSchema.TableName}\")]");
            }

            if (_useRecordTypes)
            {
                sb.AppendLine($"    public record {className}");
                sb.AppendLine("    {");
            }
            else
            {
                sb.AppendLine($"    public class {className}");
                sb.AppendLine("    {");
            }

            // Properties
            foreach (var column in tableSchema.Columns)
            {
                GenerateProperty(sb, column);
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private void GenerateProperty(StringBuilder sb, ColumnInfo column)
        {
            var propertyName = ToPascalCase(column.ColumnName);
            var csharpType = MapPostgresToCSharpType(column);

            // Data Annotations
            if (_generateDataAnnotations)
            {
                if (column.IsPrimaryKey)
                {
                    sb.AppendLine("        [Key]");
                }

                if (!column.IsNullable && csharpType != "string")
                {
                    sb.AppendLine("        [Required]");
                }

                if (column.MaxLength.HasValue && csharpType == "string")
                {
                    sb.AppendLine($"        [MaxLength({column.MaxLength.Value})]");
                }

                if (column.ColumnName != propertyName.ToLower() && column.ColumnName != ToSnakeCase(propertyName))
                {
                    sb.AppendLine($"        [Column(\"{column.ColumnName}\")]");
                }
            }

            // JSON Attributes
            if (_generateJsonAttributes)
            {
                sb.AppendLine($"        [JsonPropertyName(\"{column.ColumnName}\")]");
            }

            // Property declaration
            sb.AppendLine($"        public {csharpType} {propertyName} {{ get; set; }}");
            sb.AppendLine();
        }

        private string MapPostgresToCSharpType(ColumnInfo column)
        {
            var baseType = column.DataType.ToLower() switch
            {
                "integer" or "int" or "int4" => "int",
                "bigint" or "int8" => "long",
                "smallint" or "int2" => "short",
                "decimal" or "numeric" => "decimal",
                "real" or "float4" => "float",
                "double precision" or "float8" => "double",
                "boolean" or "bool" => "bool",
                "character" or "char" => "string",
                "character varying" or "varchar" or "text" => "string",
                "timestamp" or "timestamp without time zone" => "DateTime",
                "timestamp with time zone" or "timestamptz" => "DateTimeOffset",
                "date" => "DateTime",
                "time" or "time without time zone" => "TimeSpan",
                "uuid" => "Guid",
                "bytea" => "byte[]",
                "json" or "jsonb" => "string",
                "money" => "decimal",
                _ => "object"
            };

            // Nullable types
            if (column.IsNullable && baseType != "string" && baseType != "byte[]" && baseType != "object")
            {
                return $"{baseType}?";
            }

            return baseType;
        }

        private string ToPascalCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // スネークケースをパスカルケースに変換
            var words = text.Split('_')
                .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower());

            return string.Join("", words);
        }

        private string ToSnakeCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var sb = new StringBuilder();
            sb.Append(char.ToLower(text[0]));

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    sb.Append('_');
                    sb.Append(char.ToLower(text[i]));
                }
                else
                {
                    sb.Append(text[i]);
                }
            }

            return sb.ToString();
        }
    }
}
