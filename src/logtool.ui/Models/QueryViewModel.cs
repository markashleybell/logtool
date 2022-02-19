namespace logtool.ui.Models;

public class QueryViewModel
{
    public int Page { get; set; }

    public IEnumerable<string[]> Rows { get; set; }
}
