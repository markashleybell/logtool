namespace logtool.ui.Models;

public class SelectFilesResponse
{
    internal SelectFilesResponse(
        string[] files,
        IEnumerable<DatabaseColumn> databaseColumns,
        string[] validationErrors,
        string[] missingColumnErrors)
    {
        Files = files ?? throw new ArgumentNullException(nameof(files));
        DatabaseColumns = databaseColumns ?? Enumerable.Empty<DatabaseColumn>();
        ValidationErrors = validationErrors ?? Array.Empty<string>();
        MissingColumnErrors = missingColumnErrors ?? Array.Empty<string>();
    }

    public static SelectFilesResponse ValidationError(string[] files, string error) =>
        new(files, null, new[] { error }, null);

    public static SelectFilesResponse Success(string[] files, IEnumerable<DatabaseColumn> databaseColumns, IEnumerable<string> errors) =>
        new(files, databaseColumns, null, errors.ToArray());

    public string[] Files { get; }

    public IEnumerable<DatabaseColumn> DatabaseColumns { get; }

    public string[] ValidationErrors { get; }

    public string[] MissingColumnErrors { get; }

    public bool ErrorsOccurred =>
        ValidationErrors.Length > 0 || MissingColumnErrors.Length > 0;
}
