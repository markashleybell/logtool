namespace logtool.ui.Infrastructure;

public class FileProcessingJobCompletedEventArgs
{
    public FileProcessingJobCompletedEventArgs(
        FileProcessingJob job,
        string message = null)
    {
        Job = job;
        Message = message;
    }

    public FileProcessingJob Job { get; }

    public string Message { get; }
}
