using Microsoft.Data.Sqlite;
using static logtool.LogEntryDataFunctions;
using static logtool.ui.Functions.Functions;
using unit = System.ValueTuple;

namespace logtool.ui.Infrastructure;

public class LogImportJob : FileProcessingJob
{
    private readonly IAppClient _appClient;

    public LogImportJob(
        Guid clientID,
        IAppClient appClient,
        IEnumerable<DatabaseColumn> databaseColumns,
        string file)
        : base(
            clientID,
            file)
    {
        _appClient = appClient;

        DatabaseColumns = databaseColumns;
    }

    public IEnumerable<DatabaseColumn> DatabaseColumns { get; }

    public long EntryCount { get; private set; }

    public override Task<JobRun<unit>> ProcessFile()
    {
        var connectionString = _appClient.GetConnectionString(ClientID, SqliteOpenMode.ReadWriteCreate);

        using var conn = new SqliteConnection(connectionString);

        conn.Open();

        EntryCount = PopulateDatabaseFromFiles(conn, new[] { File }, DatabaseColumns);

        return Task.FromResult(Success());
    }
}
