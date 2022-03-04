using System.Collections.Concurrent;

namespace logtool.ui.Infrastructure;

public class JobQueue : IJobQueue
{
    public event EventHandler<FileProcessingJobCompletedEventArgs> OnFileProcessingJobCompleted;

    public void NotifyJobCompleted(FileProcessingJob job)
    {
        switch (job)
        {
            case CsvExportJob csvExportJob:
                OnFileProcessingJobCompleted?.Invoke(this, new FileProcessingJobCompletedEventArgs(job, "CSV export completed"));
                break;
            default:
                throw new NotImplementedException($"Unknown job type: {job.GetType().Name}");
        }
    }

    public ConcurrentQueue<FileProcessingJob> FileProcessingJobs { get; }
        = new ConcurrentQueue<FileProcessingJob>();
}
