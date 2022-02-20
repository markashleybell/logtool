using System.Text.Json;
using System.Text.RegularExpressions;
using logtool.ui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using static logtool.LogEntryDataFunctions;
using static logtool.ui.Constants;
using static logtool.ui.Functions.Functions;

namespace logtool.ui.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly JsonSerializerOptions _jsonOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly ILogger<ApiController> _logger;

        private readonly string _connectionString;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;

            var connectionStringBuilder = new SqliteConnectionStringBuilder {
                DataSource = GetDatabasePath(),
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            _connectionString = connectionStringBuilder.ToString();
        }

        [HttpPost]
        [Route("getfiles")]
        public IActionResult GetFiles(GetFilesRequest request)
        {
            var files = Directory.GetFiles(request.Folder, "*.log");

            return Json(files, _jsonOptions);
        }

        [HttpPost]
        [Route("selectfiles")]
        public IActionResult SelectFiles(SelectFilesRequest request)
        {
            var (valid, logColumns, error) = ValidateAndReturnColumns(request.Files);

            if (!valid)
            {
                var errorResponse = SelectFilesResponse.ValidationError(request.Files, error);

                return Json(errorResponse);
            }

            var (databaseColumns, errors) = GetDatabaseColumns(logColumns, DefaultIISW3CLogMappings);

            using var conn = new SqliteConnection(_connectionString);

            conn.Open();

            ResetDatabase(conn);

            var count = PopulateDatabaseFromFiles(conn, request.Files, databaseColumns);

            var response = SelectFilesResponse.Success(request.Files, databaseColumns, errors);

            return Json(response, _jsonOptions);
        }

        [HttpPost]
        [Route("resultcount")]
        public IActionResult ResultCount(ResultCountRequest request)
        {
            var (_, where, _, limit) = ParseSqlQuery(request.Query);

            if (limit is not null)
            {
                // If the user has specified a LIMIT manually, just return that as the number of results
                var limitMatch = Regex.Match(limit, @"LIMIT\s+(?<skip>[^\s]+,\s+)?(?<perpage>[^\s]+)", RegexOptions.IgnoreCase);

                var limitCount = int.TryParse(limitMatch.Groups["perpage"].Value, out var n) ? n : 10;

                var limitResponse = new ResultCountResponse {
                    TotalResults = limitCount,
                    TotalPages = GetPageCount(limitCount)
                };

                return Json(limitResponse, _jsonOptions);
            }

            using var conn = new SqliteConnection(_connectionString);

            conn.Open();

            var command = conn.CreateCommand();

            command.CommandText = $"SELECT COUNT(*) FROM entries {where ?? string.Empty} {limit ?? string.Empty}";

            LogQueryInformation(command);

            var count = Convert.ToInt32(command.ExecuteScalar());

            var response = new ResultCountResponse {
                TotalResults = count,
                TotalPages = GetPageCount(count)
            };

            return Json(response, _jsonOptions);
        }

        private void LogQueryInformation(SqliteCommand command)
        {
            _logger.LogInformation("Request: {Url}", Request.Path);
            _logger.LogInformation("Database path: {DatabasePath}", GetDatabasePath());
            _logger.LogInformation("Command: {CommandText}", command.CommandText);
        }
    }
}
