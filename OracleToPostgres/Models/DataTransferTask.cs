namespace OracleToPostgres.Models
{
    public class DataTransferTask
    {
        public string TaskName { get; set; } = string.Empty;
        public string OracleQuery { get; set; } = string.Empty;
        public string PostgresTableName { get; set; } = string.Empty;
        public string? PostgresServerKey { get; set; }
        public bool EnableTransform { get; set; }
    }
}
