using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ModelGenerator.Services;
using Serilog;

namespace ModelGenerator
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Serilogの設定
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("=== PostgreSQL Model Generator ===");
                Log.Information("データベースからテーブル構造を読み取り、C#モデルを生成します");
                Log.Information("");

                // 設定ファイルの読み込み
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // コマンドライン引数の解析
                string? serverKey = null;
                string? outputDir = null;
                string? namespaceName = null;
                string schemaName = "public";

                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--server" or "-s":
                            if (i + 1 < args.Length)
                                serverKey = args[++i];
                            break;
                        case "--output" or "-o":
                            if (i + 1 < args.Length)
                                outputDir = args[++i];
                            break;
                        case "--namespace" or "-n":
                            if (i + 1 < args.Length)
                                namespaceName = args[++i];
                            break;
                        case "--schema":
                            if (i + 1 < args.Length)
                                schemaName = args[++i];
                            break;
                        case "--help" or "-h":
                            ShowHelp();
                            return 0;
                    }
                }

                // サーバーキーが指定されていない場合
                if (string.IsNullOrEmpty(serverKey))
                {
                    Log.Warning("PostgreSQLサーバーが指定されていません。");
                    Log.Information("利用可能なサーバー:");

                    var servers = configuration.GetSection("PostgresServers").GetChildren();
                    foreach (var server in servers)
                    {
                        var name = server["Name"] ?? server.Key;
                        var host = server["Host"];
                        var db = server["MaintenanceDB"];
                        Log.Information($"  - {server.Key}: {name} ({host}/{db})");
                    }

                    Console.WriteLine();
                    Console.Write("使用するサーバーキーを入力してください: ");
                    serverKey = Console.ReadLine();
                }

                if (string.IsNullOrEmpty(serverKey))
                {
                    Log.Error("サーバーキーが指定されていません");
                    return 1;
                }

                // サーバー情報の取得
                var serverSection = configuration.GetSection($"PostgresServers:{serverKey}");
                if (!serverSection.Exists())
                {
                    Log.Error($"サーバー '{serverKey}' が見つかりません");
                    return 1;
                }

                var host = serverSection["Host"];
                var port = serverSection["Port"];
                var database = serverSection["MaintenanceDB"];
                var username = serverSection["Username"];
                var password = serverSection["Password"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(database))
                {
                    Log.Error("サーバー情報が不完全です");
                    return 1;
                }

                var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

                Log.Information($"接続先: {host}:{port}/{database}");
                Log.Information($"スキーマ: {schemaName}");

                // 出力設定
                outputDir ??= configuration["ModelGeneratorSettings:OutputDirectory"] ?? "GeneratedModels";
                namespaceName ??= configuration["ModelGeneratorSettings:Namespace"] ?? "GeneratedModels";
                var generateDataAnnotations = bool.Parse(configuration["ModelGeneratorSettings:GenerateDataAnnotations"] ?? "true");
                var generateJsonAttributes = bool.Parse(configuration["ModelGeneratorSettings:GenerateJsonAttributes"] ?? "false");
                var useRecordTypes = bool.Parse(configuration["ModelGeneratorSettings:UseRecordTypes"] ?? "false");

                Log.Information($"出力先: {outputDir}");
                Log.Information($"名前空間: {namespaceName}");
                Log.Information("");

                // スキーマ読み取り
                Log.Information("データベーススキーマを読み取り中...");
                var schemaReader = new PostgresSchemaReader(connectionString);
                var tableSchemas = await schemaReader.GetAllTableSchemasAsync(schemaName);

                if (tableSchemas.Count == 0)
                {
                    Log.Warning("テーブルが見つかりませんでした");
                    return 0;
                }

                Log.Information($"{tableSchemas.Count} 個のテーブルが見つかりました:");
                foreach (var table in tableSchemas)
                {
                    Log.Information($"  - {table.TableName} ({table.Columns.Count} カラム)");
                }
                Log.Information("");

                // モデル生成
                var modelGenerator = new CSharpModelGenerator(
                    outputDir,
                    namespaceName,
                    generateDataAnnotations,
                    generateJsonAttributes,
                    useRecordTypes);

                modelGenerator.GenerateModels(tableSchemas);

                Log.Information("");
                Log.Information("✓ モデル生成が完了しました！");
                Log.Information($"出力先: {Path.GetFullPath(outputDir)}");

                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "エラーが発生しました");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("PostgreSQL Model Generator");
            Console.WriteLine();
            Console.WriteLine("使い方:");
            Console.WriteLine("  dotnet run -- [オプション]");
            Console.WriteLine();
            Console.WriteLine("オプション:");
            Console.WriteLine("  -s, --server <key>       PostgreSQLサーバーキー（appsettings.jsonで定義）");
            Console.WriteLine("  -o, --output <dir>       出力ディレクトリ（デフォルト: GeneratedModels）");
            Console.WriteLine("  -n, --namespace <name>   名前空間（デフォルト: appsettings.jsonの設定）");
            Console.WriteLine("  --schema <name>          スキーマ名（デフォルト: public）");
            Console.WriteLine("  -h, --help               このヘルプを表示");
            Console.WriteLine();
            Console.WriteLine("例:");
            Console.WriteLine("  dotnet run -- --server dashboard");
            Console.WriteLine("  dotnet run -- --server dashboard --output Models --namespace MyApp.Models");
            Console.WriteLine();
        }
    }
}
