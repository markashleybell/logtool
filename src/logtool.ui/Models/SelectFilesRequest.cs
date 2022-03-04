namespace logtool.ui.Models;

public class SelectFilesRequest : ApiRequestBase
{
    public string[] Files { get; set; }
}
