namespace logtool.ui.Functions;

public static class Functions
{
    public static string GetDatabasePath() =>
        Path.GetTempPath() + @"\logtool.db";
}
