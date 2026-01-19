# データ変換機能の実装ガイド

このドキュメントでは、`DataTransformService.cs` でデータ変換ロジックを実装する方法を説明します。

## 📋 概要

データ変換は以下の3ステップで実行されます：

```
1. データの読み込み（Oracle）
   ↓
2. データの変換 ← ここをカスタマイズ
   ↓
3. データの書き込み（PostgreSQL）
```

## 🔧 変換機能の有効化

`appsettings.json` でタスクごとに変換の有効/無効を設定：

```json
{
  "DataTransferTasks": [
    {
      "TaskName": "ユーザーデータ転送",
      "OracleQuery": "SELECT * FROM USERS",
      "PostgresTableName": "users",
      "EnableTransform": true  ← trueで変換を有効化
    }
  ]
}
```

## 📝 基本的な実装パターン

### パターン1: 特定カラムの値を変換

`DataTransformService.cs` の `Transform()` メソッドを編集：

```csharp
public DataTable Transform(DataTable dataTable, string taskName)
{
    Log.Information($"[{taskName}] データ変換を開始します（{dataTable.Rows.Count} 件）");

    try
    {
        foreach (DataRow row in dataTable.Rows)
        {
            // 日付カラムの変換
            if (dataTable.Columns.Contains("CREATED_DATE"))
            {
                row["CREATED_DATE"] = TransformDate(row["CREATED_DATE"]);
            }

            // 金額カラムの変換（小数点2桁に丸める）
            if (dataTable.Columns.Contains("AMOUNT"))
            {
                row["AMOUNT"] = TransformDecimal(row["AMOUNT"]);
            }

            // ステータスコードを文字列に変換
            if (dataTable.Columns.Contains("STATUS"))
            {
                row["STATUS"] = TransformStatus(row["STATUS"]);
            }
        }

        Log.Information($"[{taskName}] データ変換が完了しました");
        return dataTable;
    }
    catch (Exception ex)
    {
        Log.Error(ex, $"[{taskName}] データ変換中にエラーが発生しました");
        throw;
    }
}
```

### パターン2: カラム名による条件分岐

タスク名やカラム構造によって処理を分ける：

```csharp
public DataTable Transform(DataTable dataTable, string taskName)
{
    Log.Information($"[{taskName}] データ変換を開始します");

    switch (taskName)
    {
        case "ユーザーデータ転送":
            return TransformUserData(dataTable);
        
        case "注文データ転送":
            return TransformOrderData(dataTable);
        
        case "商品マスタ転送":
            return TransformProductData(dataTable);
        
        default:
            Log.Warning($"[{taskName}] 変換ロジックが定義されていません");
            return dataTable;
    }
}

private DataTable TransformUserData(DataTable dataTable)
{
    foreach (DataRow row in dataTable.Rows)
    {
        // ユーザーデータ固有の変換
        if (dataTable.Columns.Contains("EMAIL"))
        {
            row["EMAIL"] = row["EMAIL"]?.ToString()?.ToLower();
        }
    }
    return dataTable;
}

private DataTable TransformOrderData(DataTable dataTable)
{
    foreach (DataRow row in dataTable.Rows)
    {
        // 注文データ固有の変換
        if (dataTable.Columns.Contains("AMOUNT"))
        {
            row["AMOUNT"] = Math.Round(Convert.ToDecimal(row["AMOUNT"]), 2);
        }
    }
    return dataTable;
}
```

## 🎯 実装例集

### 例1: 日付フォーマットの変換

```csharp
private object TransformDate(object value)
{
    if (value == null || value == DBNull.Value)
        return DBNull.Value;

    if (DateTime.TryParse(value.ToString(), out DateTime date))
    {
        // Oracle形式 → PostgreSQL形式
        return date.ToString("yyyy-MM-dd HH:mm:ss");
    }

    return value;
}
```

### 例2: 数値の丸め処理

```csharp
private object TransformDecimal(object value)
{
    if (value == null || value == DBNull.Value)
        return DBNull.Value;

    if (decimal.TryParse(value.ToString(), out decimal amount))
    {
        // 小数点以下2桁に丸める
        return Math.Round(amount, 2);
    }

    return value;
}
```

### 例3: ステータスコードの変換

```csharp
private object TransformStatus(object value)
{
    if (value == null || value == DBNull.Value)
        return DBNull.Value;

    // 数値コード → 文字列
    return value.ToString() switch
    {
        "1" => "Active",
        "2" => "Inactive",
        "3" => "Suspended",
        "9" => "Deleted",
        _ => "Unknown"
    };
}
```

### 例4: 文字列の正規化

```csharp
private object TransformString(object value)
{
    if (value == null || value == DBNull.Value)
        return DBNull.Value;

    var str = value.ToString();
    
    // トリム + 大文字変換
    return str?.Trim().ToUpper() ?? string.Empty;
}
```

### 例5: NULL値の置換

```csharp
private object ReplaceNullValue(object value, object defaultValue)
{
    if (value == null || value == DBNull.Value)
        return defaultValue;
    
    return value;
}

// 使用例
foreach (DataRow row in dataTable.Rows)
{
    row["PHONE"] = ReplaceNullValue(row["PHONE"], "未登録");
    row["ADDRESS"] = ReplaceNullValue(row["ADDRESS"], "");
}
```

### 例6: 計算列の追加

```csharp
private void AddCalculatedColumn(DataTable dataTable)
{
    // 新しいカラムを追加
    if (!dataTable.Columns.Contains("FULL_NAME"))
    {
        dataTable.Columns.Add("FULL_NAME", typeof(string));
    }

    if (!dataTable.Columns.Contains("TAX_AMOUNT"))
    {
        dataTable.Columns.Add("TAX_AMOUNT", typeof(decimal));
    }

    foreach (DataRow row in dataTable.Rows)
    {
        // フルネームを生成
        var firstName = row["FIRST_NAME"]?.ToString() ?? "";
        var lastName = row["LAST_NAME"]?.ToString() ?? "";
        row["FULL_NAME"] = $"{lastName} {firstName}".Trim();

        // 税込金額を計算（10%）
        if (decimal.TryParse(row["AMOUNT"]?.ToString(), out decimal amount))
        {
            row["TAX_AMOUNT"] = Math.Round(amount * 0.1m, 2);
        }
    }
}
```

### 例7: データのフィルタリング

```csharp
private DataTable FilterData(DataTable dataTable)
{
    // 条件に合致する行のみ抽出
    var filteredRows = dataTable.AsEnumerable()
        .Where(row => 
        {
            // 年齢が18歳以上
            if (int.TryParse(row["AGE"]?.ToString(), out int age))
            {
                return age >= 18;
            }
            return false;
        })
        .CopyToDataTable();

    Log.Information($"フィルタリング: {dataTable.Rows.Count} → {filteredRows.Rows.Count} 件");
    return filteredRows;
}
```

### 例8: 複数カラムの結合

```csharp
private void CombineColumns(DataTable dataTable)
{
    if (!dataTable.Columns.Contains("FULL_ADDRESS"))
    {
        dataTable.Columns.Add("FULL_ADDRESS", typeof(string));
    }

    foreach (DataRow row in dataTable.Rows)
    {
        var parts = new[]
        {
            row["POSTAL_CODE"]?.ToString(),
            row["PREFECTURE"]?.ToString(),
            row["CITY"]?.ToString(),
            row["ADDRESS1"]?.ToString(),
            row["ADDRESS2"]?.ToString()
        };

        row["FULL_ADDRESS"] = string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }
}
```

## 🚀 実践的な実装例

### 実例: ユーザーデータの正規化

```csharp
public DataTable Transform(DataTable dataTable, string taskName)
{
    Log.Information($"[{taskName}] データ変換を開始します（{dataTable.Rows.Count} 件）");

    try
    {
        // タスク名で処理を分岐
        if (taskName == "ユーザーデータ転送")
        {
            foreach (DataRow row in dataTable.Rows)
            {
                // メールアドレスを小文字に統一
                if (dataTable.Columns.Contains("EMAIL"))
                {
                    var email = row["EMAIL"]?.ToString()?.Trim().ToLower();
                    row["EMAIL"] = string.IsNullOrEmpty(email) ? DBNull.Value : email;
                }

                // 電話番号のフォーマット統一（ハイフン除去）
                if (dataTable.Columns.Contains("PHONE"))
                {
                    var phone = row["PHONE"]?.ToString()?.Replace("-", "").Replace(" ", "");
                    row["PHONE"] = string.IsNullOrEmpty(phone) ? DBNull.Value : phone;
                }

                // 生年月日をDate型に変換
                if (dataTable.Columns.Contains("BIRTH_DATE"))
                {
                    if (DateTime.TryParse(row["BIRTH_DATE"]?.ToString(), out DateTime birthDate))
                    {
                        row["BIRTH_DATE"] = birthDate.Date;
                    }
                }

                // ステータスを文字列に変換
                if (dataTable.Columns.Contains("STATUS"))
                {
                    row["STATUS"] = row["STATUS"]?.ToString() switch
                    {
                        "1" => "有効",
                        "0" => "無効",
                        _ => "不明"
                    };
                }
            }
        }

        Log.Information($"[{taskName}] データ変換が完了しました");
        return dataTable;
    }
    catch (Exception ex)
    {
        Log.Error(ex, $"[{taskName}] データ変換中にエラーが発生しました");
        throw;
    }
}
```

## 💡 ベストプラクティス

### 1. エラーハンドリング

```csharp
foreach (DataRow row in dataTable.Rows)
{
    try
    {
        // 変換処理
        row["AMOUNT"] = TransformDecimal(row["AMOUNT"]);
    }
    catch (Exception ex)
    {
        Log.Warning($"行 {dataTable.Rows.IndexOf(row)} の変換でエラー: {ex.Message}");
        // エラー時はデフォルト値を設定
        row["AMOUNT"] = 0;
    }
}
```

### 2. ログ出力

```csharp
Log.Information($"変換前: {row["STATUS"]}");
row["STATUS"] = TransformStatus(row["STATUS"]);
Log.Information($"変換後: {row["STATUS"]}");
```

### 3. パフォーマンス考慮

```csharp
// カラム存在チェックをループ外で実施
var hasAmountColumn = dataTable.Columns.Contains("AMOUNT");
var hasStatusColumn = dataTable.Columns.Contains("STATUS");

foreach (DataRow row in dataTable.Rows)
{
    if (hasAmountColumn)
    {
        row["AMOUNT"] = TransformDecimal(row["AMOUNT"]);
    }
    
    if (hasStatusColumn)
    {
        row["STATUS"] = TransformStatus(row["STATUS"]);
    }
}
```

## 📚 まとめ

1. **`DataTransformService.cs` の `Transform()` メソッドを編集**
2. **タスク名やカラム名で処理を分岐**
3. **エラーハンドリングとログ出力を忘れずに**
4. **`EnableTransform: true` で機能を有効化**

変換ロジックは柔軟にカスタマイズ可能です！
