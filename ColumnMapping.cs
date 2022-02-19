using Microsoft.Data.Sqlite;

namespace logtool;

public class ColumnMapping
{
    private static readonly Func<IEnumerable<LogColumn>, Func<string[], string>> _defaultLogValueGetterFactory =
        columns => {
            var column = columns.SingleOrDefault();

            if (column is null)
            {
                throw new Exception("Invalid column mapping: no source column(s) specified");
            }

            return cols => cols[column.Index];
        };

    private readonly Func<IEnumerable<LogColumn>, Func<string[], string>> _logValueGetterFactory;

    public ColumnMapping(
        string[] logColumnNames,
        string databaseColumnName,
        SqliteType databaseColumnType,
        Func<IEnumerable<LogColumn>, Func<string[], string>>? logValueGetterFactory = null)
    {
        if (logColumnNames.Length == 0)
        {
            throw new ArgumentException("Invalid column mapping: no source column(s) specified", nameof(logColumnNames));
        }

        LogColumnNames = logColumnNames;
        DatabaseColumnName = databaseColumnName;
        DatabaseColumnType = databaseColumnType;

        _logValueGetterFactory = logValueGetterFactory ?? _defaultLogValueGetterFactory;
    }

    public string[] LogColumnNames { get; }

    public string DatabaseColumnName { get; }

    public SqliteType DatabaseColumnType { get; }

    public Func<string[], string> CreateLogValueGetter(IEnumerable<LogColumn> logColumns) =>
        _logValueGetterFactory(logColumns);
}
