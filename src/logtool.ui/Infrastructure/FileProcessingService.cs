namespace logtool.ui.Infrastructure;

public class FileProcessingService : BackgroundService
{
    private readonly ILogger<FileProcessingService> _logger;
    private readonly IJobQueue _jobQueue;

    private readonly TimeSpan _loopInterval;

    public FileProcessingService(
        ILogger<FileProcessingService> logger,
        IJobQueue jobQueue)
    {
        _logger = logger;
        _jobQueue = jobQueue;

        _loopInterval = TimeSpan.FromMilliseconds(500);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_loopInterval, cancellationToken);

            if (!_jobQueue.IsEmpty && _jobQueue.TryDequeue(out var job))
            {
                switch (job)
                {
                    case LogImportJob logImportJob:
                        await logImportJob.ProcessFile();

                        _jobQueue.NotifyJobCompleted(logImportJob);

                        _logger.LogInformation("Import processed");

                        break;
                    case CsvExportJob csvExportJob:
                        await csvExportJob.ProcessFile();

                        _jobQueue.NotifyJobCompleted(csvExportJob);

                        _logger.LogInformation("Export processed");

                        break;
                    default:
                        throw new NotImplementedException($"Unknown job type: {job.GetType().Name}");
                }
            }
        }
    }
}
