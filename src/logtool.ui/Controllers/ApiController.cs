using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using logtool.ui.Models;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using static logtool.LogEntryDataFunctions;
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

        public ApiController(ILogger<ApiController> logger) =>
            _logger = logger;

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
            var connectionStringBuilder = new SqliteConnectionStringBuilder {
                DataSource = GetDatabasePath(),
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            var connectionString = connectionStringBuilder.ToString();

            var (valid, logColumns, error) = ValidateAndReturnColumns(request.Files);

            if (!valid)
            {
                var errorResponse = SelectFilesResponse.ValidationError(request.Files, error);

                return Json(errorResponse);
            }

            var (databaseColumns, errors) = GetDatabaseColumns(logColumns, DefaultIISW3CLogMappings);

            using var conn = new SqliteConnection(connectionString);

            conn.Open();

            ResetDatabase(conn);

            var count = PopulateDatabaseFromFiles(conn, request.Files, databaseColumns);

            var response = SelectFilesResponse.Success(request.Files, databaseColumns, errors);

            return Json(response, _jsonOptions);
        }
    }
}
