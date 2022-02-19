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

    public IActionResult Query(string query)
    {
        IEnumerable<string[]> GetResults()
        {
            using var conn = new SqliteConnection(_connectionString);

            conn.Open();

            var command = conn.CreateCommand();

            command.CommandText = query;

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
            Rows = GetResults()
        };

        return View(model);
    }
}
