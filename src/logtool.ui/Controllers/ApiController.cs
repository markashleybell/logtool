using System.Text.Json;
using System.Text.RegularExpressions;
using logtool.ui.Infrastructure;
using logtool.ui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using static logtool.LogEntryDataFunctions;
using static logtool.ui.Functions.Functions;

namespace logtool.ui.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : Controller
    {
        private const int ReconnectionInterval = 3000;

        private static readonly JsonSerializerOptions _jsonOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly ILogger<ApiController> _logger;
        private readonly IJobQueue _jobQueue;
        private readonly IAppClient _appClient;

        public ApiController(
            ILogger<ApiController> logger,
            IJobQueue jobQueue,
            IAppClient appClient)
        {
            _logger = logger;
            _jobQueue = jobQueue;
            _appClient = appClient;
        }

        [HttpPost]
        [Route("newclientid")]
        public Guid NewClientID()
        {
            var id = Guid.NewGuid();

            Directory.CreateDirectory(_appClient.GetDataDirectoryPath(id));

            return id;
        }

        [HttpPost]
        [Route("getfiles")]
        public string[] GetFiles(GetFilesRequest request) =>
            Directory.GetFiles(request.Folder, "*.log");

        [HttpPost]
        [Route("selectfiles")]
        public SelectFilesResponse SelectFiles(SelectFilesRequest request)
        {
            var (valid, logColumns, error) = ValidateAndReturnColumns(request.Files);

            if (!valid)
            {
                return SelectFilesResponse.ValidationError(request.Files, error);
            }

            var (databaseColumns, errors) = GetDatabaseColumns(logColumns, DefaultIISW3CLogMappings);

            var connectionString = _appClient.GetConnectionString(request.ClientID, SqliteOpenMode.ReadWriteCreate);

            using var conn = new SqliteConnection(connectionString);

            conn.Open();

            ResetDatabase(conn);

            var count = PopulateDatabaseFromFiles(conn, request.Files, databaseColumns);

            return SelectFilesResponse.Success(request.Files, databaseColumns, errors);
        }

        [HttpGet]
        [Route("rowcount")]
        public RowCountResponse RowCount(RowCountRequest request)
        {
            using var conn = new SqliteConnection(_appClient.GetConnectionString(request.ClientID));

            conn.Open();

            var command = conn.CreateCommand();

            command.CommandText = "SELECT COUNT(*) FROM entries";

            LogQueryInformation(command);

            return new RowCountResponse {
                TotalRows = Convert.ToInt32(command.ExecuteScalar())
            };
        }

        [HttpPost]
        [Route("resultcount")]
        public ResultCountResponse ResultCount(ResultCountRequest request)
        {
            var (_, where, _, limit) = ParseSqlQuery(request.Query);

            if (limit is not null)
            {
                // If the user has specified a LIMIT manually, just return that as the number of results
                var limitMatch = Regex.Match(limit, @"LIMIT\s+(?<skip>[^\s]+,\s+)?(?<perpage>[^\s]+)", RegexOptions.IgnoreCase);

                var limitCount = int.TryParse(limitMatch.Groups["perpage"].Value, out var n) ? n : 10;

                return new ResultCountResponse {
                    TotalResults = limitCount,
                    TotalPages = GetPageCount(limitCount)
                };
            }

            using var conn = new SqliteConnection(_appClient.GetConnectionString(request.ClientID));

            conn.Open();

            var command = conn.CreateCommand();

            command.CommandText = $"SELECT COUNT(*) FROM entries {where ?? string.Empty} {limit ?? string.Empty}";

            LogQueryInformation(command);

            var count = Convert.ToInt32(command.ExecuteScalar());

            return new ResultCountResponse {
                TotalResults = count,
                TotalPages = GetPageCount(count)
            };
        }

        [HttpPost]
        [Route("export")]
        public bool Export(ExportRequest request)
        {
            var job = new CsvExportJob(request.ClientID, _appClient, request.Query, _appClient.GetCsvExportTempPath(request.ClientID));

            _jobQueue.FileProcessingJobs.Enqueue(job);

            return true;
        }

        [HttpGet]
        [Route("subscribe/{clientID}")]
        public async Task Subscribe(Guid clientID, CancellationToken cancellationToken)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            async void OnFileProcessingJobCompleted(object _, FileProcessingJobCompletedEventArgs eventArgs)
            {
                if (eventArgs.Job.ClientID != clientID)
                {
                    return;
                }

                try
                {
                    var json = JsonSerializer.Serialize(new { eventArgs.Message }, _jsonOptions);

                    await Response.WriteAsync($"retry: {ReconnectionInterval}\r", cancellationToken);
                    await Response.WriteAsync($"data: {json}\r\r", cancellationToken);

                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to send notification: {Message}", ex.Message);
                }
            }

            _jobQueue.OnFileProcessingJobCompleted += OnFileProcessingJobCompleted;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogDebug("Task cancelled in Subscribe endpoint: user disconnected");
            }
            finally
            {
                _jobQueue.OnFileProcessingJobCompleted -= OnFileProcessingJobCompleted;
            }
        }

        private void LogQueryInformation(SqliteCommand command)
        {
            _logger.LogInformation("Request: {Url}", Request.Path);
            _logger.LogInformation("Command: {CommandText}", command.CommandText);
        }
    }
}
