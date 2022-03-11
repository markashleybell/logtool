namespace logtool.ui.Infrastructure;

public class FileProcessingJobCompletedEventArgs
{
    public FileProcessingJobCompletedEventArgs(
        FileProcessingJob job,
        bool queueCompleted,
        string message = null)
    {
        Job = job;
        QueueCompleted = queueCompleted;
        Message = message;
    }

    public FileProcessingJob Job { get; }

    public bool QueueCompleted { get; }

    public string Message { get; }
}
