using Microsoft.Data.Sqlite;

namespace logtool.ui.Infrastructure;

public class AppClient : IAppClient
{
    private readonly IWebHostEnvironment _environment;

    public AppClient(IWebHostEnvironment environment) =>
        _environment = environment;

    public string GetConnectionString(Guid clientID, SqliteOpenMode? openMode = null)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder {
            DataSource = $@"{GetDataDirectoryPath(clientID)}\logtool.db",
            Mode = openMode ?? SqliteOpenMode.ReadOnly
        };

        return connectionStringBuilder.ToString();
    }

    public string GetDataDirectoryPath(Guid clientID) =>
        $@"{_environment.WebRootPath}\clients\{clientID}";

    public string GetCsvExportTempPath(Guid clientID) =>
        $@"{GetDataDirectoryPath(clientID)}\logtool-export.csv";

    public string GetCsvExportDownloadUrl(Guid clientID) =>
        $"/clients/{clientID}/logtool-export.csv";
}
