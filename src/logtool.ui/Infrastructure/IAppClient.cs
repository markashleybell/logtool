using Microsoft.Data.Sqlite;

namespace logtool.ui.Infrastructure;

public interface IAppClient
{
    string GetConnectionString(Guid clientID, SqliteOpenMode? openMode = null);

    string GetDataDirectoryPath(Guid clientID);

    string GetCsvExportTempPath(Guid clientID);

    string GetCsvExportDownloadUrl(Guid clientID);
}
