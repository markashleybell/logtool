# LogTool

This is a very rough devops helper app, intended to make the task of combing through IIS log data much quicker and easier.

The basic premise is:

- You select a folder(s) containing log files
- You select the files you want to work with from those folders
- The app pulls all the data from all selected files into a SQLite database
- You run queries against the data using standard SQL

## Publish UI project as single(ish) file

```powershell
cd src\logtool.ui

dotnet publish -p:PublishSingleFile=true -r win-x64 -c Release --self-contained true -p:EnableCompressionInSingleFile=true
```