<Query Kind="Program">
  <Reference Relative="bin\Debug\net6.0\logtool.dll">D:\Src\logtool\bin\Debug\net6.0\logtool.dll</Reference>
  <NuGetReference>Microsoft.Data.Sqlite</NuGetReference>
  <Namespace>Microsoft.Data.Sqlite</Namespace>
  <Namespace>static logtool.LogEntryDataFunctions</Namespace>
</Query>

void Main()
{
    var workingDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);
    
    var files = new[] { @"C:\Users\me\Desktop\u_ex220218_x.log" };
    var db = workingDirectory + @"\tmp.db";

    var connectionStringBuilder = new SqliteConnectionStringBuilder {
        DataSource = db,
        Mode = SqliteOpenMode.ReadWriteCreate
        // Mode = SqliteOpenMode.Memory
    };

    var connectionString = connectionStringBuilder.ToString();

    var (valid, logColumns, error) = ValidateColumns(files);

    // logColumns.Dump("Source Log Columns");

    var (databaseColumns, errors) = GetDatabaseColumns(logColumns, DefaultIISW3CLogMappings);

    // databaseColumns.Dump("Database Columns");

    // errors.Dump("Not Matched");

    //GenerateInsertSql(databaseColumns).Dump();
    //GenerateInsertParameters(databaseColumns).Dump();
    //GenerateTableSql(databaseColumns).Dump();

    //var lines = File.ReadLines(file);
    //
    //lines.Select(l => l.Split(' ')).Count().Dump();

    using var conn = new SqliteConnection(connectionString);

    conn.Open();

    ResetDatabase(conn);

    var count = PopulateDatabaseFromFiles(conn, files, databaseColumns);
}
