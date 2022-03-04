using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.Sqlite;
using static logtool.ui.Functions.Functions;
using unit = System.ValueTuple;

namespace logtool.ui.Infrastructure;

public class CsvExportJob : FileProcessingJob
{
    private readonly IAppClient _appClient;
    private readonly string _query;

    public CsvExportJob(
        Guid clientID,
        IAppClient appClient,
        string query,
        string file)
        : base(
            clientID,
            file)
    {
        _query = query;
        _appClient = appClient;
    }

    public override Task<JobRun<unit>> ProcessFile()
    {
        IEnumerable<string[]> GetResults()
        {
            using var conn = new SqliteConnection(_appClient.GetConnectionString(ClientID));

            conn.Open();

            var command = conn.CreateCommand();

            command.CommandText = _query;

            using var reader = command.ExecuteReader();

            yield return Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToArray();

            while (reader.Read())
            {
                yield return Enumerable.Range(0, reader.FieldCount)
                    .Select(i => reader.GetName(i) == "date" ? reader.GetDateTime(i).ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss") : reader.GetString(i))
                    .ToArray();
            }
        }

        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture) {
            ShouldQuote = _ => true
        };

        using var writer = new StreamWriter(_appClient.GetCsvExportTempPath(ClientID), new FileStreamOptions { Mode = FileMode.Create, Access = FileAccess.Write });
        using var csv = new CsvWriter(writer, cfg);

        foreach (var row in GetResults())
        {
            foreach (var field in row)
            {
                csv.WriteField(field);
            }

            csv.NextRecord();
        }

        writer.Flush();

        return Task.FromResult(Success());
    }
}
