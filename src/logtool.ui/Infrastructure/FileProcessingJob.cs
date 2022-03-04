using unit = System.ValueTuple;

namespace logtool.ui.Infrastructure;

public abstract class FileProcessingJob
{
    protected FileProcessingJob(Guid clientID, string file)
    {
        ClientID = clientID;
        File = file;
    }

    public Guid ClientID { get; }

    public string File { get; }

    public abstract Task<JobRun<unit>> ProcessFile();
}
