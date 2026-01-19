# NuGetパッケージのダウンロードスクリプト
# オンライン環境で一度実行して、packagesフォルダを作成

$packages = @(
    @{Id="LiveChartsCore.SkiaSharpView.WPF"; Version="2.0.0-rc2"},
    @{Id="Microsoft.Extensions.Configuration"; Version="6.0.1"},
    @{Id="Microsoft.Extensions.Configuration.Json"; Version="6.0.0"},
    @{Id="Npgsql"; Version="6.0.11"},
    @{Id="Serilog"; Version="3.1.1"},
    @{Id="Serilog.Sinks.Console"; Version="5.0.1"},
    @{Id="Serilog.Sinks.File"; Version="5.0.0"},
    @{Id="System.Data.Odbc"; Version="6.0.0"}
)

$outputDir = ".\packages"

Write-Host "NuGetパッケージをダウンロードしています..." -ForegroundColor Green
Write-Host "出力先: $outputDir" -ForegroundColor Yellow

foreach ($pkg in $packages) {
    $id = $pkg.Id
    $version = $pkg.Version
    Write-Host "`nダウンロード中: $id $version" -ForegroundColor Cyan
    
    nuget install $id -Version $version -OutputDirectory $outputDir -DependencyVersion Highest
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ 完了: $id" -ForegroundColor Green
    } else {
        Write-Host "✗ エラー: $id" -ForegroundColor Red
    }
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "全パッケージのダウンロードが完了しました！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "`npackagesフォルダをオフライン環境にコピーしてください。" -ForegroundColor Yellow
