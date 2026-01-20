# 病院データベース転送設定ガイド

このファイルでは、生成されたモデルを使用したデータ転送の設定方法を説明します。

## 生成されたモデル一覧

以下の9つのモデルクラスが `OracleToPostgres/Models/` に配置されています：

1. **Departments.cs** - 部門マスタ
2. **Doctors.cs** - 医師マスタ
3. **Wards.cs** - 病棟マスタ
4. **Staff.cs** - スタッフマスタ
5. **Permissions.cs** - 権限マスタ
6. **OutpatientRecords.cs** - 外来実績データ
7. **InpatientRecords.cs** - 入院実績データ
8. **Sales.cs** - 売上データ
9. **Messages.cs** - メッセージデータ

## テーブル構造

### 1. departments（部門マスタ）
| カラム名 | 型 | NULL許可 | 説明 |
|---------|---|---------|------|
| id | int | NO | 主キー |
| code | varchar(10) | YES | 部門コード |
| name | varchar(100) | YES | 部門名 |
| display_order | int | NO | 表示順 |
| created_at | timestamp | YES | 作成日時 |

### 2. doctors（医師マスタ）
| カラム名 | 型 | NULL許可 | 説明 |
|---------|---|---------|------|
| code | varchar(20) | NO | 主キー（医師コード） |
| name | varchar(100) | YES | 医師名 |
| department_code | varchar(10) | YES | 所属部門コード |
| display_order | int | NO | 表示順 |
| created_at | timestamp | YES | 作成日時 |

### 3. wards（病棟マスタ）
| カラム名 | 型 | NULL許可 | 説明 |
|---------|---|---------|------|
| id | int | NO | 主キー |
| code | varchar(10) | YES | 病棟コード |
| name | varchar(100) | YES | 病棟名 |
| capacity | int | NO | 収容人数 |
| display_order | int | NO | 表示順 |
| created_at | timestamp | YES | 作成日時 |

### 4. staff（スタッフマスタ）
| カラム名 | 型 | NULL許可 | 説明 |
|---------|---|---------|------|
| id | varchar(20) | NO | 主キー（スタッフID） |
| name | varchar(100) | YES | スタッフ名 |
| job_type_code | varchar(2) | YES | 職種コード |
| created_at | timestamp | YES | 作成日時 |

### 5. permissions（権限マスタ）
| カラム名 | 型 | NULL許可 | 説明 |
|---------|---|---------|------|
| job_type_code | varchar(2) | NO | 主キー（職種コード） |
| job_type_name | varchar(100) | YES | 職種名 |
| level | int | NO | 権限レベル |

### 6. outpatient_records（外来実績）
| カラム名 | 型 | NULL許可 | 説明 |
|---------|---|---------|------|
| id | int | NO | 主キー |
| date | timestamp | NO | 日付 |
| department_id | int | NO | 部門ID |
| new_patients_count | int | NO | 新患数 |
| returning_patients_count | int | NO | 再来数 |
| created_at | timestamp | YES | 作成日時 |

### 7. inpatient_records（入院実績）
| カラム名 | 型 | NULL許可 | 説明 |
|---------|---|---------|------|
| id | int | NO | 主キー |
| date | timestamp | NO | 日付 |
| ward_id | int | NO | 病棟ID |
| department_id | int | NO | 部門ID |
| current_patient_count | int | NO | 現在患者数 |
| new_admission_count | int | NO | 新規入院数 |
| discharge_count | int | NO | 退院数 |
| transfer_out_count | int | NO | 転出数 |
| transfer_in_count | int | NO | 転入数 |
| created_at | timestamp | YES | 作成日時 |

### 8. sales（売上データ）
| カラム名 | 型 | NULL許可 | 説明 |
|---------|---|---------|------|
| doctor_code | varchar(20) | NO | 主キー（医師コード） |
| year_month | varchar(7) | NO | 主キー（年月） |
| outpatient_sales | bigint | NO | 外来売上 |
| inpatient_sales | bigint | NO | 入院売上 |
| updated_at | timestamp | YES | 更新日時 |

### 9. messages（メッセージ）
| カラム名 | 型 | NULL許可 | 説明 |
|---------|---|---------|------|
| id | int | NO | 主キー |
| content | text | YES | メッセージ内容 |
| created_at | timestamp | YES | 作成日時 |

## データ転送設定例

`appsettings.json` には既に全テーブルの転送タスクが設定されています。

### Oracleのテーブル名を変更する場合

`appsettings.json` の `OracleQuery` を編集してください：

```json
{
  "TaskName": "部門マスタ転送",
  "OracleQuery": "SELECT ID, CODE, NAME, DISPLAY_ORDER, CREATED_AT FROM YOUR_ORACLE_DEPARTMENTS_TABLE",
  "PostgresTableName": "departments",
  "PostgresServerKey": "dashboard",
  "EnableTransform": false
}
```

**注意**: Oracleのカラム名がPostgreSQLと異なる場合、クエリで別名を付けてください：

```sql
SELECT 
    DEPT_ID as ID,
    DEPT_CODE as CODE,
    DEPT_NAME as NAME,
    DISP_ORDER as DISPLAY_ORDER,
    CREATE_DATE as CREATED_AT
FROM ORACLE_DEPARTMENTS
```

## データ変換が必要な場合

### 日付フォーマット変換が必要な場合

```json
{
  "TaskName": "外来実績転送（変換あり）",
  "OracleQuery": "SELECT ID, DATE, DEPARTMENT_ID, NEW_PATIENTS_COUNT, RETURNING_PATIENTS_COUNT, CREATED_AT FROM ORACLE_OUTPATIENT_RECORDS",
  "PostgresTableName": "outpatient_records",
  "PostgresServerKey": "dashboard",
  "EnableTransform": true  ← trueに変更
}
```

次に `Services/DataTransformService.cs` を編集：

```csharp
public DataTable Transform(DataTable dataTable, string taskName)
{
    if (taskName == "外来実績転送（変換あり）")
    {
        foreach (DataRow row in dataTable.Rows)
        {
            // 日付フォーマット変換
            if (dataTable.Columns.Contains("DATE"))
            {
                if (DateTime.TryParse(row["DATE"]?.ToString(), out DateTime date))
                {
                    row["DATE"] = date.Date; // 時間部分を削除
                }
            }
        }
    }
    return dataTable;
}
```

## 実行順序

現在の設定では以下の順序でデータが転送されます：

1. 部門マスタ
2. 医師マスタ
3. 病棟マスタ
4. スタッフマスタ
5. 権限マスタ
6. 外来実績
7. 入院実績
8. 売上データ
9. メッセージ

マスタデータから先に転送されるため、外部キー制約がある場合でも安全です。

## 実行方法

```bash
# Visual Studioで実行
F5キーを押す

# または、リリースビルドして実行
bin\Release\net6.0-windows\OracleToPostgres.exe
```

## 注意事項

1. **Oracleのテーブル名を確認**してください
   - 現在の設定は `ORACLE_DEPARTMENTS`, `ORACLE_DOCTORS` など仮のテーブル名を使用しています
   - 実際のテーブル名に変更する必要があります

2. **カラム名の一致を確認**してください
   - OracleとPostgreSQLでカラム名が異なる場合、SQLのAS句で別名を付けてください

3. **データ型の互換性を確認**してください
   - 特に日付型（DATE, TIMESTAMP）の扱いに注意

4. **接続情報を設定**してください
   - `OracleOdbcConnectionString` にOracleの接続情報
   - `PostgresServers.dashboard.Password` にPostgreSQLのパスワード

## トラブルシューティング

### エラー: テーブルが見つからない

**原因**: Oracleのテーブル名が間違っている

**解決策**: `appsettings.json` の `OracleQuery` を実際のテーブル名に変更

### エラー: カラムが見つからない

**原因**: Oracleのカラム名がPostgreSQLと一致していない

**解決策**: SQLでAS句を使って別名を付ける

```sql
SELECT 
    ORACLE_COLUMN_NAME as PostgresColumnName
FROM ORACLE_TABLE
```

### エラー: データ型が一致しない

**原因**: OracleとPostgreSQLでデータ型に互換性がない

**解決策**: 
1. `EnableTransform: true` にする
2. `DataTransformService.cs` で型変換を実装

## まとめ

✅ 9つのモデルクラスが生成済み  
✅ `appsettings.json` に全テーブルの転送タスクを設定済み  
✅ Oracleのテーブル名を実際の名前に変更してください  
✅ 実行すればPostgreSQLにデータが転送されます  

準備完了です！🚀
