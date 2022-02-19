using logtool.ui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using static logtool.ui.Functions.Functions;

namespace logtool.ui.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly string _connectionString;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;

        var connectionStringBuilder = new SqliteConnectionStringBuilder {
            DataSource = GetDatabasePath(),
            Mode = SqliteOpenMode.ReadOnly
        };

        _connectionString = connectionStringBuilder.ToString();
    }

    public IActionResult Index()
    {
        var model = new IndexViewModel();

        return View(model);
    }

    public IActionResult Query(string query, int page = 1)
    {
        const int perPage = 5000;

        IEnumerable<string[]> GetResults()
        {
            using var conn = new SqliteConnection(_connectionString);

            conn.Open();

            var command = conn.CreateCommand();

            command.CommandText = $"{query} {(!query.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase) ? "ORDER BY date" : "")} LIMIT {perPage * (page - 1)}, {perPage}";

            _logger.LogInformation(GetDatabasePath());
            _logger.LogInformation(command.CommandText);

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
}
