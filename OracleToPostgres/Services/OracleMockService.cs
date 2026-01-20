using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Serilog;

namespace OracleToPostgres.Services
{
    /// <summary>
    /// Oracle接続のモックアップサービス
    /// テスト用のダミーデータを返す
    /// </summary>
    public class OracleMockService : IDataSource
    {
        public async Task<DataTable> ReadDataAsync(string query, string taskName)
        {
            Log.Information($"[MOCK] {taskName}: クエリを実行しています（モックデータを返します）");
            Log.Debug($"[MOCK] Query: {query}");

            await Task.Delay(500); // データベースアクセスを模擬

            // クエリからテーブル名を推測してダミーデータを生成
            if (query.ToUpper().Contains("DEPARTMENTS"))
            {
                return GenerateDepartmentsMockData();
            }
            else if (query.ToUpper().Contains("DOCTORS"))
            {
                return GenerateDoctorsMockData();
            }
            else if (query.ToUpper().Contains("WARDS"))
            {
                return GenerateWardsMockData();
            }
            else if (query.ToUpper().Contains("STAFF"))
            {
                return GenerateStaffMockData();
            }
            else if (query.ToUpper().Contains("PERMISSIONS"))
            {
                return GeneratePermissionsMockData();
            }
            else if (query.ToUpper().Contains("OUTPATIENT_RECORDS"))
            {
                return GenerateOutpatientRecordsMockData();
            }
            else if (query.ToUpper().Contains("INPATIENT_RECORDS"))
            {
                return GenerateInpatientRecordsMockData();
            }
            else if (query.ToUpper().Contains("SALES"))
            {
                return GenerateSalesMockData();
            }
            else if (query.ToUpper().Contains("MESSAGES"))
            {
                return GenerateMessagesMockData();
            }
            else
            {
                // 汎用的なダミーデータ
                return GenerateGenericMockData();
            }
        }

        private DataTable GenerateDepartmentsMockData()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("code", typeof(string));
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("display_order", typeof(int));
            table.Columns.Add("created_at", typeof(DateTime));

            table.Rows.Add(1, "INT", "内科", 1, DateTime.Now.AddMonths(-12));
            table.Rows.Add(2, "SUR", "外科", 2, DateTime.Now.AddMonths(-12));
            table.Rows.Add(3, "PED", "小児科", 3, DateTime.Now.AddMonths(-11));
            table.Rows.Add(4, "ORT", "整形外科", 4, DateTime.Now.AddMonths(-10));
            table.Rows.Add(5, "DER", "皮膚科", 5, DateTime.Now.AddMonths(-9));

            Log.Information($"[MOCK] Departments: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }

        private DataTable GenerateDoctorsMockData()
        {
            var table = new DataTable();
            table.Columns.Add("code", typeof(string));
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("department_code", typeof(string));
            table.Columns.Add("display_order", typeof(int));
            table.Columns.Add("created_at", typeof(DateTime));

            table.Rows.Add("D001", "田中 太郎", "INT", 1, DateTime.Now.AddMonths(-10));
            table.Rows.Add("D002", "鈴木 花子", "INT", 2, DateTime.Now.AddMonths(-9));
            table.Rows.Add("D003", "佐藤 次郎", "SUR", 3, DateTime.Now.AddMonths(-8));
            table.Rows.Add("D004", "高橋 三郎", "PED", 4, DateTime.Now.AddMonths(-7));
            table.Rows.Add("D005", "山本 四郎", "ORT", 5, DateTime.Now.AddMonths(-6));
            table.Rows.Add("D006", "渡辺 五郎", "DER", 6, DateTime.Now.AddMonths(-5));

            Log.Information($"[MOCK] Doctors: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }

        private DataTable GenerateWardsMockData()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("code", typeof(string));
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("capacity", typeof(int));
            table.Columns.Add("display_order", typeof(int));
            table.Columns.Add("created_at", typeof(DateTime));

            table.Rows.Add(1, "W01", "一般病棟A", 50, 1, DateTime.Now.AddMonths(-12));
            table.Rows.Add(2, "W02", "一般病棟B", 40, 2, DateTime.Now.AddMonths(-12));
            table.Rows.Add(3, "ICU", "集中治療室", 10, 3, DateTime.Now.AddMonths(-12));
            table.Rows.Add(4, "W03", "小児病棟", 30, 4, DateTime.Now.AddMonths(-11));

            Log.Information($"[MOCK] Wards: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }

        private DataTable GenerateStaffMockData()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("job_type_code", typeof(string));
            table.Columns.Add("created_at", typeof(DateTime));

            table.Rows.Add(1, "看護師 A", "01", DateTime.Now.AddMonths(-10));
            table.Rows.Add(2, "看護師 B", "01", DateTime.Now.AddMonths(-9));
            table.Rows.Add(3, "薬剤師 A", "02", DateTime.Now.AddMonths(-8));
            table.Rows.Add(4, "放射線技師 A", "03", DateTime.Now.AddMonths(-7));
            table.Rows.Add(5, "検査技師 A", "04", DateTime.Now.AddMonths(-6));

            Log.Information($"[MOCK] Staff: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }

        private DataTable GeneratePermissionsMockData()
        {
            var table = new DataTable();
            table.Columns.Add("job_type_code", typeof(string));
            table.Columns.Add("job_type_name", typeof(string));
            table.Columns.Add("level", typeof(int));

            table.Rows.Add("01", "看護師", 2);
            table.Rows.Add("02", "薬剤師", 2);
            table.Rows.Add("03", "放射線技師", 2);
            table.Rows.Add("04", "検査技師", 2);
            table.Rows.Add("05", "事務", 1);

            Log.Information($"[MOCK] Permissions: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }

        private DataTable GenerateOutpatientRecordsMockData()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("date", typeof(DateTime));
            table.Columns.Add("department_id", typeof(int));
            table.Columns.Add("new_patients_count", typeof(int));
            table.Columns.Add("returning_patients_count", typeof(int));
            table.Columns.Add("created_at", typeof(DateTime));

            var random = new Random();
            var baseDate = DateTime.Today.AddDays(-30);

            for (int i = 0; i < 30; i++)
            {
                var date = baseDate.AddDays(i);
                for (int deptId = 1; deptId <= 5; deptId++)
                {
                    table.Rows.Add(
                        i * 5 + deptId,
                        date,
                        deptId,
                        random.Next(5, 20),
                        random.Next(20, 50),
                        date
                    );
                }
            }

            Log.Information($"[MOCK] OutpatientRecords: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }

        private DataTable GenerateInpatientRecordsMockData()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("date", typeof(DateTime));
            table.Columns.Add("ward_id", typeof(int));
            table.Columns.Add("department_id", typeof(int));
            table.Columns.Add("current_patient_count", typeof(int));
            table.Columns.Add("new_admission_count", typeof(int));
            table.Columns.Add("discharge_count", typeof(int));
            table.Columns.Add("transfer_out_count", typeof(int));
            table.Columns.Add("transfer_in_count", typeof(int));
            table.Columns.Add("created_at", typeof(DateTime));

            var random = new Random();
            var baseDate = DateTime.Today.AddDays(-30);

            for (int i = 0; i < 30; i++)
            {
                var date = baseDate.AddDays(i);
                for (int wardId = 1; wardId <= 4; wardId++)
                {
                    table.Rows.Add(
                        i * 4 + wardId,
                        date,
                        wardId,
                        random.Next(1, 6),
                        random.Next(30, 50),
                        random.Next(0, 5),
                        random.Next(0, 5),
                        random.Next(0, 3),
                        random.Next(0, 3),
                        date
                    );
                }
            }

            Log.Information($"[MOCK] InpatientRecords: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }

        private DataTable GenerateSalesMockData()
        {
            var table = new DataTable();
            table.Columns.Add("doctor_code", typeof(string));
            table.Columns.Add("year_month", typeof(string));
            table.Columns.Add("outpatient_sales", typeof(decimal));
            table.Columns.Add("inpatient_sales", typeof(decimal));
            table.Columns.Add("updated_at", typeof(DateTime));

            var random = new Random();
            var doctors = new[] { "D001", "D002", "D003", "D004", "D005", "D006" };
            var today = DateTime.Today;

            for (int month = 0; month < 12; month++)
            {
                var targetMonth = today.AddMonths(-month);
                var yearMonth = targetMonth.ToString("yyyy-MM");

                foreach (var doctorCode in doctors)
                {
                    table.Rows.Add(
                        doctorCode,
                        yearMonth,
                        random.Next(1000000, 5000000),
                        random.Next(2000000, 8000000),
                        targetMonth
                    );
                }
            }

            Log.Information($"[MOCK] Sales: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }

        private DataTable GenerateMessagesMockData()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("content", typeof(string));
            table.Columns.Add("created_at", typeof(DateTime));

            table.Rows.Add(1, "システムメンテナンスのお知らせ", DateTime.Now.AddDays(-7));
            table.Rows.Add(2, "新しい電子カルテシステムの導入について", DateTime.Now.AddDays(-5));
            table.Rows.Add(3, "年末年始の診療体制について", DateTime.Now.AddDays(-3));
            table.Rows.Add(4, "院内感染対策の強化について", DateTime.Now.AddDays(-1));

            Log.Information($"[MOCK] Messages: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }

        private DataTable GenerateGenericMockData()
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("NAME", typeof(string));
            table.Columns.Add("CREATED_AT", typeof(DateTime));

            table.Rows.Add(1, "サンプルデータ1", DateTime.Now);
            table.Rows.Add(2, "サンプルデータ2", DateTime.Now);
            table.Rows.Add(3, "サンプルデータ3", DateTime.Now);

            Log.Information($"[MOCK] Generic: {table.Rows.Count}件のダミーデータを生成しました");
            return table;
        }
    }
}
