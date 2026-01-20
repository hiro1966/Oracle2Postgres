namespace OracleToPostgres.Models
{
    public class PostgresServerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string MaintenanceDB { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public ConnectionParameters? ConnectionParameters { get; set; }
    }

    public class ConnectionParameters
    {
        public string SslMode { get; set; } = "prefer";
        public int ConnectTimeout { get; set; } = 10;
    }
}
