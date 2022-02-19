namespace logtool.ui.Models;

public class SelectFilesResponse
{
    internal SelectFilesResponse(string[] files, string[] validationErrors, string[] missingColumnErrors)
    {
        Files = files ?? throw new ArgumentNullException(nameof(files));
        ValidationErrors = validationErrors ?? Array.Empty<string>();
        MissingColumnErrors = missingColumnErrors ?? Array.Empty<string>();
    }

    public static SelectFilesResponse ValidationError(string[] files, string error) =>
        new(files, new[] { error }, null);

    public static SelectFilesResponse Success(string[] files, IEnumerable<string> errors) =>
        new(files, null, errors.ToArray());

    public string[] Files { get; }

    public string[] ValidationErrors { get; }

    public string[] MissingColumnErrors { get; }

    public bool ErrorsOccurred =>
        ValidationErrors.Length > 0 || MissingColumnErrors.Length > 0;
}
