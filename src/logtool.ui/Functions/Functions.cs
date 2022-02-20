namespace logtool.ui.Functions;
using static logtool.ui.Constants;
public static class Functions
{
    public static string GetDatabasePath() =>
        Path.GetTempPath() + @"\logtool.db";

    public static int GetPageCount(int total) =>
        total % MaxResultsPerPage != 0 ? (total / MaxResultsPerPage) + 1 : (total / MaxResultsPerPage);
}
