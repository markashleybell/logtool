namespace logtool.ui.Infrastructure;

public interface IJobQueue
{
    event EventHandler<FileProcessingJobCompletedEventArgs> OnFileProcessingJobCompleted;

    bool IsEmpty { get; }

    void AddingCompleted();

    void Enqueue(FileProcessingJob job);

    bool TryDequeue(out FileProcessingJob job);

    void NotifyJobCompleted(FileProcessingJob job);
}
