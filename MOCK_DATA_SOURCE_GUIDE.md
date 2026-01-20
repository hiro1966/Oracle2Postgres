# モックデータソース使用ガイド

## 概要

OracleToPostgresツールには、実際のOracle接続を使わずにテストできる**モックデータソース**機能が組み込まれています。

## なぜモックデータソースが必要か？

- 🔒 **Oracle環境がない状態でも開発・テストが可能**
- ⚡ **高速な動作確認**（Oracle接続の待ち時間がない）
- 🛡️ **本番データに影響を与えずに安全にテスト**
- 🎯 **PostgreSQL書き込み機能の検証に集中**
- 📊 **リアルなテストデータで動作確認**

## モックモードの設定

### appsettings.json で設定

```json
{
  "AppSettings": {
    "UseMockDataSource": true
  }
}
```

- `true`: モックデータソースを使用（Oracle接続不要）
- `false`: 本番Oracleに接続

### モードの切り替え

#### 開発・テスト時（モックを使用）
```json
"UseMockDataSource": true
```

#### 本番環境（実際のOracleに接続）
```json
"UseMockDataSource": false
```

## モックデータの内容

モックサービスは以下の9つのテーブルに対応したテストデータを生成します：

### 1. 部門マスタ（departments）
```
- 内科 (ID: 1)
- 外科 (ID: 2)
- 小児科 (ID: 3)
...計10件
```

### 2. 医師マスタ（doctors）
```
- 田中一郎（内科）
- 佐藤次郎（外科）
- 鈴木三郎（小児科）
...計10件
```

### 3. 病棟マスタ（wards）
```
- A棟（定員50名）
- B棟（定員40名）
- C棟（定員30名）
...計5件
```

### 4. スタッフマスタ（staff）
```
- 看護師、医師、事務員など
...計20件
```

### 5. 権限マスタ（permissions）
```
- 医師（レベル3）
- 看護師（レベル2）
- 事務員（レベル1）
...計5件
```

### 6. 外来実績（outpatient_records）
```
- 各部門の日別の外来患者数
...計100件
```

### 7. 入院実績（inpatient_records）
```
- 各病棟の日別の入院患者数
...計150件
```

### 8. 売上データ（sales）
```
- 医師別・月別の売上データ
...計120件
```

### 9. メッセージ（messages）
```
- システムメッセージ
...計10件
```

## モックデータの特徴

### リアルなデータ生成

- ✅ **日付データ**: 過去30日間のランダムな日付
- ✅ **関連データ**: 部門と医師、病棟と実績など、関連性のあるデータ
- ✅ **数値データ**: 外来患者数、入院患者数、売上など現実的な範囲の値
- ✅ **日本語データ**: 実際の日本の病院を想定した名前とデータ

### データ量の調整

各テーブルのレコード数は、実際のテスト要件に合わせて調整できます。

`OracleToPostgres/Services/OracleMockService.cs` で変更可能：

```csharp
// 例: 外来実績を100件 → 1000件に変更
for (int i = 0; i < 1000; i++) // 元は100
{
    // ...
}
```

## モック使用時の動作

### 起動時のログ表示

```
🔧 モックデータソースを使用します
データ転送を開始します（タスク数: 9）
データソース: OracleMockService
```

### タスク実行

各タスクは実際のOracle接続の代わりに、モックサービスからデータを取得：

```
[部門マスタ転送] データを読み込んでいます...
[部門マスタ転送] 10 件のレコードを読み込みました
[部門マスタ転送] PostgreSQLへデータを書き込んでいます...
[部門マスタ転送] 完了: 10/10 件（0.52 秒）
```

### PostgreSQLへの書き込み

- モックデータは実際にPostgreSQLに書き込まれます
- テーブルが存在しない場合は自動作成されます
- データの整合性を検証できます

## テストのベストプラクティス

### 1. 段階的なテスト

1. **モックモードでUI/進捗表示のテスト**
   ```json
   "UseMockDataSource": true
   ```

2. **PostgreSQL書き込み機能のテスト**
   - モックデータでPostgreSQLへの書き込み確認
   - テーブル自動作成機能の確認
   - バッチ処理の動作確認

3. **本番Oracle接続のテスト**
   ```json
   "UseMockDataSource": false
   ```

### 2. データ変換機能のテスト

モックデータを使って、データ変換ロジックを安全にテスト：

```json
{
  "TaskName": "テスト: 変換機能",
  "EnableTransform": true,
  "UseMockDataSource": true
}
```

### 3. パフォーマンステスト

モックデータの件数を増やして、大量データ時の動作を確認。

## トラブルシューティング

### モックモードなのにOracle接続エラーが出る

**原因:** `UseMockDataSource` が正しく設定されていない

**解決策:**
```json
"AppSettings": {
  "UseMockDataSource": true  // 必ずtrueに設定
}
```

### モックデータがPostgreSQLに書き込まれない

**原因:** PostgreSQL接続文字列が間違っている

**解決策:** `appsettings.json` のPostgreSQL設定を確認：
```json
"PostgresServers": {
  "dashboard": {
    "Host": "localhost",
    "Port": 5432,
    "MaintenanceDB": "hospital_db",
    "Username": "hospital_user",
    "Password": "your_actual_password"
  }
}
```

### モックデータの内容を確認したい

**方法:** PostgreSQLに接続してデータを確認：

```sql
-- 部門データの確認
SELECT * FROM departments;

-- 医師データの確認
SELECT * FROM doctors;

-- 外来実績の確認
SELECT * FROM outpatient_records ORDER BY date DESC LIMIT 10;
```

## 実装の詳細

### IDataSource インターフェース

```csharp
public interface IDataSource
{
    Task<DataTable> ReadDataAsync(string query, string taskName);
}
```

- `OracleDataSource`: 本番Oracle接続
- `OracleMockService`: モックデータ生成

### データソースの切り替え

MainWindow.xaml.csで自動切り替え：

```csharp
IDataSource dataSource;
if (useMock)
{
    dataSource = new OracleMockService();
}
else
{
    dataSource = new OracleDataSource(oracleConn);
}
```

## まとめ

| 項目 | モックモード | 本番モード |
|------|-------------|-----------|
| Oracle接続 | 不要 | 必要 |
| 実行速度 | 高速 | Oracle次第 |
| テストデータ | 自動生成 | 実データ |
| PostgreSQL書き込み | 有効 | 有効 |
| 推奨用途 | 開発・テスト | 本番運用 |

モックモードを活用して、安全かつ効率的な開発を進めましょう！
