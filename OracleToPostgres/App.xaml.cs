using System;
using System.Windows;
using Serilog;

namespace OracleToPostgres
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // グローバルな例外ハンドラを設定
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            Log.Information("=== アプリケーション起動 ===");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.Fatal(ex, "未処理の例外が発生しました");
                MessageBox.Show(
                    $"致命的なエラーが発生しました:\n\n{ex.Message}",
                    "致命的なエラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "UI スレッドで未処理の例外が発生しました");
            
            MessageBox.Show(
                $"エラーが発生しました:\n\n{e.Exception.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("=== アプリケーション終了 ===");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
