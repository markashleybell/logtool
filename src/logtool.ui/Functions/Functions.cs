using logtool.ui.Infrastructure;
using static logtool.ui.Constants;
using unit = System.ValueTuple;

namespace logtool.ui.Functions;

public static class Functions
{
    public static int GetPageCount(int total) =>
        total % MaxResultsPerPage != 0 ? (total / MaxResultsPerPage) + 1 : (total / MaxResultsPerPage);

    public static JobRun<unit> Success() =>
        new(succeeded: true, result: new unit(), error: null, exception: null);

    public static JobRun<T> Success<T>(T result) =>
        new(succeeded: true, result: result, error: null, exception: null);

    public static JobRun<T> Failure<T>(string error, Exception exception = null) =>
        new(succeeded: false, result: default, error: error, exception: exception);

    public static JobRun<T> Failure<T>(T result, string error, Exception exception = null) =>
        new(succeeded: false, result: result, error: error, exception: exception);
}
