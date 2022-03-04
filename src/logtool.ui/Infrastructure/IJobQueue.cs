using System.Collections.Concurrent;

namespace logtool.ui.Infrastructure;

public interface IJobQueue
{
    event EventHandler<FileProcessingJobCompletedEventArgs> OnFileProcessingJobCompleted;

    void NotifyJobCompleted(FileProcessingJob job);

    ConcurrentQueue<FileProcessingJob> FileProcessingJobs { get; }
}
