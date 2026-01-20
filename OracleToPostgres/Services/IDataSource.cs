using System.Data;
using System.Threading.Tasks;

namespace OracleToPostgres.Services
{
    /// <summary>
    /// データソースのインターフェース（Oracle本番環境とモック環境を切り替えるため）
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// データソースから指定されたクエリを実行してデータを取得
        /// </summary>
        /// <param name="query">実行するクエリ</param>
        /// <param name="taskName">タスク名（ログ用）</param>
        /// <returns>取得したデータのDataTable</returns>
        Task<DataTable> ReadDataAsync(string query, string taskName);
    }
}
