<Query Kind="Program">
  <Reference Relative="..\src\logtool\bin\Debug\net6.0\logtool.dll">D:\Src\logtool\src\logtool\bin\Debug\net6.0\logtool.dll</Reference>
  <NuGetReference>Microsoft.Data.Sqlite</NuGetReference>
  <Namespace>Microsoft.Data.Sqlite</Namespace>
  <Namespace>static logtool.LogEntryDataFunctions</Namespace>
  <RuntimeVersion>6.0</RuntimeVersion>
</Query>

void Main()
{
    var workingDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);
    var artifactsDirectory = workingDirectory + @"\..\artifacts";
    
    var query = "select * from entries where xlkjlkaf order by y limit 10";
    
    var (select, where, orderby, limit) = ParseSqlQuery(query);
    
    select.Dump("SELECT");
    where.Dump("WHERE");
    orderby.Dump("ORDER BY");
    limit.Dump("LIMIT");
    
//    Directory.CreateDirectory(artifactsDirectory);
//    
//    var files = new[] { @"C:\Users\me\Desktop\u_ex220218_x.log" };
//    var db = artifactsDirectory + @"\tmp.db";
//
//    var connectionStringBuilder = new SqliteConnectionStringBuilder {
//        DataSource = db,
//        Mode = SqliteOpenMode.ReadWriteCreate
//        // Mode = SqliteOpenMode.Memory
//    };
//
//    var connectionString = connectionStringBuilder.ToString();
//
//    var (valid, logColumns, error) = ValidateAndReturnColumns(files);

    // logColumns.Dump("Source Log Columns");

    // var (databaseColumns, errors) = GetDatabaseColumns(logColumns, DefaultIISW3CLogMappings);

    // databaseColumns.Dump("Database Columns");

    // errors.Dump("Not Matched");

    //GenerateInsertSql(databaseColumns).Dump();
    //GenerateInsertParameters(databaseColumns).Dump();
    //GenerateTableSql(databaseColumns).Dump();

    //var lines = File.ReadLines(file);
    //
    //lines.Select(l => l.Split(' ')).Count().Dump();
//
//    using var conn = new SqliteConnection(connectionString);
//
//    conn.Open();
//
//    ResetDatabase(conn);
//
//    var count = PopulateDatabaseFromFiles(conn, files, databaseColumns);
//    
//    ReleaseDatabaseLock();
}

public static class TestHarness
{
    
}
