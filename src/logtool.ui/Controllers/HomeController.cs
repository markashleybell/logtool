using System.Text.Json;
using logtool.ui.Infrastructure;
using logtool.ui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using static logtool.LogEntryDataFunctions;
using static logtool.ui.Constants;
using static logtool.ui.Functions.Functions;

namespace logtool.ui.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAppClient _appClient;

    public HomeController(
        ILogger<HomeController> logger,
        IAppClient appClient)
    {
        _logger = logger;
        _appClient = appClient;
    }

    public IActionResult Index()
    {
        var model = new IndexViewModel();

        return View(model);
    }

    public IActionResult Query(Guid clientID, string query, int page = 1)
    {
        var (select, where, orderby, limit) = ParseSqlQuery(query);

        IEnumerable<string[]> GetResults()
        {
            using var conn = new SqliteConnection(_appClient.GetConnectionString(clientID));

            conn.Open();

            var command = conn.CreateCommand();

            command.CommandText = $"{select} {where} {orderby ?? "ORDER BY date"} {limit ?? $"LIMIT {MaxResultsPerPage * (page - 1)}, {MaxResultsPerPage}"}";

            LogQueryInformation(command);

            using var reader = command.ExecuteReader();

            yield return Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToArray();

            while (reader.Read())
            {
                yield return Enumerable.Range(0, reader.FieldCount)
                    .Select(i => reader.GetName(i) == "date" ? reader.GetDateTime(i).ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss") : reader.GetString(i))
                    .ToArray();
            }
        }

        var model = new QueryViewModel {
            Page = page,
            Rows = GetResults()
        };

        return View(model);
    }

    public IActionResult DownloadExport(Guid id) =>
        File(_appClient.GetCsvExportDownloadUrl(id), "text/csv", fileDownloadName: $"logtool-export-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv");

    private void LogQueryInformation(SqliteCommand command)
    {
        _logger.LogInformation("Request: {Url}", Request.Path);
        _logger.LogInformation("Command: {CommandText}", command.CommandText);
    }
}
