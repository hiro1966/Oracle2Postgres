# Oracle to PostgreSQL Data Transfer Tool

Windows 10用のデスクトップアプリケーションで、OracleデータベースからPostgreSQLへのデータ転送を行います。

## 機能

- ✅ ODBC経由でOracleデータベースに接続
- ✅ PostgreSQLへのデータ書き込み（Npgsql使用）
- ✅ **複数クエリ対応**（複数テーブルの一括転送）
- ✅ **データ変換機能**（カスタマイズ可能な変換ロジック）
- ✅ リアルタイム進捗グラフ表示（LiveCharts2）
- ✅ 詳細なログ出力（Serilog）
- ✅ 自動起動・自動終了機能
- ✅ バッチ処理でパフォーマンス最適化
- ✅ エラーハンドリングと詳細ログ
- ✅ **完全オフライン環境対応**（NuGetパッケージ同梱）

## 必要な環境

### 開発環境
- **Visual Studio 2022**
- **.NET 6.0 SDK**
- **Windows 10** 以上

### ランタイム要件
- **.NET 6.0 Runtime** (Desktop)
- **Oracle ODBC Driver** (Oracle Instant Client など)
- **PostgreSQL** (接続先サーバー)

## セットアップ手順

### 1. Visual Studio 2022で開く

```
OracleToPostgres.sln をダブルクリック
```

### 2. NuGetパッケージの準備

#### オンライン環境の場合

Visual Studioが自動的にNuGetパッケージを復元します。
手動で復元する場合：

```
ソリューションを右クリック → "NuGetパッケージの復元"
```

#### 🔒 オフライン環境の場合（推奨）

**このプロジェクトは完全オフライン対応です！**

1. **オンライン環境で準備**（初回のみ）
   ```powershell
   # プロジェクトルートで実行
   .\download-packages.ps1
   ```
   または
   ```bash
   nuget restore OracleToPostgres.sln -PackagesDirectory packages
   ```

2. **オフライン環境へ転送**
   - プロジェクトフォルダ全体（`packages/` フォルダを含む）をコピー
   - Visual Studio 2022 で開く
   - **インターネット接続不要でビルド可能**

詳細は `packages/README.md` を参照してください。

### 3. Oracle ODBCドライバのインストール

Oracle公式サイトから **Oracle Instant Client** をダウンロード＆インストール：

https://www.oracle.com/database/technologies/instant-client/downloads.html

必要なコンポーネント：
- Basic Package
- ODBC Package

インストール後、ODBCドライバを確認：
```
コントロールパネル → 管理ツール → ODBCデータソース(64ビット)
```

### 4. 設定ファイルの編集

`appsettings.json` を開いて、接続情報とタスクを設定：

```json
{
  "DatabaseSettings": {
    "OracleOdbcConnectionString": "Driver={Oracle in OraClient19Home1};Dbq=//your-oracle-host:1521/YOUR_SID;Uid=your_user;Pwd=your_password;",
    "PostgresConnectionString": "Host=your-postgres-host;Port=5432;Database=your_database;Username=your_user;Password=your_password;",
    "BatchSize": 1000
  },
  "PostgresServers": {
    "server1": {
      "Name": "メインサーバー",
      "Host": "localhost",
      "Port": 5432,
      "MaintenanceDB": "hospital_db",
      "Username": "hospital_user",
      "Password": "your_password",
      "ConnectionParameters": {
        "SslMode": "prefer",
        "ConnectTimeout": 10
      }
    }
  },
  "DataTransferTasks": [
    {
      "TaskName": "タスク1: ユーザーデータ転送",
      "OracleQuery": "SELECT * FROM USERS",
      "PostgresTableName": "users_imported",
      "PostgresServerKey": "server1",
      "EnableTransform": false
    },
    {
      "TaskName": "タスク2: 注文データ転送",
      "OracleQuery": "SELECT * FROM ORDERS WHERE ORDER_DATE >= SYSDATE - 30",
      "PostgresTableName": "orders_imported",
      "PostgresServerKey": "server1",
      "EnableTransform": true
    }
  ],
  "Logging": {
    "LogFilePath": "Logs/app-{Date}.log",
    "MinimumLevel": "Information"
  },
  "AppSettings": {
    "AutoCloseOnCompletion": true,
    "CloseDelaySeconds": 3
  }
}
```

#### 設定項目の説明

**DatabaseSettings:**
- `OracleOdbcConnectionString`: Oracle ODBC接続文字列
  - `Driver`: インストールされたOracleドライバ名
  - `Dbq`: Oracle接続先 (ホスト:ポート/SID)
  - `Uid`: Oracleユーザー名
  - `Pwd`: Oracleパスワード
- `PostgresConnectionString`: デフォルトのPostgreSQL接続文字列（PostgresServerKeyが未指定の場合に使用）
- `BatchSize`: 一度に書き込むレコード数（パフォーマンス調整用）

**PostgresServers:** （複数のPostgreSQLサーバーを定義可能）
- `Name`: サーバーの名前
- `Host`: ホスト名またはIPアドレス
- `Port`: ポート番号
- `MaintenanceDB`: データベース名
- `Username`: ユーザー名
- `Password`: パスワード
- `ConnectionParameters`: 接続パラメータ（SSL設定、タイムアウトなど）

**DataTransferTasks:** （複数設定可能）
- `TaskName`: タスクの名前（ログ表示用）
- `OracleQuery`: Oracleから実行するSQLクエリ
- `PostgresTableName`: PostgreSQLの書き込み先テーブル名
- `PostgresServerKey`: 使用するPostgreSQLサーバーのキー（省略可能、省略時はデフォルト接続を使用）
- `EnableTransform`: データ変換を有効にするか（true/false）

**Logging:**
- `LogFilePath`: ログファイルの出力先
- `MinimumLevel`: ログレベル (Debug, Information, Warning, Error)

**AppSettings:**
- `AutoCloseOnCompletion`: 処理完了後に自動終了するか
- `CloseDelaySeconds`: 自動終了までの待機秒数

### 5. Oracle ODBCドライバ名の確認方法

#### 方法1: ODBCデータソースアドミニストレーターで確認

1. `コントロールパネル` → `管理ツール` → `ODBCデータソース(64ビット)` を開く
2. `ドライバー` タブをクリック
3. Oracle関連のドライバー名を確認（例: `Oracle in OraClient19Home1`）

#### 方法2: レジストリで確認

```
HKEY_LOCAL_MACHINE\SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers
```

### 6. PostgreSQLテーブルについて

- テーブルが存在しない場合、アプリが**自動的に作成**します
- Oracleのデータ型を自動的にPostgreSQLの型に変換します
- 既存テーブルがある場合は、そのテーブルにデータを追加します

## ビルドと実行

### デバッグ実行

1. Visual Studio 2022で `OracleToPostgres.sln` を開く
2. `F5` キーを押してデバッグ実行
3. またはメニューから `デバッグ → デバッグの開始`

### リリースビルド

1. メニューから `ビルド → ソリューションのビルド`
2. 生成された実行ファイル:
   ```
   bin\Release\net6.0-windows\OracleToPostgres.exe
   ```

### スタンドアロン実行

リリースビルドしたフォルダ全体をコピーして配布できます：

```
bin\Release\net6.0-windows\
├── OracleToPostgres.exe
├── appsettings.json  (設定ファイル - 必須)
├── *.dll             (依存ライブラリ)
└── Logs\             (ログ出力先 - 自動作成)
```

## 使い方

1. **設定ファイルを確認**
   - `appsettings.json` の接続情報とタスク設定が正しいことを確認

2. **アプリケーションを起動**
   - `OracleToPostgres.exe` をダブルクリック
   - または Visual Studio から実行

3. **自動処理**
   - 起動すると自動的にデータ転送が開始されます
   - 複数のタスクが順次実行されます
   - 進捗状況がリアルタイムでグラフとログに表示されます

4. **処理完了**
   - 成功すると「全タスク完了！」と表示されます
   - 各タスクの処理結果がログに記録されます
   - `AutoCloseOnCompletion: true` の場合、指定秒数後に自動終了します
   - エラーが発生した場合はダイアログで通知されます

5. **ログ確認**
   - `Logs\app-YYYY-MM-DD.log` に詳細ログが出力されます

## データ変換機能

### 変換機能の有効化

`appsettings.json` でタスクごとに変換の有効/無効を設定：

```json
{
  "DataTransferTasks": [
    {
      "TaskName": "変換なしタスク",
      "EnableTransform": false  // ← 変換しない
    },
    {
      "TaskName": "変換ありタスク",
      "EnableTransform": true   // ← 変換する
    }
  ]
}
```

### 変換ロジックのカスタマイズ

`Services/DataTransformService.cs` の `Transform()` メソッドを編集：

```csharp
public DataTable Transform(DataTable dataTable, string taskName)
{
    foreach (DataRow row in dataTable.Rows)
    {
        // 日付カラムの変換
        if (dataTable.Columns.Contains("CREATED_DATE"))
        {
            row["CREATED_DATE"] = TransformDate(row["CREATED_DATE"]);
        }

        // 金額カラムの変換
        if (dataTable.Columns.Contains("AMOUNT"))
        {
            row["AMOUNT"] = Math.Round(Convert.ToDecimal(row["AMOUNT"]), 2);
        }

        // ステータスコードの変換
        if (dataTable.Columns.Contains("STATUS"))
        {
            row["STATUS"] = row["STATUS"]?.ToString() switch
            {
                "1" => "Active",
                "2" => "Inactive",
                _ => "Unknown"
            };
        }
    }
    return dataTable;
}
```

**詳細は `DATA_TRANSFORM_GUIDE.md` を参照してください。**

## トラブルシューティング

### Oracle接続エラー

**エラー:** `ERROR [IM002] [Microsoft][ODBC Driver Manager] データ ソース名および指定された既定のドライバーが見つかりません。`

**解決策:**
1. Oracle Instant Client が正しくインストールされているか確認
2. `appsettings.json` の `Driver={}` 部分のドライバ名が正しいか確認
3. 64ビット版のODBCドライバを使用しているか確認（.NET 6.0は64ビット）

### PostgreSQL接続エラー

**エラー:** `Connection refused` または `timeout`

**解決策:**
1. PostgreSQLサーバーが起動しているか確認
2. ファイアウォール設定を確認
3. `pg_hba.conf` で接続が許可されているか確認
4. 接続文字列のホスト名、ポート番号が正しいか確認

### データ型エラー

**エラー:** PostgreSQLへの書き込み時にデータ型エラー

**解決策:**
- `DataTransferService.cs` の `MapToPgType()` メソッドでデータ型マッピングを調整
- 特殊なOracle型（CLOB, BLOBなど）は事前にクエリで変換

### パフォーマンスが遅い

**解決策:**
1. `appsettings.json` の `BatchSize` を増やす（例: 5000, 10000）
2. Oracleクエリにインデックスを使用
3. PostgreSQLの `UNLOGGED TABLE` を検討（一時的な転送の場合）

## プロジェクト構造

```
OracleToPostgres/
├── Services/
│   ├── DataTransferService.cs      # データ転送ロジック
│   └── ConfigurationService.cs     # 設定読み込み
├── ViewModels/
│   ├── ViewModelBase.cs            # MVVM基底クラス
│   └── MainViewModel.cs            # メインウィンドウのViewModel
├── Views/
│   ├── MainWindow.xaml             # メインウィンドウUI
│   └── MainWindow.xaml.cs          # メインウィンドウコードビハインド
├── App.xaml                        # アプリケーション定義
├── App.xaml.cs                     # アプリケーションコードビハインド
├── appsettings.json                # 設定ファイル
└── OracleToPostgres.csproj         # プロジェクトファイル
```

## 依存パッケージ

| パッケージ | バージョン | 用途 |
|-----------|----------|------|
| LiveChartsCore.SkiaSharpView.WPF | 2.0.0-rc2 | リアルタイムグラフ表示 |
| Npgsql | 6.0.11 | PostgreSQL接続 |
| Serilog | 3.1.1 | 構造化ログ |
| Serilog.Sinks.File | 5.0.0 | ファイルログ出力 |
| System.Data.Odbc | 6.0.0 | ODBC接続 |
| Microsoft.Extensions.Configuration | 6.0.1 | 設定管理 |

## カスタマイズ

### データ加工処理の追加

`DataTransferService.cs` の `WriteToPostgresAsync()` メソッド内でデータを加工：

```csharp
foreach (DataRow row in dataTable.Rows)
{
    // ここでデータ加工処理を追加
    // 例: row["column_name"] = TransformData(row["column_name"]);
    
    batch.Add(row);
}
```

### UIのカスタマイズ

`Views/MainWindow.xaml` を編集してUIをカスタマイズできます。

## ライセンス

このプロジェクトはサンプルコードです。自由に改変・使用してください。

## サポート

問題が発生した場合：
1. `Logs/` フォルダ内のログファイルを確認
2. Visual Studioのデバッグモードで詳細を確認
3. 接続文字列とドライバ名を再確認

## 更新履歴

- **v1.0.0** (2026-01-19)
  - 初回リリース
  - Oracle → PostgreSQL データ転送機能
  - リアルタイム進捗グラフ
  - 自動起動・終了機能
