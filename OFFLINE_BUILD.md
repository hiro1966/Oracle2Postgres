# オフライン環境でのビルド手順

このドキュメントでは、インターネット接続のないオフライン環境でプロジェクトをビルドする方法を説明します。

## 🎯 概要

このプロジェクトは**完全オフライン対応**です。必要なNuGetパッケージをすべて `packages/` フォルダに含めることで、インターネット接続なしでビルド可能です。

## 📋 前提条件

### オンライン環境（準備用）
- Windows 10 以上
- PowerShell 5.1 以上
- NuGet CLI（または Visual Studio 2022）
- インターネット接続

### オフライン環境（実行用）
- Windows 10 以上
- Visual Studio 2022
- .NET 6.0 SDK（Visual Studio 2022に含まれる）

## 🔧 手順

### ステップ1: オンライン環境での準備

#### 方法A: PowerShellスクリプトを使用（推奨）

```powershell
# 1. プロジェクトをクローン
git clone https://github.com/hiro1966/Oracle2Postgres.git
cd Oracle2Postgres

# 2. パッケージダウンロードスクリプトを実行
.\download-packages.ps1

# 3. 完了確認
dir packages
```

#### 方法B: NuGet CLI を使用

```powershell
# 1. プロジェクトをクローン
git clone https://github.com/hiro1966/Oracle2Postgres.git
cd Oracle2Postgres

# 2. NuGetでパッケージを復元
nuget restore OracleToPostgres.sln -PackagesDirectory packages

# 3. 完了確認
dir packages
```

#### 方法C: Visual Studio 2022 を使用

```
1. Visual Studio 2022 でソリューションを開く
2. ツール → NuGet パッケージ マネージャー → パッケージ マネージャー コンソール
3. 以下のコマンドを実行:
   Update-Package -Reinstall -ProjectName OracleToPostgres
4. packages/ フォルダが生成されることを確認
```

### ステップ2: プロジェクトのパッケージング

すべてのファイルを含むフォルダ構造：

```
Oracle2Postgres/
├── OracleToPostgres.sln
├── NuGet.config              # ← 重要！
├── download-packages.ps1
├── README.md
├── CONFIGURATION_EXAMPLES.md
├── OFFLINE_BUILD.md
├── .gitignore
├── OracleToPostgres/
│   ├── OracleToPostgres.csproj
│   ├── App.xaml
│   ├── appsettings.json
│   └── ... (ソースコード)
└── packages/                 # ← 重要！このフォルダごと転送
    ├── README.md
    ├── LiveChartsCore.SkiaSharpView.WPF.2.0.0-rc2/
    ├── Microsoft.Extensions.Configuration.6.0.1/
    ├── Npgsql.6.0.11/
    └── ... (すべてのパッケージと依存関係)
```

**ZIPで圧縮**（推奨）：
```powershell
# プロジェクトルートの親フォルダで実行
Compress-Archive -Path Oracle2Postgres -DestinationPath Oracle2Postgres-Offline.zip
```

### ステップ3: オフライン環境への転送

1. **圧縮ファイルを転送**
   - `Oracle2Postgres-Offline.zip` をUSBメモリやネットワーク共有経由で転送

2. **解凍**
   ```powershell
   Expand-Archive -Path Oracle2Postgres-Offline.zip -DestinationPath C:\Projects\
   ```

### ステップ4: オフライン環境でのビルド

1. **Visual Studio 2022 を起動**

2. **ソリューションを開く**
   ```
   C:\Projects\Oracle2Postgres\OracleToPostgres.sln
   ```

3. **ビルド構成の確認**
   - Visual Studioが自動的に `NuGet.config` を読み込みます
   - `packages/` フォルダからパッケージを参照します

4. **ビルド実行**
   ```
   ビルド → ソリューションのビルド (Ctrl+Shift+B)
   ```

5. **動作確認**
   ```
   デバッグ → デバッグの開始 (F5)
   ```

## ✅ 確認方法

### パッケージが正しく含まれているか確認

```powershell
# packagesフォルダのサイズを確認（200MB以上が目安）
(Get-ChildItem packages -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
```

### NuGet.config の確認

`NuGet.config` ファイルが存在し、以下の内容であることを確認：

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="repositoryPath" value="packages" />
  </config>
  <packageSources>
    <clear />
    <add key="Local" value="packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

## 🔍 トラブルシューティング

### エラー: パッケージが見つからない

**原因**: `packages/` フォルダが正しくコピーされていない

**解決策**:
1. `packages/` フォルダの存在を確認
2. フォルダ内にパッケージが存在するか確認
3. `NuGet.config` ファイルが正しい場所にあるか確認

### エラー: 依存関係が解決できない

**原因**: 依存パッケージが不足している

**解決策**:
```powershell
# オンライン環境で依存関係を完全にダウンロード
nuget restore OracleToPostgres.sln -PackagesDirectory packages -DependencyVersion Highest
```

### Visual Studio がオンラインを参照しようとする

**原因**: NuGet.config の設定が反映されていない

**解決策**:
1. Visual Studio を再起動
2. ツール → オプション → NuGet パッケージ マネージャー → パッケージ ソース
3. "Local" ソースが追加されているか確認

## 📦 必要なパッケージリスト

このプロジェクトで使用する主要パッケージ：

| パッケージ名 | バージョン | サイズ（概算） |
|------------|----------|--------------|
| LiveChartsCore.SkiaSharpView.WPF | 2.0.0-rc2 | 約20MB |
| Npgsql | 6.0.11 | 約5MB |
| Serilog | 3.1.1 | 約500KB |
| Serilog.Sinks.Console | 5.0.1 | 約100KB |
| Serilog.Sinks.File | 5.0.0 | 約100KB |
| System.Data.Odbc | 6.0.0 | 約200KB |
| Microsoft.Extensions.Configuration | 6.0.1 | 約100KB |
| Microsoft.Extensions.Configuration.Json | 6.0.0 | 約100KB |

**合計サイズ（依存関係含む）**: 約200-300MB

## 💡 ベストプラクティス

### 定期的な更新

```powershell
# 3～6ヶ月ごとにパッケージを更新
.\download-packages.ps1
```

### バージョン管理

```
packages/ フォルダは Git リポジトリに含まれます
→ チーム全体で同じバージョンを使用可能
```

### セキュリティ

```
- オフライン環境でもセキュリティパッチが適用されたバージョンを使用
- 定期的にオンライン環境でパッケージを更新
```

## 📞 サポート

問題が発生した場合：

1. `packages/README.md` を確認
2. プロジェクトの `README.md` を確認
3. Visual Studio の出力ウィンドウでエラー詳細を確認

## 🎉 まとめ

✅ オンライン環境で `.\download-packages.ps1` を実行
✅ `packages/` フォルダごとオフライン環境に転送
✅ Visual Studio 2022 で開いてビルド
✅ **インターネット接続不要**

完全オフラインでのビルドが可能です！
