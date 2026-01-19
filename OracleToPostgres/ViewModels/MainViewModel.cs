using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace OracleToPostgres.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Dispatcher _dispatcher;
        private int _totalRecords;
        private int _processedRecords;
        private string _statusMessage = "準備中...";
        private bool _isProcessing;
        private readonly ObservableCollection<ISeries> _series;

        public MainViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            LogMessages = new ObservableCollection<string>();
            
            // グラフの初期化
            _series = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = new ObservableCollection<double> { 0 },
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0,
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 2 }
                }
            };

            Series = _series;
        }

        public ObservableCollection<string> LogMessages { get; }

        public ObservableCollection<ISeries> Series { get; }

        public int TotalRecords
        {
            get => _totalRecords;
            set => SetProperty(ref _totalRecords, value);
        }

        public int ProcessedRecords
        {
            get => _processedRecords;
            set
            {
                if (SetProperty(ref _processedRecords, value))
                {
                    OnPropertyChanged(nameof(ProgressPercentage));
                    UpdateChart();
                }
            }
        }

        public double ProgressPercentage =>
            TotalRecords > 0 ? (double)ProcessedRecords / TotalRecords * 100 : 0;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public void AddLogMessage(string message)
        {
            _dispatcher.Invoke(() =>
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                LogMessages.Add($"[{timestamp}] {message}");
                
                // ログが多くなりすぎないように制限
                if (LogMessages.Count > 100)
                {
                    LogMessages.RemoveAt(0);
                }
            });
        }

        private void UpdateChart()
        {
            _dispatcher.Invoke(() =>
            {
                if (_series.Count > 0 && _series[0] is LineSeries<double> series)
                {
                    var values = series.Values as ObservableCollection<double>;
                    values?.Add(ProgressPercentage);

                    // チャートのデータポイントを制限
                    if (values != null && values.Count > 50)
                    {
                        values.RemoveAt(0);
                    }
                }
            });
        }
    }
}
