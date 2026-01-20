# PostgreSQL Model Generator

PostgreSQLデータベースのテーブル構造を読み取り、C#のモデルクラスを自動生成するコマンドラインツールです。

## 機能

- ✅ データベース内の**全テーブル**を自動スキャン
- ✅ テーブル構造からC#クラスを自動生成
- ✅ Data Annotations対応（[Key], [Required], [MaxLength]など）
- ✅ Nullable型の自動判定
- ✅ スネークケース → パスカルケース変換
- ✅ PostgreSQLデータ型 → C#型の自動マッピング
- ✅ 複数のPostgreSQLサーバーに対応

## 使い方

### 基本的な使い方

```bash
cd ModelGenerator

# サーバーを指定してモデル生成
dotnet run -- --server dashboard

# 対話的にサーバーを選択
dotnet run
```

### オプション指定

```bash
# 出力先とネームスペースを指定
dotnet run -- --server dashboard --output ../OracleToPostgres/Models --namespace OracleToPostgres.Models

# スキーマを指定（デフォルト: public）
dotnet run -- --server dashboard --schema myschema
```

### コマンドラインオプション

| オプション | 短縮形 | 説明 | デフォルト |
|-----------|-------|------|-----------|
| `--server` | `-s` | PostgreSQLサーバーキー | 対話的に選択 |
| `--output` | `-o` | 出力ディレクトリ | GeneratedModels |
| `--namespace` | `-n` | 名前空間 | OracleToPostgres.Models |
| `--schema` | | スキーマ名 | public |
| `--help` | `-h` | ヘルプを表示 | - |

## 設定ファイル (appsettings.json)

```json
{
  "PostgresServers": {
    "dashboard": {
      "Name": "dashbord",
      "Host": "localhost",
      "Port": 5432,
      "MaintenanceDB": "hospital_db",
      "Username": "hospital_user",
      "Password": "your_password"
    },
    "warehouse": {
      "Name": "Data Warehouse",
      "Host": "warehouse.company.com",
      "Port": 5432,
      "MaintenanceDB": "warehouse_db",
      "Username": "etl_user",
      "Password": "etl_password"
    }
  },
  "ModelGeneratorSettings": {
    "OutputDirectory": "GeneratedModels",
    "Namespace": "OracleToPostgres.Models",
    "GenerateDataAnnotations": true,
    "GenerateJsonAttributes": false,
    "UseRecordTypes": false
  }
}
```

### 設定項目

**ModelGeneratorSettings:**
- `OutputDirectory`: モデルファイルの出力先
- `Namespace`: 生成されるクラスの名前空間
- `GenerateDataAnnotations`: Data Annotationsを生成するか（`[Key]`, `[Required]`など）
- `GenerateJsonAttributes`: JSON属性を生成するか（`[JsonPropertyName]`）
- `UseRecordTypes`: recordタイプを使用するか（classの代わり）

## 生成されるモデルの例

### 入力（PostgreSQLテーブル）

```sql
CREATE TABLE patients (
    patient_id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    birth_date DATE,
    email VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 出力（C#モデル）

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("patients")]
    public class Patients
    {
        [Key]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("last_name")]
        public string LastName { get; set; }

        [Column("birth_date")]
        public DateTime? BirthDate { get; set; }

        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

    }
}
```

## データ型マッピング

| PostgreSQL型 | C#型 |
|-------------|------|
| integer, int, int4 | int |
| bigint, int8 | long |
| smallint, int2 | short |
| decimal, numeric | decimal |
| real, float4 | float |
| double precision, float8 | double |
| boolean, bool | bool |
| varchar, text | string |
| timestamp | DateTime |
| timestamptz | DateTimeOffset |
| date | DateTime |
| time | TimeSpan |
| uuid | Guid |
| bytea | byte[] |
| json, jsonb | string |

※ NULL許容カラムは自動的にNullable型（`int?`, `DateTime?`など）になります。

## 実行例

### 例1: 基本的な実行

```bash
$ dotnet run -- --server dashboard

=== PostgreSQL Model Generator ===
データベースからテーブル構造を読み取り、C#モデルを生成します

接続先: localhost:5432/hospital_db
スキーマ: public
出力先: GeneratedModels
名前空間: OracleToPostgres.Models

データベーススキーマを読み取り中...
データベースから 5 個のテーブルを検出しました
テーブル 'patients' から 6 個のカラムを読み取りました
テーブル 'doctors' から 8 個のカラムを読み取りました
テーブル 'appointments' から 7 個のカラムを読み取りました
テーブル 'medical_records' から 10 個のカラムを読み取りました
テーブル 'prescriptions' から 5 個のカラムを読み取りました

5 個のテーブルが見つかりました:
  - patients (6 カラム)
  - doctors (8 カラム)
  - appointments (7 カラム)
  - medical_records (10 カラム)
  - prescriptions (5 カラム)

=== モデル生成開始: 5 個のテーブル ===
モデルファイルを生成しました: GeneratedModels/Patients.cs
モデルファイルを生成しました: GeneratedModels/Doctors.cs
モデルファイルを生成しました: GeneratedModels/Appointments.cs
モデルファイルを生成しました: GeneratedModels/MedicalRecords.cs
モデルファイルを生成しました: GeneratedModels/Prescriptions.cs
=== モデル生成完了 ===

✓ モデル生成が完了しました！
出力先: /home/user/webapp/ModelGenerator/GeneratedModels
```

### 例2: カスタム出力先

```bash
$ dotnet run -- --server dashboard --output ../OracleToPostgres/Models --namespace OracleToPostgres.Models

出力先: ../OracleToPostgres/Models
名前空間: OracleToPostgres.Models
```

## トラブルシューティング

### エラー: 接続できない

**原因**: PostgreSQLサーバーに接続できない

**解決策**:
1. `appsettings.json` の接続情報を確認
2. PostgreSQLサーバーが起動しているか確認
3. ファイアウォール設定を確認
4. 認証情報（ユーザー名、パスワード）を確認

### エラー: テーブルが見つからない

**原因**: 指定したスキーマにテーブルが存在しない

**解決策**:
1. スキーマ名を確認（デフォルトは `public`）
2. `--schema` オプションで正しいスキーマを指定
3. データベースに接続してテーブルの存在を確認

```sql
-- PostgreSQLで確認
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public';
```

## ビルド

```bash
cd ModelGenerator
dotnet build
```

## リリースビルド

```bash
cd ModelGenerator
dotnet publish -c Release -o ./publish
```

実行ファイル: `./publish/ModelGenerator.exe`（Windows）または `./publish/ModelGenerator`（Linux/Mac）

## 必要な環境

- .NET 6.0 SDK
- PostgreSQLサーバーへのアクセス権限

## 依存パッケージ

- Npgsql 6.0.11
- Serilog 3.1.1
- Microsoft.Extensions.Configuration 6.0.1

## ライセンス

このツールはOracle to PostgreSQL Data Transfer Toolプロジェクトの一部です。

## まとめ

このツールを使えば：
✅ データベース全体のモデルを数秒で生成  
✅ 手作業でのモデル作成が不要  
✅ 型安全なC#コードを自動生成  
✅ Data Annotations対応でバリデーションも自動  

開発効率が大幅に向上します！
