namespace logtool.ui.Infrastructure;

public class JobRun<T>
{
    internal JobRun(
        bool succeeded,
        T result,
        string error,
        Exception exception)
    {
        Succeeded = succeeded;
        Result = result;
        Error = error;
        Exception = exception;
    }

    public bool Succeeded { get; }

    public bool Failed => !Succeeded;

    public T Result { get; }

    public string Error { get; }

    public Exception Exception { get; set; }
}
