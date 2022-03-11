using System.Collections.Concurrent;

namespace logtool.ui.Infrastructure;

public class JobQueue : IJobQueue
{
    private static BlockingCollection<FileProcessingJob> _fileProcessingJobs = new();

    public event EventHandler<FileProcessingJobCompletedEventArgs> OnFileProcessingJobCompleted;

    public bool IsEmpty =>
        _fileProcessingJobs.IsCompleted;

    public void AddingCompleted() =>
        _fileProcessingJobs.CompleteAdding();

    public void Enqueue(FileProcessingJob job) =>
        _fileProcessingJobs.Add(job);

    public bool TryDequeue(out FileProcessingJob job) =>
        _fileProcessingJobs.TryTake(out job);

    public void NotifyJobCompleted(FileProcessingJob job)
    {
        switch (job)
        {
            case LogImportJob logImportJob:
                OnFileProcessingJobCompleted?.Invoke(this, new FileProcessingJobCompletedEventArgs(job, _fileProcessingJobs.IsCompleted, "Log import completed"));
                break;
            case CsvExportJob csvExportJob:
                OnFileProcessingJobCompleted?.Invoke(this, new FileProcessingJobCompletedEventArgs(job, _fileProcessingJobs.IsCompleted, "CSV export completed"));
                break;
            default:
                throw new NotImplementedException($"Unknown job type: {job.GetType().Name}");
        }

        if (_fileProcessingJobs.IsCompleted)
        {
            _fileProcessingJobs = new();
        }
    }
}
