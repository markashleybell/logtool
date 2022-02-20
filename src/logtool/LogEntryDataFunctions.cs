using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace logtool;

public static class LogEntryDataFunctions
{
    public static readonly Func<IEnumerable<LogColumn>, Func<string[], string>> DefaultIISW3CTimestampGetterFactory =
        columns => {
            var date = columns.SingleOrDefault(c => c.Name == "date");
            var time = columns.SingleOrDefault(c => c.Name == "time");

            if (date is null || time is null)
            {
                throw new Exception("Default timestamp columns 'date' and 'time' not found in source log");
            }

            return cols => cols[date.Index] + " " + cols[time.Index];
        };

    public static readonly Func<IEnumerable<LogColumn>, Func<string[], string>> CloudflareClientIpAddressGetterFactory =
        columns => {
            var column = columns.SingleOrDefault(c => c.Name == "cf-client-ip")
                ?? columns.SingleOrDefault(c => c.Name == "c-ip");

            if (column is null)
            {
                throw new Exception("No 'cf-client-ip' or 'c-ip' column found in source log");
            }

            return cols => cols[column.Index];
        };

    public static readonly ColumnMapping[] DefaultIISW3CLogMappings =
        new[] {
            new ColumnMapping(new[] { "date", "time" },  "date", SqliteType.Text, DefaultIISW3CTimestampGetterFactory),
            new ColumnMapping(new[] { "s-ip" }, "serverip", SqliteType.Text),
            new ColumnMapping(new[] { "cs-method" }, "method", SqliteType.Text),
            new ColumnMapping(new[] { "cs-uri-stem" }, "url", SqliteType.Text),
            new ColumnMapping(new[] { "cs-uri-query" }, "querystring", SqliteType.Text),
            new ColumnMapping(new[] { "cs-username" }, "username", SqliteType.Text),
            new ColumnMapping(new[] { "c-ip", "cf-client-ip" }, "ip", SqliteType.Text, CloudflareClientIpAddressGetterFactory),
            new ColumnMapping(new[] { "cs(User-Agent)" }, "useragent", SqliteType.Text),
            new ColumnMapping(new[] { "cs(Referer)" }, "referer", SqliteType.Text),
            new ColumnMapping(new[] { "s-port" }, "port", SqliteType.Integer),
            new ColumnMapping(new[] { "sc-status" }, "status", SqliteType.Integer),
            new ColumnMapping(new[] { "sc-substatus" }, "substatus", SqliteType.Integer),
            new ColumnMapping(new[] { "sc-win32-status" }, "winstatus", SqliteType.Integer),
            new ColumnMapping(new[] { "time-taken" }, "duration", SqliteType.Integer),
            new ColumnMapping(new[] { "sc-bytes" }, "bytessent", SqliteType.Integer),
            new ColumnMapping(new[] { "cs-bytes" }, "bytesrecieved", SqliteType.Integer),
            new ColumnMapping(new[] { "is-facebook-asn" }, "facebookasn", SqliteType.Text)
        };

    public static readonly Dictionary<string[], string> DefaultIndexes =
        new() {
            [new[] { "timestamp" }] = "ix_timestamp",
            [new[] { "date" }] = "ix_date",
            [new[] { "time" }] = "ix_time",
            [new[] { "date", "time" }] = "ix_datetime",
            [new[] { "time", "status" }] = "ix_time_status",
            [new[] { "status" }] = "ix_status",
            [new[] { "ip" }] = "ix_ip",
            [new[] { "url" }] = "ix_url"
        };

    public static void ResetDatabase(SqliteConnection connection)
    {
        using var dropCmd = new SqliteCommand("DROP TABLE IF EXISTS `entries`; VACUUM;", connection);

        dropCmd.ExecuteNonQuery();
    }

    public static IEnumerable<LogColumn> GetLogColumns(IEnumerable<string> fileHeader)
    {
        const string headerLinePrefix = "#Fields:";

        var columnNamesLine = fileHeader.SingleOrDefault(l => l.StartsWith(headerLinePrefix));

        return columnNamesLine is not null
            ? columnNamesLine
                .Replace(headerLinePrefix, string.Empty).Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select((c, i) => new LogColumn(i, c))
            : Enumerable.Empty<LogColumn>();
    }

    /// <summary>
    /// Check that the columns in all specified log files are consistent with each other and return the results.
    /// </summary>
    /// <param name="files">A list of absolute log file paths.</param>
    public static (bool valid, IEnumerable<LogColumn> columns, string? message) ValidateAndReturnColumns(IEnumerable<string> files)
    {
        var currentColumns = Enumerable.Empty<LogColumn>();

        foreach (var file in files)
        {
            var lines = File.ReadLines(file);

            var header = lines.Take(10);

            var columns = GetLogColumns(header);

            var columnsMatch = !currentColumns.Any() || columns.SequenceEqual(currentColumns);

            if (!columnsMatch)
            {
                return (false, columns, $"Columns in file {file} do not match previous files.");
            }

            currentColumns = columns;
        }

        return (true, currentColumns, default);
    }

    public static string GetSafeColumnName(string name) =>
        Regex.Replace(name, "[^a-z0-9]+", string.Empty).ToLowerInvariant();

    public static (IEnumerable<DatabaseColumn> databaseColumns, IEnumerable<string> errors) GetDatabaseColumns(
        IEnumerable<LogColumn> logColumns,
        ColumnMapping[] mappings)
    {
        var databaseColumns = new List<DatabaseColumn>();
        var errors = new List<string>();

        var remainingColumns = new List<LogColumn>(logColumns).AsEnumerable();

        /*
        Note that even though we're in a for loop, we create a
        *separate* variable (idx) to increment the index for the
        output database column.

        This is because database columns can potentially contain
        the combined or transformed values from multiple source
        log columns; in this case the index values diverge, so
        we need an independent counter for the output index.
        */
        for (int i = 0, idx = 0; i < mappings.Length; i++)
        {
            var mapping = mappings[i];

            var sourceColumnsForMapping = remainingColumns
                .Where(c => mapping.LogColumnNames.Any(lc => string.CompareOrdinal(c.Name, lc) == 0));

            if (!sourceColumnsForMapping.Any())
            {
                /*
                Log files vary widely even between individual IIS sites,
                so if the column(s) specified in the mapping don't exist
                in this log file, we just make a note and move on.
                */

                errors.Add($"Log column(s) '{string.Join("', '", mapping.LogColumnNames)}' not found in source log");

                continue;
            }

            var valueGetter = mapping.CreateLogValueGetter(sourceColumnsForMapping);

            var databaseColumn = new DatabaseColumn(
                index: idx++,
                name: mapping.DatabaseColumnName,
                dataType: mapping.DatabaseColumnType,
                sources: sourceColumnsForMapping,
                valueGetter: valueGetter);

            databaseColumns.Add(databaseColumn);

            remainingColumns = remainingColumns.Except(sourceColumnsForMapping);
        }

        // TODO: Handle columns which aren't in the mapping list

        return (databaseColumns, errors);
    }

    public static string GenerateTableSql(IEnumerable<DatabaseColumn> columns)
    {
        static string asColumnDefinition(DatabaseColumn c) =>
            $"`{c.Name}` {c.DataTypeString}";

        var columnDefinitions = string.Join("," + Environment.NewLine + "    ", columns.Select(asColumnDefinition));

        return $@"CREATE TABLE `entries` (
    {columnDefinitions}
)";
    }

    public static string GenerateInsertSql(IEnumerable<DatabaseColumn> columns) =>
        $"INSERT INTO `entries` (`{string.Join("`,`", columns.Select(c => c.Name))}`) VALUES ({string.Join(",", columns.Select(c => "$" + c.ParameterName))});";

    public static SqliteParameter[] GenerateInsertParameters(IEnumerable<DatabaseColumn> columns) =>
        columns.Select(c => new SqliteParameter(c.ParameterName, c.DataType)).ToArray();

    public static int PopulateDatabaseFromFiles(SqliteConnection connection, IEnumerable<string> files, IEnumerable<DatabaseColumn> databaseColumns)
    {
        var resultCount = 0;

        var sql = GenerateInsertSql(databaseColumns);
        var parameters = GenerateInsertParameters(databaseColumns);

        using var createCmd = new SqliteCommand(GenerateTableSql(databaseColumns), connection);

        createCmd.ExecuteNonQuery();

        using var pragmaCmd = connection.CreateCommand();

        pragmaCmd.CommandText = "PRAGMA journal_mode = MEMORY; PRAGMA synchronous = OFF;";
        pragmaCmd.ExecuteNonQuery();

        using var transaction = connection.BeginTransaction();

        using var cmd = connection.CreateCommand();

        cmd.CommandText = sql;

        cmd.Parameters.AddRange(parameters);

        cmd.Prepare();

        foreach (var file in files)
        {
            var lines = File.ReadLines(file);

            foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#")))
            {
                var rowValues = line.Split(' ');

                foreach (var c in databaseColumns)
                {
                    parameters[c.Index].Value = c.GetValue(rowValues);
                }

                // parameters.Dump();

                cmd.ExecuteNonQuery();

                resultCount++;
            }
        }

        transaction.Commit();

        using var indexCmd = connection.CreateCommand();

        var databaseColumnNames = databaseColumns.Select(c => c.Name);

        // Get any indexes where all key column names exist in the list of database columns
        var indexes = DefaultIndexes
            .Where(i => !i.Key.Except(databaseColumnNames).Any())
            .Select(i => $"CREATE INDEX {i.Value} ON `entries`(`{string.Join("`, `", i.Key)}`);");

        indexCmd.CommandText = string.Join(Environment.NewLine, indexes);
        indexCmd.ExecuteNonQuery();

        return resultCount;
    }

    public static void ReleaseDatabaseLock() =>
        SqliteConnection.ClearAllPools();

    public static (string select, string? where, string? orderby, string? limit) ParseSqlQuery(string sqlQuery)
    {
        var select = sqlQuery;

        var limit = default(string);
        var orderby = default(string);
        var where = default(string);

        var limitIndex = select.IndexOf("LIMIT", StringComparison.OrdinalIgnoreCase);

        if (limitIndex != -1)
        {
            limit = select[limitIndex..];
            select = select.Replace(limit, string.Empty);
        }

        var orderbyIndex = select.IndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);

        if (orderbyIndex != -1)
        {
            orderby = select[orderbyIndex..];
            select = select.Replace(orderby, string.Empty);
        }

        var whereIndex = select.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);

        if (whereIndex != -1)
        {
            where = select[whereIndex..];
            select = select.Replace(where, string.Empty);
        }

        return (select, where, orderby, limit);
    }
}
