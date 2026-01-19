# Oracle to PostgreSQL データ転送ツール - 設定例

## 設定ファイル (appsettings.json) のサンプル

### 例1: 複数テーブル転送（データ変換なし）

```json
{
  "DatabaseSettings": {
    "OracleOdbcConnectionString": "Driver={Oracle in OraClient19Home1};Dbq=//localhost:1521/XE;Uid=system;Pwd=oracle;",
    "PostgresConnectionString": "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres;",
    "BatchSize": 1000
  },
  "DataTransferTasks": [
    {
      "TaskName": "ユーザーデータ転送",
      "OracleQuery": "SELECT ID, NAME, EMAIL, CREATED_DATE FROM USERS WHERE ROWNUM <= 10000",
      "PostgresTableName": "users_imported",
      "EnableTransform": false
    },
    {
      "TaskName": "注文データ転送",
      "OracleQuery": "SELECT ORDER_ID, USER_ID, AMOUNT, ORDER_DATE FROM ORDERS WHERE ORDER_DATE >= SYSDATE - 30",
      "PostgresTableName": "orders_imported",
      "EnableTransform": false
    }
  ],
  "Logging": {
    "LogFilePath": "Logs/app-{Date}.log",
    "MinimumLevel": "Debug"
  },
  "AppSettings": {
    "AutoCloseOnCompletion": false,
    "CloseDelaySeconds": 5
  }
}
```

### 例2: データ変換を含む転送

```json
{
  "DatabaseSettings": {
    "OracleOdbcConnectionString": "Driver={Oracle in OraClient19Home1};Dbq=//prod-oracle.company.com:1521/PRODDB;Uid=etl_user;Pwd=SecurePass123;",
    "PostgresConnectionString": "Host=prod-postgres.company.com;Port=5432;Database=datawarehouse;Username=etl_user;Password=SecurePass456;",
    "BatchSize": 5000
  },
  "DataTransferTasks": [
    {
      "TaskName": "売上データ転送（変換あり）",
      "OracleQuery": "SELECT * FROM SALES_DATA WHERE EXTRACT(YEAR FROM SALE_DATE) = 2024",
      "PostgresTableName": "sales_data_2024",
      "EnableTransform": true
    },
    {
      "TaskName": "顧客マスタ転送（変換なし）",
      "OracleQuery": "SELECT * FROM CUSTOMERS WHERE STATUS = 'ACTIVE'",
      "PostgresTableName": "customers",
      "EnableTransform": false
    },
    {
      "TaskName": "商品マスタ転送（変換あり）",
      "OracleQuery": "SELECT PRODUCT_ID, PRODUCT_NAME, PRICE, STOCK FROM PRODUCTS",
      "PostgresTableName": "products",
      "EnableTransform": true
    }
  ],
  "Logging": {
    "LogFilePath": "Logs/transfer-{Date}.log",
    "MinimumLevel": "Information"
  },
  "AppSettings": {
    "AutoCloseOnCompletion": true,
    "CloseDelaySeconds": 10
  }
}
```

### 例3: 単一タスク（シンプルな転送）

```json
{
  "DatabaseSettings": {
    "OracleOdbcConnectionString": "Driver={Oracle in OraClient19Home1};Dbq=//localhost:1521/ORCL;Uid=user;Pwd=pass;",
    "PostgresConnectionString": "Host=localhost;Port=5432;Database=mydb;Username=user;Password=pass;",
    "BatchSize": 2000
  },
  "DataTransferTasks": [
    {
      "TaskName": "メインデータ転送",
      "OracleQuery": "SELECT * FROM MY_TABLE",
      "PostgresTableName": "my_table_copy",
      "EnableTransform": false
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

## よくある設定パターン

### パターン1: 特定の日付範囲のデータ転送

```json
"OracleQuery": "SELECT * FROM ORDERS WHERE ORDER_DATE BETWEEN TO_DATE('2024-01-01', 'YYYY-MM-DD') AND TO_DATE('2024-12-31', 'YYYY-MM-DD')"
```

### パターン2: 複数テーブルのJOIN

```json
"OracleQuery": "SELECT u.USER_ID, u.USER_NAME, o.ORDER_ID, o.ORDER_TOTAL FROM USERS u INNER JOIN ORDERS o ON u.USER_ID = o.USER_ID WHERE o.ORDER_DATE > SYSDATE - 30"
```

### パターン3: データ型変換を含むクエリ

```json
"OracleQuery": "SELECT ID, NAME, TO_CHAR(CREATED_DATE, 'YYYY-MM-DD HH24:MI:SS') AS CREATED_DATE, CAST(AMOUNT AS NUMBER(10,2)) AS AMOUNT FROM TRANSACTIONS"
```

## Oracle ODBCドライバ名の確認

Windows上で使用可能なOracleドライバ名の例：

```
Driver={Oracle in OraClient19Home1}
Driver={Oracle in instantclient_19_20}
Driver={Oracle in OraDB19Home1}
Driver={Microsoft ODBC for Oracle}
```

確認方法：
1. `コマンドプロンプト` を開く
2. 以下のコマンドを実行：

```powershell
reg query "HKLM\SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers"
```

または PowerShell で：

```powershell
Get-OdbcDriver | Where-Object {$_.Name -like "*Oracle*"}
```

## バッチサイズのチューニング

| データ量 | 推奨BatchSize | 理由 |
|---------|--------------|------|
| ~10,000件 | 500-1000 | 小規模データは小さめのバッチで十分 |
| 10,000-100,000件 | 2000-5000 | 中規模データは中程度のバッチサイズ |
| 100,000件以上 | 5000-10000 | 大規模データは大きめのバッチで高速化 |

注意: バッチサイズを大きくしすぎるとメモリ不足になる可能性があります。

## ログレベルの設定

| レベル | 用途 | 出力内容 |
|--------|------|---------|
| Debug | 開発・デバッグ | 詳細なデバッグ情報を含むすべてのログ |
| Information | 通常運用 | 重要な情報とエラー |
| Warning | 本番環境 | 警告とエラーのみ |
| Error | 最小限 | エラーのみ |

## 接続文字列のテンプレート

### Oracle ODBC接続文字列

```
基本形式:
Driver={ドライバ名};Dbq=//ホスト:ポート/サービス名;Uid=ユーザー名;Pwd=パスワード;

SID接続:
Driver={Oracle in OraClient19Home1};Dbq=//192.168.1.100:1521/ORCL;Uid=myuser;Pwd=mypass;

TNS名使用:
Driver={Oracle in OraClient19Home1};Dbq=MYDB_TNS;Uid=myuser;Pwd=mypass;
```

### PostgreSQL接続文字列

```
基本形式:
Host=ホスト;Port=ポート;Database=データベース名;Username=ユーザー名;Password=パスワード;

SSL接続:
Host=mydb.postgres.com;Port=5432;Database=mydb;Username=myuser;Password=mypass;SSL Mode=Require;

接続タイムアウト設定:
Host=localhost;Port=5432;Database=mydb;Username=myuser;Password=mypass;Timeout=30;Command Timeout=60;
```

## トラブルシューティング用設定

デバッグ時は以下の設定を使用：

```json
{
  "DatabaseSettings": {
    "OracleOdbcConnectionString": "...",
    "PostgresConnectionString": "...",
    "OracleQuery": "SELECT * FROM YOUR_TABLE WHERE ROWNUM <= 100",
    "PostgresTableName": "test_table",
    "BatchSize": 10
  },
  "Logging": {
    "LogFilePath": "Logs/debug-{Date}.log",
    "MinimumLevel": "Debug"
  },
  "AppSettings": {
    "AutoCloseOnCompletion": false,
    "CloseDelaySeconds": 60
  }
}
```

- 小さいデータ量（ROWNUM <= 100）でテスト
- BatchSizeを小さく（10）して詳細動作を確認
- 自動終了を無効化（AutoCloseOnCompletion: false）
- ログレベルをDebugに設定
