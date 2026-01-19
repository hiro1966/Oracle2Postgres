using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using OracleToPostgres.Models;

namespace OracleToPostgres.Services
{
    public class ConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public string GetOracleConnectionString() =>
            _configuration["DatabaseSettings:OracleOdbcConnectionString"] ?? string.Empty;

        public string GetPostgresConnectionString() =>
            _configuration["DatabaseSettings:PostgresConnectionString"] ?? string.Empty;

        public int GetBatchSize() =>
            int.TryParse(_configuration["DatabaseSettings:BatchSize"], out var size) ? size : 1000;

        public List<DataTransferTask> GetDataTransferTasks()
        {
            var tasks = _configuration.GetSection("DataTransferTasks")
                .Get<List<DataTransferTask>>();

            return tasks ?? new List<DataTransferTask>();
        }

        public string GetLogFilePath() =>
            _configuration["Logging:LogFilePath"] ?? "Logs/app-{Date}.log";

        public string GetMinimumLogLevel() =>
            _configuration["Logging:MinimumLevel"] ?? "Information";

        public bool GetAutoCloseOnCompletion() =>
            bool.TryParse(_configuration["AppSettings:AutoCloseOnCompletion"], out var autoClose) && autoClose;

        public int GetCloseDelaySeconds() =>
            int.TryParse(_configuration["AppSettings:CloseDelaySeconds"], out var delay) ? delay : 3;
    }
}
