# packages フォルダについて

このフォルダには、オフライン環境でビルドするために必要なNuGetパッケージが格納されます。

## オンライン環境での準備

### 方法1: PowerShellスクリプトを使用（推奨）

```powershell
# プロジェクトルートで実行
.\download-packages.ps1
```

このスクリプトは以下のパッケージと依存関係を自動ダウンロードします：
- LiveChartsCore.SkiaSharpView.WPF 2.0.0-rc2
- Microsoft.Extensions.Configuration 6.0.1
- Microsoft.Extensions.Configuration.Json 6.0.0
- Npgsql 6.0.11
- Serilog 3.1.1
- Serilog.Sinks.Console 5.0.1
- Serilog.Sinks.File 5.0.0
- System.Data.Odbc 6.0.0

### 方法2: Visual Studioでパッケージ復元

```bash
# コマンドプロンプトまたはPowerShellで実行
cd /path/to/OracleToPostgres
nuget restore OracleToPostgres.sln -PackagesDirectory packages
```

## オフライン環境でのビルド

1. `packages` フォルダごとプロジェクトをオフライン環境にコピー
2. Visual Studio 2022 でソリューションを開く
3. NuGet.config が自動的にローカルパッケージを参照
4. そのままビルド可能

## フォルダ構造

```
packages/
├── LiveChartsCore.SkiaSharpView.WPF.2.0.0-rc2/
├── Microsoft.Extensions.Configuration.6.0.1/
├── Npgsql.6.0.11/
├── Serilog.3.1.1/
└── ... (依存パッケージも含まれます)
```

## 注意事項

- `packages` フォルダは Git リポジトリに含まれます（.gitignore から除外）
- 初回のみオンライン環境でダウンロードが必要
- 以降は完全オフラインで動作可能
