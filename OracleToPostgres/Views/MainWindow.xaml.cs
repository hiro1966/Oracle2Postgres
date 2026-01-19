using System;
using System.Threading.Tasks;
using System.Windows;
using OracleToPostgres.Services;
using OracleToPostgres.ViewModels;
using Serilog;

namespace OracleToPostgres.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ConfigurationService _configService;

        public MainWindow()
        {
            InitializeComponent();

            _configService = new ConfigurationService();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // ログの初期化
            InitializeLogging();

            // ウィンドウロード時に自動実行
            Loaded += MainWindow_Loaded;
        }

        private void InitializeLogging()
        {
            var logFilePath = _configService.GetLogFilePath();
            var logLevel = _configService.GetMinimumLogLevel();

            var levelSwitch = new Serilog.Core.LoggingLevelSwitch();
            levelSwitch.MinimumLevel = logLevel switch
            {
                "Debug" => Serilog.Events.LogEventLevel.Debug,
                "Information" => Serilog.Events.LogEventLevel.Information,
                "Warning" => Serilog.Events.LogEventLevel.Warning,
                "Error" => Serilog.Events.LogEventLevel.Error,
                _ => Serilog.Events.LogEventLevel.Information
            };

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.File(logFilePath, 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("アプリケーション起動");
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 少し待ってからUIを更新
            await Task.Delay(500);

            // データ転送を自動開始
            await StartDataTransferAsync();
        }

        private async Task StartDataTransferAsync()
        {
            try
            {
                _viewModel.StatusMessage = "データ転送を準備中...";
                _viewModel.IsProcessing = true;
                _viewModel.AddLogMessage("データ転送処理を開始します");

                // 設定を読み込み
                var oracleConn = _configService.GetOracleConnectionString();
                var postgresConn = _configService.GetPostgresConnectionString();
                var oracleQuery = _configService.GetOracleQuery();
                var postgresTable = _configService.GetPostgresTableName();
                var batchSize = _configService.GetBatchSize();

                // データ転送サービスの作成
                var transferService = new DataTransferService(
                    oracleConn,
                    postgresConn,
                    oracleQuery,
                    postgresTable,
                    batchSize);

                // イベントハンドラの設定
                transferService.ProgressChanged += (s, args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        _viewModel.TotalRecords = args.TotalRecords;
                        _viewModel.ProcessedRecords = args.ProcessedRecords;
                        _viewModel.StatusMessage = $"データ転送中... ({args.ProcessedRecords}/{args.TotalRecords})";
                    });
                };

                transferService.LogMessageReceived += (s, message) =>
                {
                    _viewModel.AddLogMessage(message);
                };

                // データ転送実行
                var result = await Task.Run(() => transferService.TransferDataAsync());

                // 結果の表示
                if (result.IsSuccess)
                {
                    _viewModel.StatusMessage = "データ転送完了！";
                    _viewModel.AddLogMessage($"✓ 正常に完了しました ({result.ProcessedRecords} 件)");
                    _viewModel.AddLogMessage($"処理時間: {result.Duration.TotalSeconds:F2} 秒");
                    
                    Log.Information("データ転送が正常に完了しました");

                    // 自動終了の処理
                    if (_configService.GetAutoCloseOnCompletion())
                    {
                        var delaySeconds = _configService.GetCloseDelaySeconds();
                        _viewModel.AddLogMessage($"{delaySeconds}秒後にアプリケーションを終了します...");
                        
                        await Task.Delay(delaySeconds * 1000);
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    _viewModel.StatusMessage = "エラーが発生しました";
                    _viewModel.AddLogMessage($"✗ エラー: {result.ErrorMessage}");
                    
                    Log.Error("データ転送中にエラーが発生: {Error}", result.ErrorMessage);
                    
                    MessageBox.Show(
                        $"データ転送中にエラーが発生しました:\n\n{result.ErrorMessage}",
                        "エラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _viewModel.StatusMessage = "予期しないエラーが発生しました";
                _viewModel.AddLogMessage($"✗ 予期しないエラー: {ex.Message}");
                
                Log.Error(ex, "予期しないエラーが発生しました");
                
                MessageBox.Show(
                    $"予期しないエラーが発生しました:\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _viewModel.IsProcessing = false;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Log.Information("アプリケーション終了");
            Log.CloseAndFlush();
            base.OnClosed(e);
        }
    }
}
